using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Godot;

namespace STS2SkinChanger.Core;

internal static class CrashDiagnostics
{
    private static int _started;
    private static string? _report;
    private static System.Threading.Timer? _notifications;
    private static int _hangLogged;
    private static int _observerErrorLogged;
    private static Process? _observer;
    private static int _readyReported;
    private static DateTime _observerLaunched;

    internal static void Start()
    {
        if (Interlocked.Exchange(ref _started, 1) != 0) return;
        try
        {
            var root = Path.Combine(SkinChangerPaths.Storage.RootDirectory, "diagnostics");
            Directory.CreateDirectory(root);
            RecoverPending(root);
            PruneCompleted(root);
            var session = Guid.NewGuid().ToString("N");
            _report = Path.Combine(root, session);
            Directory.CreateDirectory(_report);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => RecordManaged(
                e.IsTerminating ? "terminating" : "unhandled", SafeExceptionText(e.ExceptionObject));
            TaskScheduler.UnobservedTaskException += (_, e) => RecordManaged("unobserved-task (not necessarily fatal)", SafeExceptionText(e.Exception));
            // Never SetObserved, swallow exceptions, close the game or interfere with crash handling.
            if (!OperatingSystem.IsWindows())
            {
                ModLog.Info("[崩溃诊断] 托管异常记录已启用；原生退出/窗口监视仅支持 Windows。");
                return;
            }
            var script = ExtractWatcher(root);
            using var process = Process.GetCurrentProcess();
            var config = CreateConfiguration(process, Path.Combine(OS.GetUserDataDir(), "logs", "godot.log"),
                _report, session, Entry.InternalTestVersion);
            var configPath = Path.Combine(_report, "session.json");
            File.WriteAllText(configPath, JsonSerializer.Serialize(config), new UTF8Encoding(false));
            // Warn is intentionally outside the bounded Info budget: the observer binds to this marker.
            ModLog.Warn("[崩溃诊断] 已启用自动监视；session=" + session + "；诊断目录=" + _report);
            _observer = Process.Start(CreateWatcherStart(script, configPath));
            _observerLaunched = DateTime.UtcNow;
            if (_observer == null) throw new IOException("Cannot start crash observer.");
            _notifications = new System.Threading.Timer(_ =>
            {
                try
                {
                    var ready = File.Exists(Path.Combine(_report!, "watcher-ready"));
                    if (ready && Interlocked.Exchange(ref _readyReported, 1) == 0)
                        ModLog.Info("[崩溃诊断] 后台监视已就绪。");
                    if (!ready && DateTime.UtcNow - _observerLaunched > TimeSpan.FromSeconds(10) &&
                        _observer?.HasExited == true && Interlocked.Exchange(ref _observerErrorLogged, 1) == 0)
                        ModLog.Warn("[崩溃诊断] 后台监视未就绪即退出（可能被系统策略阻止）；本次保留托管异常记录。诊断目录=" + _report);
                    var observerError = Path.Combine(_report!, "watcher-error.txt");
                    if (File.Exists(observerError) && Interlocked.Exchange(ref _observerErrorLogged, 1) == 0)
                        ModLog.Warn("[崩溃诊断] 监视器报告异常，详见 " + observerError);
                    var hang = Path.Combine(_report!, "hang-pending.txt");
                    if (File.Exists(hang) && Interlocked.Exchange(ref _hangLogged, 1) == 0)
                        ModLog.Warn("[崩溃诊断] " + File.ReadAllText(hang, Encoding.UTF8));
                }
                catch { /* Diagnostics never affects gameplay. */ }
            }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        }
        catch (Exception error) { ModLog.Warn("[崩溃诊断] 自动监视未能启动：" + error.GetBaseException().Message); }
    }

    internal static object CreateConfiguration(Process process, string log, string report, string session, string version) => new
    {
        ProcessId = process.Id, ProcessName = process.ProcessName,
        ProcessStartedUtc = process.StartTime.ToUniversalTime().ToString("O"),
        LogPath = Path.GetFullPath(log), ReportDirectory = Path.GetFullPath(report),
        SessionId = session, ModVersion = version, HangSeconds = 30
    };

    internal static ProcessStartInfo CreateWatcherStart(string script, string config)
    {
        var executable = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.System),
            "WindowsPowerShell", "v1.0", "powershell.exe");
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden
        };
        foreach (var argument in new[] { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", script, "-ConfigPath", config })
            start.ArgumentList.Add(argument);
        return start;
    }

    internal static string ExtractWatcher(string root)
    {
        using var stream = typeof(CrashDiagnostics).Assembly.GetManifestResourceStream("STS2SkinChanger.Diagnostics.CrashWatch.ps1")
            ?? throw new IOException("Embedded crash observer missing.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = reader.ReadToEnd();
        var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(content)))[..16];
        var path = Path.Combine(root, "watcher-" + hash + ".ps1");
        // Windows PowerShell 5.1 needs the BOM for Chinese source text. Atomic publication keeps
        // a second game instance from observing a partially written script.
        if (!File.Exists(path) || File.ReadAllText(path, Encoding.UTF8) != content)
        {
            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try { File.WriteAllText(temporary, content, new UTF8Encoding(true)); File.Move(temporary, path, true); }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
        return path;
    }

    internal static void MarkNativeQuitCompleted()
    {
        try { if (_report != null) File.WriteAllText(Path.Combine(_report, "quit-complete"), DateTime.UtcNow.ToString("O")); }
        catch { }
    }

    internal static void MarkForcedShutdown()
    {
        try { if (_report != null) File.WriteAllText(Path.Combine(_report, "forced-exit"), "Existing quit watchdog timed out after native Quit returned."); }
        catch { }
    }

    private static string SafeExceptionText(object? error)
    {
        try { return error?.ToString() ?? "unknown"; }
        catch { return "Exception text could not be formatted: " + error?.GetType().FullName; }
    }

    private static void RecordManaged(string kind, string detail)
    {
        try
        {
            if (_report == null) return;
            var eventPath = Path.Combine(_report, "managed-events.log");
            if (kind != "terminating" && File.Exists(eventPath) && new FileInfo(eventPath).Length >= 262144) return;
            if (detail.Length > 32768) detail = detail[..32768] + " [truncated]";
            File.AppendAllText(Path.Combine(_report, "managed-events.log"),
                $"{DateTime.UtcNow:O} [{kind}] {detail}\n", new UTF8Encoding(false));
            if (kind == "terminating")
            {
                File.WriteAllText(Path.Combine(_report, "diagnostic.txt"),
                    "[SkinChanger.CrashDiagnostics] 托管未处理异常（进程终止）：\n" + detail, new UTF8Encoding(false));
                File.WriteAllText(Path.Combine(_report, "pending-recovery"), "Terminating managed exception; awaiting observer annotation.");
            }
        }
        catch { }
    }

    internal static void PruneCompleted(string root)
    {
        var prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var completed = Directory.EnumerateDirectories(root)
            .Where(path => Guid.TryParseExact(Path.GetFileName(path), "N", out _) &&
                (File.Exists(Path.Combine(path, "normal-exit")) || File.Exists(Path.Combine(path, "appended")) || File.Exists(Path.Combine(path, "recovered"))))
            .OrderByDescending(Directory.GetCreationTimeUtc).ToArray();
        var normal = 0; var abnormal = 0;
        foreach (var path in completed)
        {
            var keep = File.Exists(Path.Combine(path, "normal-exit")) ? ++normal <= 3 : ++abnormal <= 8;
            var absolute = Path.GetFullPath(path);
            if (keep || !absolute.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                File.GetAttributes(absolute).HasFlag(FileAttributes.ReparsePoint)) continue;
            try { Directory.Delete(absolute, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    internal static void RecoverPending(string root)
    {
        foreach (var directory in Directory.EnumerateDirectories(root))
        {
            if (!Guid.TryParseExact(Path.GetFileName(directory), "N", out _)) continue;
            var pending = Path.Combine(directory, "pending-recovery");
            var summary = Path.Combine(directory, "diagnostic.txt");
            if (!File.Exists(pending) || !File.Exists(summary)) continue;
            var text = File.ReadAllText(summary, Encoding.UTF8);
            ModLog.Warn("[崩溃诊断] 上次日志无法追加，现补记：\n" + text[..Math.Min(text.Length, 65536)]);
            File.Move(pending, Path.Combine(directory, "recovered"), true);
        }
    }
}
