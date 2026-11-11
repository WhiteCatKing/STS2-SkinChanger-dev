using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Enchantments;

public sealed class TarDevilReversedEnchantment : EnchantmentModel
{
	private decimal _hpLostSinceLastPlay;

	private int _appliedReductions;

	public override bool HasExtraCardText => true;

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		CardModel card = ((EnchantmentModel)this).Card;
		if (((card != null) ? card.CombatState : null) == null || card.CombatState != target.CombatState || target != card.Owner.Creature || (decimal)result.UnblockedDamage <= 0m)
		{
			return Task.CompletedTask;
		}
		_hpLostSinceLastPlay += (decimal)result.UnblockedDamage;
		int num = (int)(_hpLostSinceLastPlay / 3m);
		int num2 = num - _appliedReductions;
		for (int i = 0; i < num2; i++)
		{
			card.EnergyCost.AddUntilPlayed(-1, false);
		}
		_appliedReductions = num;
		return Task.CompletedTask;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card == ((EnchantmentModel)this).Card)
		{
			_hpLostSinceLastPlay = default(decimal);
			_appliedReductions = 0;
		}
		return Task.CompletedTask;
	}
}
