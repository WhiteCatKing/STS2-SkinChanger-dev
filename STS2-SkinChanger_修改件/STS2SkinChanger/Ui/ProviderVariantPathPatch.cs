using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Ui;

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
