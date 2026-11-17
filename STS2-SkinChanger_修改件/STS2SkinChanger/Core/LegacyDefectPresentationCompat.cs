using System.Reflection;
using Godot;

namespace STS2SkinChanger.Core;

// This legacy AnimatedSprite pack calls PatchAll before Preload. A removed game argument
// aborts PatchAll, leaving its otherwise valid scene fields null for every room callback.
internal static class LegacyDefectPresentationCompat
{
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    internal static MethodInfo? BackgroundCallback(Assembly assembly) =>
        assembly.GetName().Name == "DefectSkinMod"
            ? assembly.GetType("DefectSkinMod.Scripts.NCharacterSelectScreen_SelectCharacter_Patch")?.GetMethod("Postfix", Static)
            : null;

    internal static void ApplyModeBackground(Control screen, string groupId)
    {
        if (groupId != "defect" || screen is MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen ||
            SkinService.GetSelectedCreatureRuntimeProvider(groupId) is not { } provider ||
            ManagedSkinModLoader.GetActiveProviderAssembly(provider) is not { } assembly ||
            BackgroundCallback(assembly) == null ||
            STS2SkinChanger.Ui.CharacterSelectScreenAccess.Background(screen) is not { } container) return;
        EnsureScenes(assembly);
        if (BackgroundScene(assembly) is not { } scene)
            throw new AppearanceResourceUnavailableException("鸡煲选角背景尚未就绪。");
        if (container.GetNodeOrNull("DefectAnimatedBg") != null) return;
        var background = scene.Instantiate<Control>(PackedScene.GenEditState.Disabled);
        background.Name = "DefectAnimatedBg";
        background.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        background.MouseFilter = Control.MouseFilterEnum.Ignore;
        foreach (var child in container.GetChildren()) { container.RemoveChild(child); child.QueueFree(); }
        container.AddChild(background);
    }

    internal static PackedScene? BackgroundScene(Assembly assembly) => BackgroundCallback(assembly) == null ? null :
        assembly.GetType("DefectSkinMod.Scripts.DefectSkinGlobals")?.GetField("SelectBgScene", Static)?.GetValue(null) as PackedScene;

    internal static PackedScene? PreviewScene(Assembly assembly) => BackgroundCallback(assembly) == null ? null :
        assembly.GetType("DefectSkinMod.Scripts.DefectSkinGlobals")?.GetField("CharacterScene", Static)?.GetValue(null) as PackedScene;

    internal static void ApplyPreviewBody(MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals visuals, string? groupId)
    {
        if (groupId != "defect" || SkinService.GetSelectedCreatureRuntimeProvider(groupId) is not { } providerId ||
            ManagedSkinModLoader.GetActiveProviderAssembly(providerId) is not { } assembly ||
            PreviewScene(assembly) is not { } scene) return;
        // Reuse the author's actor/scale/idle without its combat Ready callback, global active
        // sprite collection, direction timer or gameplay lifecycle. The preview owns this node.
        var actor = scene.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
        actor.Name = "SkinChangerDefectPreviewActor";
        actor.Position = Vector2.Zero;
        actor.Scale = new Vector2(2.5f, 2.5f);
        visuals.AddChild(actor);
        PlayIdle(actor);
        visuals.GetNodeOrNull<Node2D>("%Visuals")?.Hide();
    }

    private static void PlayIdle(Node node)
    {
        if (node is AnimatedSprite2D sprite && sprite.SpriteFrames?.HasAnimation("Idle") == true)
            sprite.Play("Idle");
        foreach (var child in node.GetChildren()) PlayIdle(child);
    }

    internal static void EnsureScenes(Assembly assembly)
    {
        if (BackgroundCallback(assembly) == null) return;
        var globals = assembly.GetType("DefectSkinMod.Scripts.DefectSkinGlobals");
        if (globals?.GetField("CharacterScene", Static)?.GetValue(null) is PackedScene &&
            globals.GetField("SelectBgScene", Static)?.GetValue(null) is PackedScene) return;
        globals?.GetMethod("Preload", Static, null, Type.EmptyTypes, null)?.Invoke(null, null);
    }
}
