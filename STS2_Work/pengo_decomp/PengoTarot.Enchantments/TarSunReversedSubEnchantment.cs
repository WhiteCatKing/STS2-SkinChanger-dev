using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarSunReversedSubEnchantment : EnchantmentModel
{
	private bool _isInHand;

	public override bool HasExtraCardText => true;

	public override async Task AfterCardChangedPiles(CardModel card, PileType oldPile, AbstractModel? clonedBy)
	{
		if (card == ((EnchantmentModel)this).Card)
		{
			bool isInHand = _isInHand;
			TarSunReversedSubEnchantment tarSunReversedSubEnchantment = this;
			CardPile pile = card.Pile;
			tarSunReversedSubEnchantment._isInHand = pile != null && (int)pile.Type == 2;
			if (_isInHand && !isInHand)
			{
				await PlayerCmd.GainEnergy(1m, card.Owner);
			}
		}
	}
}
