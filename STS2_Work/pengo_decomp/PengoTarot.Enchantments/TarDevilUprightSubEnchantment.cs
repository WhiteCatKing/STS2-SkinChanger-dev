using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarDevilUprightSubEnchantment : EnchantmentModel
{
	private bool _isInHand;

	public override bool HasExtraCardText => true;

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPile, AbstractModel? clonedBy)
	{
		if (card == ((EnchantmentModel)this).Card)
		{
			bool isInHand = _isInHand;
			TarDevilUprightSubEnchantment tarDevilUprightSubEnchantment = this;
			CardPile pile = card.Pile;
			tarDevilUprightSubEnchantment._isInHand = pile != null && (int)pile.Type == 2;
			if (_isInHand && !isInHand)
			{
				await CardPileCmd.Draw((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), 2m, card.Owner, false);
			}
		}
	}
}
