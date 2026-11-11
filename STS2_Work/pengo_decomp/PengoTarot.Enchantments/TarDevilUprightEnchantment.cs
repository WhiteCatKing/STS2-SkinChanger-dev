using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace PengoTarot.Enchantments;

public sealed class TarDevilUprightEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override bool ShouldGlowRed => true;

	protected override void OnEnchant()
	{
		((EnchantmentModel)this).Card.EnergyCost.UpgradeBy(-1);
	}

	public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (card.Owner != ((EnchantmentModel)this).Card.Owner)
		{
			return true;
		}
		CardPile pile = ((EnchantmentModel)this).Card.Pile;
		if (pile == null || (int)pile.Type != 2)
		{
			return true;
		}
		if ((int)autoPlayType != 0 || card == ((EnchantmentModel)this).Card)
		{
			return true;
		}
		if (card.Enchantment is TarDevilUprightEnchantment)
		{
			return true;
		}
		if (card is Enthralled)
		{
			return true;
		}
		return false;
	}
}
