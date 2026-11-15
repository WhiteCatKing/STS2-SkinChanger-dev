using System.Security.Cryptography;
using System.Text;

namespace STS2SkinChanger.Core;

/// <summary>
/// 生成好的外观覆盖包（PCK）的跨会话缓存。
///
/// 为什么需要它：一个完整外观覆盖包在大型皮肤包上实测 100-400 MB，而启动/进对局时一定会
/// 重新生成一次。以前每次启动都要重写几百 MB（既慢又吃 C 盘），而这些字节完全是确定的：
/// 同样的游戏版本 + 同样的皮肤包版本 + 同样的选择 = 同样的覆盖包。
///
/// 三条约束，缺一不可：
///   1. **只缓存确定的输出**：文件名是覆盖包内容签名的哈希，内容不对应签名就会命中不了；
///   2. **包一变就整目录作废**：<see cref="BeginSession"/> 会比对"材质戳"（游戏 PCK 与所有
///      皮肤包 PCK 的大小/修改时间），不一样就清空——避免换包后还挂着旧资源的覆盖包；
///   3. **有硬上限**：超过 <see cref="MaxBytes"/> 时按最久未使用删除，缓存不会无限长大。
/// 这份缓存放在 `%APPDATA%\SlayTheSpire2\Gurio.SkinChanger\overlay-cache`，不在每次启动都会
/// 清空的会话缓存目录里，所以能真正跨会话复用。
/// </summary>
internal static class OverlayCacheStore
{
    /// <summary>缓存上限。3 GB 大约能放 8-20 个大型覆盖包，够常用组合反复启动使用。</summary>
    private const long MaxBytes = 3L * 1024 * 1024 * 1024;

    private static readonly object Sync = new();
    private static string? _stamp;

    internal static string DirectoryPath => Path.Combine(
        SkinChangerPaths.Storage.RootDirectory,
        "overlay-cache");

    private static string StampFile => Path.Combine(DirectoryPath, "stamp.txt");

    /// <summary>
    /// 每次初始化时调用：材质戳变化就清空缓存，然后按上限修剪。
    /// 返回是否复用了已有缓存（仅用于日志）。
    /// </summary>
    internal static bool BeginSession(string stamp)
    {
        lock (Sync)
        {
            _stamp = stamp;
            try
            {
                if (System.IO.Directory.Exists(DirectoryPath))
                {
                    var previous = File.Exists(StampFile)
                        ? File.ReadAllText(StampFile).Trim()
                        : string.Empty;
                    if (!previous.Equals(stamp, StringComparison.Ordinal))
                    {
                        DeleteAll();
                        ModLog.Info("皮肤包或游戏版本有变化：已清空外观覆盖包缓存。");
                    }
                }

                System.IO.Directory.CreateDirectory(DirectoryPath);
                File.WriteAllText(StampFile, stamp);
                TrimToLimit();
            }
            catch (Exception exception)
            {
                // 缓存不可用不影响正确性，只是退化成每次重新生成。
                ModLog.Warn("外观覆盖包缓存不可用：" + exception.GetBaseException().Message);
                _stamp = null;
            }

            return System.IO.Directory.Exists(DirectoryPath);
        }
    }

    /// <summary>按覆盖包签名找已经生成好的文件；命中即刷新访问时间，供 LRU 修剪使用。</summary>
    internal static string? TryGet(string signature)
    {
        lock (Sync)
        {
            if (_stamp == null)
            {
                return null;
            }

            var path = PathFor(signature);
            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                File.SetLastAccessTimeUtc(path, DateTime.UtcNow);
                return path;
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>本次生成的覆盖包写到哪：命中缓存时直接复用，未命中时写进缓存目录。</summary>
    internal static string ReservePath(string signature)
    {
        System.IO.Directory.CreateDirectory(DirectoryPath);
        return PathFor(signature);
    }

    /// <summary>写入成功后调用：保持上限。</summary>
    internal static void AfterWrite()
    {
        lock (Sync)
        {
            TrimToLimit();
        }
    }

    private static string PathFor(string signature) =>
        Path.Combine(DirectoryPath, signature.ToLowerInvariant() + ".pck");

    /// <summary>按"最久未使用"删到上限以下；跳过正在被本进程挂载的文件由操作系统占用自然兜底。</summary>
    private static void TrimToLimit()
    {
        try
        {
            if (!System.IO.Directory.Exists(DirectoryPath))
            {
                return;
            }

            var files = new DirectoryInfo(DirectoryPath)
                .EnumerateFiles("*.pck")
                .OrderBy(file => file.LastAccessTimeUtc)
                .ToArray();
            var total = files.Sum(file => file.Length);
            foreach (var file in files)
            {
                if (total <= MaxBytes)
                {
                    break;
                }

                try
                {
                    total -= file.Length;
                    file.Delete();
                }
                catch
                {
                    // 正在被 Godot 读的旧覆盖包删不掉：下次启动再修剪。
                }
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("修剪外观覆盖包缓存失败：" + exception.GetBaseException().Message);
        }
    }

    private static void DeleteAll()
    {
        foreach (var file in System.IO.Directory.EnumerateFiles(DirectoryPath))
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // 留给下次启动。
            }
        }
    }

    /// <summary>
    /// 材质戳：游戏主包 + 所有皮肤包的大小与修改时间。任何一方更新都会让它变化，
    /// 从而作废缓存——这是"不会挂到旧皮肤"的核心保证。
    /// </summary>
    internal static string ComputeStamp(string gamePackPath, IEnumerable<string> providerPackPaths)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendFileStamp(hash, gamePackPath);
        foreach (var path in providerPackPaths
                     .Where(path => !string.IsNullOrWhiteSpace(path))
                     .Distinct(StringComparer.OrdinalIgnoreCase)
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            AppendFileStamp(hash, path);
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static void AppendFileStamp(IncrementalHash hash, string path)
    {
        hash.AppendData(Encoding.UTF8.GetBytes(Path.GetFullPath(path)));
        try
        {
            var info = new FileInfo(path);
            hash.AppendData(BitConverter.GetBytes(info.Exists ? info.Length : -1));
            hash.AppendData(BitConverter.GetBytes(
                info.Exists ? info.LastWriteTimeUtc.Ticks : 0));
        }
        catch
        {
            hash.AppendData(BitConverter.GetBytes(-2L));
        }
    }
}
