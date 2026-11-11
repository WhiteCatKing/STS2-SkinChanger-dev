using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class SlimedBerserkerAnimatorAdapter
{
	private sealed class PhaseState
	{
		public bool IsTransformed { get; set; }

		public bool IsTransitionPending { get; set; }
	}

	internal const string ProfileId = "suppressed_servant_for_slimed_berserker";

	private static readonly ConditionalWeakTable<SlimedBerserker, PhaseState> States = new ConditionalWeakTable<SlimedBerserker, PhaseState>();

	public static CreatureAnimator Create(SlimedBerserker servant, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		PhaseState state = States.GetOrCreateValue(servant);
		state.IsTransformed = state.IsTransformed || IsBelowHalf(servant);
		state.IsTransitionPending = false;
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Idle);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Charge);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Cast);
		AnimState val5 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Buff);
		AnimState val6 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Special);
		AnimState val7 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val8 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Hurt);
		AnimState val9 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		AnimState val10 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Death);
		AnimState val11 = AttackChain("normal_attack_ready", "normal_attack_play1", "normal_attack_end", val);
		AnimState val12 = AttackChain("stance1_attack_ready", "stance1_attack_play1", "stance1_attack_end", val2);
		RequireAnimations(controller);
		val3.NextState = val2;
		val4.NextState = val;
		val5.NextState = val;
		val6.NextState = val2;
		val7.NextState = val;
		val8.NextState = val2;
		CreatureAnimator val13 = new CreatureAnimator(state.IsTransformed ? val2 : val, controller);
		val13.AddAnyState("PhaseTwo", val3, (Func<bool>)null);
		val13.AddAnyState("Hit", val8, (Func<bool>)(() => state.IsTransformed));
		val13.AddAnyState("Hit", val7, (Func<bool>)null);
		val13.AddAnyState("Attack", val12, (Func<bool>)(() => state.IsTransformed));
		val13.AddAnyState("Attack", val11, (Func<bool>)null);
		val13.AddAnyState("Hug", val6, (Func<bool>)(() => state.IsTransformed));
		val13.AddAnyState("Hug", val4, (Func<bool>)null);
		val13.AddAnyState("Vomit", val6, (Func<bool>)(() => state.IsTransformed));
		val13.AddAnyState("Vomit", val5, (Func<bool>)null);
		val13.AddAnyState("Dead", val10, (Func<bool>)(() => state.IsTransformed));
		val13.AddAnyState("Dead", val9, (Func<bool>)null);
		return val13;
	}

	public static bool TryMapPresentation(SlimedBerserker servant, string trigger, out EnemyAction action)
	{
		PhaseState value;
		bool flag = States.TryGetValue(servant, out value) && value.IsTransformed;
		EnemyAction enemyAction;
		switch (trigger)
		{
		case "PhaseTwo":
			enemyAction = EnemyAction.Charge;
			break;
		case "Attack":
			if (flag)
			{
				enemyAction = EnemyAction.HeavyAttack;
				break;
			}
			goto default;
		case "Hug":
		case "Vomit":
			if (flag)
			{
				enemyAction = EnemyAction.Special;
				break;
			}
			goto default;
		default:
			enemyAction = EnemyAction.Idle;
			break;
		}
		action = enemyAction;
		bool flag2 = trigger == "PhaseTwo";
		if (!flag2)
		{
			bool flag3 = flag;
			if (flag3)
			{
				bool flag4;
				switch (trigger)
				{
				case "Attack":
				case "Hug":
				case "Vomit":
					flag4 = true;
					break;
				default:
					flag4 = false;
					break;
				}
				flag3 = flag4;
			}
			flag2 = flag3;
		}
		return flag2;
	}

	public static bool TryHandleCommittedTrigger(NCreature creature, SlimedBerserker servant, string trigger)
	{
		try
		{
			if (!EnemyVisualHostAdapter.TryResolveAppliedProfile(creature, out EnemyReplacementProfile profile) || profile.Id != "suppressed_servant_for_slimed_berserker")
			{
				return false;
			}
			PhaseState orCreateValue = States.GetOrCreateValue(servant);
			if (trigger == "Hit" && orCreateValue.IsTransitionPending)
			{
				CompletePhaseTwo(creature, servant, orCreateValue);
				return true;
			}
			if (!orCreateValue.IsTransformed)
			{
				return false;
			}
			MegaSprite spineBody = creature.Visuals.SpineBody;
			MegaAnimationState val = ((spineBody != null) ? spineBody.TryGetAnimationState() : null);
			if (val == null)
			{
				return false;
			}
			string value;
			switch (trigger)
			{
			case "PhaseTwo":
				value = "normal_to_stance1";
				PlaySequence(val, new _003C_003Ez__ReadOnlySingleElementList<string>("normal_to_stance1"), "stance1_idle");
				break;
			case "Attack":
				value = "stance1_attack_ready";
				PlaySequence(val, new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "stance1_attack_ready", "stance1_attack_play1", "stance1_attack_end" }), "stance1_idle");
				break;
			case "Hug":
			case "Vomit":
				value = "stance1_buff";
				PlaySequence(val, new _003C_003Ez__ReadOnlySingleElementList<string>("stance1_buff"), "stance1_idle");
				break;
			case "Hit":
				value = "stance1_hit";
				PlaySequence(val, new _003C_003Ez__ReadOnlySingleElementList<string>("stance1_hit"), "stance1_idle");
				break;
			case "Dead":
				value = "stance1_death";
				PlaySequence(val, new _003C_003Ez__ReadOnlySingleElementList<string>("stance1_death"), null);
				break;
			case "Idle":
				value = "stance1_idle";
				val.SetAnimation("stance1_idle", true, 0);
				break;
			default:
				return false;
			}
			Entry.Log.Info("Oppressed Servant routed committed phase trigger " + $"[trigger={trigger}, animation={value}].", 1);
			return true;
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Oppressed Servant phase trigger routing failed; using animator graph [trigger=" + trigger + ", error=" + ex.Message + "].", 1);
			return false;
		}
	}

	public static bool TryEnterPhaseTwo(SlimedBerserker servant, bool expectHurtTrigger)
	{
		NCreature creatureNode = ((MonsterModel)servant).Creature.GetCreatureNode();
		if (creatureNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) || !EnemyVisualHostAdapter.TryResolveAppliedProfile(creatureNode, out EnemyReplacementProfile profile) || profile.Id != "suppressed_servant_for_slimed_berserker" || !IsBelowHalf(servant))
		{
			return false;
		}
		PhaseState orCreateValue = States.GetOrCreateValue(servant);
		if (orCreateValue.IsTransformed || orCreateValue.IsTransitionPending)
		{
			return false;
		}
		orCreateValue.IsTransitionPending = true;
		if (!expectHurtTrigger)
		{
			CompletePhaseTwo(creatureNode, servant, orCreateValue);
		}
		return true;
	}

	private static void CompletePhaseTwo(NCreature creature, SlimedBerserker servant, PhaseState state)
	{
		state.IsTransitionPending = false;
		if (GodotObject.IsInstanceValid((GodotObject)(object)creature) && !state.IsTransformed && IsBelowHalf(servant))
		{
			state.IsTransformed = true;
			Entry.Log.Info("Oppressed Servant entered phase two " + $"[hp={((MonsterModel)servant).Creature.CurrentHp}/" + $"{((MonsterModel)servant).Creature.MaxHp}].", 1);
			creature.SetAnimationTrigger("PhaseTwo");
		}
	}

	private static void PlaySequence(MegaAnimationState state, IReadOnlyList<string> animations, string? loopAfter)
	{
		state.SetAnimation(animations[0], false, 0);
		for (int i = 1; i < animations.Count; i++)
		{
			state.AddAnimation(animations[i], 0f, false, 0);
		}
		if (loopAfter != null)
		{
			state.AddAnimation(loopAfter, 0f, true, 0);
		}
	}

	private static bool IsBelowHalf(SlimedBerserker servant)
	{
		if (((MonsterModel)servant).Creature.IsAlive)
		{
			return ((MonsterModel)servant).Creature.CurrentHp * 2 <= ((MonsterModel)servant).Creature.MaxHp;
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
		string[] array = new string[11]
		{
			"normal_attack_ready", "normal_attack_play1", "normal_attack_end", "normal_to_stance1", "stance1_attack_ready", "stance1_attack_play1", "stance1_attack_end", "stance1_buff", "stance1_idle", "stance1_hit",
			"stance1_death"
		};
		foreach (string text in array)
		{
			if (!controller.HasAnimation(text))
			{
				throw new InvalidOperationException("Oppressed Servant is missing phase animation '" + text + "'.");
			}
		}
	}
}
