using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.addons.mega_text;

using STS2SkinChanger.Core.Features;

namespace STS2SkinChanger.Core;

internal static class EventSkinRuntime
{
    internal const string TextureSourceMeta = "sc_event_texture_source";
    internal const string VfxSourceMeta = "sc_event_vfx_source";
    internal const string VfxOriginMeta = "sc_event_vfx_origin";
    internal const string NativeVfxOrigin = "native";
    private const string PortraitMeta = "sc_event_portrait";
    private const string PhobiaPortraitMeta = "sc_event_phobia_portrait";
    private static readonly System.Reflection.FieldInfo EventField = AccessTools.Field(typeof(NEventLayout), "_event");

    public static bool IsManaged(EventModel model) => model is not AncientEventModel &&
        SkinService.Catalog?.Groups.Any(group => group.Id == EventSkinPolicy.GroupId(model.Id.Entry)) == true;

    /// <summary>
    /// True only when the event's resolved selection (whether chosen directly or resolved via
    /// "Follow Category") activates an actual visual skin option rather than vanilla or a
    /// text-only option.
    /// </summary>
    internal static bool HasActiveVisualSelection(string groupId)
    {
        var catalog = SkinService.Catalog;
        if (catalog == null || !EventSkinPolicy.IsEventGroup(groupId))
        {
            return false;
        }

        return catalog.IsEventSelectionVisuallyActive(groupId, SkinService.GetDisplayedSelection(groupId));
    }

    /// <summary>
    /// True when event portrait/scene/VFX loading should go through an isolated runtime pack.
    /// Text-only selections (including via "Follow Category") never ship visual assets and read
    /// the game's original texture/scene directly instead of packing a private vanilla PCK.
    /// </summary>
    internal static bool ShouldUseIsolatedVisuals(string groupId)
    {
        var catalog = SkinService.Catalog;
        if (catalog == null || !EventSkinPolicy.IsEventGroup(groupId))
        {
            return false;
        }

        var selection = SkinService.GetDisplayedSelection(groupId);
        if (catalog.IsEventOptionTextOnly(groupId, selection))
        {
            return false;
        }

        return catalog.HasVisualEventOption(groupId);
    }

    public static void RememberPortrait(NEventLayout layout, Texture2D portrait, Texture2D? phobia)
    {
        string Source(Texture2D texture) => texture.GetMeta(TextureSourceMeta, texture.ResourcePath).AsString();
        var source = Source(portrait);
        if (!EventSkinPolicy.IsEventGroup(SkinService.Catalog?.FindGroupIdForResourcePath(source) ?? "")) return;
        layout.SetMeta(PortraitMeta, source);
        layout.SetMeta(PhobiaPortraitMeta, phobia == null ? "" : Source(phobia));
    }

    // Called only after a successful selection transaction. Do not call SetEvent, BeginEvent,
    // RefreshEventState, AddOptions or OnRoomEnter: those can reset choices, votes and rewards.
    public static void RefreshCurrent(string groupId)
    {
        if (!EventSkinPolicy.IsEventGroup(groupId)) return;
        if (groupId == FakeMerchantEventFrame.GroupId)
        {
            FakeMerchantEventFrame.RefreshCurrent();
            return;
        }
        var layout = NEventRoom.Instance?.Layout;
        if (layout == null || !GodotObject.IsInstanceValid(layout) ||
            EventField.GetValue(layout) is not EventModel model ||
            EventSkinPolicy.GroupId(model.Id.Entry) != groupId) return;
        try
        {
            layout.SetTitle(model.Title.GetFormattedText());
            if (model.Description != null)
                layout.GetNodeOrNull<MegaRichTextLabel>("%EventDescription")?.SetTextAutoSize(model.Description.GetFormattedText());
            foreach (var button in layout.OptionButtons)
            {
                var title = button.Option.Title?.GetFormattedText() ?? "";
                var description = button.Option.Description?.GetFormattedText() ?? "";
                var color = button.Option.IsLocked ? "red" : "gold";
                var text = title.Length == 0 ? description : $"[{color}][b]{title}[/b][/{color}]\n{description}";
                button.GetNodeOrNull<MegaRichTextLabel>("%Text")?.SetTextAutoSize(text);
            }
            var isolate = ShouldUseIsolatedVisuals(groupId);
            var portrait = layout.GetMeta(PortraitMeta, "").AsString();
            if (portrait.Length > 0)
            {
                var phobia = model.HasPhobiaModePortrait
                    ? $"res://images/events/{model.Id.Entry.ToLowerInvariant()}_phobia_mode.png" : "";
                var portraitTexture = isolate
                    ? LoadTexture(groupId, portrait)
                    : PreloadManager.Cache.GetTexture2D(portrait);
                var phobiaTexture = phobia.Length == 0
                    ? null
                    : (isolate
                        ? LoadTexture(groupId, phobia)
                        : PreloadManager.Cache.GetTexture2D(phobia));
                layout.SetPortrait(portraitTexture, phobiaTexture);
            }
            var hadVfx = RefreshVfx(layout, groupId, model.HasVfx, isolate);
            if (!hadVfx && model.HasVfx)
            {
                var vfx = model.CreateVfx();
                layout.AddVfxAnchoredToPortrait(vfx);
                vfx.Position = EventModel.VfxOffset;
            }
        }
        catch (Exception exception)
        {
            // A presentation failure must not propagate into event progression or poison the
            // saved selection transaction. Existing gameplay nodes remain intact.
            ModLog.Error($"刷新事件外观 {model.Id.Entry} 失败：{exception}");
        }
    }

    internal static Texture2D LoadTexture(string groupId, string path)
    {
        var texture = (Texture2D)SkinService.GetOrLoadRuntimeResource(groupId, path);
        texture.SetMeta(TextureSourceMeta, path);
        return texture;
    }

    internal static bool TryLoadInitialPortrait(EventModel model, bool phobia, out Texture2D texture)
    {
        texture = null!;
        if (!IsManaged(model)) return false;
        var group = EventSkinPolicy.GroupId(model.Id.Entry);
        if (!ShouldUseIsolatedVisuals(group)) return false;
        var path = AccessTools.Property(typeof(EventModel), phobia
            ? "InitialPhobiaModePortraitPath" : "InitialPortraitPath")?.GetValue(model) as string;
        // Preserve custom/private portraits supplied by gameplay events and author code.
        if (path == null || SkinService.Catalog!.FindGroupIdForResourcePath(path) != group ||
            !SkinService.Catalog.HasEventResource(group, SkinService.GetDisplayedSelection(group), path)) return false;
        texture = LoadTexture(group, path);
        return true;
    }

    internal static Texture2D ResolvePortraitArgument(Texture2D texture)
    {
        var path = texture.GetMeta(TextureSourceMeta, texture.ResourcePath).AsString();
        var group = SkinService.Catalog?.FindGroupIdForResourcePath(path);
        return group != null && EventSkinPolicy.IsEventGroup(group) && ShouldUseIsolatedVisuals(group)
            ? LoadTexture(group, path)
            : texture;
    }

    internal static string ResolveVfxPath(EventModel model) =>
        $"res://scenes/vfx/events/{model.Id.Entry.ToLowerInvariant()}_vfx.tscn";

    internal static string ResolveVfxOrigin(string groupId, bool isolated) =>
        isolated
            ? "isolated:" + SkinService.GetDisplayedSelection(groupId)
            : NativeVfxOrigin;

    internal static void MarkVfxInstance(EventModel model, Node2D? vfx, bool isolated)
    {
        if (vfx == null || !GodotObject.IsInstanceValid(vfx))
        {
            return;
        }

        var groupId = EventSkinPolicy.GroupId(model.Id.Entry);
        vfx.SetMeta(VfxSourceMeta, ResolveVfxPath(model));
        vfx.SetMeta(VfxOriginMeta, ResolveVfxOrigin(groupId, isolated));
    }

    internal static bool ReconcileTrackedVfxNodes<TNode>(
        IEnumerable<TNode> trackedNodes,
        Func<TNode, string> getSource,
        Func<TNode, string> getOrigin,
        Func<TNode, string, string, bool> replaceWithIsolated,
        Action<TNode> removeNode,
        bool enabled,
        bool isolate,
        string expectedOrigin)
    {
        var found = false;
        foreach (var old in trackedNodes.ToArray())
        {
            if (!enabled)
            {
                removeNode(old);
                continue;
            }

            var currentOrigin = getOrigin(old);
            if (string.Equals(currentOrigin, expectedOrigin, StringComparison.OrdinalIgnoreCase))
            {
                if (!found)
                {
                    found = true;
                }
                else
                {
                    removeNode(old);
                }

                continue;
            }

            if (isolate && !found)
            {
                var path = getSource(old);
                if (replaceWithIsolated(old, path, expectedOrigin))
                {
                    found = true;
                }
            }

            removeNode(old);
        }

        return found;
    }

    private static bool RefreshVfx(Node root, string groupId, bool enabled, bool isolate)
    {
        var tracked = new List<Node2D>();
        CollectTrackedVfxNodes(root, tracked);
        var expectedOrigin = ResolveVfxOrigin(groupId, isolate);
        return ReconcileTrackedVfxNodes(
            tracked,
            node => node.GetMeta(VfxSourceMeta, string.Empty).AsString(),
            node => node.GetMeta(VfxOriginMeta, string.Empty).AsString(),
            (old, path, origin) =>
            {
                var parent = old.GetParent();
                if (parent == null || string.IsNullOrEmpty(path))
                {
                    return false;
                }

                var replacement = SkinService.InstantiateRuntimeScene<Node2D>(groupId, path);
                replacement.SetMeta(VfxSourceMeta, path);
                replacement.SetMeta(VfxOriginMeta, origin);
                replacement.Transform = old.Transform;
                replacement.ZIndex = old.ZIndex;
                parent.AddChild(replacement);
                parent.MoveChild(replacement, old.GetIndex());
                return true;
            },
            old =>
            {
                old.GetParent()?.RemoveChild(old);
                old.QueueFree();
            },
            enabled,
            isolate,
            expectedOrigin);
    }

    private static void CollectTrackedVfxNodes(Node parent, List<Node2D> results)
    {
        foreach (var child in parent.GetChildren())
        {
            if (child is Node2D tracked && child.HasMeta(VfxSourceMeta))
            {
                results.Add(tracked);
            }
            else
            {
                CollectTrackedVfxNodes(child, results);
            }
        }
    }
}

[FeaturePatch(FeatureIds.Event)]
[HarmonyPatch]
internal static class EventSkinResourceAvailabilityPatch
{
    private static IEnumerable<System.Reflection.MethodBase> TargetMethods() =>
    [
        AccessTools.PropertyGetter(typeof(EventModel), nameof(EventModel.HasVfx)),
        AccessTools.PropertyGetter(typeof(EventModel), nameof(EventModel.HasPhobiaModePortrait))
    ];
    private static bool Prefix(EventModel __instance, System.Reflection.MethodBase __originalMethod, ref bool __result)
    {
        if (!EventSkinRuntime.IsManaged(__instance)) return true;
        var id = __instance.Id.Entry.ToLowerInvariant();
        var groupId = EventSkinPolicy.GroupId(id);
        var path = __originalMethod.Name == "get_HasVfx" ? $"res://scenes/vfx/events/{id}_vfx.tscn" :
            $"res://images/events/{id}_phobia_mode.png";
        __result = SkinService.Catalog!.HasEventResource(groupId, SkinService.Config.GetSelection(groupId), path);
        return false;
    }
}

[FeaturePatch(FeatureIds.Event)]
[HarmonyPatch(typeof(LocTable), nameof(LocTable.GetRawText))]
internal static class EventSkinTextPatch
{
    [HarmonyPriority(Priority.Last)]
    private static bool Prefix(string ____name, string key, ref string __result)
    {
        if (____name != "events" || SkinService.Catalog == null) return true;
        if (!SkinService.Catalog.TryResolveEventText(key, ModLocalization.CurrentLanguage,
                SkinService.Config.Selections, out var text)) return true;
        __result = text;
        return false;
    }
}

[FeaturePatch(FeatureIds.Event)]
[HarmonyPatch(typeof(EventModel), nameof(EventModel.CreateVfx))]
internal static class EventSkinVfxPatch
{
    private static bool Prefix(EventModel __instance, ref Node2D __result)
    {
        var groupId = EventSkinPolicy.GroupId(__instance.Id.Entry);
        if (!EventSkinRuntime.IsManaged(__instance) || !EventSkinRuntime.ShouldUseIsolatedVisuals(groupId)) return true;
        var path = EventSkinRuntime.ResolveVfxPath(__instance);
        try
        {
            __result = SkinService.InstantiateRuntimeScene<Node2D>(groupId, path);
            EventSkinRuntime.MarkVfxInstance(__instance, __result, isolated: true);
            return false;
        }
        catch (Exception exception)
        {
            ModLog.Error($"事件特效加载失败 {__instance.Id.Entry}：{exception.Message}");
            return true;
        }
    }

    private static void Postfix(EventModel __instance, Node2D? __result)
    {
        try
        {
            if (__result != null &&
                GodotObject.IsInstanceValid(__result) &&
                EventSkinRuntime.IsManaged(__instance) &&
                !__result.HasMeta(EventSkinRuntime.VfxSourceMeta))
            {
                EventSkinRuntime.MarkVfxInstance(__instance, __result, isolated: false);
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"标记事件原生特效失败 {__instance.Id.Entry}：{exception.GetBaseException().Message}");
        }
    }
}

[FeaturePatch(FeatureIds.Event)]
[HarmonyPatch(typeof(NEventLayout), nameof(NEventLayout.SetPortrait))]
internal static class EventSkinPortraitTrackingPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Prefix(ref Texture2D portrait, ref Texture2D? phobiaModePortrait)
    {
        // Resolve at the final layout boundary too, including later event pages, without
        // restarting gameplay when AssetCache still holds another skin's canonical texture.
        try
        {
            if (portrait != null) portrait = EventSkinRuntime.ResolvePortraitArgument(portrait);
            if (phobiaModePortrait != null) phobiaModePortrait = EventSkinRuntime.ResolvePortraitArgument(phobiaModePortrait);
        }
        catch (Exception error) { ModLog.Warn("事件图片未能刷新，保留当前展示：" + error.GetBaseException().Message); }
    }
    private static void Postfix(NEventLayout __instance, Texture2D portrait, Texture2D? phobiaModePortrait) =>
        EventSkinRuntime.RememberPortrait(__instance, portrait, phobiaModePortrait);
}

[FeaturePatch(FeatureIds.Event)]
[HarmonyPatch]
internal static class EventSkinInitialPortraitPatch
{
    private static IEnumerable<System.Reflection.MethodBase> TargetMethods() =>
    [
        AccessTools.Method(typeof(EventModel), nameof(EventModel.CreateInitialPortrait)),
        AccessTools.Method(typeof(EventModel), nameof(EventModel.CreateInitialPhobiaModePortrait))
    ];
    private static bool Prefix(EventModel __instance, System.Reflection.MethodBase __originalMethod, ref Texture2D __result)
    {
        try
        {
            if (!EventSkinRuntime.TryLoadInitialPortrait(__instance,
                    __originalMethod.Name == nameof(EventModel.CreateInitialPhobiaModePortrait), out var portrait)) return true;
            __result = portrait;
            return false;
        }
        catch (Exception error)
        {
            ModLog.Warn("事件初始图片未能隔离加载：" + error.GetBaseException().Message);
            return true;
        }
    }
}
