using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(CharacterModel), "get_RestSiteAnimPath")]
internal static class ProviderRestSiteVariantPathPatch
{
	[HarmonyPrefix]
	private static bool Prefix(CharacterModel __instance, ref string __result)
	{
		string text = ContextualSkinControls.ResolveVariantScenePath(__instance, "rest");
		if (text == null)
		{
			return true;
		}
		__result = text;
		return false;
	}
}
