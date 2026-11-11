using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarHangedManUprightEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.FromKeyword((CardKeyword)1) };

	public override bool CanEnchant(CardModel card)
	{
		if (!((EnchantmentModel)this).CanEnchant(card))
		{
			return false;
		}
		return card.Keywords.Contains((CardKeyword)1);
	}

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel exhaustedCard, bool causedByEthereal)
	{
		if (exhaustedCard != ((EnchantmentModel)this).Card && ((EnchantmentModel)this).Card != null)
		{
			CardPile pile = ((EnchantmentModel)this).Card.Pile;
			if (pile != null && (int)pile.Type == 1)
			{
				await CardPileCmd.Add(exhaustedCard, (PileType)3, (CardPilePosition)1, (AbstractModel)null, false);
				await CardCmd.AutoPlay(choiceContext, ((EnchantmentModel)this).Card, (Creature)null, (AutoPlayType)1, false, false);
			}
		}
	}
}
