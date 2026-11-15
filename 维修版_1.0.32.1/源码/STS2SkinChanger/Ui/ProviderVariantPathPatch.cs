using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Ui;

// 常驻·共享：角色变体 / 商人房 —— 解析角色变体在商人房用的场景路径。详见 docs/refactor/resident-patches.md
/// <summary>
/// Provider variant packs may ship one merchant and one rest-site scene per optional variant.
/// When the player picks a synthetic variant option, route those scenes by the option's suffix.
/// No provider name is hard-coded; the scene naming contract is documented in
/// <see cref="STS2SkinChanger.Catalog.ProviderVariantOptions"/>.
/// </summary>
[HarmonyPatch(typeof(CharacterModel), "get_MerchantAnimPath")]
internal static class ProviderMerchantVariantPathPatch
{
    [HarmonyPrefix]
    private static bool Prefix(CharacterModel __instance, ref string __result)
    {
        var path = ContextualSkinControls.ResolveVariantScenePath(__instance, "merchant");
        if (path == null) return true;
        __result = path;
        return false;
    }
}

// 常驻·共享：角色变体 / 火堆 —— 解析角色变体在火堆用的场景路径。详见 docs/refactor/resident-patches.md
[HarmonyPatch(typeof(CharacterModel), "get_RestSiteAnimPath")]
internal static class ProviderRestSiteVariantPathPatch
{
    [HarmonyPrefix]
    private static bool Prefix(CharacterModel __instance, ref string __result)
    {
        var path = ContextualSkinControls.ResolveVariantScenePath(__instance, "rest");
        if (path == null) return true;
        __result = path;
        return false;
    }
}
