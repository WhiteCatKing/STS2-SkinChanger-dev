using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Powers;

public sealed class PlanetSaturnPower : PowerModel
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if (!ValuePropExtensions.IsPoweredAttack(props))
		{
			return Task.CompletedTask;
		}
		((PowerModel)this).Flash();
		return Task.CompletedTask;
	}

	public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (target != ((PowerModel)this).Owner)
		{
			return 0m;
		}
		if (!ValuePropExtensions.IsPoweredAttack(props))
		{
			return 0m;
		}
		if (amount >= (decimal)((PowerModel)this).Amount)
		{
			return 0m;
		}
		return (decimal)((PowerModel)this).Amount - amount;
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((PowerModel)this).Owner.Side != side)
		{
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}
}
