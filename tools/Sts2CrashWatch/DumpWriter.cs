using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

// 崩溃现场转储：卡死时对活进程写全量 minidump；崩溃退出后捡 WER / .NET 生成的 dump。
internal static class DumpWriter
{
    private const int MiniDumpWithFullMemory = 0x00000002;
    private const int MiniDumpWithHandleData = 0x00000004;
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

    public static string? WriteForHungProcess(Process process, string directory)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(
            directory,
            $"dump_{DateTime.Now:yyyyMMdd_HHmmss}_pid{process.Id}.dmp");

        try
        {
            using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            var ok = MiniDumpWriteDump(
                process.Handle,
                (uint)process.Id,
                file.SafeFileHandle.DangerousGetHandle(),
                MiniDumpWithFullMemory | MiniDumpWithHandleData | MiniDumpWithThreadInfo,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

            if (ok && new FileInfo(path).Length > 0)
            {
                return path;
            }

            Console.WriteLine("[转储] MiniDumpWriteDump 失败，Win32 错误=" + Marshal.GetLastWin32Error());
            TryDelete(path);
            return null;
        }
        catch (Exception exception)
        {
            Console.WriteLine("[转储] 写入失败: " + exception.Message);
            TryDelete(path);
            return null;
        }
    }

    // 进程已退出时，找本次会话期间新出现的崩溃转储（WER / dotnet）
    public static string? FindFreshDump(DateTime since, string directory)
    {
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
