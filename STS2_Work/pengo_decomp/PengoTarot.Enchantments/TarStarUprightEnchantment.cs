using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Enchantments;

public sealed class TarStarUprightEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.Static((StaticHoverTip)5, Array.Empty<DynamicVar>()) };

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		CardModel card = ((EnchantmentModel)this).Card;
		object obj;
		if (card == null)
		{
			obj = null;
		}
		else
		{
			Player owner = card.Owner;
			obj = ((owner != null) ? owner.PlayerCombatState : null);
		}
		if (obj != null)
		{
			int stars = ((EnchantmentModel)this).Card.Owner.PlayerCombatState.Stars;
			if (stars > 0)
			{
				await CreatureCmd.GainBlock(((EnchantmentModel)this).Card.Owner.Creature, (decimal)stars, (ValueProp)8, (CardPlay)null, false);
			}
		}
	}
}
