using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patch.Card;

public static class AscendersBaneTowerPatch
{
	[HarmonyPatch(typeof(CardModel), "CanPlay", new Type[] { })]
	public static class CardModel_CanPlay_TowerPatch
	{
		[HarmonyPostfix]
		private static void Postfix(CardModel __instance, ref bool __result)
		{
			if (__instance is AscendersBane && ShouldApply())
			{
				__result = true;
			}
		}
	}

	[HarmonyPatch(typeof(CardModel), "GetDescriptionForPile", new Type[]
	{
		typeof(PileType),
		typeof(Creature)
	})]
	public static class CardModel_GetDescriptionForPile_TowerPatch
	{
		[HarmonyPostfix]
		private static void Postfix(CardModel __instance, ref string __result)
		{
			if (!(__instance is AscendersBane) || !ShouldApply())
			{
				return;
			}
			LocString ifExists = LocString.GetIfExists("gameplay_ui", "BAL_CFW_TOWER_CARD_DESC");
			if (ifExists != null)
			{
				string formattedText = ifExists.GetFormattedText();
				if (!string.IsNullOrEmpty(formattedText))
				{
					__result = __result + "\n" + formattedText;
				}
			}
		}
	}

	[HarmonyPatch]
	public static class CardModel_CanPlayReason_TowerPatch
	{
		private static MethodBase TargetMethod()
		{
			return AccessTools.Method(typeof(CardModel), "CanPlay", new Type[2]
			{
				typeof(UnplayableReason).MakeByRefType(),
				typeof(AbstractModel).MakeByRefType()
			}, (Type[])null);
		}

		[HarmonyPostfix]
		private static void Postfix(CardModel __instance, ref bool __result, ref UnplayableReason reason, ref AbstractModel? preventer)
		{
			if (__instance is AscendersBane && ShouldApply())
			{
				reason = (UnplayableReason)0;
				preventer = null;
				__result = true;
			}
		}
	}

	[HarmonyPatch(typeof(AbstractModel), "AfterCardExhausted")]
	public static class AbstractModel_AfterCardExhausted_TowerPatch
	{
		[HarmonyPrefix]
		private static bool Prefix(ref Task __result, AbstractModel __instance, PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
		{
			if (!(__instance is AscendersBane) || card != __instance)
			{
				return true;
			}
			if (!ShouldApply())
			{
				return true;
			}
			__result = ExhaustAllHandAsync(choiceContext, card);
			return false;
		}

		private static async Task ExhaustAllHandAsync(PlayerChoiceContext choiceContext, CardModel ascendersBane)
		{
			Player owner = ascendersBane.Owner;
			object obj;
			if (owner == null)
			{
				obj = null;
			}
			else
			{
				PlayerCombatState playerCombatState = owner.PlayerCombatState;
				if (playerCombatState == null)
				{
					obj = null;
				}
				else
				{
					CardPile hand = playerCombatState.Hand;
					obj = ((hand == null) ? null : hand.Cards?.ToList());
				}
			}
			List<CardModel> list = (List<CardModel>)obj;
			if (list == null || list.Count == 0)
			{
				return;
			}
			foreach (CardModel item in list)
			{
				await CardCmd.Exhaust(choiceContext, item, false, false);
			}
		}
	}

	[HarmonyPatch(typeof(EnchantmentModel), "CanEnchant")]
	public static class EnchantmentModel_CanEnchant_TowerPatch
	{
		[HarmonyPrefix]
		private static bool Prefix(EnchantmentModel __instance, CardModel card, ref bool __result)
		{
			if (!(card is AscendersBane))
			{
				return true;
			}
			if (!ShouldApply())
			{
				return true;
			}
			__result = true;
			return false;
		}
	}

	private const int TowerFlagIndex = 16;

	internal static bool ShouldApply()
	{
		if (ConfigFloatingWindowRunData.GetTarFlag(16))
		{
			return RunManager.Instance.IsInProgress;
		}
		return false;
	}
}
