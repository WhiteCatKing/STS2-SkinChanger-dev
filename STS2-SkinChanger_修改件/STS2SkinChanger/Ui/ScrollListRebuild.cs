using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

// Rebuild the rows, not the viewport. Keeping the ScrollContainer in the scene tree also
// keeps its scrollbars/ranges alive until Godot lays out the completed replacement rows.
// A generation token prevents a deferred restoration from racing a later rebuild.
internal static class ScrollListRebuild
{
    private const string ScrollName = "SkinChangerRetainedListScroll";
    private const string ContextMeta = "skin_changer_list_context";
    private const string PositionMeta = "skin_changer_list_scroll_position";
    private const string GenerationMeta = "skin_changer_list_scroll_generation";

    internal static ScrollContainer Begin(VBoxContainer content, string contextId)
    {
        var scroll = content.GetNodeOrNull<ScrollContainer>(ScrollName);
        if (scroll == null)
        {
            scroll = new ScrollContainer { Name = ScrollName };
            content.AddChild(scroll);
        }
        var previousContext = scroll.GetMeta(ContextMeta, string.Empty).AsString();
        var position = previousContext.Equals(contextId, StringComparison.OrdinalIgnoreCase)
            ? scroll.ScrollVertical
            : 0;

        foreach (var child in content.GetChildren())
        {
            if (child == scroll)
            {
                continue;
            }
            content.RemoveChild(child);
            child.QueueFree();
        }
        // Native scrollbars are internal children and are excluded by GetChildren().
        foreach (var child in scroll.GetChildren())
        {
            scroll.RemoveChild(child);
            child.QueueFree();
        }

        scroll.SetMeta(PositionMeta, position);
        scroll.SetMeta(GenerationMeta, scroll.GetMeta(GenerationMeta, 0L).AsInt64() + 1L);
        scroll.ScrollHorizontal = 0;
        scroll.SetMeta(ContextMeta, contextId);
        return scroll;
    }

    internal static void PlaceAfterHeader(ScrollContainer scroll)
    {
        var content = scroll.GetParent();
        content.MoveChild(scroll, content.GetChildCount() - 1);
        var generation = scroll.GetMeta(GenerationMeta, 0L).AsInt64();
        var position = scroll.GetMeta(PositionMeta, 0).AsInt32();
        // [延迟调度/Critical] 原触发时机：PlaceAfterHeader 调序后立即入队；最早可执行帧：本帧末 idle；
        // 顺序依赖：必须先执行 QueueSort() 再排第二跳；能否合并：不可按同名跨跳合并（以 GenerationMeta 淘汰过期）；
        // 重试上限：1 次；取消条件：!IsInstanceValid(scroll) 或 GenerationMeta 变化。
        DeferredDispatch.CallDeferred("ScrollListRebuild.QueueSort", scroll, () =>
        {
            if (!GodotObject.IsInstanceValid(scroll) ||
                scroll.GetMeta(GenerationMeta, 0L).AsInt64() != generation)
            {
                return;
            }
            // The first deferred step lets containers recompute their minimum sizes and scrollbar
            // range. Restoring on the following idle step avoids Godot clamping the retained value
            // to zero while the newly-added rows still report an empty range.
            scroll.QueueSort();
            // [延迟调度/Critical] 原触发时机：第一跳 QueueSort() 后嵌套入队；最早可执行帧：第一跳之后的下一 idle 步；
            // 顺序依赖：必须严格晚于第一跳 QueueSort()；能否合并：不可与第一跳合并；
            // 重试上限：1 次；取消条件：!IsInstanceValid(scroll) 或 GenerationMeta 变化。
            DeferredDispatch.CallDeferred("ScrollListRebuild.RestoreScroll", scroll, () =>
            {
                if (GodotObject.IsInstanceValid(scroll) &&
                    scroll.GetMeta(GenerationMeta, 0L).AsInt64() == generation)
                {
                    scroll.ScrollVertical = position;
                }
            });
        });
    }
}
