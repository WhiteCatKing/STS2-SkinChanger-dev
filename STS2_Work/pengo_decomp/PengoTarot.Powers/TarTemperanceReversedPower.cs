using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Powers;

public sealed class TarTemperanceReversedPower : PowerModel
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (target == ((PowerModel)this).Owner && result.UnblockedDamage > 0)
		{
			((PowerModel)this).Flash();
			int num = result.UnblockedDamage * ((PowerModel)this).Amount;
			if (num > 0)
			{
				await PowerCmd.Apply<PlanetGoldPower>(choiceContext, ((PowerModel)this).Owner, (decimal)num, ((PowerModel)this).Owner, (CardModel)null, true);
				SfxCmd.Play("event:/sfx/ui/gold/gold_1", 1f);
			}
		}
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
