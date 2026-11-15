using STS2SkinChanger.Core;
// 玩家把 AppData\Roaming\SlayTheSpire2 整个搬到大盘、原地只留一个 junction 是常见做法
// （本机就是 C:\Users\…\Roaming\SlayTheSpire2 -> D:\Storage\AppData\Roaming\SlayTheSpire2）。
// 旧 SafePath 连根目录自身也做 reparse 判定，于是缓存目录建不出来、退出清理删 0 个文件、
// 8 GB 裁剪整轮被否决，缓存一路堆到 63 GB。这里锁住两件事：根是链接时清理/裁剪照样工作；
// 预算不够时先删最旧的会话。
internal static class SkinCacheStorageTests
{
    public static void Run()
    {
        JunctionUserDirectoryStillCleans();
        TrimRemovesOldestSessionsFirst();
    }

    private static void JunctionUserDirectoryStillCleans()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "sc-junction-" + Guid.NewGuid().ToString("N"));
        var real = Path.Combine(baseDir, "real");
        var link = Path.Combine(baseDir, "link");
        Directory.CreateDirectory(real);
        try
        {
            if (!TryCreateJunction(link, real))
            {
                Console.WriteLine("  [跳过] 当前环境建不了 junction，跳过「用户目录是链接」回归用例。");
                return;
            }

            // 旧版本留下的会话残留：junction 生效后没有任何路径能删掉它。
            var staleRuntime = Path.Combine(real, "Gurio.SkinChanger", "cache", "stale-session", "runtime");
            Directory.CreateDirectory(staleRuntime);
            File.WriteAllBytes(Path.Combine(staleRuntime, "001.pck"), new byte[4096]);

            var temp = Path.Combine(baseDir, "temp");
            Directory.CreateDirectory(temp);
            using var storage = new SkinChangerStorage(link, temp, "junction-gate");
            storage.StartCacheSession();
            var cleanup = storage.CleanCaches();

            Require(cleanup.RemovedFiles >= 1, "用户目录是 junction 时，退出清理仍必须删掉旧会话缓存。");
            Require(
                !Directory.Exists(Path.Combine(real, "Gurio.SkinChanger", "cache", "stale-session")),
                "旧会话目录必须被清掉。");
        }
        finally
        {
            DeleteQuietly(baseDir);
        }
    }

    private static void TrimRemovesOldestSessionsFirst()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), "sc-trim-" + Guid.NewGuid().ToString("N"));
        var user = Path.Combine(baseDir, "user");
        var temp = Path.Combine(baseDir, "temp");
        var cacheRoot = Path.Combine(user, "Gurio.SkinChanger", "cache");
        Directory.CreateDirectory(cacheRoot);
        Directory.CreateDirectory(temp);
        var oldSession = Path.Combine(cacheRoot, "session-old");
        var midSession = Path.Combine(cacheRoot, "session-mid");
        var newSession = Path.Combine(cacheRoot, "session-new");
        try
        {
            foreach (var (session, ageMinutes) in new[] { (oldSession, 30), (midSession, 20), (newSession, 10) })
            {
                Directory.CreateDirectory(Path.Combine(session, "runtime"));
                File.WriteAllBytes(Path.Combine(session, "runtime", "001.pck"), new byte[1024]);
                Directory.SetLastWriteTimeUtc(session, DateTime.UtcNow.AddMinutes(-ageMinutes));
            }

            using var storage = new SkinChangerStorage(user, temp, "trim-order");
            var report = storage.TrimCacheBudget(1536);

            Require(report.RemovedSessions >= 1, "超出预算时必须真的删掉会话，而不是静默跳过。");
            Require(!Directory.Exists(oldSession), "预算不够时必须先删最旧的会话。");
            Require(Directory.Exists(newSession), "预算不够时必须保留最新的会话。");
        }
        finally
        {
            DeleteQuietly(baseDir);
        }
    }

    private static bool TryCreateJunction(string link, string target)
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo(
                "cmd.exe",
                "/c mklink /J \"" + link + "\" \"" + target + "\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process is null) return false;
            process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            process.WaitForExit(5000);
            return process.HasExited && process.ExitCode == 0 && Directory.Exists(link);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static void DeleteQuietly(string directory)
    {
        try { Directory.Delete(directory, recursive: true); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new InvalidOperationException(reason);
    }
}
