using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Enchantments;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(CardModel), "SetToFreeThisCombat")]
public static class CardModel_SetToFreeThisCombat_Patch
{
	[HarmonyPostfix]
	private static void Postfix(CardModel __instance)
	{
		if (__instance.Enchantment is TarFoolReversedEnchantment tarFoolReversedEnchantment)
		{
			tarFoolReversedEnchantment.IgnoreGenerationCost = true;
		}
	}
}
