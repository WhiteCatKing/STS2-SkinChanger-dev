using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Enchantments;

public sealed class TarJusticeUprightEnchantment : EnchantmentModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.FromKeyword((CardKeyword)1) };

	public override bool HasExtraCardText => true;

	public override bool CanEnchant(CardModel card)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		if (!((EnchantmentModel)this).CanEnchant(card))
		{
			return false;
		}
		if (card.Keywords.Contains((CardKeyword)1))
		{
			return false;
		}
		if ((int)card.TargetType == 3)
		{
			return false;
		}
		return (int)card.Type == 1;
	}

	protected override void OnEnchant()
	{
		((EnchantmentModel)this).Card.AddKeyword((CardKeyword)1);
	}

	public override decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (ValuePropExtensions.IsPoweredAttack(props))
		{
			return 2m;
		}
		return 1m;
	}
}
