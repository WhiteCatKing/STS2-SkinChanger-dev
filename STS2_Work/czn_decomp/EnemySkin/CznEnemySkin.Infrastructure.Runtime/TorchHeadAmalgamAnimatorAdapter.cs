using System;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class TorchHeadAmalgamAnimatorAdapter
{
	public static CreatureAnimator Create(MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Cast);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		AnimState val5 = new AnimState("normal_attack_ready", false);
		AnimState val6 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.BasicAttack);
		AnimState val7 = new AnimState("normal_attack_end", false);
		QueenAnimatorAdapter.RequireAttackPhases(controller, "Rusty Nail Seven Arms");
		val5.NextState = val6;
		val6.NextState = val7;
		val7.NextState = val;
		val2.NextState = val;
		val3.NextState = val;
		CreatureAnimator val8 = new CreatureAnimator(val, controller);
		val8.AddAnyState("DebuffTrigger", val2, (Func<bool>)null);
		val8.AddAnyState("Attack", val5, (Func<bool>)null);
		val8.AddAnyState("Dead", val4, (Func<bool>)null);
		val8.AddAnyState("Hit", val3, (Func<bool>)null);
		return val8;
	}
}
