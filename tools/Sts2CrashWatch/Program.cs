using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

// StS2 崩溃监视器（可分发版）
// 与游戏同时启动、晚于游戏退出；只把「确认无法恢复」的情况记为崩溃：
//   · 启动/运行崩溃：进程异常退出（退出码非 0）或日志出现致命标记
//   · 卡死：窗口连续无响应 ≥ 阈值，且同一时间段内 godot.log 完全没有增长（加载慢不算）
// 报告输出：桌面\杀戮尖塔2崩溃报告\<yyyy.MM.dd_HH：mm>崩溃报告\
//   其中含该次会话的 godot.log 副本 + 崩溃报告.md

internal static class Program
{
    private const string GameProcess = "SlayTheSpire2";
    private const string SteamUrl = "steam://rungameid/2868840";
    private const string AppId = "2868840";

    private static readonly string Desktop =
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

    private static readonly string ReportRoot = Path.Combine(Desktop, "杀戮尖塔2崩溃报告");

    private static readonly string UserData = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SlayTheSpire2");

    private static readonly string LogPath = Path.Combine(UserData, "logs", "godot.log");

    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        using var mutex = new Mutex(true, "Sts2CrashWatch.SingleInstance", out var createdNew);
        if (!createdNew)
        {
            Console.WriteLine("[监视] 已经有一个监视器在运行，本窗口将退出。");
            Thread.Sleep(2500);
            return 0;
        }

        var launchGame = !args.Contains("--no-launch");
        var hangSeconds = ReadIntArg(args, "--hang-seconds", 30);

        var gameDir = ResolveGameDir();
        Console.WriteLine("StS2 崩溃监视器");
        Console.WriteLine("  游戏目录: " + (gameDir ?? "未找到（不影响监视，只影响版本信息）"));
        Console.WriteLine("  日志文件: " + LogPath);
        Console.WriteLine("  报告位置: " + ReportRoot);
        Console.WriteLine($"  卡死阈值: 窗口无响应且日志停滞 ≥ {hangSeconds} 秒");
        Console.WriteLine();

        WerDumpConfig.Enable();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => WerDumpConfig.Disable();
        Console.CancelKeyPress += (_, _) => WerDumpConfig.Disable();

        var everLaunched = false;
        var idleBeat = DateTime.Now;

        while (true)
        {
            var process = FindGame();
            if (process == null)
            {
                if (launchGame && !everLaunched)
                {
                    Console.WriteLine("[监视] 正在确认游戏是否已经启动…");
                    if (WaitForGame(TimeSpan.FromSeconds(8)) != null)
                    {
                        Console.WriteLine("[监视] 游戏已在启动/运行，只监视，不再重复拉起。");
                        continue;
                    }

                    Console.WriteLine("[监视] 正在启动游戏…");
                    try
                    {
                        Process.Start(new ProcessStartInfo(SteamUrl) { UseShellExecute = true });
                        everLaunched = true;
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("[监视] 启动失败: " + exception.Message + "（请手动打开游戏）");
                    }
                }

                if (DateTime.Now - idleBeat >= TimeSpan.FromSeconds(30))
                {
                    idleBeat = DateTime.Now;
                    Console.WriteLine($"[监视] 等待游戏启动…（每 30 秒提示一次，双击桌面快捷方式或手动开游戏）");
                }

                Thread.Sleep(2000);
                continue;
            }

            Watch(process, hangSeconds, gameDir);
            Thread.Sleep(1500);
        }
    }

    private static int ReadIntArg(string[] args, string name, int fallback)
    {
        var index = Array.IndexOf(args, name);
        if (index >= 0 && index + 1 < args.Length && int.TryParse(args[index + 1], out var value) && value > 0)
        {
            return value;
        }

        return fallback;
    }

    private static Process? WaitForGame(TimeSpan timeout)
    {
        var deadline = DateTime.Now + timeout;
        while (DateTime.Now < deadline)
        {
            var found = FindGame();
            if (found != null)
            {
                return found;
            }

            Thread.Sleep(500);
        }

        return null;
    }

    private static Process? FindGame()
    {
        try
        {
            return Process.GetProcessesByName(GameProcess).FirstOrDefault(p =>
            {
                try
                {
                    _ = p.Handle;
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }
        catch
        {
            return null;
        }
    }

    private static void Watch(Process process, int hangSeconds, string? gameDir)
    {
        var sessionStart = DateTime.Now;
        long peakMemory = 0;
        var unresponsiveSince = DateTime.MinValue;
        long lastLogLength = SafeLogLength();
        var lastLogChange = DateTime.Now;
        var lastBeat = DateTime.Now;
        var hangConfirmed = false;
        string? dumpPath = null;

        Console.WriteLine($"[监视] 已连接 PID={process.Id}，开始监视。");

        while (true)
        {
            try
            {
                if (process.HasExited)
                {
                    break;
                }

                process.Refresh();

                if (process.WorkingSet64 > peakMemory)
                {
                    peakMemory = process.WorkingSet64;
                }

                var length = SafeLogLength();
                if (length != lastLogLength)
                {
                    lastLogLength = length;
                    lastLogChange = DateTime.Now;
                    unresponsiveSince = DateTime.MinValue;
                }

                if (DateTime.Now - lastBeat >= TimeSpan.FromSeconds(30))
                {
                    lastBeat = DateTime.Now;
                    Console.WriteLine($"[监视] 运行中 {uptimeSoFar(process, sessionStart):0.0} 分钟 | 内存 {process.WorkingSet64 / 1024 / 1024} MB | 日志 {length / 1024 / 1024} MB | 无响应={(process.Responding ? "否" : "是")}");
                }

                if (!process.Responding)
                {
                    if (unresponsiveSince == DateTime.MinValue)
                    {
                        unresponsiveSince = DateTime.Now;
                    }

                    var silentFor = (DateTime.Now - lastLogChange).TotalSeconds;
                    var stuckFor = (DateTime.Now - unresponsiveSince).TotalSeconds;

                    if (!hangConfirmed && stuckFor >= hangSeconds && silentFor >= hangSeconds)
                    {
                        hangConfirmed = true;
                        Console.WriteLine($"[监视] 已确认卡死：窗口无响应 {stuckFor:0} 秒，日志停滞 {silentFor:0} 秒。");
                        dumpPath = DumpWriter.WriteForHungProcess(process, Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "Sts2CrashWatch"));
                    }
                }
                else
                {
                    unresponsiveSince = DateTime.MinValue;
                }
            }
            catch
            {
                break;
            }

            Thread.Sleep(1000);
        }

        var exitedAt = DateTime.Now;
        int? exitCode = null;
        try
        {
            exitCode = process.ExitCode;
        }
        catch
        {
            // 取不到按异常处理
        }

        var uptime = exitedAt - sessionStart;
        var analysis = Analyze(uptime, exitCode, sessionStart);

        try
        {
            process.Dispose();
        }
        catch
        {
            // ignore
        }

        if (analysis == null && !hangConfirmed)
        {
            Console.WriteLine($"[监视] 正常退出（退出码={exitCode?.ToString() ?? "未知"}，时长 {uptime.TotalMinutes:0.0} 分钟），不生成报告。");
            return;
        }

        var kind = hangConfirmed ? "卡死" : analysis!.Kind;
        var reason = hangConfirmed
            ? $"窗口连续无响应且日志完全停滞（阈值 {hangSeconds} 秒），无法自行恢复。"
            : analysis!.Reason;

        if (dumpPath == null)
        {
            dumpPath = DumpWriter.FindFreshDump(sessionStart);
        }

        var folder = WriteReport(kind, reason, sessionStart, exitedAt, uptime, exitCode, peakMemory, hangConfirmed, dumpPath, gameDir);
        Console.WriteLine("[监视] 已生成报告: " + folder);
    }

    private static double uptimeSoFar(Process process, DateTime sessionStart) =>
        (DateTime.Now - sessionStart).TotalMinutes;

    private sealed record Analysis(string Kind, string Reason);

    private static Analysis? Analyze(TimeSpan uptime, int? exitCode, DateTime sessionStart)
    {
        if (exitCode is not null and not 0)
        {
            return new Analysis(uptime.TotalSeconds < 90 ? "启动崩溃" : "运行崩溃",
                $"进程以非 0 退出码结束（{exitCode}）。");
        }

        var (fatal, marker) = ScanLogForFatal(sessionStart);
        if (fatal)
        {
            return new Analysis(uptime.TotalSeconds < 90 ? "启动崩溃" : "运行崩溃",
                "日志出现致命标记：" + marker);
        }

        return null;
    }

    private static readonly string[] FatalMarkers =
    [
        "Unhandled exception",
        "EXCEPTION_ACCESS_VIOLATION",
        "Access violation",
        "0xC0000005",
        "Stack overflow",
        "Out of memory",
        "System.TypeLoadException",
        "Fatal error",
        "core/object/worker_thread_pool.cpp"
    ];

    private static (bool Fatal, string Marker) ScanLogForFatal(DateTime sessionStart)
    {
        if (!File.Exists(LogPath))
        {
            return (false, string.Empty);
        }

        try
        {
            var info = new FileInfo(LogPath);
            if (info.LastWriteTime < sessionStart.AddMinutes(-2))
            {
                return (false, string.Empty);
            }

            var tail = ReadLogTail(6000);
            foreach (var marker in FatalMarkers)
            {
                if (tail.Any(line => line.Contains(marker, StringComparison.OrdinalIgnoreCase)))
                {
                    return (true, marker);
                }
            }
        }
        catch
        {
            // ignore
        }

        return (false, string.Empty);
    }

    private static long SafeLogLength()
    {
        try
        {
            return File.Exists(LogPath) ? new FileInfo(LogPath).Length : 0;
        }
        catch
        {
            return 0;
        }
    }

    private static List<string> ReadLogTail(int lines)
    {
        if (!File.Exists(LogPath))
        {
            return [];
        }

        try
        {
            using var stream = new FileStream(LogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var all = new List<string>();
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                all.Add(line);
            }

            return all.Count <= lines ? all : all.GetRange(all.Count - lines, lines);
        }
        catch
        {
            return [];
        }
    }

    private static string WriteReport(
        string kind,
        string reason,
        DateTime started,
        DateTime exitedAt,
        TimeSpan uptime,
        int? exitCode,
        long peakMemory,
        bool hang,
        string? dumpPath,
        string? gameDir)
    {
        Directory.CreateDirectory(ReportRoot);

        var folderName = $"{exitedAt:yyyy.MM.dd_HH：mm}崩溃报告";
        var folder = Path.Combine(ReportRoot, folderName);
        Directory.CreateDirectory(folder);

        // 1) 复制该次会话的 godot.log
        try
        {
            if (File.Exists(LogPath))
            {
                File.Copy(LogPath, Path.Combine(folder, "godot.log"), overwrite: true);
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine("[报告] 复制日志失败: " + exception.Message);
        }

        // 1.5) 把转储复制进报告文件夹（玩家只需发这一个文件夹）
        if (!string.IsNullOrEmpty(dumpPath) && File.Exists(dumpPath))
        {
            try { File.Copy(dumpPath, Path.Combine(folder, "crash.dmp"), overwrite: true); }
            catch { }
        }

        // 2) 写崩溃报告.md
        var body = new StringBuilder();
        body.AppendLine("# 杀戮尖塔 2 崩溃报告");
        body.AppendLine();
        body.AppendLine("| 项 | 值 |");
        body.AppendLine("| --- | --- |");
        body.AppendLine($"| 类型 | {kind} |");
        body.AppendLine($"| 判定依据 | {reason} |");
        body.AppendLine($"| 发生时间 | {exitedAt:yyyy-MM-dd HH:mm:ss} |");
        body.AppendLine($"| 启动时间 | {started:yyyy-MM-dd HH:mm:ss} |");
        body.AppendLine($"| 运行时长 | {uptime.TotalMinutes:0.0} 分钟 |");
        body.AppendLine($"| 退出码 | {(exitCode?.ToString() ?? "未知")} |");
        body.AppendLine($"| 无响应 | {(hang ? "是" : "否")} |");
        body.AppendLine($"| 内存峰值 | {peakMemory / 1024 / 1024} MB |");
        body.AppendLine($"| 游戏版本 | {ReadGameVersion(gameDir)} |");
        body.AppendLine($"| 转储文件 | {(dumpPath == null ? "无" : dumpPath)} |");
        body.AppendLine($"| 日志副本 | 同目录 `godot.log` |");
        body.AppendLine();

        body.AppendLine("## 崩溃前日志（末尾 120 行）");
        body.AppendLine("```");
        foreach (var line in ReadLogTail(120))
        {
            body.AppendLine(line);
        }

        body.AppendLine("```");
        body.AppendLine();

        var errors = ReadLogTail(8000)
            .Where(l => l.Contains("[ERROR]", StringComparison.OrdinalIgnoreCase) ||
                        l.Contains("[SkinChanger] [WARN]", StringComparison.OrdinalIgnoreCase))
            .TakeLast(30)
            .ToList();

        if (errors.Count > 0)
        {
            body.AppendLine("## 错误与告警（末尾 30 条）");
            body.AppendLine("```");
            foreach (var line in errors)
            {
                body.AppendLine(line);
            }

            body.AppendLine("```");
            body.AppendLine();
        }

        var hangFile = Path.Combine(UserData, "Gurio.SkinChanger", "startup_hang.txt");
        if (File.Exists(hangFile))
        {
            body.AppendLine("## 皮肤切换器启动卡死记录");
            body.AppendLine("```");
            try
            {
                body.AppendLine(File.ReadAllText(hangFile));
            }
            catch
            {
                body.AppendLine("（读取失败）");
            }

            body.AppendLine("```");
            body.AppendLine();
        }

        var selections = Path.Combine(UserData, "Gurio.SkinChanger", "skin_changer.json");
        if (File.Exists(selections))
        {
            body.AppendLine("## 当前皮肤选择（摘要）");
            body.AppendLine("```json");
            try
            {
                body.AppendLine(SummarizeSelections(selections));
            }
            catch
            {
                body.AppendLine("（读取失败）");
            }

            body.AppendLine("```");
            body.AppendLine();
        }

        body.AppendLine("## 环境");
        body.AppendLine("```");
        body.AppendLine("OS     : " + Environment.OSVersion);
        body.AppendLine("CPU    : " + Environment.ProcessorCount + " 逻辑核心");
        body.AppendLine("游戏   : " + (gameDir ?? "未检测到"));
        body.AppendLine("监视器 : " + typeof(Program).Assembly.GetName().Version);
        body.AppendLine("```");

        File.WriteAllText(Path.Combine(folder, "崩溃报告.md"), body.ToString(), new UTF8Encoding(false));
        return folder;
    }

    private static string SummarizeSelections(string path)
    {
        var keys = new[] { "\"ironclad\"", "\"silent\"", "\"defect\"", "\"necrobinder\"", "\"regent\"", "\"merchant\"" };
        return string.Join(Environment.NewLine, File.ReadAllLines(path)
            .Select(l => l.Trim())
            .Where(l => keys.Any(k => l.StartsWith(k, StringComparison.OrdinalIgnoreCase)))
            .Take(12));
    }

    private static string ReadGameVersion(string? gameDir)
    {
        if (gameDir == null)
        {
            return "未知";
        }

        var info = Path.Combine(gameDir, "release_info.json");
        if (!File.Exists(info))
        {
            return "未知";
        }

        try
        {
            return Regex.Replace(File.ReadAllText(info).Trim(), @"\s+", " ");
        }
        catch
        {
            return "未知";
        }
    }

    // ---- 游戏目录定位：注册表 SteamPath → libraryfolders.vdf → 常见盘符 ----

    private static string? ResolveGameDir()
    {
        foreach (var library in EnumerateSteamLibraries())
        {
            var candidate = Path.Combine(library, "steamapps", "common", "Slay the Spire 2");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        foreach (var drive in new[] { "C", "D", "E", "F", "G" })
        {
            var candidate = $@"{drive}:\steam\steamapps\common\Slay the Spire 2";
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static IEnumerable<string> EnumerateSteamLibraries()
    {
        var steamRoot = ReadRegistryString(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath")
                        ?? ReadRegistryString(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath");

        if (string.IsNullOrWhiteSpace(steamRoot))
        {
            yield break;
        }

        steamRoot = steamRoot.Replace('/', '\\');
        yield return steamRoot;

        var vdf = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(vdf))
        {
            yield break;
        }

        string text;
        try
        {
            text = File.ReadAllText(vdf);
        }
        catch
        {
            yield break;
        }

        foreach (Match match in Regex.Matches(text, "\"path\"\\s+\"(?<p>[^\"]+)\""))
        {
            var path = match.Groups["p"].Value.Replace("\\\\", "\\");
            if (Directory.Exists(path))
            {
                yield return path;
            }
        }
    }

    private static string? ReadRegistryString(string key, string name)
    {
        try
        {
            using var baseKey = Microsoft.Win32.RegistryKey.OpenBaseKey(
                key.StartsWith("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase)
                    ? Microsoft.Win32.RegistryHive.CurrentUser
                    : Microsoft.Win32.RegistryHive.LocalMachine,
                Microsoft.Win32.RegistryView.Default);
            using var sub = baseKey.OpenSubKey(StripRoot(key));
            return sub?.GetValue(name) as string;
        }
        catch
        {
            return null;
        }
    }

    private static string StripRoot(string key)
    {
        var parts = key.Split('\\', 2);
        return parts.Length > 1 ? parts[1] : key;
    }
}








