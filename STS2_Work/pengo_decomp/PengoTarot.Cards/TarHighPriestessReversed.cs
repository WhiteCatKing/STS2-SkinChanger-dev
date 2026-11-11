using System.Collections.Generic;
using MegaCrit.Sts2.Core.HoverTips;
using PengoTarot.Enchantments;

namespace PengoTarot.Cards;

public sealed class TarHighPriestessReversed : TarCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<TarHighPriestessReversedEnchantment>(1);
}
