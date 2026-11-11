using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarHighPriestessReversedEnchantment : EnchantmentModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[2]
	{
		HoverTipFactory.FromKeyword((CardKeyword)5),
		HoverTipFactory.FromKeyword((CardKeyword)2)
	};

	public override bool HasExtraCardText => true;

	public override bool CanEnchant(CardModel card)
	{
		if (!((EnchantmentModel)this).CanEnchant(card))
		{
			return false;
		}
		return !card.Keywords.Contains((CardKeyword)2);
	}

	protected override void OnEnchant()
	{
		((EnchantmentModel)this).Card.AddKeyword((CardKeyword)5);
	}

	public override Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Invalid comparison between Unknown and I4
		if ((int)side != 1 || ((EnchantmentModel)this).Card == null)
		{
			return Task.CompletedTask;
		}
		Player owner = ((EnchantmentModel)this).Card.Owner;
		if (!participants.Contains(owner.Creature))
		{
			return Task.CompletedTask;
		}
		CardPile pile = ((EnchantmentModel)this).Card.Pile;
		if (pile == null || (int)pile.Type != 2)
		{
			return Task.CompletedTask;
		}
		CardPile pile2 = PileTypeExtensions.GetPile((PileType)2, owner);
		int num = ListExtensions.IndexOf<CardModel>(pile2.Cards, ((EnchantmentModel)this).Card);
		if (num <= 0)
		{
			return Task.CompletedTask;
		}
		for (int i = 0; i < num; i++)
		{
			CardModel val = pile2.Cards[i];
			if (!val.Keywords.Contains((CardKeyword)2))
			{
				CardCmd.ApplyKeyword(val, (CardKeyword[])(object)new CardKeyword[1] { (CardKeyword)2 });
			}
		}
		return Task.CompletedTask;
	}
}
