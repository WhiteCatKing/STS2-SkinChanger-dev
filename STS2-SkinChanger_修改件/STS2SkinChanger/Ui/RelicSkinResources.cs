using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class RelicSkinResources
{
    private static readonly HashSet<string> ReportedFailures =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> ReportedStandaloneOverrides =
        new(StringComparer.OrdinalIgnoreCase);

    public static void Replace(string resourcePath, ref Texture2D? result)
    {
        try
        {
            // A selected character's retained icon callback can return an explicit standalone
            // texture instead of an atlas slice. Do not overwrite that authored result with our
            // vanilla atlas repair. Unselected files and shared atlases still use normal isolation.
            if (result != null &&
                SkinService.IsSelectedCharacterStandaloneTexture(result.ResourcePath))
            {
                if (ReportedStandaloneOverrides.Add(resourcePath + "\n" + result.ResourcePath))
                    ModLog.Info($"已保留选中角色皮肤的独立遗物图标：{resourcePath} -> {result.ResourcePath}");
                return;
            }
            result = SkinService.GetRelicIconOverride(resourcePath) ?? result;
        }
        catch (Exception exception)
        {
            if (ReportedFailures.Add(resourcePath))
            {
                ModLog.Warn(
                    $"隔离加载遗物图标失败，已保留当前图标 {resourcePath}：" +
                    exception.GetBaseException().Message);
            }
        }
    }

    public static void ReplaceCatalogIcon(RelicModel? model, ref Texture2D? result)
    {
        if (model == null)
        {
            return;
        }

        var plan = FrameworkSkinRuntime.ResolveRelicVisual(model, largeIcon: false);
        if (plan != null)
        {
            Texture2D? frameworkResult = result;
            if (!FrameworkRelicTextureLoader.Apply(plan.IconPath, ref frameworkResult!))
            {
                result = frameworkResult;
                return;
            }
        }

        Replace(model.PackedIconPath, ref result);
    }

    public static void ReplaceDetailIcon(RelicModel? model, ref Texture2D? result)
    {
        if (model == null)
        {
            return;
        }

        var plan = FrameworkSkinRuntime.ResolveRelicVisual(model, largeIcon: true);
        if (plan != null)
        {
            Texture2D? frameworkResult = result;
            if (!FrameworkRelicTextureLoader.Apply(plan.IconPath, ref frameworkResult!))
            {
                result = frameworkResult;
                return;
            }
        }

        ReplaceBigIcon(model, ref result);
    }

    public static void ReplaceBigIcon(RelicModel model, ref Texture2D? result)
    {
        if (FrameworkSkinRuntime.ResolveRelicVisual(model, largeIcon: true) != null)
        {
            return;
        }

        var bigIconPath = TryDeriveBigIconPath(model.PackedIconPath);
        if (bigIconPath == null)
        {
            return;
        }

        try
        {
            if (result != null &&
                SkinService.IsSelectedCharacterStandaloneTexture(result.ResourcePath))
            {
                return;
            }

            result = SkinService.GetRelicBigIconOverride(bigIconPath) ?? result;
        }
        catch (Exception exception)
        {
            if (ReportedFailures.Add(bigIconPath))
            {
                ModLog.Warn(
                    $"隔离加载遗物大图标失败，已保留当前图标 {bigIconPath}：" +
                    exception.GetBaseException().Message);
            }
        }
    }

    internal static string? TryDeriveBigIconPath(string? packedIconPath)
    {
        if (string.IsNullOrWhiteSpace(packedIconPath))
        {
            return null;
        }

        const string marker = "/atlases/relic_atlas.sprites/";
        var index = packedIconPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0 || !packedIconPath.EndsWith(".tres", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var prefix = packedIconPath[..index];
        var stem = packedIconPath[(index + marker.Length)..^5];
        return string.IsNullOrWhiteSpace(stem) ? null : $"{prefix}/relics/{stem}.png";
    }
}

[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.Icon), MethodType.Getter)]
internal static class RelicIconSkinPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(RelicModel __instance, ref Texture2D? __result) =>
        RelicSkinResources.Replace(__instance.PackedIconPath, ref __result);
}

[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.IconOutline), MethodType.Getter)]
internal static class RelicIconOutlineSkinPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(RelicModel __instance, ref Texture2D? __result)
    {
        var normalPath = __instance.PackedIconPath;
        var outlinePath = normalPath.Replace(
            "/relic_atlas.sprites/",
            "/relic_outline_atlas.sprites/",
            StringComparison.OrdinalIgnoreCase);
        if (!outlinePath.Equals(normalPath, StringComparison.OrdinalIgnoreCase))
        {
            RelicSkinResources.Replace(outlinePath, ref __result);
        }
    }
}

[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.BigIcon), MethodType.Getter)]
internal static class RelicBigIconSkinPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(RelicModel __instance, ref Texture2D? __result) =>
        RelicSkinResources.ReplaceBigIcon(__instance, ref __result);
}
