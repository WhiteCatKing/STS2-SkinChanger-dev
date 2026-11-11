using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(CharacterModel), "get_TrailPath")]
internal static class FrameworkCharacterTrailPatch
{
	[HarmonyPrefix]
	[HarmonyPriority(600)]
	private static bool Prefix(CharacterModel __instance, ref string __result)
	{
		if (!FrameworkSkinRuntime.TryGetCharacterResource(__instance, "CardTrail", out string path))
		{
			return true;
		}
		__result = path;
		return false;
	}
}
