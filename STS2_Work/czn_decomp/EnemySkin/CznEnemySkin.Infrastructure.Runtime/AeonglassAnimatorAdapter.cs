using System;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class AeonglassAnimatorAdapter
{
	public static CreatureAnimator Create(MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Cast);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		AnimState val5 = new AnimState("normal_attack_ready", false);
		AnimState val6 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.BasicAttack);
		AnimState val7 = new AnimState("normal_attack_end", false);
		AnimState val8 = new AnimState("normal_attack_ready", false);
		AnimState val9 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.HeavyAttack);
		AnimState val10 = new AnimState("normal_attack_end", false);
		string[] array = new string[2] { "normal_attack_ready", "normal_attack_end" };
		foreach (string text in array)
		{
			if (!controller.HasAnimation(text))
			{
				throw new InvalidOperationException("Viadius attack chain is missing official phase '" + text + "'.");
			}
		}
		val5.NextState = val6;
		val6.NextState = val7;
		val7.NextState = val;
		val8.NextState = val9;
		val9.NextState = val10;
		val10.NextState = val;
		val2.NextState = val;
		val3.NextState = val;
		CreatureAnimator val11 = new CreatureAnimator(val, controller);
		val11.AddAnyState("Idle", val, (Func<bool>)null);
		val11.AddAnyState("Cast", val2, (Func<bool>)null);
		val11.AddAnyState("AttackHeavy", val5, (Func<bool>)null);
		val11.AddAnyState("AttackDouble", val8, (Func<bool>)null);
		val11.AddAnyState("Dead", val4, (Func<bool>)null);
		val11.AddAnyState("Hit", val3, (Func<bool>)null);
		return val11;
	}
}
