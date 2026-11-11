using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using PengoTarot.Enchantments;

namespace PengoTarot.Cards;

public sealed class PlanetMercury : PlanetCard
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<PlanetMercuryEnchantment>(1);

	public PlanetMercury()
		: base((CardType)3)
	{
	}
}
