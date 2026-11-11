using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(CharacterModel), "get_MerchantAnimPath")]
internal static class ProviderMerchantVariantPathPatch
{
	[HarmonyPrefix]
	private static bool Prefix(CharacterModel __instance, ref string __result)
	{
		string text = ContextualSkinControls.ResolveVariantScenePath(__instance, "merchant");
		if (text == null)
		{
			return true;
		}
		__result = text;
		return false;
	}
}
