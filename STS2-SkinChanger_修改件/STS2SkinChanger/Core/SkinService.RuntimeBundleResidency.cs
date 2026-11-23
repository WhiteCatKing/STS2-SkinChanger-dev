using System.Diagnostics;
using System.Runtime.CompilerServices;
using Godot;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Core;

internal static partial class SkinService
{
    private static readonly RuntimeBundleResidency<RuntimeResourceBundleState> StageRuntimeBundles =
        new(RuntimeLoadSafety.StageBundleBudget, RuntimeLoadSafety.StageBundleLimit);
    private static readonly CardSourceCheckCache StageSourceChecks =
        new(static source => source.IsCurrent(), static () => System.Environment.TickCount64);
    private static readonly ConditionalWeakTable<Node, StageDisplayHold> StageDisplayHolds = new();

    private sealed record StageBundleSource(string Family, CardFileStamp[] Stamps)
    {
        internal string Key { get; } = string.Join("\n", Stamps.Select(stamp => stamp.Key));
    }

    private static void RequireStageSources(StageBundleSource sources, bool fresh = false)
    {
        foreach (var stamp in sources.Stamps)
        {
            if (fresh ? stamp.IsCurrent() : StageSourceChecks.IsCurrent(stamp)) continue;
            StageSourceChecks.Clear();
            foreach (var (key, prepared) in PreparedRuntimeOverlays.ToArray())
            {
                if (prepared.StageSource?.Stamps.Contains(stamp) != true) continue;
                StageRuntimeBundles.Forget(prepared.StageResidencyKey!);
                PreparedRuntimeOverlays.Remove(key);
            }
            foreach (var (key, prepared) in LastPreparedRuntimeOverlays.ToArray())
                if (prepared.StageSource?.Stamps.Contains(stamp) == true)
                    LastPreparedRuntimeOverlays.Remove(key);

            // The catalog holds the old PCK handle. A new alias from those bytes would still
            // be stale. Leave it to the existing failure policy until the catalog is reopened.
            throw new AppearanceResourceUnavailableException("战损外观包已变更，等待重新扫描：" + stamp.PhysicalPath);
        }
    }

    private static bool IsValidRuntimeBundle(RuntimeResourceBundleState? bundle) =>
        bundle != null && bundle.Resources.Values.All(GodotObject.IsInstanceValid);

    private static RuntimeBundleResidency<RuntimeResourceBundleState>.Lease? HoldStageBundle(
        string key, Dictionary<string, Resource> resources) =>
        ReferenceEquals(StageRuntimeBundles.Find(key)?.Resources, resources) ? StageRuntimeBundles.Hold(key) : null;

    private static void KeepStageDisplay(Node node, RuntimeBundleResidency<RuntimeResourceBundleState>.Lease lease)
    {
        var hold = StageDisplayHolds.GetValue(node, static owner =>
        {
            var lifetime = new StageDisplayHold();
            owner.TreeExiting += lifetime.Release;
            return lifetime;
        });
        hold.Set(lease);
    }

    private static string FormatStageResidencyMetrics(long start, long mount, long load, long callback,
        double callbackMs, int builtCount, string? admission, string? evictionReason, bool trimmedBeforeDecode)
    {
        var preparationMs = Stopwatch.GetElapsedTime(start, mount).TotalMilliseconds;
        var mountMs = Stopwatch.GetElapsedTime(mount, load).TotalMilliseconds;
        var creationMs = Stopwatch.GetElapsedTime(load, callback).TotalMilliseconds;
        var (segment, slowest) = new[]
        {
            ("准备", preparationMs), ("挂载", mountMs), ("资源创建", creationMs), ("显示回调", callbackMs)
        }.MaxBy(segment => segment.Item2);
        var reason = evictionReason ?? "-";
        if (trimmedBeforeDecode)
            reason = reason == "-" ? "解码前内存压力" : "解码前内存压力+" + reason;
        return $" 建包={builtCount} 准备={preparationMs:F1}ms 挂载={mountMs:F1}ms " +
            $"资源创建={creationMs:F1}ms 显示回调={callbackMs:F1}ms 最慢段={segment}/{slowest:F1}ms" +
            $" 战损缓存={admission ?? "命中"} 驻留包={StageRuntimeBundles.Count}" +
            $" 纹理字节={StageRuntimeBundles.Bytes} 淘汰原因={reason}";
    }

    private sealed class StageDisplayHold
    {
        private RuntimeBundleResidency<RuntimeResourceBundleState>.Lease? _lease;

        internal void Set(RuntimeBundleResidency<RuntimeResourceBundleState>.Lease lease)
        {
            _lease?.Dispose();
            _lease = lease;
        }

        internal void Release()
        {
            _lease?.Dispose();
            _lease = null;
        }
    }
}
