using System;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class BerculaVantomAnimatorAdapter
{
	private sealed class StanceState
	{
		public bool IsCharged = true;
	}

	private static readonly ConditionalWeakTable<Vantom, StanceState> States = new ConditionalWeakTable<Vantom, StanceState>();

	public static CreatureAnimator Create(Vantom vantom, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Expected O, but got Unknown
		StanceState stance = States.GetOrCreateValue(vantom);
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState nextState = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.BasicAttack);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.HeavyAttack);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Cast);
		AnimState val5 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Buff);
		AnimState val6 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Charge);
		AnimState val7 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val8 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Hurt);
		AnimState val9 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		AnimState val10 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Death);
		val2.NextState = nextState;
		val3.NextState = val;
		val4.NextState = nextState;
		val5.NextState = nextState;
		val6.NextState = val;
		val7.NextState = nextState;
		val8.NextState = val;
		CreatureAnimator val11 = new CreatureAnimator(val, controller);
		val11.AddAnyState("Attack", val2, (Func<bool>)null);
		val11.AddAnyState("ATTACK_DOUBLE", val3, (Func<bool>)null);
		val11.AddAnyState("ATTACK_HEAVY", val4, (Func<bool>)null);
		val11.AddAnyState("BUFF", val5, (Func<bool>)null);
		val11.AddAnyState("CHARGE_UP", val6, (Func<bool>)null);
		val11.AddAnyState("Hit", val8, (Func<bool>)(() => stance.IsCharged));
		val11.AddAnyState("Hit", val7, (Func<bool>)(() => !stance.IsCharged));
		val11.AddAnyState("Dead", val10, (Func<bool>)(() => stance.IsCharged));
		val11.AddAnyState("Dead", val9, (Func<bool>)(() => !stance.IsCharged));
		return val11;
	}

	public static void ObserveTrigger(Vantom vantom, string trigger)
	{
		StanceState orCreateValue = States.GetOrCreateValue(vantom);
		if (string.Equals(trigger, "CHARGE_UP", StringComparison.Ordinal))
		{
			orCreateValue.IsCharged = true;
		}
		else if (string.Equals(trigger, "ATTACK_HEAVY", StringComparison.Ordinal))
		{
			orCreateValue.IsCharged = false;
		}
	}
}
