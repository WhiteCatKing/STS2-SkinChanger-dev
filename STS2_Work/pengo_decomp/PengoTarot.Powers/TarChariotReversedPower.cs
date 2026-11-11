using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PengoTarot.Data.Divination;

namespace PengoTarot.Powers;

public sealed class TarChariotReversedPower : PowerModel
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.FromPower<VulnerablePower>((int?)null) };

	public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (dealer != ((PowerModel)this).Owner || !target.IsPlayer || result.UnblockedDamage <= 0 || target.Player == null)
		{
			return;
		}
		ICombatState combat = ((PowerModel)this).Owner.CombatState;
		if (combat == null)
		{
			return;
		}
		HashSet<ulong> shared = EliteDivinationSharedState.Chariot(combat);
		if (!shared.Add(target.Player.NetId))
		{
			return;
		}
		((PowerModel)this).Flash();
		await PowerCmd.Apply<VulnerablePower>(choiceContext, target, 1m, ((PowerModel)this).Owner, (CardModel)null, false);
		if (shared.Count < combat.Players.Count)
		{
			return;
		}
		foreach (Creature enemy in combat.Enemies)
		{
			await PowerCmd.Remove<TarChariotReversedPower>(enemy);
		}
	}
}
