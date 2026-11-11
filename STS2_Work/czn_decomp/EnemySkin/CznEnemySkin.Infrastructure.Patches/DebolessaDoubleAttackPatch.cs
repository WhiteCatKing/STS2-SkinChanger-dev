using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(AttackCommand), "Execute")]
internal static class DebolessaDoubleAttackPatch
{
	private static void Prefix(AttackCommand __instance, string? ____attackerAnimName, bool ____playOnEveryHit, int ____hitCount, Func<Task>? ____beforeDamage, Func<Task>? ____afterAttackerAnim, List<Func<Creature, Node2D?>> ____customHitVfxNodes, Creature? ____visualAttacker, bool ____shouldPlayAnimation, float[] ____waitBeforeHit, bool ____spawnVfxOnEachCreature, bool ____spawnVfxOnCreatureCenter)
	{
		Creature attacker = __instance.Attacker;
		MonsterModel obj = ((attacker != null) ? attacker.Monster : null);
		LagavulinMatriarch val = (LagavulinMatriarch)(object)((obj is LagavulinMatriarch) ? obj : null);
		if (val == null || ____attackerAnimName != "AttackDouble" || ____playOnEveryHit || ____hitCount != 2 || !____shouldPlayAnimation || ____waitBeforeHit.Any((float v) => v > 0f) || ____spawnVfxOnEachCreature || !____spawnVfxOnCreatureCenter || ____visualAttacker != null || ____beforeDamage != null || ____afterAttackerAnim != null || ____customHitVfxNodes.Count != 0 || __instance.HitVfx != "vfx/vfx_attack_slash" || __instance.HitSfx != null || __instance.TmpHitSfx != null || !__instance.IsMultiTargeted || __instance.IsRandomlyTargeted || !EnemyVisualHostAdapter.TryResolveAppliedProfile(__instance.Attacker.GetCreatureNode(), out EnemyReplacementProfile profile) || profile.Id != "debolessa_for_lagavulin_matriarch")
		{
			return;
		}
		bool shellAwake = val.IsShellAwake;
		float interval = (shellAwake ? 0.6667f : 0.8333f);
		__instance.WithHitFx((string)null, (string)null, (string)null);
		int generation = Entry.ActionPresentation.EffectsGeneration;
		int targetHitIndex = 0;
		Configure(__instance, interval, (float seconds) => Cmd.Wait(seconds, false), delegate
		{
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			Creature[] array = __instance.Attacker.CombatState.PlayerCreatures.Where((Creature c) => c.IsAlive).ToArray();
			if (!shellAwake)
			{
				Creature[] array2 = array;
				foreach (Creature val2 in array2)
				{
					if (!Entry.Replacements.IsEnabled(profile) || !Entry.PresentationSettings.ActionEffectsEnabled || generation != Entry.ActionPresentation.EffectsGeneration || !EnemyTargetAttackPatch.TryPlay(val2, profile, EnemyAction.HeavyAttack, generation, atEntry: false, targetHitIndex))
					{
						VfxCmd.PlayOnCreatureCenter(val2, "vfx/vfx_attack_slash");
					}
				}
				targetHitIndex++;
			}
			else if (array.Length == 1)
			{
				VfxCmd.PlayOnCreatureCenter(array[0], "vfx/vfx_attack_slash");
			}
			else if (array.Length > 1)
			{
				VfxCmd.PlayOnSide(array[0].Side, "vfx/vfx_attack_slash", __instance.Attacker.CombatState);
			}
		});
	}

	internal static void Configure(AttackCommand command, float interval, Func<float, Task> wait, Action playHit)
	{
		Task secondHitReady = Task.CompletedTask;
		int hit = 0;
		command.AfterAttackerAnim((Func<Task>)delegate
		{
			secondHitReady = wait(interval);
			return Task.CompletedTask;
		});
		command.BeforeDamage((Func<Task>)async delegate
		{
			if (hit++ > 0)
			{
				await secondHitReady;
			}
			playHit();
		});
	}
}
