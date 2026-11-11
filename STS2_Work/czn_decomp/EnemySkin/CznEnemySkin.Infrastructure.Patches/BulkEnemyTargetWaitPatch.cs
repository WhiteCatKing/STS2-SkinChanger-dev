using System.Threading.Tasks;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using CznEnemySkin.Profiles;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(CreatureCmd), "TriggerAnim")]
internal static class BulkEnemyTargetWaitPatch
{
	internal static float MinimumWait(string profileId, string trigger, MonsterModel? monster = null)
	{
		GremlinMerc val = (GremlinMerc)(object)((monster is GremlinMerc) ? monster : null);
		if (val != null && trigger == "AttackDouble" && GremlinMercAnimatorAdapter.IsGimmeMove(val))
		{
			return 0f;
		}
		switch (profileId)
		{
		case "snipper_for_assassin_ruby_raider":
			if (!(trigger == "Attack"))
			{
				break;
			}
			return 0.55f;
		case "unanswered_for_axe_ruby_raider":
			if (!(trigger == "Attack"))
			{
				break;
			}
			return 0.8f;
		case "spawning_insect_for_bowlbug_egg":
			if (!(trigger == "Attack"))
			{
				break;
			}
			return 0.5f;
		case "single_edge_lepturus_for_calcified_cultist":
			if (!(trigger == "Attack"))
			{
				break;
			}
			return 0.5f;
		case "heavy_armor_insect_for_bowlbug_rock":
			if (!(trigger == "Attack"))
			{
				break;
			}
			return 0.5f;
		case "cavalry_automata_for_bygone_effigy":
			if (!(trigger == "Attack"))
			{
				break;
			}
			return 0.03f;
		case "bolt_runner_for_brute_ruby_raider":
			if (!(trigger == "Attack"))
			{
				break;
			}
			return 0.5f;
		case "dust_insect_for_bowlbug_silk":
			if (!(trigger == "Attack"))
			{
				break;
			}
			return 0.5f;
		case "gloomy_beast_for_scroll_of_biting":
			if (!(trigger == "Attack"))
			{
				break;
			}
			return 0.53f;
		case "senectus_for_soul_fysh":
			if (!(trigger == "Attack") && !(trigger == "AttackBeckon"))
			{
				break;
			}
			return 1f;
		case "viadius_for_aeonglass":
			if (!(trigger == "AttackHeavy") && !(trigger == "AttackDouble"))
			{
				break;
			}
			return 0.5347f;
		case "resentful_convict_for_the_insatiable":
			if (!(trigger == "Thrash"))
			{
				if (!(trigger == "Bite"))
				{
					break;
				}
				return 1.4f;
			}
			return 0.7333f;
		case "bercula_for_vantom":
			if (!(trigger == "ATTACK_HEAVY"))
			{
				break;
			}
			return 1.02f;
		}
		return GeneratedSharedHitRoutes.MinimumWait(profileId, trigger, monster);
	}

	internal static float PlaybackWait(string profileId, string trigger, MonsterModel? monster)
	{
		float num = MinimumWait(profileId, trigger, monster);
		if (num > 0f && monster is Crusher && Entry.MoveSemantics.TryResolve(monster, trigger, out var action) && action == EnemyAction.Cast && !Entry.MoveSemantics.TryConsumePreparationWait(monster, trigger))
		{
			return 0f;
		}
		return num;
	}

	private static void Postfix(Creature creature, string triggerName, ref Task __result)
	{
		if (!EnemyVisualHostAdapter.TryResolveAppliedProfile(creature.GetCreatureNode(), out EnemyReplacementProfile profile))
		{
			return;
		}
		float num = PlaybackWait(profile.Id, triggerName, creature.Monster);
		bool flag = profile.Id == "debolessa_for_lagavulin_matriarch";
		if (flag)
		{
			bool flag2 = ((triggerName == "AttackHeavy" || triggerName == "AttackDouble") ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			MonsterModel monster = creature.Monster;
			LagavulinMatriarch val = (LagavulinMatriarch)(object)((monster is LagavulinMatriarch) ? monster : null);
			if (val != null)
			{
				num = (val.IsShellAwake ? 0.6667f : 0.7993f);
			}
		}
		int num2;
		EnemyAction action = default(EnemyAction);
		if (profile.Id == "deathchrome_for_test_subject")
		{
			MonsterModel monster2 = creature.Monster;
			TestSubject val2 = (TestSubject)(object)((monster2 is TestSubject) ? monster2 : null);
			if (val2 != null)
			{
				num2 = (TestSubjectAnimatorAdapter.TryMapPresentation(val2, triggerName, out action) ? 1 : 0);
				goto IL_00b4;
			}
		}
		num2 = 0;
		goto IL_00b4;
		IL_00b4:
		flag = (byte)num2 != 0;
		if (flag)
		{
			bool flag2 = (uint)(action - 4) <= 1u;
			flag = flag2;
		}
		if (flag)
		{
			num = 0.5467f;
		}
		if (num > 0f)
		{
			global::_003C_003Ey__InlineArray2<Task> buffer = default(global::_003C_003Ey__InlineArray2<Task>);
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 0) = __result;
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 1) = Cmd.Wait(num, false);
			__result = Task.WhenAll(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray2<Task>, Task>(in buffer, 2));
		}
	}
}
