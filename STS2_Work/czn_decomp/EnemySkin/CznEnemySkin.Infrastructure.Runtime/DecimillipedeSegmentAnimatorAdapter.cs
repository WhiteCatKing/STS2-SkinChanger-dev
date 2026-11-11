using System;
using System.Linq;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class DecimillipedeSegmentAnimatorAdapter
{
	public static CreatureAnimator? Create(MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		EnemyAnimationBinding enemyAnimationBinding = Binding(profile, EnemyAction.Idle);
		EnemyAnimationBinding enemyAnimationBinding2 = Binding(profile, EnemyAction.Hurt);
		EnemyAnimationBinding enemyAnimationBinding3 = Binding(profile, EnemyAction.Death);
		EnemyAnimationBinding enemyAnimationBinding4 = Binding(profile, EnemyAction.WakeUp);
		string[] array = new string[4] { enemyAnimationBinding.CznAnimationName, enemyAnimationBinding2.CznAnimationName, enemyAnimationBinding3.CznAnimationName, enemyAnimationBinding4.CznAnimationName };
		foreach (string text in array)
		{
			if (!controller.HasAnimation(text))
			{
				return null;
			}
		}
		AnimState val = new AnimState(enemyAnimationBinding.CznAnimationName, true);
		AnimState val2 = new AnimState(enemyAnimationBinding2.CznAnimationName, false);
		AnimState val3 = new AnimState(enemyAnimationBinding3.CznAnimationName, false);
		AnimState val4 = new AnimState(enemyAnimationBinding4.CznAnimationName, false);
		val2.NextState = val;
		val4.NextState = val;
		CreatureAnimator val5 = new CreatureAnimator(val, controller);
		val5.AddAnyState("Revive", val4, (Func<bool>)null);
		val5.AddAnyState("Hit", val2, (Func<bool>)null);
		val5.AddAnyState("Dead", val3, (Func<bool>)null);
		return val5;
	}

	private static EnemyAnimationBinding Binding(EnemyReplacementProfile profile, EnemyAction action)
	{
		return profile.Monster.AnimationBindings.First((EnemyAnimationBinding binding) => binding.Stance == EnemyPresentationStance.Normal && binding.Action == action);
	}
}
