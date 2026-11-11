using System;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class LivingShieldAnimatorAdapter
{
	public static CreatureAnimator Create(LivingShield shield, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		AnimState val4 = AttackChain("normal_attack_ready", "normal_attack_play1", "normal_attack_end", val);
		AnimState val5 = AttackChain("normal_attack_ready", "normal_attack_play2", "normal_attack_end", val);
		RequireAnimations(controller);
		val2.NextState = val;
		CreatureAnimator val6 = new CreatureAnimator(val, controller);
		val6.AddAnyState("Attack", val4, (Func<bool>)(() => IsMove(shield, EnemyAction.BasicAttack)));
		val6.AddAnyState("Attack", val5, (Func<bool>)(() => IsMove(shield, EnemyAction.HeavyAttack)));
		val6.AddAnyState("Attack", val4, (Func<bool>)null);
		val6.AddAnyState("Hit", val2, (Func<bool>)null);
		val6.AddAnyState("Dead", val3, (Func<bool>)null);
		return val6;
	}

	private static bool IsMove(LivingShield shield, EnemyAction expected)
	{
		if (Entry.MoveSemantics.TryResolve((MonsterModel)(object)shield, "Attack", out var action))
		{
			return action == expected;
		}
		return false;
	}

	private static AnimState AttackChain(string ready, string play, string end, AnimState idle)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Expected O, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		AnimState val = new AnimState(ready, false);
		AnimState val2 = new AnimState(play, false);
		AnimState nextState = new AnimState(end, false)
		{
			NextState = idle
		};
		val.NextState = val2;
		val2.NextState = nextState;
		return val;
	}

	private static void RequireAnimations(MegaSprite controller)
	{
		string[] array = new string[4] { "normal_attack_ready", "normal_attack_play1", "normal_attack_play2", "normal_attack_end" };
		foreach (string text in array)
		{
			if (!controller.HasAnimation(text))
			{
				throw new InvalidOperationException("Saint Hound is missing attack phase '" + text + "'.");
			}
		}
	}
}
