using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using STS2SkinChanger.Core.Safety;

namespace STS2SkinChanger.Core;

/// <summary>
/// 商人规范骨骼的基线还原。
/// <para>
/// Merchant2CuteII 这类整包商人皮肤被激活时，它自己的预加载器会用 CacheMode.Reuse 读一遍商人规范骨骼
/// （<c>shop_merchant_top.tres</c> / <c>merchant_hand_skel_data.tres</c>），并一直持有。读的时候它的 PCK
/// 盖在包栈上，于是 Godot 资源缓存里这两条规范路径就成了它的骨骼。之后换回「游戏默认」（或者涅奥这类
/// 不带规范资源的选择），我们只重挂了覆盖包，缓存没动；下次进店，游戏的商人场景按规范路径取到的还是
/// 它的骨骼，却没有它的位姿（0.18 缩放 / 偏移），于是放大飞到左上角。外观面板里再选一次能恢复，
/// 是因为那条路走运行时别名重建了节点。
/// </para>
/// <para>
/// 这里在商人分组的选择变化后（延后一帧，避开选择事务里的锁和挂载），如果新选择不提供规范骨骼、
/// 而缓存里那份又不是我们还原过的，就按「游戏默认」读一份基线骨骼，TakeOverPath 回规范路径，
/// 之后游戏再按规范路径取到的就是原版。
/// </para>
/// </summary>
internal static class MerchantCanonicalBaseline
{
    internal const string GroupId = "merchant";
    internal const string TopPath = "res://animations/backgrounds/merchant_room/top/shop_merchant_top.tres";
    internal const string HandPath = "res://animations/backgrounds/merchant_room/hand/merchant_hand_skel_data.tres";
    private const string SafetyKey = "merchant-baseline";

    private static readonly string[] CanonicalPaths = [TopPath, HandPath];
    private static readonly Dictionary<string, WeakReference<Resource>> Installed = new(StringComparer.Ordinal);
    private static bool _scheduled;

    /// <summary>选择事务里调用：商人分组受影响时，下一帧检查一次规范骨骼。</summary>
    public static void ScheduleAfterSelection(IEnumerable<string> affectedGroups)
    {
        // 假商人分组和真商人共用整包提供者（联动），从图鉴里改假商人也会换掉真商人。
        if (_scheduled ||
            !affectedGroups.Any(group =>
                group.Equals(GroupId, StringComparison.OrdinalIgnoreCase) ||
                group.Equals("fake_merchant_monster", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _scheduled = true;
        Callable.From(() =>
        {
            _scheduled = false;
            Safe.Run(SafetyKey, RestoreIfStale);
        }).CallDeferred();
    }

    private static void RestoreIfStale()
    {
        if (CanonicalPaths.Any(path => SkinService.SelectedSkinProvidesResource(GroupId, path)))
        {
            // 新选择自己提供规范骨骼（整包商人皮肤），位姿也由它负责，不动。
            return;
        }

        var stale = CanonicalPaths.Where(IsStale).ToArray();
        if (stale.Length == 0)
        {
            return;
        }

        var resources = SkinService.LoadBaselineResources(GroupId, stale);
        var restored = new List<string>(stale.Length);
        foreach (var path in stale)
        {
            if (!resources.TryGetValue(path, out var baseline) || baseline == null)
            {
                ModLog.Warn($"[商人] 读取规范骨骼基线失败，保持现状：{path}");
                continue;
            }

            baseline.TakeOverPath(path);
            if (PreloadManager.Cache.ContainsKey(path))
            {
                PreloadManager.Cache.SetAsset(path, baseline);
            }

            Installed[path] = new WeakReference<Resource>(baseline);
            restored.Add(Path.GetFileName(path));
        }

        if (restored.Count > 0)
        {
            ModLog.Info($"[商人] 已按游戏默认还原商人规范骨骼缓存：{string.Join("、", restored)}。");
        }
    }

    /// <summary>缓存里有这条规范路径，并且不是我们还原过的那份。没缓存过就没有可污染的东西。</summary>
    private static bool IsStale(string path)
    {
        if (!ResourceLoader.HasCached(path))
        {
            return false;
        }

        var cached = ResourceLoader.Load<Resource>(path, null, ResourceLoader.CacheMode.Reuse);
        return cached != null && !IsInstalled(path, cached);
    }

    private static bool IsInstalled(string path, Resource resource) =>
        Installed.TryGetValue(path, out var reference) &&
        reference.TryGetTarget(out var installed) &&
        ReferenceEquals(installed, resource);

    /// <summary>进店诊断：记一行商人主体骨骼的来源，便于实测时确认是否拿到了基线。</summary>
    public static void LogRoomSkeleton(NMerchantRoom room)
    {
        var button = FindButton(room);
        if (button?.GetNodeOrNull<Node>("%MerchantVisual") is not { } visual ||
            visual.Get("skeleton_data_res").AsGodotObject() is not Resource data)
        {
            return;
        }

        var source = IsInstalled(TopPath, data)
            ? "已还原的基线"
            : string.IsNullOrEmpty(data.ResourcePath) ? "（无路径）" : data.ResourcePath;
        ModLog.Info(
            $"[商人] 进店商人骨骼：{source}；选择={SkinService.GetEffectiveSelectionForDiagnostics(GroupId)}，" +
            $"缩放={(visual as Node2D)?.Scale.X:0.####}。");
    }

    private static NMerchantButton? FindButton(Node root)
    {
        var stack = new Stack<Node>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (!GodotObject.IsInstanceValid(node))
            {
                continue;
            }

            if (node is NMerchantButton button)
            {
                return button;
            }

            foreach (var child in node.GetChildren())
            {
                stack.Push(child);
            }
        }

        return null;
    }
}
