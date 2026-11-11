using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

[HarmonyPatch(typeof(CardModel), "ResolveEnergyXValue")]
public static class CardModel_ResolveEnergyXValue_Patch
{
	private static bool Prefix(CardModel __instance, ref int __result)
	{
		if (__instance.Enchantment is TarStarReversedEnchantment { IsSwappedToStarX: not false })
		{
			__result = __instance.ResolveStarXValue();
			return false;
		}
		return true;
	}
}
