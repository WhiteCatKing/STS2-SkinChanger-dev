using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

// 崩溃现场转储：卡死时对活进程写全量 minidump；崩溃退出后捡 WER / .NET 生成的 dump。
internal static class DumpWriter
{
    // 只保留线程栈和模块信息：完整内存转储一份就上 GB，卡死诊断用不到。
    private const int MiniDumpWithThreadInfo = 0x00001000;

    [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", SetLastError = true)]
    private static extern bool MiniDumpWriteDump(
        IntPtr hProcess,
        uint processId,
        IntPtr hFile,
        int dumpType,
        IntPtr exceptionParam,
        IntPtr userStreamParam,
        IntPtr callbackParam);

    private static readonly TimeSpan WriteTimeout = TimeSpan.FromSeconds(20);

    public static string? WriteForHungProcess(Process process, string directory)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(
            directory,
            $"dump_{DateTime.Now:yyyyMMdd_HHmmss}_pid{process.Id}.dmp");

        Exception? failure = null;
        using var finished = new ManualResetEventSlim(false);
        var worker = new Thread(() =>
        {
            try
            {
                using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
                var ok = MiniDumpWriteDump(
                    process.Handle,
                    (uint)process.Id,
                    file.SafeFileHandle.DangerousGetHandle(),
                    MiniDumpWithThreadInfo,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero);

                if (!ok)
                {
                    failure = new IOException("MiniDumpWriteDump 失败，Win32 错误=" + Marshal.GetLastWin32Error());
                }
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                finished.Set();
            }
        })
        {
            IsBackground = true,
            Name = "Sts2CrashWatch.DumpWriter"
        };
        worker.Start();

        // A hung process can block MiniDumpWriteDump indefinitely; the watcher must keep going.
        if (!finished.Wait(WriteTimeout))
        {
            Console.WriteLine($"[转储] 写入超过 {WriteTimeout.TotalSeconds:0} 秒未完成，放弃本次转储。");
            TryDelete(path);
            return null;
        }

        if (failure != null)
        {
            Console.WriteLine("[转储] 写入失败: " + failure.Message);
            TryDelete(path);
            return null;
        }

        try
        {
            if (new FileInfo(path).Length > 0)
            {
                return path;
            }
        }
        catch (IOException)
        {
        }

        Console.WriteLine("[转储] 转储文件为空，已删除。");
        TryDelete(path);
        return null;
    }

    // 进程已退出时，找本次会话期间新出现的崩溃转储（WER / dotnet）
    public static string? FindFreshDump(DateTime since)
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sts2CrashWatch");
        var candidates = new List<string>();
        candidates.AddRange(SafeEnumerate(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CrashDumps")));
        candidates.AddRange(SafeEnumerate(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp")));
        candidates.AddRange(SafeEnumerate(directory));

        return candidates
            .Where(p => p.EndsWith(".dmp", StringComparison.OrdinalIgnoreCase) ||
                        p.EndsWith(".mdmp", StringComparison.OrdinalIgnoreCase))
            .Select(p => new FileInfo(p))
            .Where(f => f.Exists && f.LastWriteTime >= since.AddSeconds(-5))
            .OrderByDescending(f => f.LastWriteTime)
            .Select(f => f.FullName)
            .FirstOrDefault();
    }

    private static IEnumerable<string> SafeEnumerate(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return [];
        }

        try
        {
            return Directory.EnumerateFiles(directory, "*.dmp", SearchOption.TopDirectoryOnly)
                .Concat(Directory.EnumerateFiles(directory, "*.mdmp", SearchOption.TopDirectoryOnly))
                .ToArray();
        }
        catch
        {
            return [];
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // ignore
        }
    }

    public static void ApplyDotnetDumpEnvironment(ProcessStartInfo startInfo, string directory)
    {
        Directory.CreateDirectory(directory);
        startInfo.Environment["DOTNET_DbgEnableMiniDump"] = "1";
        startInfo.Environment["DOTNET_DbgMiniDumpType"] = "4";
        startInfo.Environment["DOTNET_DbgMiniDumpName"] = Path.Combine(directory, "dotnet_crash_%p.dmp");
    }
}

