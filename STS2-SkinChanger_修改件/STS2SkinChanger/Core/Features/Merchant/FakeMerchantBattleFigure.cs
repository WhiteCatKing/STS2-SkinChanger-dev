using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core.Features;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

// Carry the actual rendered shop figure across the transition. The duplicate has no room
// script or signals and does not take over any of the combat creature's gameplay nodes.
internal static class FakeMerchantBattleFigure
{
    private static Node2D? _shopFigure;
    private static string? _selection;
    private static string? _spineSkin;

    internal static void Capture(NFakeMerchant __instance)
    {
        _shopFigure?.Free();
        _shopFigure = null;
        _spineSkin = null;
        _selection = SkinService.GetDisplayedSelection(MerchantRuntimeAppearance.FakeMerchantGroupId);
        if (_selection == SkinCatalog.BaseOptionId || __instance.MerchantButton is not { } button) return;

        var figure = FindShopFigure(button);
        if (figure == null || (figure.GetClass() == "SpineSprite" &&
            HasNativeCombatClips(new MegaSprite(figure)))) return;

        _shopFigure = CopyArtwork(figure);
        _spineSkin = ReadSkin(figure);
    }

    internal static void Apply(NCreature creature)
    {
        if (creature.Entity.Monster is not FakeMerchantMonster || _shopFigure is not { } figure) return;
        _shopFigure = null;
        if (SkinService.GetDisplayedSelection(MerchantRuntimeAppearance.FakeMerchantGroupId) != _selection ||
            !NeedsShopFigure(creature))
        {
            figure.Free();
            return;
        }

        Attach(creature, figure, _spineSkin);
    }

    internal static void RefreshAfterSelection(NCreature creature)
    {
        if (creature.Entity.Monster is FakeMerchantMonster && NeedsShopFigure(creature))
            _ = RebuildFromShopPreviewAsync(creature, creature.Visuals);
    }

    private static bool NeedsShopFigure(NCreature creature)
    {
        const string group = MerchantRuntimeAppearance.FakeMerchantGroupId;
        var selection = SkinService.GetDisplayedSelection(group);
        return selection != SkinCatalog.BaseOptionId &&
            !(creature.Visuals.SpineBody is { } spine && HasNativeCombatClips(spine) &&
              SkinService.SelectedSkinProvidesResource(group,
                  "res://animations/backgrounds/fake_merchant_room/top/fake_merchant_top.tres")) &&
            !SkinService.SelectedSkinProvidesResource(group,
                ContextualSkinControls.CanonicalScenePath("creature_visuals/fake_merchant_monster")) &&
            !Descendants(creature.Visuals).Any(node => node.HasMeta("NpcFeminization2Visual") &&
                node is CanvasItem { Visible: true });
    }

    private static async Task RebuildFromShopPreviewAsync(NCreature creature, NCreatureVisuals visuals)
    {
        // The shop skin may apply its skeleton from a room callback on a later frame. Let it
        // finish in a hidden native preview, then copy only the rendered figure into combat.
        var selection = SkinService.GetDisplayedSelection(MerchantRuntimeAppearance.FakeMerchantGroupId);
        Control? host = null;
        var body = visuals.Body;
        var wasVisible = body.Visible;
        var installed = false;
        body.Hide();
        try
        {
            var preview = MerchantRuntimeAppearance.InstantiateMerchantPreviewScene(
                MerchantRuntimeAppearance.FakeMerchantGroupId).Scene;
            host = new Control { Visible = false, Size = new Vector2(1920, 1080) };
            var tree = creature.GetTree();
            creature.AddChild(host);
            host.AddChild(preview);
            var button = ((NFakeMerchant)preview).MerchantButton;
            var native = button.GetNodeOrNull<Node2D>("%MerchantVisual");
            for (var frame = 0; frame < 30; frame++)
            {
                await host.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                if (!GodotObject.IsInstanceValid(creature) || !creature.IsInsideTree() ||
                    creature.Visuals != visuals || selection != SkinService.GetDisplayedSelection(
                        MerchantRuntimeAppearance.FakeMerchantGroupId) || !NeedsShopFigure(creature)) return;

                var figure = FindShopFigure(button);
                if (figure == null) continue;
                if (figure.GetClass() == "SpineSprite" && HasNativeCombatClips(new MegaSprite(figure)))
                    return;
                if (ReferenceEquals(native, figure) && frame < 29 &&
                    SkinService.GetSelectedRuntimeProvider(MerchantRuntimeAppearance.FakeMerchantGroupId) != null)
                    continue;
                var result = CopyArtwork(figure);
                if (result == null) continue;
                Attach(creature, result, ReadSkin(figure));
                installed = true;
                return;
            }
        }
        catch (Exception error)
        {
            ModLog.Warn("假商人战斗外观预览重建失败：" + error.GetBaseException().Message);
        }
        finally
        {
            if (GodotObject.IsInstanceValid(host)) host.QueueFree();
            if (!installed && GodotObject.IsInstanceValid(body)) body.Visible = wasVisible;
        }
    }

    private static bool HasNativeCombatClips(MegaSprite spine) =>
        spine.HasAnimation("combat_idle_loop") && spine.HasAnimation("attack") &&
        spine.HasAnimation("hurt") && spine.HasAnimation("die");

    private static Node2D? FindShopFigure(Node button) =>
        Descendants(button).OfType<Node2D>().FirstOrDefault(node =>
            node.Visible && node.GetClass() == "SpineSprite" && node.HasMeta("NpcFeminization2Visual")) ??
        button.GetNodeOrNull<Node2D>("%MerchantVisual");

    private static Node2D? CopyArtwork(Node2D figure)
    {
        if (figure.GetClass() == "SpineSprite")
        {
            using var data = figure.Get("skeleton_data_res");
            if (data.AsGodotObject() is not Resource skeletonData) return null;
            // Neow's shop Spine has an attached viewport and presentation controls.
            // Copy the skeleton resource, not those children or their viewport textures.
            using var created = ClassDB.Instantiate("SpineSprite");
            var spine = (Node2D)created.AsGodotObject();
            spine.Set("skeleton_data_res", skeletonData);
            return spine;
        }

        return figure is Sprite2D { Texture: { } texture } image
            ? new Sprite2D { Texture = texture, Centered = image.Centered, Offset = image.Offset }
            : null;
    }

    private static string? ReadSkin(Node2D figure)
    {
        if (figure.GetClass() != "SpineSprite" || new MegaSprite(figure).GetSkeleton() is not { } skeleton)
            return null;

        using var skin = skeleton.BoundObject.Call("get_skin");
        using var name = skin.AsGodotObject()?.Call("get_name") ?? default;
        return name.AsString();
    }

    private static void Attach(NCreature creature, Node2D figure, string? skinName)
    {
        var body = creature.Visuals.Body;
        figure.Hide();
        body.Hide();
        creature.Visuals.AddChild(figure);
        var nativeBounds = creature.Visuals.SpineBody?.GetSkeleton()?.GetBounds();
        var targetHeight = nativeBounds is { Size.Y: > 0 } bounds
            ? bounds.Size.Y * Math.Abs(body.Scale.Y)
            : creature.Visuals.Bounds.Size.Y;
        if (figure.GetClass() == "SpineSprite")
        {
            var spine = new MegaSprite(figure);
            figure.RunWhenSpineReady(spine, state =>
            {
                var skeleton = spine.GetSkeleton();
                if (skeleton == null) return;
                if (!string.IsNullOrEmpty(skinName))
                {
                    skeleton.SetSkinByName(skinName);
                    skeleton.SetSlotsToSetupPose();
                }
                var idle = new[] { "idle_loop", "idle", "relaxed_loop" }.FirstOrDefault(spine.HasAnimation);
                if (idle != null) state.SetAnimation(idle, loop: true);
                FitAndFloat(figure, body, skeleton.GetBounds(), targetHeight);
            });
        }
        else if (figure is Sprite2D image)
        {
            FitAndFloat(figure, body, image.GetRect(), targetHeight);
        }

        ModLog.Info($"假商人战斗沿用售货骨骼：{SkinService.GetDisplayedSelection(MerchantRuntimeAppearance.FakeMerchantGroupId)}。");
    }

    private static void FitAndFloat(Node2D figure, Node2D body, Rect2 bounds, float targetHeight)
    {
        if (!GodotObject.IsInstanceValid(body) || bounds.Size.Y <= 0 || targetHeight <= 0)
        {
            if (GodotObject.IsInstanceValid(body)) body.Show();
            figure.QueueFree();
            return;
        }

        var scale = targetHeight / bounds.Size.Y;
        figure.Scale = new Vector2(scale, scale);
        figure.Position = body.Position - new Vector2(bounds.GetCenter().X, bounds.End.Y) * scale;
        figure.Show();
        var tween = figure.CreateTween().SetLoops().SetTrans(Tween.TransitionType.Sine);
        var position = figure.Position;
        tween.TweenProperty(figure, "position", position + Vector2.Up * 16f, 1.1);
        tween.TweenProperty(figure, "position", position, 1.1);
    }

    private static IEnumerable<Node> Descendants(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }
}


[FeaturePatch(FeatureIds.Merchant)]
[HarmonyPatch(typeof(NFakeMerchant), nameof(NFakeMerchant.FoulPotionThrown))]
internal static class FakeMerchantBattleFigureCapturePatch
{
    private static void Prefix(NFakeMerchant __instance) => FakeMerchantBattleFigure.Capture(__instance);
}

[FeaturePatch(FeatureIds.Merchant)]
[HarmonyPatch(typeof(NCreature), nameof(NCreature._Ready))]
internal static class FakeMerchantBattleFigureReadyPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NCreature __instance) => FakeMerchantBattleFigure.Apply(__instance);
}
