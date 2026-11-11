using System;
using System.Collections.Generic;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class LagavulinMatriarchAnimatorAdapter
{
	internal const string ProfileId = "debolessa_for_lagavulin_matriarch";

	public static CreatureAnimator Create(LagavulinMatriarch matriarch, MegaSprite controller)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Expected O, but got Unknown
		RequireAnimations(controller);
		AnimState val = new AnimState("stance2_idle", true);
		AnimState val2 = new AnimState("stance1_hit", false);
		AnimState val3 = new AnimState("stance1_to_normal", false);
		AnimState val4 = new AnimState("normal_idle", true);
		AnimState val5 = new AnimState("normal_hit", false);
		AnimState val6 = new AnimState("normal_debuff_1", false);
		AnimState val7 = new AnimState("normal_to_stance1", false);
		AnimState val8 = new AnimState("stance1_idle", true);
		AnimState val9 = new AnimState("stance1_hit", false);
		AnimState val10 = new AnimState("normal_debuff_1", false);
		AnimState val11 = new AnimState("normal_death", false);
		AnimState val12 = AttackChain("normal_attack_ready", new _003C_003Ez__ReadOnlySingleElementList<string>("normal_attack_play1"), "normal_attack_end", val4);
		AnimState val13 = AttackChain("normal_attack_ready", new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "normal_attack_play1", "normal_attack_play2" }), "normal_attack_end", val4);
		AnimState val14 = AttackChain("stance1_attack_ready", new _003C_003Ez__ReadOnlySingleElementList<string>("stance1_attack_play1"), "stance1_attack_end", val8);
		AnimState val15 = AttackChain("stance1_attack_ready", new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "stance1_attack_play1", "stance1_attack_play2" }), "stance1_attack_end", val8);
		val2.NextState = val3;
		val3.NextState = val4;
		val5.NextState = val4;
		val6.NextState = val4;
		val7.NextState = val8;
		val9.NextState = val8;
		val10.NextState = val8;
		CreatureAnimator val16 = new CreatureAnimator(matriarch.IsShellAwake ? val8 : val4, controller);
		val16.AddAnyState("Sleep", val, (Func<bool>)null);
		val16.AddAnyState("Wake", val3, (Func<bool>)(() => !matriarch.IsAwake && !matriarch.IsShellAwake));
		val16.AddAnyState("Wake", val8, (Func<bool>)(() => !matriarch.IsAwake && matriarch.IsShellAwake));
		val16.AddAnyState("PhaseTwo", val7, (Func<bool>)(() => matriarch.IsShellAwake));
		val16.AddAnyState("Cast", val6, (Func<bool>)(() => !matriarch.IsShellAwake));
		val16.AddAnyState("Cast", val10, (Func<bool>)(() => matriarch.IsShellAwake));
		val16.AddAnyState("AttackHeavy", val12, (Func<bool>)(() => !matriarch.IsShellAwake));
		val16.AddAnyState("AttackHeavy", val14, (Func<bool>)(() => matriarch.IsShellAwake));
		val16.AddAnyState("AttackDouble", val13, (Func<bool>)(() => !matriarch.IsShellAwake));
		val16.AddAnyState("AttackDouble", val15, (Func<bool>)(() => matriarch.IsShellAwake));
		val16.AddAnyState("Dead", val11, (Func<bool>)null);
		val16.AddAnyState("Hit", val5, (Func<bool>)(() => matriarch.IsAwake && !matriarch.IsShellAwake));
		val16.AddAnyState("Hit", val9, (Func<bool>)(() => matriarch.IsAwake && matriarch.IsShellAwake));
		val16.AddAnyState("Hit", val2, (Func<bool>)(() => !matriarch.IsAwake));
		return val16;
	}

	public static bool TryBeginPhaseTwo(LagavulinMatriarch matriarch)
	{
		NCreature creatureNode = ((MonsterModel)matriarch).Creature.GetCreatureNode();
		if (!matriarch.IsShellAwake || creatureNode == null || !EnemyVisualHostAdapter.TryResolveAppliedProfile(creatureNode, out EnemyReplacementProfile profile) || profile.Id != "debolessa_for_lagavulin_matriarch")
		{
			return false;
		}
		creatureNode.SetAnimationTrigger("PhaseTwo");
		return true;
	}

	public static bool TryMapPresentation(LagavulinMatriarch matriarch, string trigger, out EnemyAction action)
	{
		action = trigger switch
		{
			"PhaseTwo" => EnemyAction.Buff, 
			"AttackHeavy" => (!matriarch.IsShellAwake) ? EnemyAction.BasicAttack : EnemyAction.Special, 
			"AttackDouble" => (!matriarch.IsShellAwake) ? EnemyAction.HeavyAttack : EnemyAction.Charge, 
			_ => EnemyAction.Idle, 
		};
		switch (trigger)
		{
		case "PhaseTwo":
		case "AttackHeavy":
		case "AttackDouble":
			return true;
		default:
			return false;
		}
	}

	private static AnimState AttackChain(string readyAnimation, IReadOnlyList<string> playAnimations, string endAnimation, AnimState idle)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		AnimState val = new AnimState(readyAnimation, false);
		AnimState val2 = val;
		using (IEnumerator<string> enumerator = playAnimations.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				AnimState val4 = (val2.NextState = new AnimState(enumerator.Current, false));
				val2 = val4;
			}
		}
		AnimState nextState = new AnimState(endAnimation, false)
		{
			NextState = idle
		};
		val2.NextState = nextState;
		return val;
	}

	private static void RequireAnimations(MegaSprite controller)
	{
		string[] array = new string[17]
		{
			"stance2_idle", "stance1_hit", "stance1_to_normal", "normal_idle", "normal_hit", "normal_debuff_1", "normal_attack_ready", "normal_attack_play1", "normal_attack_play2", "normal_attack_end",
			"normal_to_stance1", "stance1_idle", "stance1_attack_ready", "stance1_attack_play1", "stance1_attack_play2", "stance1_attack_end", "normal_death"
		};
		foreach (string text in array)
		{
			if (!controller.HasAnimation(text))
			{
				throw new InvalidOperationException("Debolessa is missing animation '" + text + "'.");
			}
		}
	}
}
