using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Enchantments;

public sealed class TarEmperorReversedEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.FromKeyword((CardKeyword)5) };

	public override Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		if (((EnchantmentModel)this).Card.Owner == player)
		{
			CardPile pile = ((EnchantmentModel)this).Card.Pile;
			if (pile != null && (int)pile.Type == 2)
			{
				List<CardModel> list = PileTypeExtensions.GetPile((PileType)2, player).Cards.ToList();
				if (list.Count == 0)
				{
					return Task.CompletedTask;
				}
				int num = Math.Min(2, list.Count);
				List<CardModel> list2 = new List<CardModel>();
				List<CardModel> list3 = new List<CardModel>(list);
				RunRngSet rng = player.RunState.Rng;
				for (int i = 0; i < num; i++)
				{
					CardModel item = rng.CombatCardSelection.NextItem<CardModel>((IEnumerable<CardModel>)list3);
					list2.Add(item);
					list3.Remove(item);
				}
				foreach (CardModel item2 in list2)
				{
					if (!item2.Keywords.Contains((CardKeyword)5))
					{
						CardCmd.ApplyKeyword(item2, (CardKeyword[])(object)new CardKeyword[1] { (CardKeyword)5 });
					}
				}
				return Task.CompletedTask;
			}
		}
		return Task.CompletedTask;
	}
}
