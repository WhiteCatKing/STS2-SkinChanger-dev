using System;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class TestSubjectAnimatorAdapter
{
	internal const string ProfileId = "deathchrome_for_test_subject";

	private const float EntranceDurationSeconds = 2.6667f;

	public static CreatureAnimator Create(TestSubject subject, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Expected O, but got Unknown
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Expected O, but got Unknown
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Expected O, but got Unknown
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected O, but got Unknown
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Expected O, but got Unknown
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Expected O, but got Unknown
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Expected O, but got Unknown
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Expected O, but got Unknown
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Expected O, but got Unknown
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Expected O, but got Unknown
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Expected O, but got Unknown
		RequireAnimations(controller);
		AnimState val = new AnimState("stance1_idle", true);
		AnimState val2 = new AnimState("stance1_hit", false)
		{
			NextState = val
		};
		AnimState val3 = new AnimState("stance1_attack_ready", false);
		AnimState val4 = new AnimState("stance1_attack_play2", false);
		AnimState nextState = new AnimState("stance1_attack_end", false)
		{
			NextState = val
		};
		val3.NextState = val4;
		val4.NextState = nextState;
		AnimState nextState2 = new AnimState("stance2_idle", true);
		AnimState val5 = new AnimState("stance1_to_stance2", false)
		{
			NextState = nextState2
		};
		AnimState val6 = new AnimState("stance3_idle", true);
		AnimState val7 = new AnimState("stance2_to_stance3", false)
		{
			NextState = val6
		};
		AnimState val8 = new AnimState("stance3_hit", false)
		{
			NextState = val6
		};
		AnimState val9 = new AnimState("stance3_attack_ready", false);
		AnimState val10 = new AnimState("stance3_attack_play1", false);
		AnimState nextState3 = new AnimState("stance3_attack_end", false)
		{
			NextState = val6
		};
		val9.NextState = val10;
		val10.NextState = nextState3;
		AnimState nextState4 = new AnimState("stance4_idle", true);
		AnimState val11 = new AnimState("stance3_to_stance4", false)
		{
			NextState = nextState4
		};
		AnimState nextState5 = new AnimState("stance5_idle", true);
		AnimState val12 = new AnimState("stance4_to_stance5", false)
		{
			NextState = nextState5
		};
		AnimState val13 = new AnimState("stance5_hit", false)
		{
			NextState = nextState5
		};
		AnimState val14 = new AnimState("stance5_attack_ready", false);
		AnimState val15 = new AnimState("stance5_attack_play1", false);
		AnimState nextState6 = new AnimState("stance5_attack_end", false)
		{
			NextState = nextState5
		};
		val14.NextState = val15;
		val15.NextState = nextState6;
		AnimState val16 = new AnimState("stance5_attack_ready", false);
		AnimState val17 = new AnimState("stance5_attack_play2", false);
		AnimState nextState7 = new AnimState("stance5_attack_end", false)
		{
			NextState = nextState5
		};
		val16.NextState = val17;
		val17.NextState = nextState7;
		AnimState val18 = new AnimState("stance5_defense_1", false)
		{
			NextState = nextState5
		};
		AnimState val19 = new AnimState("stance5_death", false);
		CreatureAnimator val20 = new CreatureAnimator(val, controller);
		val20.AddAnyState("Hit", val2, (Func<bool>)(() => Phase(subject) == 0));
		val20.AddAnyState("BiteTrigger", val3, (Func<bool>)(() => Phase(subject) == 0));
		val20.AddAnyState("GrowthSpurtTrigger", val, (Func<bool>)(() => Phase(subject) == 0));
		val20.AddAnyState("DeadTrigger", val5, (Func<bool>)(() => Phase(subject) == 0));
		val20.AddAnyState("RespawnTrigger", val7, (Func<bool>)(() => Phase(subject) == 0));
		val20.AddAnyState("Hit", val8, (Func<bool>)(() => Phase(subject) == 1));
		val20.AddAnyState("MultiAttackTrigger", val9, (Func<bool>)(() => Phase(subject) == 1));
		val20.AddAnyState("GrowthSpurtTrigger", val6, (Func<bool>)(() => Phase(subject) == 1));
		val20.AddAnyState("DeadTrigger", val11, (Func<bool>)(() => Phase(subject) == 1));
		val20.AddAnyState("RespawnTrigger", val12, (Func<bool>)(() => Phase(subject) == 1));
		val20.AddAnyState("Hit", val13, (Func<bool>)(() => Phase(subject) == 2));
		val20.AddAnyState("MultiAttackTrigger", val14, (Func<bool>)(() => Phase(subject) == 2));
		val20.AddAnyState("BiteTrigger", val16, (Func<bool>)(() => Phase(subject) == 2));
		val20.AddAnyState("GrowthSpurtTrigger", val18, (Func<bool>)(() => Phase(subject) == 2));
		val20.AddAnyState("BurnTrigger", val18, (Func<bool>)(() => Phase(subject) == 2));
		val20.AddAnyState("Dead", val19, (Func<bool>)null);
		return val20;
	}

	public static bool TryMapPresentation(TestSubject subject, string trigger, out EnemyAction action)
	{
		int num = Phase(subject);
		EnemyAction enemyAction;
		switch (trigger)
		{
		case "BiteTrigger":
			if (num != 0)
			{
				if (num != 2)
				{
					goto default;
				}
				enemyAction = EnemyAction.Special;
				break;
			}
			enemyAction = EnemyAction.BasicAttack;
			break;
		case "MultiAttackTrigger":
			if (num != 1)
			{
				if (num != 2)
				{
					goto default;
				}
				enemyAction = EnemyAction.Cast;
				break;
			}
			enemyAction = EnemyAction.HeavyAttack;
			break;
		case "BurnTrigger":
			if (num != 2)
			{
				goto default;
			}
			enemyAction = EnemyAction.Buff;
			break;
		case "DeadTrigger":
			if (num != 0)
			{
				if (num != 1)
				{
					goto default;
				}
				enemyAction = EnemyAction.ChargeDeath;
				break;
			}
			enemyAction = EnemyAction.StunTransition;
			break;
		case "RespawnTrigger":
			if (num != 0)
			{
				if (num != 1)
				{
					goto default;
				}
				enemyAction = EnemyAction.ChargeRecovery;
				break;
			}
			enemyAction = EnemyAction.WakeUp;
			break;
		default:
			enemyAction = EnemyAction.Idle;
			break;
		}
		action = enemyAction;
		switch (trigger)
		{
		case "BiteTrigger":
			if (num != 0)
			{
				goto case "BurnTrigger";
			}
			goto IL_00f7;
		case "MultiAttackTrigger":
			if (num != 1)
			{
				goto case "BurnTrigger";
			}
			goto IL_00f7;
		case "BurnTrigger":
			if (num != 2)
			{
				break;
			}
			goto IL_00f7;
		case "DeadTrigger":
		case "RespawnTrigger":
			{
				if ((uint)num > 1u)
				{
					break;
				}
				goto IL_00f7;
			}
			IL_00f7:
			return true;
		}
		return false;
	}

	public static void ObserveBossMusicPhase(TestSubject subject, string trigger)
	{
		if (!(trigger != "RespawnTrigger"))
		{
			int num = Phase(subject);
			if (num < 2)
			{
				Entry.RunMusic.SetBossMusicPhase(num + 1);
			}
		}
	}

	public static bool TryPlayEntrance(NCreature creature, EnemyReplacementProfile profile)
	{
		MegaSprite spineBody = creature.Visuals.SpineBody;
		MegaAnimationState val = ((spineBody != null) ? spineBody.TryGetAnimationState() : null);
		SceneTree tree = ((Node)creature).GetTree();
		if (val == null || tree == null)
		{
			return false;
		}
		val.SetAnimation("normal_enter", false, 0);
		val.AddAnimation("normal_to_stance1", 0f, false, 0);
		val.AddAnimation("stance1_idle", 0f, true, 0);
		Entry.ActionPresentation.Play(creature, profile, EnemyAction.Entrance);
		tree.CreateTimer(2.6666998863220215, false, false, false).Timeout += delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)creature))
			{
				Entry.ActionPresentation.Play(creature, profile, EnemyAction.Charge);
			}
		};
		return true;
	}

	private static int Phase(TestSubject subject)
	{
		if (((MonsterModel)subject).Creature.HasPower<NemesisPower>() && !((MonsterModel)subject).Creature.HasPower<AdaptablePower>())
		{
			return 2;
		}
		return ((MonsterModel)subject).Creature.HasPower<PainfulStabsPower>() ? 1 : 0;
	}

	private static void RequireAnimations(MegaSprite controller)
	{
		string[] array = new string[25]
		{
			"stance1_idle", "stance1_hit", "stance1_attack_ready", "stance1_attack_play1", "stance1_attack_play2", "stance1_attack_end", "stance1_to_stance2", "stance2_idle", "stance2_to_stance3", "stance3_idle",
			"stance3_hit", "stance3_attack_ready", "stance3_attack_play1", "stance3_attack_end", "stance3_to_stance4", "stance4_idle", "stance4_to_stance5", "stance5_idle", "stance5_hit", "stance5_attack_ready",
			"stance5_attack_play1", "stance5_attack_play2", "stance5_attack_end", "stance5_defense_1", "stance5_death"
		};
		foreach (string text in array)
		{
			if (!controller.HasAnimation(text))
			{
				throw new InvalidOperationException("Deathchrome is missing animation '" + text + "'.");
			}
		}
	}
}
