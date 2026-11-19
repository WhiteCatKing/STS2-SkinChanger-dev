using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Timeline;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class TimelineEpochSkinResources
{
    private static readonly HashSet<string> ReportedFailures =
        new(StringComparer.OrdinalIgnoreCase);

    public static void Replace(string? epochId, bool thumbnail, ref Texture2D result)
    {
        if (string.IsNullOrWhiteSpace(epochId))
        {
            return;
        }

        try
        {
            result = SkinService.GetEpochPortraitOverride(epochId, thumbnail) ?? result;
        }
        catch (Exception exception)
        {
            var key = $"{epochId}:{(thumbnail ? "thumb" : "full")}";
            if (ReportedFailures.Add(key))
            {
                ModLog.Warn(
                    $"隔离加载时间线插图失败，已保留当前贴图 {key}：" +
                    exception.GetBaseException().Message);
            }
        }
    }
}

[HarmonyPatch(typeof(EpochModel), nameof(EpochModel.Portrait), MethodType.Getter)]
internal static class EpochPortraitThumbnailSkinPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(EpochModel __instance, ref Texture2D __result) =>
        TimelineEpochSkinResources.Replace(__instance.Id, thumbnail: true, ref __result);
}

[HarmonyPatch(typeof(EpochModel), nameof(EpochModel.RealPortrait), MethodType.Getter)]
internal static class EpochRealPortraitSkinPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(EpochModel __instance, ref Texture2D __result) =>
        TimelineEpochSkinResources.Replace(__instance.Id, thumbnail: false, ref __result);
}
