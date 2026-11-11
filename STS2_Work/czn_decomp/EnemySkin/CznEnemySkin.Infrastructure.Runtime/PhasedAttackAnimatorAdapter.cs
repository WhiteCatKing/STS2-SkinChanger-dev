using System;
using System.Linq;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class PhasedAttackAnimatorAdapter
{
	public static CreatureAnimator Create(MegaSprite controller, EnemyReplacementProfile profile, bool includeCast, params string[] attackAnimations)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		if (attackAnimations.Length == 0)
		{
			throw new ArgumentException("At least one attack phase is required.", "attackAnimations");
		}
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		AnimState[] array = attackAnimations.Select((string animation) => CreatePhase(controller, animation)).ToArray();
		for (int i = 0; i < array.Length - 1; i++)
		{
			array[i].NextState = array[i + 1];
		}
		array[^1].NextState = val;
		val2.NextState = val;
		CreatureAnimator val4 = new CreatureAnimator(val, controller);
		val4.AddAnyState("Idle", val, (Func<bool>)null);
		val4.AddAnyState("Attack", array[0], (Func<bool>)null);
		val4.AddAnyState("Hit", val2, (Func<bool>)null);
		val4.AddAnyState("Dead", val3, (Func<bool>)null);
		if (includeCast)
		{
			AnimState val5 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Cast);
			val5.NextState = val;
			val4.AddAnyState("Cast", val5, (Func<bool>)null);
		}
		return val4;
	}

	private static AnimState CreatePhase(MegaSprite controller, string animation)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		if (!controller.HasAnimation(animation))
		{
			throw new InvalidOperationException("Replacement skeleton is missing attack phase '" + animation + "'.");
		}
		return new AnimState(animation, false);
	}
}
