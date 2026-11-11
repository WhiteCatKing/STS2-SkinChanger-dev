using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Rewards;
using PengoTarot.Data.Judgement;

namespace PengoTarot.Patches.Divination;

[HarmonyPatch]
public static class PortableTarotRewardContextPatch
{
	[HarmonyPatch]
	private static class SimpleRewardCreated
	{
		private static MethodBase TargetMethod()
		{
			return AccessTools.Method(typeof(NSimpleCardSelectScreen), "Create", new Type[2]
			{
				typeof(IReadOnlyList<CardCreationResult>),
				typeof(CardSelectorPrefs)
			}, (Type[])null);
		}

		private static void Postfix(IReadOnlyList<CardCreationResult> cards, NSimpleCardSelectScreen __result)
		{
			PortableTarotRewardContextRegistry.Register((Node)(object)__result, cards.Select((CardCreationResult result) => result.Card));
			((Node)__result).TreeExiting += delegate
			{
				PortableTarotRewardContextRegistry.Unregister((Node)(object)__result);
			};
		}
	}

	[HarmonyPatch(typeof(CardSelectCmd), "FromSimpleGridForRewards")]
	private static class SimpleRewardLogicalContext
	{
		private static void Prefix(List<CardCreationResult> cards, Player player)
		{
			PortableTarotRewardContextRegistry.Register(player, PortableTarotUseContext.SimpleGridReward, cards.Select((CardCreationResult result) => result.Card));
		}

		private static void Postfix(Player player, ref Task<IEnumerable<CardModel>> __result)
		{
			__result = AwaitSimpleRewardAndClear(__result, player);
		}

		private static async Task<IEnumerable<CardModel>> AwaitSimpleRewardAndClear(Task<IEnumerable<CardModel>> task, Player player)
		{
			try
			{
				return await task;
			}
			finally
			{
				PortableTarotRewardContextRegistry.Unregister(player, PortableTarotUseContext.SimpleGridReward);
			}
		}
	}

	[HarmonyPatch(typeof(CardReward), "OnSelect")]
	[HarmonyPrefix]
	private static void CardRewardSelecting(CardReward __instance)
	{
		PortableTarotRewardContextRegistry.Register(((Reward)__instance).Player, PortableTarotUseContext.CardReward, __instance.Cards);
	}

	[HarmonyPatch(typeof(CardReward), "OnSelect")]
	[HarmonyPostfix]
	private static void CardRewardSelected(CardReward __instance, ref Task<bool> __result)
	{
		__result = AwaitCardRewardAndClear(__result, ((Reward)__instance).Player);
	}

	private static async Task<bool> AwaitCardRewardAndClear(Task<bool> task, Player player)
	{
		try
		{
			return await task;
		}
		finally
		{
			PortableTarotRewardContextRegistry.Unregister(player, PortableTarotUseContext.CardReward);
		}
	}

	[HarmonyPatch(typeof(CardReward), "Reroll")]
	[HarmonyPostfix]
	private static void CardRewardRerolled(CardReward __instance)
	{
		PortableTarotRewardContextRegistry.Register(((Reward)__instance).Player, PortableTarotUseContext.CardReward, __instance.Cards);
	}

	[HarmonyPatch(typeof(NCardRewardSelectionScreen), "ShowScreen")]
	[HarmonyPostfix]
	private static void CardRewardShown(IReadOnlyList<CardCreationResult> options, IReadOnlyList<CardRewardAlternative> extraOptions, NCardRewardSelectionScreen? __result)
	{
		if (__result != null)
		{
			PortableTarotRewardContextRegistry.Register((Node)(object)__result, options.Select((CardCreationResult result) => result.Card));
		}
	}

	[HarmonyPatch(typeof(NCardRewardSelectionScreen), "RefreshOptions")]
	[HarmonyPostfix]
	private static void CardRewardRefreshed(NCardRewardSelectionScreen __instance, IReadOnlyList<CardCreationResult> options)
	{
		PortableTarotRewardContextRegistry.Register((Node)(object)__instance, options.Select((CardCreationResult result) => result.Card));
	}

	[HarmonyPatch(typeof(NCardRewardSelectionScreen), "_ExitTree")]
	[HarmonyPostfix]
	private static void CardRewardClosed(NCardRewardSelectionScreen __instance)
	{
		PortableTarotRewardContextRegistry.Unregister((Node)(object)__instance);
	}
}
