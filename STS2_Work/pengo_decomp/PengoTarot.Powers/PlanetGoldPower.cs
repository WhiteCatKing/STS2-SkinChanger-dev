using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace PengoTarot.Powers;

public sealed class PlanetGoldPower : PowerModel
{
	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)1;

	public override Task AfterCombatEnd(CombatRoom room)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		((PowerModel)this).Flash();
		if (((PowerModel)this).Amount > 0 && ((PowerModel)this).Owner.Player != null)
		{
			room.AddExtraReward(((PowerModel)this).Owner.Player, (Reward)new GoldReward(((PowerModel)this).Amount, ((PowerModel)this).Owner.Player, false));
		}
		return Task.CompletedTask;
	}
}
