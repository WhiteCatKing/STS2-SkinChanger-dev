using System.Diagnostics;
using System.Text;

// StS2 崩溃监视器
// 与游戏同时启动，游戏退出后再退出。检测：启动崩溃 / 运行中崩溃或卡死 / 退出崩溃。
// 报告写到桌面 StS2崩溃报告\crash_yyyyMMdd_HHmmss_类型.md

internal static class Program
{
    private const string GameProcess = "SlayTheSpire2";
    private const string SteamUrl = "steam://rungameid/2868840";

    private static readonly string Desktop =
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

    private static readonly string ReportDir = Path.Combine(Desktop, "StS2崩溃报告");

    private static readonly string UserData = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SlayTheSpire2");

    private static readonly string GameDir = ResolveGameDir();

    private static string ResolveGameDir()
    {
        foreach (var drive in new[] { "D", "C", "E", "F" })
        {
            var candidate = $@"{drive}:\steam\steamapps\common\Slay the Spire 2";
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return @"D:\steam\steamapps\common\Slay the Spire 2";
    }

    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        var launchGame = !args.Contains("--no-launch");
        Console.WriteLine("StS2 崩溃监视器");
        Console.WriteLine("  游戏目录: " + GameDir);
        Console.WriteLine("  报告目录: " + ReportDir);
        Console.WriteLine("  日志目录: " + Path.Combine(UserData, "logs"));

        var everLaunched = false;
        while (true)
        {
            var process = FindGame();
            if (process == null)
            {
                if (launchGame && !everLaunched)
                {
                    Console.WriteLine("[监视] 启动游戏…");
                    try
                    {
                        Process.Start(new ProcessStartInfo(SteamUrl) { UseShellExecute = true });
                        everLaunched = true;
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("[监视] 启动失败: " + exception.Message);
                    }
                }

                Thread.Sleep(2000);
                continue;
            }

            Watch(process);
            Thread.Sleep(1500);
        }
    }

    private static Process? FindGame()
    {
        try
        {
            var processes = Process.GetProcessesByName(GameProcess);
            return processes.FirstOrDefault(p =>
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

    private static void Watch(Process process)
    {
        var started = DateTime.Now;
        long peakMemory = 0;
        var unresponsiveSince = DateTime.MinValue;
        var hung = false;

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
                var memory = process.WorkingSet64;
                if (memory > peakMemory)
                {
                    peakMemory = memory;
                }

                if (!process.Responding)
                {
                    if (unresponsiveSince == DateTime.MinValue)
                    {
                        unresponsiveSince = DateTime.Now;
                    }
                    else if (!hung && (DateTime.Now - unresponsiveSince).TotalSeconds >= 30)
                    {
                        hung = true;
                        Console.WriteLine("[监视] 检测到窗口无响应超过 30 秒。");
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
            // 拿不到退出码时按异常处理
        }

        var uptime = exitedAt - started;
        var analysis = Analyze(uptime, exitCode, hung);

        try
        {
            process.Dispose();
        }
        catch
        {
            // ignore
        }

        if (analysis == null)
        {
            Console.WriteLine($"[监视] 正常退出（退出码={exitCode?.ToString() ?? "未知"}，时长={uptime.TotalMinutes:0.0} 分钟），不生成报告。");
            return;
        }

        var path = WriteReport(analysis, started, exitedAt, uptime, exitCode, peakMemory, hung);
        Console.WriteLine("[监视] 已生成报告: " + path);
    }

    private sealed record Analysis(string Kind, string Reason);

    private static Analysis? Analyze(TimeSpan uptime, int? exitCode, bool hung)
    {
        var crashMarkers = ScanLogForCrashMarkers();

        if (hung)
        {
            return new Analysis("卡死", "窗口连续 30 秒无响应，进程未退出。");
        }

        var abnormalExit = exitCode is not null and not 0;

        if (abnormalExit)
        {
            var kind = uptime.TotalSeconds < 60 ? "启动崩溃" : "运行崩溃";
            return new Analysis(kind, $"退出码 {exitCode}（非 0 表示异常终止）。");
        }

        if (crashMarkers.Count > 0)
        {
            var kind = uptime.TotalSeconds < 60 ? "启动崩溃" : "运行崩溃";
            return new Analysis(kind, "日志中出现崩溃标记：" + string.Join(" / ", crashMarkers.Take(4)));
        }

        // 退出崩：正常退出码但日志没有收尾标志
        var tail = ReadLogTail(40);
        var sawShutdown = tail.Any(line =>
            line.Contains("FMOD Sound System: System released", StringComparison.OrdinalIgnoreCase) ||
            line.Contains("Steamworks shutdown succeeded", StringComparison.OrdinalIgnoreCase));

        if (!sawShutdown && uptime.TotalSeconds > 30)
        {
            return new Analysis("退出异常", "进程已退出但日志缺少正常关停标志（FMOD/Steamworks shutdown）。");
        }

        return null;
    }

    private static readonly string[] Markers =
    [
        "Unhandled exception",
        "Access violation",
        "EXCEPTION_ACCESS_VIOLATION",
        "0xC0000005",
        "Fatal",
        "CRASH",
        "System.TypeLoadException",
        "TargetInvocationException",
        "Stack overflow",
        "Out of memory",
        "ERROR: Parameter \"mem\" is null",
        "core/object/worker_thread_pool.cpp"
    ];

    private static List<string> ScanLogForCrashMarkers()
    {
        var found = new List<string>();
        foreach (var line in ReadLogTail(4000))
        {
            foreach (var marker in Markers)
            {
                if (line.Contains(marker, StringComparison.OrdinalIgnoreCase) && !found.Contains(marker))
                {
                    found.Add(marker);
                }
            }
        }

        return found;
    }

    private static List<string> ReadLogTail(int lines)
    {
        var path = Path.Combine(UserData, "logs", "godot.log");
        if (!File.Exists(path))
        {
            return [];
        }

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
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
        Analysis analysis,
        DateTime started,
        DateTime exitedAt,
        TimeSpan uptime,
        int? exitCode,
        long peakMemory,
        bool hung)
    {
        Directory.CreateDirectory(ReportDir);

        var stamp = exitedAt.ToString("yyyyMMdd_HHmmss");
        var path = Path.Combine(ReportDir, $"crash_{stamp}_{Sanitize(analysis.Kind)}.md");

        var logPath = Path.Combine(UserData, "logs", "godot.log");
        var tail = ReadLogTail(200);
        var errorLines = ReadLogTail(4000)
            .Where(l => l.Contains("[ERROR]", StringComparison.OrdinalIgnoreCase) ||
                        l.Contains("[WARN] [SkinChanger]", StringComparison.OrdinalIgnoreCase))
            .TakeLast(40)
            .ToList();

        var body = new StringBuilder();
        body.AppendLine($"# 崩溃报告 · {analysis.Kind}");
        body.AppendLine();
        body.AppendLine("| 项 | 值 |");
        body.AppendLine("| --- | --- |");
        body.AppendLine($"| 检测时间 | {exitedAt:yyyy-MM-dd HH:mm:ss} |");
        body.AppendLine($"| 类型 | {analysis.Kind} |");
        body.AppendLine($"| 判定依据 | {analysis.Reason} |");
        body.AppendLine($"| 启动时间 | {started:yyyy-MM-dd HH:mm:ss} |");
        body.AppendLine($"| 运行时长 | {uptime.TotalMinutes:0.0} 分钟 |");
        body.AppendLine($"| 退出码 | {(exitCode?.ToString() ?? "未知")} |");
        body.AppendLine($"| 无响应 | {(hung ? "是（≥30 秒）" : "否")} |");
        body.AppendLine($"| 内存峰值 | {peakMemory / 1024 / 1024} MB |");
        body.AppendLine($"| 游戏版本 | {ReadGameVersion()} |");
        body.AppendLine($"| 日志 | `{logPath}` |");
        body.AppendLine();

        body.AppendLine("## 崩溃前日志（末尾 200 行）");
        body.AppendLine("```");
        foreach (var line in tail)
        {
            body.AppendLine(line);
        }

        body.AppendLine("```");
        body.AppendLine();

        if (errorLines.Count > 0)
        {
            body.AppendLine("## 错误与告警行（末尾 40 条）");
            body.AppendLine("```");
            foreach (var line in errorLines)
            {
                body.AppendLine(line);
            }

            body.AppendLine("```");
            body.AppendLine();
        }

        var hang = Path.Combine(UserData, "Gurio.SkinChanger", "startup_hang.txt");
        if (File.Exists(hang))
        {
            body.AppendLine("## SkinChanger 启动卡死记录（startup_hang.txt）");
            body.AppendLine("```");
            try
            {
                body.AppendLine(File.ReadAllText(hang));
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
            body.AppendLine("## 当前皮肤选择（skin_changer.json 摘要）");
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
        body.AppendLine("OS    : " + Environment.OSVersion);
        body.AppendLine("CPU   : " + Environment.ProcessorCount + " 逻辑核心");
        body.AppendLine("游戏  : " + GameDir);
        body.AppendLine("监视器: " + typeof(Program).Assembly.GetName().Version);
        body.AppendLine("```");

        File.WriteAllText(path, body.ToString(), new UTF8Encoding(false));
        return path;
    }

    private static string ReadGameVersion()
    {
        var info = Path.Combine(GameDir, "release_info.json");
        if (!File.Exists(info))
        {
            return "未知";
        }

        try
        {
            return File.ReadAllText(info).Replace("\n", " ").Trim();
        }
        catch
        {
            return "未知";
        }
    }

    private static string SummarizeSelections(string path)
    {
        var lines = File.ReadAllLines(path);
        var picked = lines
            .Select(l => l.Trim())
            .Where(l => l.StartsWith("\"ironclad\"", StringComparison.OrdinalIgnoreCase) ||
                        l.StartsWith("\"silent\"", StringComparison.OrdinalIgnoreCase) ||
                        l.StartsWith("\"defect\"", StringComparison.OrdinalIgnoreCase) ||
                        l.StartsWith("\"necrobinder\"", StringComparison.OrdinalIgnoreCase) ||
                        l.StartsWith("\"regent\"", StringComparison.OrdinalIgnoreCase) ||
                        l.StartsWith("\"merchant\"", StringComparison.OrdinalIgnoreCase))
            .Take(12);
        return string.Join(Environment.NewLine, picked);
    }

    private static string Sanitize(string value) =>
        string.Concat(value.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}

