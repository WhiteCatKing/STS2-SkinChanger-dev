using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Models.Events;
using STS2SkinChanger.Ui;

using STS2SkinChanger.Core.Features;

namespace STS2SkinChanger.Core;

// ============================================================================================
// 假商人“事件”构图补丁（只为重绘假商人帐篷的事件外观包服务，与背景切换功能互相独立）
//
// 作用：把假商人事件画面里的动画帐篷按“战斗背景”的规则重新摆放（等比铺满事件画面、居中、
//       超出裁剪），再整体右移 HorizontalShift 像素，用来把原画右侧那条偏白的空余推出屏幕。
//
// 位置：
//   本文件 = 全部实现（约 150 行），游戏侧只有 1 个 Harmony 补丁：NFakeMerchant._Ready 的 Postfix。
//
// 以后不需要时的删除方式：
//   1. 删掉本文件 `STS2SkinChanger/Core/FakeMerchantEventFrame.cs` 即可 —— Entry 的补丁安装器
//      按 [HarmonyPatch] 类逐个扫描，文件删掉后补丁不会再生效，事件恢复游戏原构图。
//   2. 顺手删掉文档里“假商人事件构图补丁”一节的说明
//      （docs/issues/ISSUE_2026-11-07_fake_merchant_background.md）。
//   没有任何存档、配置或外部文件依赖；事件外观包本身不受影响。
//
// 实测数据（1920×1080 画布）：骨架范围 = (-1151.28,-358.47) + (4866,2880)，骨架 scale 0.5，
// 铺满缩放 0.789，原始居中原点 (454.27,113.26)；右移 41px 后白边刚好出屏（作者实机确认）。
// ============================================================================================

/// <summary>
/// Re-frames the fake merchant event stage so the redrawn tent fills the screen like the battle
/// background does. See the block comment above for the removal instructions.
/// </summary>
internal static class FakeMerchantEventFrame
{
    private const string SpritePath = "SceneContainer/BgContainer/FakeMerchantBackground";
    private const string ContainerPath = "SceneContainer/BgContainer";
    private const string CutterPath = "SceneContainer/SpineSprite2";

    /// <summary>
    /// Canvas pixels to move the whole background (and its decorations) to the right after the
    /// cover fit. The redrawn tent's art stops short of the skeleton bounds on the right, which
    /// left a pale strip on screen; 41 px pushes exactly that strip off the screen edge.
    /// </summary>
    private const float HorizontalShift = 41f;

    private const int MaxWaitFrames = 300;
    private const float MinArtSize = 8f;

    internal const string GroupId = EventSkinPolicy.Prefix + "fake_merchant_sale_background";
    internal const string WorkshopProviderId = "NSFW-Event";

    internal static string SelectedProvider =>
        SkinService.Config.Selections.TryGetValue(GroupId, out var selection)
            ? selection
            : SkinService.Catalog?.Groups.Any(group => group.Id == GroupId &&
                group.Options.Any(option => option.Id == WorkshopProviderId)) == true
                ? WorkshopProviderId
                : STS2SkinChanger.Catalog.SkinCatalog.BaseOptionId;
    private const string ScenePath = "res://scenes/events/custom/fake_merchant.tscn";
    private static WeakReference<NFakeMerchant>? _current;
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<NFakeMerchant, FakeMerchant> Models = new();
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<CanvasItem, Layout> Layouts = new();
    private sealed record Layout(Transform2D Transform, bool Visible);

    internal static void RefreshPreview(NFakeMerchant screen) => Apply(screen, null);

    internal static void RefreshCurrent()
    {
        if (_current?.TryGetTarget(out var screen) == true && GodotObject.IsInstanceValid(screen) &&
            screen.IsInsideTree() && Models.TryGetValue(screen, out var model) && !model.StartedFight)
            Apply(screen, model);
    }

    public static void Schedule(NFakeMerchant screen, FakeMerchant? model)
    {
        if (model is not { StartedFight: false } || MerchantRuntimeAppearance.IsMerchantPreviewRoot(screen)) return;
        _current = new WeakReference<NFakeMerchant>(screen);
        Models.GetValue(screen, _ => model);
        Callable.From(() =>
        {
            if (GodotObject.IsInstanceValid(screen) && screen.IsInsideTree() && !model.StartedFight)
                Apply(screen, model);
        }).CallDeferred();
    }

    private static void Apply(NFakeMerchant screen, FakeMerchant? model)
    {
        try
        {
            var provider = SelectedProvider;
            var art = SkinService.Catalog?.HasFakeMerchantEventImage(provider) == true
                ? SkinService.GetOrLoadRuntimeResource(GroupId, FakeMerchantEventArtPolicy.EventImage) as Texture2D
                : null;
            var original = screen.GetNodeOrNull<Control>(ContainerPath);
            var cutter = screen.GetNodeOrNull<Node2D>(CutterPath);
            foreach (var item in new CanvasItem?[] { original, cutter })
            {
                if (item == null) continue;
                var layout = Layouts.GetValue(item, node => new Layout(node.GetTransform(), node.Visible));
                ApplyGlobalTransform(item, ((item.GetParent() as CanvasItem)?.GetGlobalTransform() ??
                    Transform2D.Identity) * layout.Transform);
                item.Visible = art == null && layout.Visible;
            }

            if (screen.GetNodeOrNull<TextureRect>("SkinChangerFakeMerchantEventImage") is { } previous)
            {
                screen.RemoveChild(previous);
                previous.QueueFree();
            }
            if (art != null)
            {
                var backdrop = new TextureRect
                {
                    Name = "SkinChangerFakeMerchantEventImage",
                    Texture = art,
                    ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                    StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };
                screen.AddChild(backdrop);
                screen.MoveChild(backdrop, 0);
                backdrop.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
                return;
            }

            var template = SkinService.InstantiateMerchantVariantScene(GroupId, ScenePath, true, null);
            try
            {
                foreach (var path in new[] { SpritePath, CutterPath })
                {
                    if (screen.GetNodeOrNull<Node2D>(path) is not { } sprite ||
                        template.GetNodeOrNull<Node2D>(path) is not { } baseline) continue;
                    using var value = baseline.Get("skeleton_data_res");
                    if (value.AsGodotObject() is not Resource resource) continue;
                    var index = resource.ResourcePath.IndexOf("animations/", StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        var canonical = "res://" + resource.ResourcePath[index..];
                        if (provider == WorkshopProviderId)
                        {
                            // Workshop 3765802910 replaces the atlas pages only. Load the
                            // native skeleton with those pages in the provider's isolated pack.
                            resource = SkinService.WithBackgroundEventResource(provider, canonical, selected => selected);
                        }
                    }
                    sprite.Set("skeleton_data_res", resource);
                }
            }
            finally { template.Free(); }
            if (provider == WorkshopProviderId) _ = FitAsync(screen, model, provider);
        }
        catch (Exception exception)
        {
            ModLog.Warn("刷新假商人事件背景失败：" + exception.GetBaseException().Message);
        }
    }

    private static async Task FitAsync(NFakeMerchant screen, FakeMerchant? model, string provider)
    {
        try
        {
            var tree = screen.GetTree();
            if (tree == null)
            {
                return;
            }

            var sprite = screen.GetNodeOrNull<Node2D>(SpritePath);
            var container = screen.GetNodeOrNull<Control>(ContainerPath);
            var cutter = screen.GetNodeOrNull<Node2D>(CutterPath);
            if (sprite == null || container == null)
            {
                ModLog.Warn($"假商人事件场景缺少 {SpritePath}，跳过构图调整。");
                return;
            }

            // The SpineSprite builds its skeleton asynchronously after _Ready, so the bounds are
            // only meaningful once an animation pose has been applied.
            var bounds = default(Rect2);
            var ready = false;
            for (var frame = 0; frame < MaxWaitFrames; frame++)
            {
                if (!GodotObject.IsInstanceValid(screen) || !GodotObject.IsInstanceValid(sprite) || model?.StartedFight == true || SelectedProvider != provider)
                {
                    return;
                }

                var candidate = ReadBounds(sprite);
                if (candidate.Size.X > MinArtSize && candidate.Size.Y > MinArtSize)
                {
                    bounds = candidate;
                    ready = true;
                    break;
                }

                await screen.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
            }

            if (!ready)
            {
                ModLog.Warn($"等不到假商人帐篷的骨架范围（{MaxWaitFrames} 帧），保持原构图。");
                return;
            }

            if (model?.StartedFight == true || SelectedProvider != provider)
                return;

            // Fit to the screen's own rect instead of the viewport: the same scene is also
            // instantiated as a small compendium preview, and that one must fill its own box.
            var screenScale = screen.GetGlobalTransform().Scale;
            var target = new Rect2(screen.GetGlobalPosition(), screen.Size * screenScale);
            if (target.Size.X < 64f || target.Size.Y < 64f)
            {
                ModLog.Warn($"假商人事件画面尺寸异常（{target.Size}），保持原构图。");
                return;
            }

            var spriteScale = sprite.Scale;
            if (spriteScale.X <= 0.0001f || spriteScale.Y <= 0.0001f)
            {
                spriteScale = Vector2.One;
            }

            var localPosition = sprite.Position + bounds.Position * spriteScale;
            var localSize = bounds.Size * spriteScale;
            if (localSize.X < MinArtSize || localSize.Y < MinArtSize ||
                localSize.X > 100000f || localSize.Y > 100000f)
            {
                ModLog.Warn($"假商人帐篷范围异常（{localSize}），保持原构图。");
                return;
            }

            // Cover fit + horizontal shift; vertical framing stays untouched.
            var scale = Mathf.Max(target.Size.X / localSize.X, target.Size.Y / localSize.Y);
            var localCenter = localPosition + localSize * 0.5f;
            var origin = target.Position + target.Size * 0.5f - localCenter * scale +
                         new Vector2(HorizontalShift, 0f);

            // The tent, its glitter/fire/speck decorations all live in this container, so moving
            // and scaling the container keeps the composition intact.
            ApplyGlobalTransform(
                container,
                new Transform2D(0f, new Vector2(scale, scale), 0f, origin));

            if (cutter != null && GodotObject.IsInstanceValid(cutter))
            {
                // The front "cutter" layer sits outside the container but shares the tent's
                // skeleton space, so it gets the tent's own skeleton-space mapping.
                var cutterScale = new Vector2(
                    scale * spriteScale.X,
                    scale * spriteScale.Y);
                ApplyGlobalTransform(cutter, new Transform2D(0f, cutterScale, 0f, origin));
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn(
                "假商人事件构图调整失败，保持游戏原构图：" +
                exception.GetBaseException().Message);
        }
    }

    private static Rect2 ReadBounds(Node2D sprite)
    {
        try
        {
            var skeleton = new MegaSprite(sprite).GetSkeleton();
            return skeleton?.GetBounds() ?? default;
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// CanvasItem's global transform is read-only in Godot 4, so convert the wanted global mapping
    /// into the node's own local transform and set that.
    /// </summary>
    private static void ApplyGlobalTransform(CanvasItem item, Transform2D global)
    {
        var parentGlobal = (item.GetParent() as CanvasItem)?.GetGlobalTransform() ?? Transform2D.Identity;
        var local = parentGlobal.AffineInverse() * global;
        switch (item)
        {
            case Control control:
                control.Scale = local.Scale;
                control.Position = local.Origin;
                break;
            case Node2D node:
                node.Transform = local;
                break;
        }
    }
}

[FeaturePatch(FeatureIds.Merchant)]
[HarmonyPatch(typeof(NFakeMerchant), "_Ready")]
internal static class FakeMerchantEventFramePatch
{
    private static void Postfix(NFakeMerchant __instance, FakeMerchant? ____event) =>
        FakeMerchantEventFrame.Schedule(__instance, ____event);
}
