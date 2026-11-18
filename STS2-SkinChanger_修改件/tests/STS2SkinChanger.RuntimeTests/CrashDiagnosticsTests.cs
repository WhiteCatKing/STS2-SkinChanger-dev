using System.Diagnostics;
using System.Text;
using System.Text.Json;
using STS2SkinChanger.Core;

internal static class CrashDiagnosticsTests
{
    internal static void Run()
    {
        if (!OperatingSystem.IsWindows()) { Console.WriteLine("Windows observer integration skipped on this OS."); return; }
        var root = Path.Combine(Path.GetTempPath(), "sc crash diagnostics 中文 " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var script = CrashDiagnostics.ExtractWatcher(root);
            Require(File.ReadAllBytes(script).Take(3).SequenceEqual(new byte[] { 0xef, 0xbb, 0xbf }), "Windows PowerShell source must preserve UTF-8 Chinese.");
            CheckPolicy(root, script);
            Check(root, script, "normal", 0, rotate: false, readOnly: false);
            Check(root, script, "crash", 7, rotate: true, readOnly: true, releaseReadOnly: true);
            Check(root, script, "locked", 7, rotate: false, readOnly: true);
            Check(root, script, "forced", 0, rotate: false, readOnly: false);
            Check(root, script, "early", 7, rotate: false, readOnly: false, early: true);
            var silence = AuditLogSilencer.Install();
            try
            {
                CrashDiagnostics.RecoverPending(root);
                Require(!Directory.EnumerateFiles(root, "pending-recovery", SearchOption.AllDirectories).Any() &&
                    Directory.EnumerateFiles(root, "recovered", SearchOption.AllDirectories).Any(), "Locked-log diagnostics must replay once on the next startup.");
                CrashDiagnostics.RecoverPending(root);
            }
            finally { silence.UnpatchAll(silence.Id); }
            CheckRetention(root);
            CheckPruneLimits(root);
            Console.WriteLine("Crash diagnostics passed: real hidden observer/fixture processes, clean exit, nonzero exit despite quit marker, old log rotation, read-only fallback and one-time recovery. No game process was launched.");
        }
        finally { try { Directory.Delete(root, true); } catch { } }
    }

    private static void Check(string root, string script, string label, int exitCode, bool rotate, bool readOnly, bool early = false, bool releaseReadOnly = false)
    {
        var session = Guid.NewGuid().ToString("N");
        var report = Path.Combine(root, session); Directory.CreateDirectory(report);
        var log = Path.Combine(root, label + " godot.log");
        File.WriteAllText(log, "session=" + session + "\nERROR: harmless shutdown resource leak\nSteamworks shutdown succeeded!\n", new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(report, "quit-complete"), "normal quit entry completed");
        if (label == "forced") File.WriteAllText(Path.Combine(report, "forced-exit"), "fixture exit watchdog");
        var targetStart = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe"))
        { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
        foreach (var arg in new[] { "-NoProfile", "-NonInteractive", "-Command", "Start-Sleep -Seconds 7; exit " + exitCode }) targetStart.ArgumentList.Add(arg);
        if (early)
        {
            targetStart.ArgumentList[targetStart.ArgumentList.Count - 1] = "Start-Sleep -Seconds 1; exit " + exitCode;
            File.WriteAllText(log, "session=" + session + "\nstartup incomplete\n");
            File.Delete(Path.Combine(report, "quit-complete"));
        }
        using var target = Process.Start(targetStart)!;
        var config = Path.Combine(report, "session.json");
        File.WriteAllText(config, JsonSerializer.Serialize(CrashDiagnostics.CreateConfiguration(target, log, report, session, "test")));
        using var held = readOnly ? File.Open(log, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete) : null;
        var start = CrashDiagnostics.CreateWatcherStart(script, config);
        Require(!start.UseShellExecute && start.CreateNoWindow && start.WindowStyle == ProcessWindowStyle.Hidden &&
            start.ArgumentList.Contains(script) && start.ArgumentList.Contains(config), "Observer must be hidden and pass paths as literal arguments.");
        if (early) Require(target.WaitForExit(4000), "Early-exit fixture did not terminate.");
        using var observer = Process.Start(start)!;
        try
        {
            var deadline = DateTime.UtcNow.AddSeconds(6);
            while (!File.Exists(Path.Combine(report, "watcher-ready")) && !observer.HasExited && DateTime.UtcNow < deadline) Thread.Sleep(50);
            if (!early) Require(File.Exists(Path.Combine(report, "watcher-ready")), "Observer failed to bind: " + ReadError(report));
            if (releaseReadOnly) held?.Dispose();
            if (label == "crash") CheckDump(root, script, target.Id);
            var original = log;
            if (rotate)
            {
                original = log + ".old";
                File.Move(log, original);
                File.WriteAllText(log, "new game session; do not annotate this log");
            }
            Require(target.WaitForExit(12000) && observer.WaitForExit(20000), "Fixture observer failed to stop with its one target.");
            Require(observer.ExitCode == 0, "Observer process failed: " + ReadError(report));
            if (exitCode == 0 && label != "forced")
            {
                Require(File.Exists(Path.Combine(report, "normal-exit")) && !File.Exists(Path.Combine(report, "diagnostic.txt")),
                    "A normal exit with ordinary ERROR text must not be reported as a crash.");
            }
            else
            {
                var text = File.ReadAllText(Path.Combine(report, "diagnostic.txt"));
                Require((early ? text.Contains("退出码未知") : label == "forced" ? text.Contains("退出收尾超时") : text.Contains("0x00000007") && text.Contains("异常退出")) && text.Contains(session), "Exit evidence/session were lost.");
                Require(File.Exists(Path.Combine(report, "godot-tail.log")), "Diagnostic snapshot missing.");
                if (readOnly && !releaseReadOnly) Require(File.Exists(Path.Combine(report, "pending-recovery")), "Read-only log must retain a next-start recovery record.");
                else Require(File.ReadAllText(original).Contains("SkinChanger.CrashDiagnostics"), "Original log was not annotated after target exit.");
                if (rotate) Require(!File.ReadAllText(log).Contains("SkinChanger.CrashDiagnostics"), "Old crash was written into a new game's log.");
            }
        }
        finally
        {
            // Only fixture processes created above are ever stopped; never inspect/control the game.
            if (!target.HasExited) target.Kill(true);
            if (!observer.HasExited) observer.Kill(true);
            target.WaitForExit(); observer.WaitForExit();
        }
    }
    private static void CheckPolicy(string root, string script)
    {
        var policy = Path.Combine(root, "policy.ps1");
        var path = script.Replace("'", "''");
        File.WriteAllText(policy, $$"""
$tokens=$null; $errors=$null
$ast=[System.Management.Automation.Language.Parser]::ParseFile('{{path}}',[ref]$tokens,[ref]$errors)
if ($errors.Count) { throw $errors[0] }
$fn=$ast.Find({param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq 'Test-HangCondition'},$true)
. ([scriptblock]::Create($fn.Extent.Text))
if (Test-HangCondition $false $false 60 60 30) { throw 'No window must not trigger' }
if (Test-HangCondition $true $true 60 60 30) { throw 'Responding window must not trigger' }
if (Test-HangCondition $true $false 60 5 30) { throw 'Growing log must not trigger' }
if (Test-HangCondition $true $false 5 60 30) { throw 'Brief stall must not trigger' }
if (-not (Test-HangCondition $true $false 30 30 30)) { throw 'Both thresholds must trigger' }
""", new UTF8Encoding(true));
        var start = CrashDiagnostics.CreateWatcherStart(policy, "unused");
        start.ArgumentList.RemoveAt(start.ArgumentList.Count - 1);
        start.ArgumentList.RemoveAt(start.ArgumentList.Count - 1);
        using var process = Process.Start(start)!;
        Require(process.WaitForExit(10000) && process.ExitCode == 0, "Actual observer hang policy failed.");
    }

    private static void CheckDump(string root, string script, int targetId)
    {
        var file = Path.Combine(root, "dump-test.ps1");
        var report = Path.Combine(root, "dump-fixture"); Directory.CreateDirectory(report);
        File.WriteAllText(file, $$"""
$ErrorActionPreference='Stop'
$report='{{report.Replace("'", "''")}}'
$utf8=[Text.UTF8Encoding]::new($false)
$target=[Diagnostics.Process]::GetProcessById({{targetId}})
$tokens=$null; $errors=$null
$ast=[System.Management.Automation.Language.Parser]::ParseFile('{{script.Replace("'", "''")}}',[ref]$tokens,[ref]$errors)
foreach ($name in @('Write-Text','Ensure-CrashNative','Capture-HangDump')) {
 $fn=$ast.Find({param($n) $n -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -eq $name},$true)
 . ([scriptblock]::Create($fn.Extent.Text))
}
$dump=Capture-HangDump
if (-not $dump -or -not [IO.File]::Exists($dump)) { throw 'Native fixture minidump failed' }
$bytes=[IO.File]::ReadAllBytes($dump)
if ([Text.Encoding]::ASCII.GetString($bytes,0,4) -ne 'MDMP') { throw 'Invalid minidump signature' }
""", new UTF8Encoding(true));
        var start = CrashDiagnostics.CreateWatcherStart(file, "unused");
        start.ArgumentList.RemoveAt(start.ArgumentList.Count - 1); start.ArgumentList.RemoveAt(start.ArgumentList.Count - 1);
        using var process = Process.Start(start)!;
        Require(process.WaitForExit(10000) && process.ExitCode == 0, "Actual native minidump capture on the harmless fixture failed.");
    }

    private static void CheckRetention(string root)
    {
        for (var i = 0; i < 12; i++)
        {
            foreach (var marker in new[] { "normal-exit", "appended" })
            {
                var directory = Path.Combine(root, Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, marker), "fixture");
                Directory.SetCreationTimeUtc(directory, DateTime.UtcNow.AddDays(-i - 1));
            }
        }
        var pending = Path.Combine(root, Guid.NewGuid().ToString("N")); Directory.CreateDirectory(pending);
        File.WriteAllText(Path.Combine(pending, "pending-recovery"), "retain unfinished reports");
        var unrelated = Path.Combine(root, "unrelated"); Directory.CreateDirectory(unrelated);
        File.WriteAllText(Path.Combine(unrelated, "normal-exit"), "not an owned session");
        CrashDiagnostics.PruneCompleted(root);
        Require(Directory.Exists(pending) && Directory.Exists(unrelated), "Cleanup must retain unfinished and unrelated folders.");
        var owned = Directory.EnumerateDirectories(root).Where(d => Guid.TryParseExact(Path.GetFileName(d), "N", out _)).ToArray();
        Require(owned.Count(d => File.Exists(Path.Combine(d, "normal-exit"))) == 3 &&
            owned.Count(d => File.Exists(Path.Combine(d, "appended")) || File.Exists(Path.Combine(d, "recovered"))) == 8,
            "Completed normal/crash history must be bounded separately.");
    }

    private static void CheckPruneLimits(string root)
    {
        var sandbox = Path.Combine(root, "prune limits " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(sandbox);
        var stale = NewSessionDirectory(sandbox);
        File.WriteAllText(Path.Combine(stale, "pending-recovery"), "abandoned run");
        Age(stale, 20);
        var current = NewSessionDirectory(sandbox);
        File.WriteAllText(Path.Combine(current, "pending-recovery"), "the session being observed");
        Age(current, 30);
        CrashDiagnostics.Prune(sandbox, current, 512L * 1024 * 1024);
        Require(!Directory.Exists(stale), "Abandoned old reports must be pruned by the same age rule.");
        Require(Directory.Exists(current), "The session currently being observed must never be pruned.");

        var budget = Path.Combine(root, "prune budget " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(budget);
        var oldest = NewSessionDirectory(budget);
        var middle = NewSessionDirectory(budget);
        var newest = NewSessionDirectory(budget);
        foreach (var directory in new[] { (oldest, 3), (middle, 2), (newest, 1) })
        {
            File.WriteAllBytes(Path.Combine(directory.Item1, "payload.bin"), new byte[1024 * 1024]);
            File.WriteAllText(Path.Combine(directory.Item1, "normal-exit"), "fixture");
            Age(directory.Item1, directory.Item2);
        }

        // 三份各约 1 MB，预算 2.5 MB：只够删掉最旧的一份。
        CrashDiagnostics.Prune(budget, null, 2500L * 1024);
        Require(!Directory.Exists(oldest) && Directory.Exists(middle) && Directory.Exists(newest),
            "The total byte budget must drop the oldest report first.");
        Directory.Delete(budget, true);
        Directory.Delete(sandbox, true);
    }

    private static string NewSessionDirectory(string root)
    {
        var directory = Path.Combine(root, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void Age(string directory, int days)
    {
        var when = DateTime.UtcNow.AddDays(-days);
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            File.SetLastWriteTimeUtc(file, when);
        }

        Directory.SetCreationTimeUtc(directory, when);
        Directory.SetLastWriteTimeUtc(directory, when);
    }

    private static string ReadError(string report) => File.Exists(Path.Combine(report, "watcher-error.txt"))
        ? File.ReadAllText(Path.Combine(report, "watcher-error.txt")) : "no observer error file";
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}
