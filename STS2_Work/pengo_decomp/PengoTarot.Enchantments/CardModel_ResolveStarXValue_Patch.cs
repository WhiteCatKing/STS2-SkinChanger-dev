using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

[HarmonyPatch(typeof(CardModel), "ResolveStarXValue")]
public static class CardModel_ResolveStarXValue_Patch
{
	private static bool Prefix(CardModel __instance, ref int __result)
	{
		if (__instance.Enchantment is TarStarReversedEnchantment { IsSwappedToEnergyX: not false })
		{
			__result = __instance.ResolveEnergyXValue();
			return false;
		}
		return true;
	}
}
