using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Events;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core.Features;

namespace STS2SkinChanger.Core;

internal static class VakuuVanillaArt
{
    internal const string EasterEggId = "__vakuu_default_easter_egg__";
    internal const string NormalPath = "res://images/ancients/vakuu_placeholder.png";
    internal const string EasterEggPath = "res://images/ancients/vakuu_angry.png";

    internal static string? PathFor(string group, string selection) =>
        !group.Equals("vakuu", StringComparison.OrdinalIgnoreCase) ? null :
        selection == SkinCatalog.BaseOptionId ? NormalPath : selection == EasterEggId ? EasterEggPath : null;

    internal static void Apply(Control root, string selection)
    {
        if (PathFor("vakuu", selection) is not { } path) return;
        if (root.GetNodeOrNull<TextureRect>("TextureRect") is not { } image)
            throw new InvalidOperationException("瓦库官方背景缺少 TextureRect。");
        image.Texture = SkinService.LoadVanillaVakuuArt(path);
    }
}

internal static partial class SkinService
{
    internal static Texture2D LoadVanillaVakuuArt(string path)
    {
        lock (Sync)
            return LoadRuntimeResourcesForSelection("vakuu", SkinCatalog.BaseOptionId, [path], false)[path] as Texture2D
                ?? throw new InvalidOperationException("瓦库官方图片不是贴图：" + path);
    }
}

// Preserve the native callback and its progression bookkeeping. Explicit vanilla choices
// pin their selected art afterwards; inherit-category keeps the game's native decision.
[FeaturePatch(FeatureIds.Ancient)]
[HarmonyPatch(typeof(Vakuu), nameof(Vakuu.ConfigureBackgroundScene))]
internal static class VakuuVanillaArtPatch
{
    private static void Postfix(Control backgroundScene)
    {
        try
        {
            var selected = SkinService.GetDisplayedSelection("vakuu");
            if (SkinService.GetAncientOverrideSelection("vakuu") != SkinService.InheritAncientSelectionId &&
                VakuuVanillaArt.PathFor("vakuu", selected) != null)
                VakuuVanillaArt.Apply(backgroundScene, selected);
        }
        catch (Exception error) { ModLog.Warn("瓦库官方背景选择应用失败：" + error.GetBaseException().Message); }
    }
}
