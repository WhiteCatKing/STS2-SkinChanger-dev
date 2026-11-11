using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(CardPoolModel), "get_FrameMaterialPath")]
internal static class FrameworkCardFramePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(600)]
	private static bool Prefix(CardPoolModel __instance, ref string __result)
	{
		if (!FrameworkSkinRuntime.TryGetPoolResource(__instance, "CardFrameMaterial", out string path))
		{
			return true;
		}
		__result = path;
		return false;
	}
}
