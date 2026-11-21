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
[HarmonyPatch]
internal static class VakuuVanillaArtPatch
{
    // The normal/egg callback was introduced on beta; stable has neither the callback nor
    // the egg option. Keep one binary loadable and leave stable's native behavior intact.
    // Prepare 必须存在：TargetMethods 一个都不产出时 Harmony 会抛
    // "Undefined target method for patch method"，让整个 ancient 模块回滚
    // （1.0.37.3 在正式版 v0.111.1 上就是这样丢掉了图鉴右上角的「切换」入口）。
    private static bool Prepare() => FindTarget() is not null;

    private static System.Reflection.MethodBase? FindTarget() =>
        AccessTools.Method(typeof(Vakuu), "ConfigureBackgroundScene");

    private static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        if (FindTarget() is { } method) yield return method;
    }

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
