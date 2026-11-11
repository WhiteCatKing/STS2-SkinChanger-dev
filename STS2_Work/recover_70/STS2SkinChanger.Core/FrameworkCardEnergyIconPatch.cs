using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(CardPoolModel), "get_EnergyIconPath")]
internal static class FrameworkCardEnergyIconPatch
{
	[HarmonyPrefix]
	[HarmonyPriority(600)]
	private static bool Prefix(CardPoolModel __instance, ref string __result)
	{
		if (!FrameworkSkinRuntime.TryGetPoolResource(__instance, "EnergyIcon", out string path))
		{
			return true;
		}
		__result = path;
		return false;
	}
}
