using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Enchantments;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(CardPileCmd), "CheckIfDrawIsPossibleAndShowThoughtBubbleIfNot")]
public static class CardPileCmd_CheckIfDraw_Patch
{
	private static bool Prefix(Player player, ref bool __result)
	{
		if (PileTypeExtensions.GetPile((PileType)2, player).Cards.Any((CardModel c) => c.Enchantment is TarDeathReversedEnchantment))
		{
			__result = false;
			return false;
		}
		return true;
	}
}
