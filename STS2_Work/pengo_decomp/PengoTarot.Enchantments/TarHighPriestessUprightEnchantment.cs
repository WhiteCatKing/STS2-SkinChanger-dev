using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarHighPriestessUprightEnchantment : EnchantmentModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.FromKeyword((CardKeyword)2) };

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
		((EnchantmentModel)this).Card.AddKeyword((CardKeyword)2);
	}
}
