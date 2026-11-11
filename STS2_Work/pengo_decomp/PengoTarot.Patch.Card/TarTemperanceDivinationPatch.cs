using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;
using PengoTarot.Powers;

namespace PengoTarot.Patch.Card;

public static class TarTemperanceDivinationPatch
{
	[HarmonyPatch(typeof(Hook), "AfterCardPlayed")]
	public static class Hook_AfterCardPlayed_TemperancePatch
	{
		[HarmonyPostfix]
		private static async void Postfix(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			Creature creature = cardPlay.Card.Owner.Creature;
			if (creature.IsPlayer && ShouldApply())
			{
				await PowerCmd.Apply<TarTemperanceReversedPower>(choiceContext, creature, 1m, creature, cardPlay.Card, false);
			}
		}
	}

	[HarmonyPatch(typeof(RewardsSet), "GenerateRewardsFor", new Type[]
	{
		typeof(Player),
		typeof(AbstractRoom)
	})]
	public static class RewardsSet_GenerateRewardsFor_TemperancePatch
	{
		[HarmonyPostfix]
		private static void Postfix(ref List<Reward> __result)
		{
			if (ShouldApply())
			{
				__result.RemoveAll((Reward r) => r is GoldReward);
			}
		}
	}

	private const int TemperanceFlagIndex = 14;

	private static bool ShouldApply()
	{
		if (ConfigFloatingWindowRunData.GetTarFlag(14))
		{
			return RunManager.Instance.IsInProgress;
		}
		return false;
	}
}
