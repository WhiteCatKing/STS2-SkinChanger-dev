using System;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class TheInsatiableAnimatorAdapter
{
	public static CreatureAnimator Create(MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.BasicAttack);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.HeavyAttack);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Buff);
		AnimState val5 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Cast);
		AnimState val6 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Special);
		AnimState val7 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val8 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		QueenAnimatorAdapter.RequireAttackPhases(controller, "Resentful Convict");
		AnimState val9 = new AnimState("normal_attack_ready", false);
		AnimState val10 = new AnimState("normal_attack_end", false);
		val9.NextState = val2;
		val2.NextState = val10;
		val10.NextState = val;
		val3.NextState = val;
		val4.NextState = val;
		val5.NextState = val;
		val6.NextState = val;
		val7.NextState = val;
		CreatureAnimator val11 = new CreatureAnimator(val, controller);
		val11.AddAnyState("Cast", val4, (Func<bool>)null);
		val11.AddAnyState("Attack", val9, (Func<bool>)null);
		val11.AddAnyState("LiquifySand", val5, (Func<bool>)null);
		val11.AddAnyState("Salivate", val4, (Func<bool>)null);
		val11.AddAnyState("Thrash", val9, (Func<bool>)null);
		val11.AddAnyState("Bite", val3, (Func<bool>)null);
		val11.AddAnyState("EatPlayerTrigger", val6, (Func<bool>)null);
		val11.AddAnyState("Hit", val7, (Func<bool>)null);
		val11.AddAnyState("Dead", val8, (Func<bool>)null);
		return val11;
	}
}
