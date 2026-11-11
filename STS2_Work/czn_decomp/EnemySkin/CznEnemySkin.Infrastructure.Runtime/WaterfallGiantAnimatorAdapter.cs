using System;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Saves;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class WaterfallGiantAnimatorAdapter
{
	private sealed class PresentationState
	{
		public bool IsTerminalSequence { get; set; }

		public bool DeathPresentationScheduled { get; set; }
	}

	internal const float TerminalHitDelaySeconds = 2.25f;

	private const float TerminalAttackDurationSeconds = 4.3333f;

	private const float TerminalPresentationDurationSeconds = 4.3667f;

	private static readonly ConditionalWeakTable<WaterfallGiant, PresentationState> States = new ConditionalWeakTable<WaterfallGiant, PresentationState>();

	public static CreatureAnimator Create(WaterfallGiant giant, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.BasicAttack);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.HeavyAttack);
		AnimState val4 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Buff);
		AnimState val5 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Cast);
		AnimState val6 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Special);
		AnimState val7 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.StunTransition);
		AnimState val8 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val9 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		QueenAnimatorAdapter.RequireAttackPhases(controller, "Golden Mane");
		AnimState val10 = new AnimState("normal_attack_ready", false);
		AnimState val11 = new AnimState("normal_attack_end", false);
		AnimState val12 = new AnimState("normal_attack_ready", false);
		AnimState val13 = new AnimState("normal_attack_end", false);
		val10.NextState = val2;
		val2.NextState = val11;
		val11.NextState = val;
		val12.NextState = val3;
		val3.NextState = val13;
		val13.NextState = val;
		val4.NextState = val;
		val5.NextState = val;
		val8.NextState = val;
		val6.NextState = val9;
		CreatureAnimator val14 = new CreatureAnimator(val, controller);
		val14.AddAnyState("Idle", val, (Func<bool>)null);
		val14.AddAnyState("Cast", val5, (Func<bool>)null);
		val14.AddAnyState("Attack", val10, (Func<bool>)null);
		val14.AddAnyState("AttackBuff", val4, (Func<bool>)null);
		val14.AddAnyState("AttackDebuff", val12, (Func<bool>)null);
		val14.AddAnyState("Heal", val5, (Func<bool>)null);
		val14.AddAnyState("Erupt", val6, (Func<bool>)null);
		val14.AddAnyState("AboutToBlow", val7, (Func<bool>)null);
		val14.AddAnyState("Dead", val9, (Func<bool>)(() => !IsTerminalSequence(giant)));
		val14.AddAnyState("Hit", val8, (Func<bool>)(() => !IsTerminalSequence(giant)));
		return val14;
	}

	public static void MarkTerminalSequence(WaterfallGiant giant)
	{
		States.GetOrCreateValue(giant).IsTerminalSequence = true;
	}

	public static bool IsTerminalSequence(WaterfallGiant giant)
	{
		if (States.TryGetValue(giant, out PresentationState value))
		{
			return value.IsTerminalSequence;
		}
		return false;
	}

	public static float GetRewardDelayOverride()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		if ((int)SaveManager.Instance.PrefsSave.FastMode != 2)
		{
			return 4.3667f;
		}
		return 8.7334f;
	}

	public static bool TryScheduleDeathPresentation(WaterfallGiant giant, NCreature creature, EnemyReplacementProfile profile)
	{
		PresentationState orCreateValue = States.GetOrCreateValue(giant);
		if (!orCreateValue.IsTerminalSequence || orCreateValue.DeathPresentationScheduled)
		{
			return false;
		}
		SceneTree tree = ((Node)creature).GetTree();
		if (tree == null)
		{
			return false;
		}
		orCreateValue.DeathPresentationScheduled = true;
		tree.CreateTimer(4.3333001136779785, false, false, false).Timeout += delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)creature))
			{
				Entry.ActionPresentation.Play(creature, profile, EnemyAction.Death);
			}
		};
		return true;
	}
}
