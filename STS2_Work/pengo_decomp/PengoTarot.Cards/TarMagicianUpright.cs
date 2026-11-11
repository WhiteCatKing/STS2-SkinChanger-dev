using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using PengoTarot.Enchantments;

namespace PengoTarot.Cards;

public sealed class TarMagicianUpright : TarCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<TarMagicianUprightEnchantment>(1);
}
