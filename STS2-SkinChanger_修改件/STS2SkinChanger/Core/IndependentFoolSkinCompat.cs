using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

internal static class IndependentFoolSkinCompat
{
    private static readonly HashSet<Assembly> Prepared = [];
    internal static void Prepare(Assembly assembly)
    {
        if (assembly.GetName().Name != "NailongFool" || !Prepared.Add(assembly)) return;
        var mount = assembly.GetType("NailongFool.NailongFoolMod")?.GetMethod("MountAssets", BindingFlags.Static | BindingFlags.NonPublic);
        if (mount != null) new Harmony(Entry.ModId + ".nailong-pack").Patch(mount,
            prefix: new HarmonyMethod(typeof(IndependentFoolSkinCompat), nameof(SkipAuthorMount)));
    }
    private static bool SkipAuthorMount() => false; // SC mounts the selected .assets via its ordinary isolated pack path.

    internal static MethodInfo? SelectionBackgroundContract(Assembly assembly) =>
        assembly.GetName().Name == "FoolSkinStatic" ? AccessTools.Method(
            assembly.GetType("FoolSkinStatic.Patches"), "BuildPortrait", [typeof(Node)]) : null;

    internal static void ApplySelectionBackground(Control screen, string groupId)
    {
        if (screen is MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen ||
            groupId != "fool" || SkinService.GetSelectedCreatureRuntimeProvider(groupId) is not { } provider ||
            ManagedSkinModLoader.GetActiveProviderAssembly(provider) is not { } assembly ||
            SelectionBackgroundContract(assembly) is not { } build ||
            STS2SkinChanger.Ui.CharacterSelectScreenAccess.Background(screen) is not { } container) return;
        foreach (var background in container.GetChildren().OfType<Control>().Where(c => c.Visible))
        {
            // Preserve the author's portrait animation and native mode viewport/material.
            if (BuildSelectionBackground(assembly, background))
                background.GetNodeOrNull<Node2D>("SpineSprite")?.Hide();
        }
    }

    internal static bool BuildSelectionBackground(Assembly assembly, Node background)
    {
        if (SelectionBackgroundContract(assembly) is not { } build) return false;
        var patches = build.DeclaringType!;
        AccessTools.Method(patches, "EnsurePortrait").Invoke(null, null);
        if (AccessTools.Field(patches, "_portrait").GetValue(null) is not Texture2D) return false;
        build.Invoke(null, [background]);
        return true;
    }

    internal static void ApplyPreview(NCreatureVisuals visuals, string? groupId)
    {
        if (groupId != "fool" || SkinService.GetSelectedCreatureRuntimeProvider(groupId) is not { } provider ||
            ManagedSkinModLoader.GetActiveProviderAssembly(provider) is not { } assembly ||
            assembly.GetName().Name != "FoolSkinStatic") return;
        var patches = assembly.GetType("FoolSkinStatic.Patches", true)!;
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        patches.GetMethod("EnsureCharacter", flags)!.Invoke(null, null);
        var open = patches.GetField("_texture", flags)?.GetValue(null) as Texture2D;
        var closed = patches.GetField("_closed", flags)?.GetValue(null) as Texture2D;
        if (open == null) return;
        var sprite = patches.GetMethod("MakeSprite", flags)!.Invoke(null, [visuals, open, closed]) as Sprite2D;
        if (sprite == null) return;
        patches.GetMethod("Play", flags)!.Invoke(null, [sprite, "idle"]);
        patches.GetMethod("BlinkLoop", flags, null, [typeof(Sprite2D)], null)!.Invoke(null, [sprite]);
        // Stable exposes GetCurrentBody(); beta adds Body. Resolve the native branch contract.
        GameBranchApi.CurrentBody(visuals)?.Hide();
    }
}
