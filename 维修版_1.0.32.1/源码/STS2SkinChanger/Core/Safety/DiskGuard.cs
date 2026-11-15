using System;
using System.IO;

namespace STS2SkinChanger.Core.Safety;

/// <summary>
/// 写盘熔断（需求 1：防无限循环把 C 盘写爆）。三道闸：
/// <list type="number">
/// <item><b>会话写盘总量</b>：本次游戏会话里本 Mod 写入的字节数超过 <see cref="SessionBudgetBytes"/>
/// 就跳闸，本会话不再写任何缓存 / 覆盖包。正常一次会话首装 630–830 MB、之后多为增量或命中缓存；
/// 6 GB 是待实测校准的资源预算（连续换很多套不同大包也可能合法累计到），触发说明"本会话写得太多"，
/// 不等于一定是循环。</item>
/// <item><b>剩余空间下限</b>：目标盘写完后剩余空间会低于 <see cref="MinFreeBytes"/> 时拒绝这一次写入
/// （不跳闸：用户清理磁盘后同一会话里可以继续）。</item>
/// <item><b>已跳闸</b>：任何写入直接拒绝。</item>
/// </list>
/// 拒绝时抛 <see cref="DiskGuardException"/>（是 <see cref="IOException"/>，和"磁盘真写满"走同一条
/// 现有失败路径：换肤失败、保持当前外观）。
/// 接入点：<c>PckArchive.WriteEntries</c>（所有 PCK 覆盖包）和 <c>SkinChangerStorage.WriteCache</c>（所有缓存写入）。
/// </summary>
internal static class DiskGuard
{
    public const string BreakerKey = "disk";

    private static readonly object Gate = new();
    private static long _sessionWrittenBytes;

    [ThreadStatic]
    private static long _accountedOnThread;

    public static long SessionBudgetBytes { get; set; } = 6L * 1024 * 1024 * 1024;

    public static long MinFreeBytes { get; set; } = 2L * 1024 * 1024 * 1024;

    /// <summary>返回路径所在盘的剩余字节数；拿不到返回 null（此时不按剩余空间拦截）。</summary>
    public static Func<string, long?> FreeSpaceProbe { get; set; } = DefaultFreeSpace;

    public static long SessionWrittenBytes
    {
        get
        {
            lock (Gate)
            {
                return _sessionWrittenBytes;
            }
        }
    }

    public static bool IsTripped => CircuitBreaker.IsOpen(BreakerKey);

    /// <summary>写入前检查（已知大小）。不通过则抛 <see cref="DiskGuardException"/>。</summary>
    public static void EnsureCanWrite(string path, long bytes, string purpose)
    {
        if (!TryCheck(path, Math.Max(0, bytes), purpose, out var reason))
        {
            throw new DiskGuardException(reason!);
        }
    }

    /// <summary>写入前检查（大小未知时传 0，只看是否跳闸和当前剩余空间）。</summary>
    public static bool CanWrite(string path, string purpose, out string? reason) =>
        TryCheck(path, 0, purpose, out reason);

    /// <summary>写入完成后记账。超过会话预算即跳闸（这一次已经写了，之后的拒绝）。</summary>
    public static void Account(long bytes)
    {
        if (bytes <= 0)
        {
            return;
        }

        long total;
        lock (Gate)
        {
            _sessionWrittenBytes += bytes;
            total = _sessionWrittenBytes;
        }

        _accountedOnThread += bytes;
        if (total > SessionBudgetBytes)
        {
            CircuitBreaker.Trip(
                BreakerKey,
                $"本会话已写入 {Mb(total)}，超过上限 {Mb(SessionBudgetBytes)}（可能是循环重写，也可能只是本会话换得太多）");
        }
    }

    /// <summary>当前线程累计记过的字节数。外层包装用它扣掉内层已记的部分，避免重复记账。</summary>
    public static long AccountedOnCurrentThread => _accountedOnThread;

    internal static void ResetForTests()
    {
        lock (Gate)
        {
            _sessionWrittenBytes = 0;
        }

        _accountedOnThread = 0;
        FreeSpaceProbe = DefaultFreeSpace;
        SessionBudgetBytes = 6L * 1024 * 1024 * 1024;
        MinFreeBytes = 2L * 1024 * 1024 * 1024;
    }

    private static bool TryCheck(string path, long bytes, string purpose, out string? reason)
    {
        reason = null;
        if (IsTripped)
        {
            reason = $"写盘熔断已触发（{CircuitBreaker.TripReason(BreakerKey)}），本会话不再写入：{purpose}";
            return false;
        }

        var written = SessionWrittenBytes;
        if (written + bytes > SessionBudgetBytes)
        {
            CircuitBreaker.Trip(
                BreakerKey,
                $"本会话已写入 {Mb(written)}，再写 {Mb(bytes)}（{purpose}）将超过上限 {Mb(SessionBudgetBytes)}（可能是循环重写，也可能只是本会话换得太多）");
            reason = $"写盘熔断已触发，拒绝写入：{purpose}";
            return false;
        }

        long? free;
        try
        {
            free = FreeSpaceProbe(path);
        }
        catch
        {
            free = null;
        }

        if (free is { } available && available - bytes < MinFreeBytes)
        {
            reason =
                $"磁盘剩余 {Mb(available)}，写入 {Mb(bytes)}（{purpose}）后将低于保底 {Mb(MinFreeBytes)}，已拒绝。" +
                "请清理磁盘；皮肤保持当前外观。";
            SafetyLog.Throttled("disk:low", "[熔断] " + reason);
            return false;
        }

        return true;
    }

    private static long? DefaultFreeSpace(string path)
    {
        var root = Path.GetPathRoot(Path.GetFullPath(path));
        if (string.IsNullOrEmpty(root))
        {
            return null;
        }

        var drive = new DriveInfo(root);
        return drive.IsReady ? drive.AvailableFreeSpace : null;
    }

    private static string Mb(long bytes) => $"{bytes / 1024.0 / 1024.0:F0} MB";
}

internal sealed class DiskGuardException(string message) : IOException(message);
