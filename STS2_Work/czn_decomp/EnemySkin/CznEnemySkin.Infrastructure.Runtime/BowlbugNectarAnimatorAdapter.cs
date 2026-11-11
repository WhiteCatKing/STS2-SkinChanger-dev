using System;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class BowlbugNectarAnimatorAdapter
{
	private sealed class PresentationState
	{
		public bool IsEmpowered { get; set; }
	}

	private static readonly ConditionalWeakTable<BowlbugNectar, PresentationState> States = new ConditionalWeakTable<BowlbugNectar, PresentationState>();

	public static CreatureAnimator Create(BowlbugNectar bowlbug, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState nextState = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.BasicAttack);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Special);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Buff);
		AnimState val5 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val6 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Hurt);
		AnimState val7 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		AnimState val8 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Death);
		val2.NextState = val;
		val3.NextState = nextState;
		val4.NextState = nextState;
		val5.NextState = val;
		val6.NextState = nextState;
		CreatureAnimator val9 = new CreatureAnimator(val, controller);
		val9.AddAnyState("Attack", val2, (Func<bool>)(() => !IsEmpowered(bowlbug)));
		val9.AddAnyState("Attack", val3, (Func<bool>)(() => IsEmpowered(bowlbug)));
		val9.AddAnyState("Buff", val4, (Func<bool>)null);
		val9.AddAnyState("Hit", val5, (Func<bool>)(() => !IsEmpowered(bowlbug)));
		val9.AddAnyState("Hit", val6, (Func<bool>)(() => IsEmpowered(bowlbug)));
		val9.AddAnyState("Dead", val7, (Func<bool>)(() => !IsEmpowered(bowlbug)));
		val9.AddAnyState("Dead", val8, (Func<bool>)(() => IsEmpowered(bowlbug)));
		return val9;
	}

	public static void ObserveTrigger(BowlbugNectar bowlbug, string trigger)
	{
		if (trigger == "Buff")
		{
			States.GetOrCreateValue(bowlbug).IsEmpowered = true;
		}
	}

	public static bool TryMapPresentation(BowlbugNectar bowlbug, string trigger, out EnemyAction action)
	{
		if (trigger == "Attack" && IsEmpowered(bowlbug))
		{
			action = EnemyAction.Special;
			return true;
		}
		action = EnemyAction.Idle;
		return false;
	}

	private static bool IsEmpowered(BowlbugNectar bowlbug)
	{
		if (States.TryGetValue(bowlbug, out PresentationState value))
		{
			return value.IsEmpowered;
		}
		return false;
	}
}
