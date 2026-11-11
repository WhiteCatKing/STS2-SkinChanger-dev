using System;
using System.Collections.Generic;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class SoulNexusAnimatorAdapter
{
	public static CreatureAnimator Create(SoulNexus nexus, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Cast);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		AnimState val5 = AttackChain("normal_job_attack_ready", new _003C_003Ez__ReadOnlySingleElementList<string>("normal_job_attack_play"), "normal_job_attack_end", val);
		AnimState val6 = AttackChain("normal_attack_ready", new _003C_003Ez__ReadOnlySingleElementList<string>("normal_attack_play1"), "normal_attack_end", val);
		RequireAnimations(controller);
		val2.NextState = val;
		val3.NextState = val;
		CreatureAnimator val7 = new CreatureAnimator(val, controller);
		val7.AddAnyState("Attack", val5, (Func<bool>)(() => IsMove(nexus, EnemyAction.BasicAttack)));
		val7.AddAnyState("Attack", val6, (Func<bool>)(() => IsMove(nexus, EnemyAction.HeavyAttack)));
		val7.AddAnyState("Attack", val5, (Func<bool>)null);
		val7.AddAnyState("Cast", val2, (Func<bool>)null);
		val7.AddAnyState("Hit", val3, (Func<bool>)null);
		val7.AddAnyState("Dead", val4, (Func<bool>)null);
		return val7;
	}

	private static bool IsMove(SoulNexus nexus, EnemyAction expected)
	{
		if (Entry.MoveSemantics.TryResolve((MonsterModel)(object)nexus, "Attack", out var action))
		{
			return action == expected;
		}
		return false;
	}

	private static AnimState AttackChain(string ready, IReadOnlyList<string> plays, string end, AnimState idle)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		AnimState val = new AnimState(ready, false);
		AnimState nextState = new AnimState(end, false)
		{
			NextState = idle
		};
		AnimState val2 = val;
		using (IEnumerator<string> enumerator = plays.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				AnimState val4 = (val2.NextState = new AnimState(enumerator.Current, false));
				val2 = val4;
			}
		}
		val2.NextState = nextState;
		return val;
	}

	private static void RequireAnimations(MegaSprite controller)
	{
		string[] array = new string[7] { "normal_attack_ready", "normal_attack_play1", "normal_attack_play2", "normal_attack_end", "normal_job_attack_ready", "normal_job_attack_play", "normal_job_attack_end" };
		foreach (string text in array)
		{
			if (!controller.HasAnimation(text))
			{
				throw new InvalidOperationException("Shy Gardener is missing attack phase '" + text + "'.");
			}
		}
	}
}
