using System;
using System.Linq;
using System.Threading.Tasks;
using CznCore.Presentation;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;

namespace CznCore.Gameplay.Dots;

public static class SharedDotSettlement
{
	public static async Task Resolve(PowerModel power, SharedDotKind kind, Action<string> presentHit, Func<Task> completed)
	{
		await ResolveWithAudio(power, kind, delegate(string route)
		{
			presentHit(route);
			return false;
		}, completed);
	}

	public static async Task ResolveWithAudio(PowerModel power, SharedDotKind kind, Func<string, bool> presentHit, Func<Task> completed)
	{
		if (power.Amount <= 0 || !power.Owner.IsAlive || CombatManager.Instance.IsOverOrEnding)
		{
			return;
		}
		int snapshot = power.Amount;
		bool pain = kind == SharedDotKind.Pain;
		int hits = ((!pain) ? 1 : snapshot);
		for (int i = 0; i < hits; i++)
		{
			if (!power.Owner.IsAlive)
			{
				break;
			}
			if (CombatManager.Instance.IsOverOrEnding)
			{
				break;
			}
			bool replaced = !TestMode.IsOn && presentHit(pain ? "pain" : "rift");
			await SharedDotImpactAudioScope.Run(power.Owner, replaced, () => CreatureCmd.Damage((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), power.Owner, pain ? 1m : (1m * (decimal)snapshot), (ValueProp)(pain ? 4 : 6), (CardModel)null, (CardPlay)null));
		}
		if (power.Owner.IsAlive && power.Owner.Powers.Contains(power))
		{
			await PowerCmd.ModifyAmount((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), power, (decimal)(-Math.Min(power.Amount, snapshot - snapshot / 2)), (Creature)null, (CardModel)null, false);
		}
		await completed();
	}
}
