using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;

namespace PengoTarot.Enchantments;

public sealed class TarHierophantReversedEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override bool CanEnchant(CardModel card)
	{
		return ((EnchantmentModel)this).CanEnchant(card);
	}

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		Player player = ((EnchantmentModel)this).Card.Owner;
		CardPile pile = PileTypeExtensions.GetPile((PileType)2, player);
		Rng rng = player.RunState.Rng.CombatCardSelection;
		List<CardModel> list = (from c in pile.Cards.Where(delegate(CardModel c)
			{
				if (!c.IsUpgraded)
				{
					Wither val = (Wither)(object)((c is Wither) ? c : null);
					if (val != null)
					{
						return GetWitherFakeLevel(val) > 0;
					}
					return false;
				}
				return true;
			})
			orderby (c is Wither) ? 1 : 0
			select c).ToList();
		if (list.Count == 0)
		{
			return;
		}
		LocString val2 = new LocString("gameplay_ui", "CHOOSE_CARD_DOWNGRADE_HEADER");
		CardSelectorPrefs val3 = new CardSelectorPrefs(val2, 0, list.Count);
		((CardSelectorPrefs)(ref val3)).set_Cancelable(false);
		((CardSelectorPrefs)(ref val3)).set_RequireManualConfirmation(false);
		CardSelectorPrefs val4 = val3;
		List<CardModel> list2 = (await CardSelectCmd.FromHand(choiceContext, player, val4, (Func<CardModel, bool>)delegate(CardModel c)
		{
			if (!c.IsUpgraded)
			{
				Wither val5 = (Wither)(object)((c is Wither) ? c : null);
				if (val5 != null)
				{
					return GetWitherFakeLevel(val5) > 0;
				}
				return false;
			}
			return true;
		}, (AbstractModel)(object)this)).ToList();
		foreach (CardModel item in list2)
		{
			Wither val6 = (Wither)(object)((item is Wither) ? item : null);
			if (val6 != null)
			{
				DowngradeWither(val6);
			}
			else
			{
				CardCmd.Downgrade(item);
			}
		}
		CardPile pile2 = PileTypeExtensions.GetPile((PileType)1, player);
		List<CardModel> list3 = (from c in Enumerable.Concat(second: PileTypeExtensions.GetPile((PileType)3, player).Cards, first: pile2.Cards)
			where c.IsUpgradable && !(c is Wither)
			select c).ToList();
		foreach (CardModel item2 in list2)
		{
			_ = item2;
			if (list3.Count == 0)
			{
				break;
			}
			int num = Math.Min(3, list3.Count);
			List<CardModel> picked = IEnumerableExtensions.TakeRandom<CardModel>((IEnumerable<CardModel>)list3, num, rng).ToList();
			foreach (CardModel item3 in picked)
			{
				CardCmd.Upgrade(item3, (CardPreviewStyle)1);
				CardCmd.Preview(item3, 1.2f, (CardPreviewStyle)1);
			}
			list3.RemoveAll((CardModel c) => picked.Contains(c));
		}
		await Task.CompletedTask;
	}

	private static int GetWitherFakeLevel(Wither wither)
	{
		return (int)typeof(Wither).GetField("_fakeUpgradeLevel", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(wither);
	}

	private static void DowngradeWither(Wither wither)
	{
		int witherFakeLevel = GetWitherFakeLevel(wither);
		if (witherFakeLevel > 0)
		{
			typeof(Wither).GetField("_fakeUpgradeLevel", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(wither, 0);
			((CardModel)wither).DynamicVars["Damage"].UpgradeValueBy(-3m * (decimal)witherFakeLevel);
		}
	}
}
