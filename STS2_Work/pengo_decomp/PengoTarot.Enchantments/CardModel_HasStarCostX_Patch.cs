using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

[HarmonyPatch(typeof(CardModel), "get_HasStarCostX")]
public static class CardModel_HasStarCostX_Patch
{
	private static bool Prefix(CardModel __instance, ref bool __result)
	{
		if (__instance.Enchantment is TarStarReversedEnchantment tarStarReversedEnchantment)
		{
			if (tarStarReversedEnchantment.IsSwappedToStarX)
			{
				__result = true;
				return false;
			}
			if (tarStarReversedEnchantment.IsSwappedToEnergyX)
			{
				__result = false;
				return false;
			}
		}
		return true;
	}
}
