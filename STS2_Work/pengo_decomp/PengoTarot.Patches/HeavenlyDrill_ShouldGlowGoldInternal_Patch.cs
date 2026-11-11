using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using PengoTarot.Enchantments;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(HeavenlyDrill), "get_ShouldGlowGoldInternal")]
public static class HeavenlyDrill_ShouldGlowGoldInternal_Patch
{
	private static bool Prefix(HeavenlyDrill __instance, ref bool __result)
	{
		if (!(((CardModel)__instance).Enchantment is TarStarReversedEnchantment tarStarReversedEnchantment))
		{
			return true;
		}
		int intValue = ((CardModel)__instance).DynamicVars["Energy"].IntValue;
		if (tarStarReversedEnchantment.IsSwappedToStarX)
		{
			__result = ((CardModel)__instance).Owner.PlayerCombatState.Stars >= intValue;
		}
		else
		{
			__result = ((CardModel)__instance).Owner.PlayerCombatState.Energy >= intValue;
		}
		return false;
	}
}
