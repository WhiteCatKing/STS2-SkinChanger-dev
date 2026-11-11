using System;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class KnowledgeDemonAnimatorAdapter
{
	public static CreatureAnimator Create(KnowledgeDemon demon, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState nextState = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Cast);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Buff);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val5 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Hurt);
		AnimState val6 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		AnimState val7 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Death);
		AnimState val8 = new AnimState("normal_attack_ready", false);
		AnimState val9 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.BasicAttack);
		AnimState val10 = new AnimState("normal_attack_end", false);
		AnimState val11 = new AnimState("normal_job_attack_ready", false);
		AnimState val12 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.HeavyAttack);
		AnimState val13 = new AnimState("normal_job_attack_end", false);
		string[] array = new string[4] { "normal_attack_ready", "normal_attack_end", "normal_job_attack_ready", "normal_job_attack_end" };
		foreach (string text in array)
		{
			if (!controller.HasAnimation(text))
			{
				throw new InvalidOperationException("Howling Prodigal attack chain is missing official phase '" + text + "'.");
			}
		}
		val8.NextState = val9;
		val9.NextState = val10;
		val10.NextState = val;
		val11.NextState = val12;
		val12.NextState = val13;
		val13.NextState = nextState;
		val2.NextState = val;
		val3.NextState = val;
		val4.NextState = val;
		val5.NextState = nextState;
		CreatureAnimator val14 = new CreatureAnimator(val, controller);
		val14.AddAnyState("LightAttackTrigger", val8, (Func<bool>)null);
		val14.AddAnyState("MediumAttackTrigger", val8, (Func<bool>)null);
		val14.AddAnyState("HeavyAttackTrigger", val11, (Func<bool>)null);
		val14.AddAnyState("MindRotTrigger", val2, (Func<bool>)null);
		val14.AddAnyState("HealTrigger", val3, (Func<bool>)null);
		val14.AddAnyState("Dead", val6, (Func<bool>)(() => !demon.IsBurnt));
		val14.AddAnyState("Hit", val4, (Func<bool>)(() => !demon.IsBurnt));
		val14.AddAnyState("Dead", val7, (Func<bool>)(() => demon.IsBurnt));
		val14.AddAnyState("Hit", val5, (Func<bool>)(() => demon.IsBurnt));
		return val14;
	}
}
