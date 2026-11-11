using System;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class BowlbugRockAnimatorAdapter
{
	public static CreatureAnimator Create(BowlbugRock bowlbug, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.BasicAttack);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Cast);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.StunLoop);
		AnimState val5 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val6 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val7 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		val2.NextState = val;
		val3.NextState = val;
		val5.NextState = val;
		val6.NextState = val4;
		CreatureAnimator val8 = new CreatureAnimator(val, controller);
		val8.AddAnyState("Attack", val2, (Func<bool>)null);
		val8.AddAnyState("Cast", val3, (Func<bool>)null);
		val8.AddAnyState("Stun", val4, (Func<bool>)null);
		val8.AddAnyState("Unstun", val, (Func<bool>)null);
		val8.AddAnyState("Hit", val5, (Func<bool>)(() => !bowlbug.IsOffBalance));
		val8.AddAnyState("Hit", val6, (Func<bool>)(() => bowlbug.IsOffBalance));
		val8.AddAnyState("Dead", val7, (Func<bool>)null);
		return val8;
	}
}
