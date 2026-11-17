using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class OtherCreaturePreview
{
    internal static bool HasModel(string name) => typeof(MonsterModel).Assembly.GetType(name) != null;

    internal static Node2D CreateOsty()
    {
        // Match character select: author display factory, drawable scene, then selected skeleton.
        if (!string.IsNullOrEmpty(SkinService.Config.CompanionImpostorSkinId) &&
            OstyImpostorBridge.CreateSelectDisplay() is { } display) return display;
        var group = OstySkinChoices.GroupId;
        if (OstyModelPreviewControls.TryLoadSceneForCurrentSelection(group, out var scene, out _))
        {
            try
            {
                var root = scene!.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
                if (OstyModelPreviewPanel.HasDrawableNode(root)) return root;
                root.Free();
            }
            catch (Exception error)
            {
                ModLog.Warn("奥斯提图鉴场景未就绪，改用选人骨架：" + error.GetBaseException().Message);
            }
        }
        var data = OstyModelPreviewControls.LoadSkeletonForCurrentSelection(group, out var path)
            ?? throw new InvalidOperationException("奥斯提预览骨架未加载：" + path);
        using var created = ClassDB.Instantiate("SpineSprite");
        var spine = created.AsGodotObject() as Node2D
            ?? throw new InvalidOperationException("无法创建奥斯提 SpineSprite。");
        spine.Set("skeleton_data_res", data);
        spine.Scale = Vector2.One * .26f;
        return spine;
    }

    internal static void Prepare(OtherCreatureDefinition creature, Node root)
    {
        if (creature.Id == "osty")
        {
            if (root.GetClass() == "SpineSprite")
            {
                if (root.HasMethod("set_time_scale")) { using var speed = root.Call("set_time_scale", 1.0); }
                if (root.HasMethod("set_update_mode")) { using var mode = root.Call("set_update_mode", 0); }
                if (root.HasMethod("update_skeleton")) { using var update = root.Call("update_skeleton", 0f); }
                var sprite = new MegaSprite(root);
                root.RunWhenSpineReady(sprite, _ =>
                {
                    if (!GodotObject.IsInstanceValid(root) || root is not Node2D node) return;
                    using var skeleton = root.Call("get_skeleton");
                    if (skeleton.AsGodotObject() is not { } value) return;
                    using var bounds = value.Call("get_bounds");
                    Fit(node, bounds.AsRect2());
                });
            }
            else if (root is Node2D node)
                Fit(node, OstyModelPreviewPanel.MeasureSceneBounds(node));
        }
        if (creature.Id != "byrdine_monster") return;
        // The native canonical model combines hair and eyes without a live pet/player.
        var type = typeof(MonsterModel).Assembly.GetType(creature.RequiredModelType!);
        if (type == null) return;
        var model = ModelDb.GetByIdOrNull<MonsterModel>(ModelDb.GetId(type));
        if (model == null) return;
        var bird = new MegaSprite(root);
        root.RunWhenSpineReady(bird, _ =>
        {
            var skeleton = bird.GetSkeleton();
            if (skeleton?.GetData()?.FindSkin("eye/ironclad") != null) model.SetupSkins(bird, skeleton);
        });
    }

    internal static IReadOnlyList<string> IdleAliases(OtherCreatureDefinition creature) =>
        creature.Id == "osty" && !string.IsNullOrEmpty(SkinService.Config.CompanionImpostorSkinId)
            ? new[] { OstyImpostorBridge.SelectDisplayAnimation }.Concat(creature.IdleAliases).ToArray()
            : creature.IdleAliases;

    private static void Fit(Node2D node, Rect2 bounds)
    {
        if (bounds.Size.X <= 1 || bounds.Size.Y <= 1) return;
        var scale = Math.Min(320f / bounds.Size.X, 320f / bounds.Size.Y);
        node.Scale = Vector2.One * scale;
        node.Position = new Vector2(960, 600) - bounds.GetCenter() * scale;
    }
}
