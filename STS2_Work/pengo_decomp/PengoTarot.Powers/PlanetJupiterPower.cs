using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace PengoTarot.Powers;

public sealed class PlanetJupiterPower : PowerModel
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (target != ((PowerModel)this).Owner || dealer == null || !ValuePropExtensions.IsPoweredAttack(props) || result.UnblockedDamage + result.OverkillDamage <= 0)
		{
			return;
		}
		((PowerModel)this).Flash();
		int goldAmount = (result.UnblockedDamage + result.OverkillDamage) * ((PowerModel)this).Amount;
		foreach (Player player in ((PowerModel)this).CombatState.Players)
		{
			await PowerCmd.Apply<PlanetGoldPower>(choiceContext, player.Creature, (decimal)goldAmount, dealer, (CardModel)null, true);
		}
		SfxCmd.Play("event:/sfx/ui/gold/gold_1", 1f);
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
