using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

[HarmonyPatch(typeof(CardModel), "GetStarCostWithModifiers")]
public static class CardModel_GetStarCostWithModifiers_Patch
{
	private static bool Prefix(CardModel __instance, ref int __result)
	{
		if (__instance.Enchantment is TarStarReversedEnchantment { IsSwappedToEnergyX: not false })
		{
			__result = 0;
			return false;
		}
		return true;
	}
}
