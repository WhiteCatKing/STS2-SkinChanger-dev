using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace STS2SkinChanger.Core;

internal sealed class AppearanceResourceUnavailableException(string message) : InvalidOperationException(message);

// Pure preflight checks: reject before asking a native decoder to allocate memory.
internal static partial class RuntimeLoadSafety
{
    internal const long ResidentCacheBudget = 512L * 1024 * 1024;
    internal const long MaximumSingleLoadBytes = 1536L * 1024 * 1024;
    internal const int BundleLimit = 8;
    // Three separately aliased 748 MiB models fit, with room for another small stage.
    internal const long StageBundleBudget = 2560L * 1024 * 1024;
    internal const int StageBundleLimit = 12;
    internal const long MinimumHeadroom = 512L * 1024 * 1024;
    internal const long OverlayPreparationBytes = 64L * 1024 * 1024;

    // The same logical texture may ship its authoring PNG and the imported CTEX Godot uses.
    // Count the source too when the redirect is incomplete or points outside this overlay.
    internal static long EstimateOverlayTextureBytes(IReadOnlyDictionary<string, byte[]> files,
        IEnumerable<string>? paths = null)
    {
        var candidates = paths?.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return EstimateTextureBytes((candidates as IEnumerable<string> ?? files.Keys)
            .Where(path => files.ContainsKey(path) && !UsesPackedTextureImport(path, files, candidates))
            .Select(path => KeyValuePair.Create(path, files[path])));
    }

    private static bool UsesPackedTextureImport(string path, IReadOnlyDictionary<string, byte[]> files,
        IReadOnlySet<string>? candidates)
    {
        if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
            !files.TryGetValue(path + ".import", out var redirect)) return false;

        var text = Encoding.UTF8.GetString(redirect);
        if (InvalidImport().IsMatch(text)) return false;
        var targets = ImportPaths().Matches(text);
        return targets.Count != 0 && targets.All(match =>
        {
            var target = match.Groups[1].Value;
            return target.EndsWith(".ctex", StringComparison.OrdinalIgnoreCase) &&
                (candidates == null || candidates.Contains(target)) &&
                files.TryGetValue(target, out var data) && data.AsSpan().StartsWith("GST2"u8);
        });
    }

    [GeneratedRegex(@"(?mi)^\s*path(?:\.[a-z0-9_]+)?\s*=\s*""(res://[^""\r\n]+)""")]
    private static partial Regex ImportPaths();

    [GeneratedRegex(@"(?mi)^\s*valid\s*=\s*false\b")]
    private static partial Regex InvalidImport();

    internal static long EstimateTextureBytes(IEnumerable<KeyValuePair<string, byte[]>> files)
    {
        long bytes = 0;
        foreach (var pair in files)
        {
            var data = pair.Value.AsSpan();
            if (data.Length >= 24 && data[0] == 0x89 && data.Slice(1, 3).SequenceEqual("PNG"u8) &&
                data.Slice(12, 4).SequenceEqual("IHDR"u8))
            {
                bytes = checked(bytes + PixelBudget(BinaryPrimitives.ReadUInt32BigEndian(data[16..]),
                    BinaryPrimitives.ReadUInt32BigEndian(data[20..]), pair.Key));
                continue;
            }
            if (data.Length < 4 || !data[..4].SequenceEqual("GST2"u8))
            {
                if (pair.Key.EndsWith(".ctex", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("压缩贴图头无效：" + pair.Key);
                continue;
            }
            // Godot CompressedTexture2D: 36 byte header, format(u32), actual w/h(u16).
            if (data.Length < 52) throw new InvalidDataException("截断的压缩贴图：" + pair.Key);
            var width = BinaryPrimitives.ReadUInt16LittleEndian(data[40..]);
            var height = BinaryPrimitives.ReadUInt16LittleEndian(data[42..]);
            bytes = checked(bytes + PixelBudget(width, height, pair.Key));
        }
        return bytes;
    }

    private static long PixelBudget(uint width, uint height, string path)
    {
        if (width == 0 || height == 0 || width > 16384 || height > 16384)
            throw new InvalidDataException("贴图尺寸超出安全范围：" + path);
        return (long)width * height * 8; // decoded pixels + upload/mipmap headroom
    }

    internal static bool HasHeadroom(long available, long estimated) =>
        available >= MinimumHeadroom && estimated <= Math.Max(0, available - MinimumHeadroom);

    internal static bool HasPreparationHeadroom(long physical, long commit) =>
        physical >= MinimumHeadroom && HasHeadroom(commit, OverlayPreparationBytes);

    internal static void RequirePreparationHeadroom() =>
        RequireMemory(OverlayPreparationBytes, preparingOverlay: true);

    internal static void RequireHeadroom(long estimated) => RequireMemory(estimated, preparingOverlay: false);

    internal static bool CanRetainStageBundles(long extraBytes)
    {
        if (!OperatingSystem.IsWindows()) return true;
        var status = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        return !GlobalMemoryStatusEx(ref status) ||
            HasHeadroom((long)Math.Min(status.AvailablePhysical, status.AvailablePageFile),
                Math.Max(512L * 1024 * 1024, extraBytes));
    }

    private static void RequireMemory(long estimated, bool preparingOverlay)
    {
        if (!OperatingSystem.IsWindows()) return;
        var status = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        if (!GlobalMemoryStatusEx(ref status)) return;
        var physical = (long)status.AvailablePhysical;
        var commit = (long)status.AvailablePageFile;
        // Overlay staging allocates managed buffers. Require their commit budget plus a physical
        // floor, then check the actual native decode estimate against both limits separately.
        var allowed = preparingOverlay ? HasPreparationHeadroom(physical, commit) :
            HasHeadroom(Math.Min(physical, commit), estimated);
        if (!allowed)
        {
            const long mib = 1024 * 1024;
            var stage = preparingOverlay ? "组装外观资源包" : "加载外观资源";
            throw new AppearanceResourceUnavailableException(
                $"内存余量不足，已停止{stage}并保留现有模型。可用物理内存 {physical / mib} MiB，" +
                $"可用提交量 {commit / mib} MiB，本阶段需约 {(long)Math.Ceiling(estimated / (double)mib)} MiB，" +
                $"保护预留 {MinimumHeadroom / mib} MiB。");
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatus
    {
        public uint Length, Load;
        public ulong TotalPhysical, AvailablePhysical, TotalPageFile, AvailablePageFile,
            TotalVirtual, AvailableVirtual, AvailableExtendedVirtual;
    }
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatus status);
}

internal sealed class ResourceFailureCircuit
{
    private readonly Dictionary<string, DateTime> _failures = new(StringComparer.OrdinalIgnoreCase);
    internal bool IsBlocked(string key, DateTime now) => _failures.TryGetValue(key, out var until) && now < until;
    internal void Fail(string key, DateTime now)
    {
        if (_failures.Count >= 256) _failures.Remove(_failures.Keys.First());
        _failures[key] = now.AddSeconds(30);
    }
}
