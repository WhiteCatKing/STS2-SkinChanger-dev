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
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Enchantments;

public sealed class TarJusticeReversedEnchantment : EnchantmentModel
{
	private decimal _pendingDamage;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[2]
	{
		HoverTipFactory.FromKeyword((CardKeyword)1),
		HoverTipFactory.Static((StaticHoverTip)5, Array.Empty<DynamicVar>())
	};

	public override bool HasExtraCardText => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
	{
		new DynamicVar("DamageDealt", 0m)
	};

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

	public override Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (cardSource == ((EnchantmentModel)this).Card)
		{
			_pendingDamage += (decimal)(result.TotalDamage + result.OverkillDamage);
		}
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card == ((EnchantmentModel)this).Card && !(_pendingDamage <= 0m))
		{
			decimal pendingDamage = _pendingDamage;
			_pendingDamage = default(decimal);
			((EnchantmentModel)this).DynamicVars["DamageDealt"].BaseValue = pendingDamage;
			((EnchantmentModel)this).RecalculateValues();
			await CreatureCmd.GainBlock(((EnchantmentModel)this).Card.Owner.Creature, pendingDamage, (ValueProp)8, cardPlay, false);
		}
	}
}
