using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Enchantments;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(CardModel), "get_TargetType")]
public static class CardModel_TargetType_Patch
{
	private static bool Prefix(CardModel __instance, ref TargetType __result)
	{
		EnchantmentModel enchantment = __instance.Enchantment;
		if ((enchantment is PlanetMercuryEnchantment || enchantment is PlanetVenusEnchantment || enchantment is PlanetEarthEnchantment || enchantment is PlanetMarsEnchantment) ? true : false)
		{
			__result = (TargetType)6;
			return false;
		}
		return true;
	}
}
