using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace STS2SkinChanger.Core;

internal sealed class AppearanceResourceUnavailableException(string message) : InvalidOperationException(message);

// Pure preflight checks: reject before asking a native decoder to allocate memory.
internal static class RuntimeLoadSafety
{
    internal const long ResidentCacheBudget = 512L * 1024 * 1024;
    internal const long MaximumSingleLoadBytes = 1536L * 1024 * 1024;
    internal const int BundleLimit = 8;
    internal const long MinimumHeadroom = 512L * 1024 * 1024;

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

    internal static void RequireHeadroom(long estimated)
    {
        if (!OperatingSystem.IsWindows()) return;
        var status = new MemoryStatus { Length = (uint)Marshal.SizeOf<MemoryStatus>() };
        if (GlobalMemoryStatusEx(ref status) && !HasHeadroom(
                (long)Math.Min(status.AvailablePhysical, status.AvailablePageFile), estimated))
            throw new AppearanceResourceUnavailableException("内存余量不足，已停止本次外观加载并保留现有模型。");
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
