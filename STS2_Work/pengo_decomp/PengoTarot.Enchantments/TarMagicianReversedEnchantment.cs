using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarMagicianReversedEnchantment : EnchantmentModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.FromKeyword((CardKeyword)1) };

	public override bool HasExtraCardText => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		return (int)cardType != 3;
	}

	public override bool CanEnchant(CardModel card)
	{
		if (!((EnchantmentModel)this).CanEnchant(card))
		{
			return false;
		}
		return !card.Keywords.Contains((CardKeyword)1);
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card == ((EnchantmentModel)this).Card && ((EnchantmentModel)this).Card != null && !((EnchantmentModel)this).Card.HasBeenRemovedFromState)
		{
			await CardPileCmd.Add(((EnchantmentModel)this).Card, (PileType)1, (CardPilePosition)3, (AbstractModel)null, false);
		}
	}
}
