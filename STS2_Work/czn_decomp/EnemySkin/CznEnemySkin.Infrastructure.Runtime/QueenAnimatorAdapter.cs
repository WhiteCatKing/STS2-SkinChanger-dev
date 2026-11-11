using System;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class QueenAnimatorAdapter
{
	private sealed class PhaseState
	{
		public bool IsTransformed { get; set; }
	}

	internal const string ProfileId = "shadow_collector_for_queen";

	private static readonly ConditionalWeakTable<Queen, PhaseState> States = new ConditionalWeakTable<Queen, PhaseState>();

	public static CreatureAnimator Create(Queen queen, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		PhaseState state = States.GetOrCreateValue(queen);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Idle);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Charge);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Cast);
		AnimState val5 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Cast);
		AnimState val6 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val7 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Hurt);
		AnimState val8 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		AnimState val9 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Death);
		AnimState val10 = AttackChain("normal_attack_ready", "normal_attack_play1", "normal_attack_end", val);
		AnimState val11 = AttackChain("stance1_attack_ready", "stance1_attack_play1", "stance1_attack_end", val2);
		AnimState val12 = AttackChain("stance1_attack_ready", "stance1_attack_play2", "stance1_attack_end", val2);
		RequireAnimations(controller);
		val3.NextState = val2;
		val4.NextState = val;
		val5.NextState = val2;
		val6.NextState = val;
		val7.NextState = val2;
		CreatureAnimator val13 = new CreatureAnimator(state.IsTransformed ? val2 : val, controller);
		val13.AddAnyState("PhaseTwo", val3, (Func<bool>)null);
		val13.AddAnyState("Cast", val5, (Func<bool>)(() => state.IsTransformed));
		val13.AddAnyState("Cast", val4, (Func<bool>)null);
		val13.AddAnyState("Attack", val12, (Func<bool>)(() => state.IsTransformed && IsMove(queen, EnemyAction.Special)));
		val13.AddAnyState("Attack", val11, (Func<bool>)(() => state.IsTransformed));
		val13.AddAnyState("Attack", val10, (Func<bool>)null);
		val13.AddAnyState("Hit", val7, (Func<bool>)(() => state.IsTransformed));
		val13.AddAnyState("Hit", val6, (Func<bool>)null);
		val13.AddAnyState("Dead", val9, (Func<bool>)(() => state.IsTransformed));
		val13.AddAnyState("Dead", val8, (Func<bool>)null);
		return val13;
	}

	public static bool TryEnterPhaseTwo(Queen queen)
	{
		NCreature creatureNode = ((MonsterModel)queen).Creature.GetCreatureNode();
		if (creatureNode == null || !EnemyVisualHostAdapter.TryResolveAppliedProfile(creatureNode, out EnemyReplacementProfile profile) || profile.Id != "shadow_collector_for_queen")
		{
			return false;
		}
		PhaseState orCreateValue = States.GetOrCreateValue(queen);
		if (orCreateValue.IsTransformed)
		{
			return false;
		}
		orCreateValue.IsTransformed = true;
		creatureNode.SetAnimationTrigger("PhaseTwo");
		return true;
	}

	public static bool TryMapPresentation(Queen queen, string trigger, out EnemyAction action)
	{
		if (trigger == "PhaseTwo")
		{
			action = EnemyAction.Charge;
			return true;
		}
		PhaseState value;
		bool flag = States.TryGetValue(queen, out value) && value.IsTransformed;
		if (trigger == "Attack" && flag)
		{
			action = (IsMove(queen, EnemyAction.Special) ? EnemyAction.Special : EnemyAction.HeavyAttack);
			return true;
		}
		action = EnemyAction.Idle;
		return false;
	}

	private static bool IsMove(Queen queen, EnemyAction expected)
	{
		if (Entry.MoveSemantics.TryResolve((MonsterModel)(object)queen, "Attack", out var action))
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
		RequireAttackPhases(controller, "Shadow Collector");
		string[] array = new string[8] { "normal_to_stance1", "stance1_idle", "stance1_attack_ready", "stance1_attack_play1", "stance1_attack_play2", "stance1_attack_end", "stance1_hit", "stance1_death" };
		foreach (string text in array)
		{
			if (!controller.HasAnimation(text))
			{
				throw new InvalidOperationException("Shadow Collector is missing phase animation '" + text + "'.");
			}
		}
	}

	internal static void RequireAttackPhases(MegaSprite controller, string replacementName)
	{
		string[] array = new string[2] { "normal_attack_ready", "normal_attack_end" };
		foreach (string text in array)
		{
			if (!controller.HasAnimation(text))
			{
				throw new InvalidOperationException($"{replacementName} attack chain is missing official phase '{text}'.");
			}
		}
	}
}
