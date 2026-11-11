using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patch.Card;

public static class TarMoonDivinationPatch
{
	[HarmonyPatch(typeof(CardPileCmd), "Shuffle")]
	public static class CardPileCmd_Shuffle_MoonPatch
	{
		[HarmonyPrefix]
		private static bool Prefix(PlayerChoiceContext choiceContext, Player player, ref Task __result)
		{
			if (!ShouldApply() || _inMoonShuffle)
			{
				return true;
			}
			__result = MoonShuffle(choiceContext, player);
			return false;
		}

		private static async Task MoonShuffle(PlayerChoiceContext choiceContext, Player player)
		{
			if (CombatManager.Instance.IsOverOrEnding)
			{
				return;
			}
			CardPile pile = PileTypeExtensions.GetPile((PileType)2, player);
			int count = pile.Cards.Count;
			_inMoonShuffle = true;
			try
			{
				if (count > 0)
				{
					await CardCmd.Discard(choiceContext, (IEnumerable<CardModel>)pile.Cards.ToList());
				}
				await CardPileCmd.Shuffle(choiceContext, player);
				if (count > 0 && !CombatManager.Instance.IsOverOrEnding)
				{
					await CardPileCmd.Draw(choiceContext, (decimal)count, player, false);
				}
			}
			finally
			{
				_inMoonShuffle = false;
			}
		}
	}

	private const int MoonFlagIndex = 18;

	private static bool _inMoonShuffle;

	private static bool ShouldApply()
	{
		if (ConfigFloatingWindowRunData.GetTarFlag(18))
		{
			return RunManager.Instance.IsInProgress;
		}
		return false;
	}
}
