using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using STS2SkinChanger.Core;

namespace STS2SkinChanger;

[HarmonyPatch(typeof(ModManager), "GetModdedLocTables")]
internal static class CosmeticLocalizationOwnershipPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(ref IEnumerable<string> __result)
	{
		__result = SkinService.FilterModdedLocalizationTables(__result);
	}
}
