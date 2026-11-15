using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

/// <summary>
/// 代码型商人皮肤（NpcFeminization 框架，例如「商人娘化涅奥版」）的兼容枝条。
/// 这类包在商人骨骼节点下面挂自己的覆盖层，并把商人按钮的 MouseFilter 改成 Ignore，由它自己的
/// 命中区域转发点击。两个问题需要我们这边处理：
/// <list type="number">
/// <item>它按骨骼数据资源的 <c>ResourcePath</c> 全等匹配原版商人。我们重建 / 刷新缓存之后，资源可能是
/// 用 CacheMode.Ignore 读回来的，路径为空，或者是别名路径，它就找不到目标。回放前把这两个节点的
/// 骨骼数据接管回规范路径。</item>
/// <item>它没有对外的"卸下"入口。切走时它的覆盖层和命中区域还留在场上，按钮仍是 Ignore，商人就点不动了。
/// 切走前把带它标记的节点移出场景树（会触发它自己的 Dispose，恢复 MouseFilter），再兜底把按钮恢复为可点击。</item>
/// </list>
/// </summary>
internal static class CodeDrivenMerchantCompat
{
    private const string MerchantTopPath = "res://animations/backgrounds/merchant_room/top/shop_merchant_top.tres";
    private const string MerchantHandPath = "res://animations/backgrounds/merchant_room/hand/merchant_hand_skel_data.tres";
    private const string OverlayMeta = "NpcFeminization2Visual";

    /// <summary>回放房间就绪步骤之前调用：让商人和手部骨骼数据带上规范路径。</summary>
    public static void EnsureCanonicalTargets(NMerchantRoom room)
    {
        foreach (var (node, canonicalPath) in EnumerateTargets(room))
        {
            try
            {
                if (node.Get("skeleton_data_res").AsGodotObject() is not Resource data)
                {
                    continue;
                }

                var current = data.ResourcePath;
                if (string.Equals(current, canonicalPath, StringComparison.Ordinal))
                {
                    continue;
                }

                data.TakeOverPath(canonicalPath);
                ModLog.Info($"[商人] 代码型皮肤目标骨骼路径已接管：{(string.IsNullOrEmpty(current) ? "（空）" : current)} → {canonicalPath}");
            }
            catch (Exception exception)
            {
                ModLog.Warn("[商人] 接管代码型皮肤目标骨骼路径失败：" + exception.GetBaseException().Message);
            }
        }
    }

    /// <summary>从代码型商人皮肤切走之前调用：移除它留下的覆盖层，恢复商人按钮的点击。</summary>
    public static void RemoveLeftovers(NMerchantRoom room)
    {
        var removed = 0;
        foreach (var node in Descendants(room).Where(node => node.HasMeta(OverlayMeta)).ToArray())
        {
            if (!GodotObject.IsInstanceValid(node) || node.IsQueuedForDeletion())
            {
                continue;
            }

            // 先移出场景树：它的展示层订阅了 TreeExiting → Dispose，会自己把按钮的 MouseFilter 还原。
            node.GetParent()?.RemoveChild(node);
            node.QueueFree();
            removed++;
        }

        foreach (var button in Descendants(room).OfType<NMerchantButton>())
        {
            if (button.MouseFilter == Control.MouseFilterEnum.Ignore)
            {
                button.MouseFilter = Control.MouseFilterEnum.Stop;
            }
        }

        if (removed > 0)
        {
            ModLog.Info($"[商人] 已移除代码型商人皮肤留下的 {removed} 个覆盖节点，并恢复商人按钮点击。");
        }
    }

    private static IEnumerable<(Node Node, string CanonicalPath)> EnumerateTargets(NMerchantRoom room)
    {
        foreach (var button in Descendants(room).OfType<NMerchantButton>())
        {
            if (button.GetNodeOrNull<Node>("%MerchantVisual") is { } visual && GodotObject.IsInstanceValid(visual))
            {
                yield return (visual, MerchantTopPath);
            }
        }

        foreach (var hand in Descendants(room).OfType<NMerchantHand>())
        {
            if (hand.GetParent() is Node2D spine && GodotObject.IsInstanceValid(spine))
            {
                yield return (spine, MerchantHandPath);
            }
        }
    }

    private static IEnumerable<Node> Descendants(Node root)
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

            yield return node;
            foreach (var child in node.GetChildren())
            {
                stack.Push(child);
            }
        }
    }
}
