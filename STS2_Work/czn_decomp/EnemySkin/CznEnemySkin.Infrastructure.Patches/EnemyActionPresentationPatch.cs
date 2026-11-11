using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NCreature), "SetAnimationTrigger", new Type[] { typeof(string) })]
internal static class EnemyActionPresentationPatch
{
	private static readonly Dictionary<string, EnemyAction> SharedTriggerActions = new Dictionary<string, EnemyAction>(StringComparer.Ordinal)
	{
		["SlashTrigger"] = EnemyAction.BasicAttack,
		["BoomerangTrigger"] = EnemyAction.HeavyAttack,
		["Attack"] = EnemyAction.BasicAttack,
		["AttackSingle"] = EnemyAction.BasicAttack,
		["HeavyAttack"] = EnemyAction.HeavyAttack,
		["HeavyAttackTrigger"] = EnemyAction.HeavyAttack,
		["DoubleAttackTrigger"] = EnemyAction.HeavyAttack,
		["MultiAttack"] = EnemyAction.HeavyAttack,
		["AttackTriple"] = EnemyAction.HeavyAttack,
		["DoubleAttack"] = EnemyAction.HeavyAttack,
		["Buff"] = EnemyAction.Buff,
		["Angry"] = EnemyAction.Buff,
		["Crank"] = EnemyAction.Buff,
		["Cast"] = EnemyAction.Cast,
		["flamethrower"] = EnemyAction.Cast,
		["Web"] = EnemyAction.Cast,
		["Curl"] = EnemyAction.Buff,
		["AttackHeavy"] = EnemyAction.BasicAttack,
		["AttackDouble"] = EnemyAction.HeavyAttack,
		["AttackBuff"] = EnemyAction.Buff,
		["AttackEnd"] = EnemyAction.Buff,
		["TRIPLE_ATTACK"] = EnemyAction.HeavyAttack,
		["TripleAttack"] = EnemyAction.HeavyAttack,
		["AttackTripleTrigger"] = EnemyAction.HeavyAttack,
		["Chomp"] = EnemyAction.BasicAttack,
		["FlailAttack"] = EnemyAction.BasicAttack,
		["SwipePower"] = EnemyAction.HeavyAttack,
		["RamAttack"] = EnemyAction.HeavyAttack,
		["Lash"] = EnemyAction.HeavyAttack,
		["Vines"] = EnemyAction.Cast,
		["Plow"] = EnemyAction.Charge,
		["EndPlow"] = EnemyAction.ChargeEnd,
		["Stun"] = EnemyAction.StunTransition,
		["Unstun"] = EnemyAction.WakeUp,
		["Hit"] = EnemyAction.Hurt,
		["PlowHit"] = EnemyAction.Hurt,
		["Dead"] = EnemyAction.Death
	};

	private static bool Prefix(NCreature __instance, string trigger)
	{
		Creature entity = __instance.Entity;
		if (((entity != null) ? entity.Monster : null) is ToughEgg)
		{
			ToughEggCompoundBodyAdapter.ObserveTrigger(__instance, trigger);
		}
		else
		{
			Creature entity2 = __instance.Entity;
			if (((entity2 != null) ? entity2.Monster : null) is Tunneler)
			{
				TunnelerCompoundBodyAdapter.ObserveTrigger(__instance, trigger);
			}
		}
		Creature entity3 = __instance.Entity;
		MonsterModel obj = ((entity3 != null) ? entity3.Monster : null);
		SpinyToad val = (SpinyToad)(object)((obj is SpinyToad) ? obj : null);
		if (val != null)
		{
			SpinyToadAnimatorAdapter.ObserveTrigger(__instance, val, trigger);
		}
		Creature entity4 = __instance.Entity;
		MonsterModel obj2 = ((entity4 != null) ? entity4.Monster : null);
		BowlbugNectar val2 = (BowlbugNectar)(object)((obj2 is BowlbugNectar) ? obj2 : null);
		if (val2 != null)
		{
			BowlbugNectarAnimatorAdapter.ObserveTrigger(val2, trigger);
		}
		Creature entity5 = __instance.Entity;
		MonsterModel obj3 = ((entity5 != null) ? entity5.Monster : null);
		SlimedBerserker val3 = (SlimedBerserker)(object)((obj3 is SlimedBerserker) ? obj3 : null);
		if (val3 != null)
		{
			return !SlimedBerserkerAnimatorAdapter.TryHandleCommittedTrigger(__instance, val3, trigger);
		}
		return true;
	}

	private static void Postfix(NCreature __instance, string trigger)
	{
		try
		{
			Creature entity = __instance.Entity;
			MonsterModel obj = ((entity != null) ? entity.Monster : null);
			TestSubject val = (TestSubject)(object)((obj is TestSubject) ? obj : null);
			if (val != null)
			{
				TestSubjectAnimatorAdapter.ObserveBossMusicPhase(val, trigger);
			}
			Creature entity2 = __instance.Entity;
			MonsterModel obj2 = ((entity2 != null) ? entity2.Monster : null);
			Vantom val2 = (Vantom)(object)((obj2 is Vantom) ? obj2 : null);
			if (val2 != null)
			{
				BerculaVantomAnimatorAdapter.ObserveTrigger(val2, trigger);
			}
			if (!TryResolveAppliedProfile(__instance, out EnemyReplacementProfile profile))
			{
				return;
			}
			bool flag = trigger == "Attack";
			if (flag)
			{
				Creature entity3 = __instance.Entity;
				MonsterModel val3 = ((entity3 != null) ? entity3.Monster : null);
				bool flag2 = ((val3 is Crusher || val3 is Rocket) ? true : false);
				flag = flag2;
			}
			if (flag)
			{
				return;
			}
			Creature entity4 = __instance.Entity;
			if (DecimillipedePresentationAdapter.OwnsCombatActionPresentation((entity4 != null) ? entity4.Monster : null, profile, trigger))
			{
				return;
			}
			if (profile.Id == "winged_humanface_for_owl_magistrate")
			{
				Creature entity5 = __instance.Entity;
				MonsterModel obj3 = ((entity5 != null) ? entity5.Monster : null);
				OwlMagistrate val4 = (OwlMagistrate)(object)((obj3 is OwlMagistrate) ? obj3 : null);
				if (val4 != null)
				{
					OwlMagistrateAnimatorAdapter.ObserveTrigger(val4, trigger);
				}
			}
			if (!TryMapAction(profile, __instance, trigger, out var action))
			{
				return;
			}
			int num;
			if (action == EnemyAction.Death)
			{
				Creature entity6 = __instance.Entity;
				MonsterModel obj4 = ((entity6 != null) ? entity6.Monster : null);
				WaterfallGiant val5 = (WaterfallGiant)(object)((obj4 is WaterfallGiant) ? obj4 : null);
				if (val5 != null)
				{
					num = (WaterfallGiantAnimatorAdapter.IsTerminalSequence(val5) ? 1 : 0);
					goto IL_0131;
				}
			}
			num = 0;
			goto IL_0131;
			IL_0131:
			bool flag3 = (byte)num != 0;
			if (trigger == "Erupt")
			{
				Creature entity7 = __instance.Entity;
				MonsterModel obj5 = ((entity7 != null) ? entity7.Monster : null);
				WaterfallGiant val6 = (WaterfallGiant)(object)((obj5 is WaterfallGiant) ? obj5 : null);
				if (val6 != null)
				{
					WaterfallGiantAnimatorAdapter.TryScheduleDeathPresentation(val6, __instance, profile);
				}
			}
			bool num2 = Entry.PlowMotion.Handle(__instance, profile, trigger);
			if (!num2 && action == EnemyAction.Death && profile.Monster.DeathBodyPolicy == EnemyDeathBodyPolicy.HideForOfficialEffect && Entry.ActionPresentation.CanPlayEffects(profile, action))
			{
				EnemyVisualHostAdapter.TryHideBodyForOfficialDeathEffect(__instance, profile);
			}
			if (!num2 && !flag3)
			{
				EnemyTargetCastPresentation.Play(__instance, profile, action);
				EnemyActionPresentationRuntime actionPresentation = Entry.ActionPresentation;
				EnemyReplacementProfile profile2 = profile;
				EnemyAction action2 = action;
				int includeAudio;
				if (action == EnemyAction.Death)
				{
					Creature entity8 = __instance.Entity;
					includeAudio = ((((entity8 != null) ? entity8.Monster : null) is WaterfallGiant) ? 1 : 0);
				}
				else
				{
					includeAudio = 1;
				}
				Creature entity9 = __instance.Entity;
				MonsterModel obj6 = ((entity9 != null) ? entity9.Monster : null);
				CeremonialBeast val7 = (CeremonialBeast)(object)((obj6 is CeremonialBeast) ? obj6 : null);
				actionPresentation.Play(__instance, profile2, action2, (byte)includeAudio != 0, (val7 != null && val7.IsInSecondPhase) ? EnemyPresentationStance.Transformed : EnemyPresentationStance.Normal);
			}
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Enemy action presentation adapter failed: " + ex.Message, 1);
		}
	}

	internal static bool TryResolveAppliedProfile(NCreature? creature, out EnemyReplacementProfile profile)
	{
		return EnemyVisualHostAdapter.TryResolveAppliedProfile(creature, out profile);
	}

	private static bool TryMapAction(EnemyReplacementProfile profile, NCreature creature, string trigger, out EnemyAction action)
	{
		Creature entity = creature.Entity;
		if (((entity != null) ? entity.Monster : null) is TheInsatiable && trigger == "EatPlayerTrigger")
		{
			action = EnemyAction.Special;
			return true;
		}
		Creature entity2 = creature.Entity;
		if (((entity2 != null) ? entity2.Monster : null) is Vantom && trigger == "ATTACK_HEAVY")
		{
			action = EnemyAction.Cast;
			return true;
		}
		Creature entity3 = creature.Entity;
		if (((entity3 != null) ? entity3.Monster : null) is SoulFysh && trigger == "AttackDebuffTrigger")
		{
			action = EnemyAction.Cast;
			return true;
		}
		Creature entity4 = creature.Entity;
		MonsterModel obj = ((entity4 != null) ? entity4.Monster : null);
		SlimedBerserker val = (SlimedBerserker)(object)((obj is SlimedBerserker) ? obj : null);
		if (val != null && SlimedBerserkerAnimatorAdapter.TryMapPresentation(val, trigger, out action))
		{
			return true;
		}
		Creature entity5 = creature.Entity;
		MonsterModel obj2 = ((entity5 != null) ? entity5.Monster : null);
		SpinyToad val2 = (SpinyToad)(object)((obj2 is SpinyToad) ? obj2 : null);
		if (val2 != null && SpinyToadAnimatorAdapter.TryMapPresentation(val2, trigger, out action))
		{
			return true;
		}
		Creature entity6 = creature.Entity;
		MonsterModel val3 = ((entity6 != null) ? entity6.Monster : null);
		if (val3 != null && Entry.MoveSemantics.TryResolve(val3, trigger, out action))
		{
			return true;
		}
		Creature entity7 = creature.Entity;
		MonsterModel obj3 = ((entity7 != null) ? entity7.Monster : null);
		Queen val4 = (Queen)(object)((obj3 is Queen) ? obj3 : null);
		if (val4 != null && QueenAnimatorAdapter.TryMapPresentation(val4, trigger, out action))
		{
			return true;
		}
		Creature entity8 = creature.Entity;
		MonsterModel obj4 = ((entity8 != null) ? entity8.Monster : null);
		TestSubject val5 = (TestSubject)(object)((obj4 is TestSubject) ? obj4 : null);
		if (val5 != null && TestSubjectAnimatorAdapter.TryMapPresentation(val5, trigger, out action))
		{
			return true;
		}
		Creature entity9 = creature.Entity;
		MonsterModel obj5 = ((entity9 != null) ? entity9.Monster : null);
		BowlbugNectar val6 = (BowlbugNectar)(object)((obj5 is BowlbugNectar) ? obj5 : null);
		if (val6 != null && BowlbugNectarAnimatorAdapter.TryMapPresentation(val6, trigger, out action))
		{
			return true;
		}
		if (profile.Id == "debolessa_for_lagavulin_matriarch")
		{
			Creature entity10 = creature.Entity;
			MonsterModel obj6 = ((entity10 != null) ? entity10.Monster : null);
			LagavulinMatriarch val7 = (LagavulinMatriarch)(object)((obj6 is LagavulinMatriarch) ? obj6 : null);
			if (val7 != null && LagavulinMatriarchAnimatorAdapter.TryMapPresentation(val7, trigger, out action))
			{
				return true;
			}
		}
		if (trigger == "Cast")
		{
			Creature entity11 = creature.Entity;
			if (((entity11 != null) ? entity11.Monster : null) is KinFollower)
			{
				action = EnemyAction.Buff;
				return true;
			}
		}
		if (trigger == "AttackDouble")
		{
			Creature entity12 = creature.Entity;
			MonsterModel obj7 = ((entity12 != null) ? entity12.Monster : null);
			GremlinMerc val8 = (GremlinMerc)(object)((obj7 is GremlinMerc) ? obj7 : null);
			if (val8 != null && GremlinMercAnimatorAdapter.IsGimmeMove(val8))
			{
				action = EnemyAction.Buff;
				return true;
			}
		}
		if (trigger == "AttackDefendTrigger")
		{
			Creature entity13 = creature.Entity;
			if (((entity13 != null) ? entity13.Monster : null) is SlitheringStrangler)
			{
				action = EnemyAction.HeavyAttack;
				return true;
			}
		}
		if (trigger == "Attack")
		{
			Creature entity14 = creature.Entity;
			if (((entity14 != null) ? entity14.Monster : null) is OwlMagistrate && creature.Entity.HasPower<SoarPower>())
			{
				action = EnemyAction.HeavyAttack;
				return true;
			}
		}
		string triggerAliasKey = trigger.Replace("_", string.Empty);
		EnemyAnimationBinding enemyAnimationBinding = profile.Monster.AnimationBindings.FirstOrDefault((EnemyAnimationBinding binding) => string.Equals(binding.TriggerAliasKey, triggerAliasKey, StringComparison.OrdinalIgnoreCase));
		if (enemyAnimationBinding != null)
		{
			action = enemyAnimationBinding.Action;
			return true;
		}
		if (!SharedTriggerActions.TryGetValue(trigger, out action))
		{
			return false;
		}
		if (trigger == "Attack")
		{
			Creature entity15 = creature.Entity;
			MonsterModel obj8 = ((entity15 != null) ? entity15.Monster : null);
			CeremonialBeast val9 = (CeremonialBeast)(object)((obj8 is CeremonialBeast) ? obj8 : null);
			if (val9 != null && CeremonialBeastAnimatorAdapter.IsStampMove(val9))
			{
				action = EnemyAction.Buff;
			}
		}
		return true;
	}
}
