using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarWorldUprightEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.Static((StaticHoverTip)3, Array.Empty<DynamicVar>()) };

	public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
	{
		if (((EnchantmentModel)this).Card == null)
		{
			return;
		}
		CardPile pile = ((EnchantmentModel)this).Card.Pile;
		PileType? val = ((pile != null) ? new PileType?(pile.Type) : ((PileType?)null));
		bool flag;
		if (val.HasValue)
		{
			PileType valueOrDefault = val.GetValueOrDefault();
			if ((int)valueOrDefault == 0 || (int)valueOrDefault == 5)
			{
				flag = true;
				goto IL_006a;
			}
		}
		flag = false;
		goto IL_006a;
		IL_006a:
		if (!flag)
		{
			CardPile pile2 = ((EnchantmentModel)this).Card.Pile;
			if (pile2 == null || (int)pile2.Type != 2)
			{
				await CardPileCmd.Add(((EnchantmentModel)this).Card, (PileType)2, (CardPilePosition)1, (AbstractModel)null, false);
				int withModifiers = ((EnchantmentModel)this).Card.EnergyCost.GetWithModifiers((CostModifiers)(-1));
				((EnchantmentModel)this).Card.EnergyCost.SetThisTurn(withModifiers + 1, false);
			}
		}
	}
}
