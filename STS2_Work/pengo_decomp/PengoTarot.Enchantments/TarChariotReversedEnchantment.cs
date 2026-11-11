using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PengoTarot.Utils;

namespace PengoTarot.Enchantments;

public sealed class TarChariotReversedEnchantment : EnchantmentModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.FromPower<VulnerablePower>((int?)null) };

	public override bool HasExtraCardText => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		return (int)cardType == 1;
	}

	public override bool CanEnchant(CardModel card)
	{
		if (!((EnchantmentModel)this).CanEnchant(card))
		{
			return false;
		}
		return MultiHitDetector.IsMultiHitCard(card);
	}

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		if (((EnchantmentModel)this).Card != null)
		{
			VulnerablePower val = await PowerCmd.Apply<VulnerablePower>(choiceContext, ((EnchantmentModel)this).Card.Owner.Creature, 1m, ((EnchantmentModel)this).Card.Owner.Creature, ((EnchantmentModel)this).Card, false);
			if (val != null)
			{
				((PowerModel)val).SkipNextDurationTick = false;
			}
		}
	}

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (cardSource == ((EnchantmentModel)this).Card && target != null && dealer != null)
		{
			await PowerCmd.Apply<VulnerablePower>(choiceContext, target, 1m, dealer, ((EnchantmentModel)this).Card, false);
		}
	}
}
