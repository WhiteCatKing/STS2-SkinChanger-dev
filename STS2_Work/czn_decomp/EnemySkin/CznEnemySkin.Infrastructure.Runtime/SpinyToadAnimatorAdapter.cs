using System;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class SpinyToadAnimatorAdapter
{
	private const float StanceAttackDurationSeconds = 1.8001f;

	public static CreatureAnimator Create(SpinyToad toad, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Idle);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.BasicAttack);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.HeavyAttack);
		AnimState val5 = AttackChain("stance1_attack_ready", "stance1_attack_play1", "stance1_attack_end", val2);
		AnimState val6 = AttackChain("stance1_attack_ready", "stance1_attack_play1", "stance1_attack_end", val4);
		AnimState val7 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Buff);
		AnimState val8 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val9 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Hurt);
		AnimState val10 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		AnimState val11 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Death);
		val3.NextState = val;
		val7.NextState = val2;
		val4.NextState = val;
		val8.NextState = val;
		val9.NextState = val2;
		CreatureAnimator val12 = new CreatureAnimator(val, controller);
		val12.AddAnyState("Spiked", val7, (Func<bool>)null);
		val12.AddAnyState("Unspiked", val6, (Func<bool>)null);
		val12.AddAnyState("Attack", val3, (Func<bool>)(() => !IsSpiny(toad)));
		val12.AddAnyState("Attack", val5, (Func<bool>)(() => IsSpiny(toad)));
		val12.AddAnyState("Hit", val8, (Func<bool>)(() => !IsSpiny(toad)));
		val12.AddAnyState("Hit", val9, (Func<bool>)(() => IsSpiny(toad)));
		val12.AddAnyState("Dead", val10, (Func<bool>)(() => !IsSpiny(toad)));
		val12.AddAnyState("Dead", val11, (Func<bool>)(() => IsSpiny(toad)));
		return val12;
	}

	public static void ObserveTrigger(NCreature creature, SpinyToad toad, string trigger)
	{
		if (trigger != "Unspiked" || !EnemyVisualHostAdapter.TryResolveAppliedProfile(creature, out EnemyReplacementProfile profile) || profile.Id != "evil_dullahan_for_spiny_toad")
		{
			return;
		}
		((Node)creature).GetTree().CreateTimer(1.8000999689102173, false, false, false).Timeout += delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)creature))
			{
				Entry.ActionPresentation.Play(creature, profile, EnemyAction.HeavyAttack);
			}
		};
	}

	public static bool TryMapPresentation(SpinyToad toad, string trigger, out EnemyAction action)
	{
		if (trigger == "Unspiked" || (trigger == "Attack" && IsSpiny(toad)))
		{
			action = EnemyAction.Special;
			return true;
		}
		action = EnemyAction.Idle;
		return false;
	}

	private static AnimState AttackChain(string ready, string play, string end, AnimState next)
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
			NextState = next
		};
		val.NextState = val2;
		val2.NextState = nextState;
		return val;
	}

	private static bool IsSpiny(SpinyToad toad)
	{
		return toad.IsSpiny;
	}
}
