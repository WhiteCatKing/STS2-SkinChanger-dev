using System.Collections.Generic;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(VfxCmd), "PlayOnCreatureCenters")]
internal static class EyeTargetVfxPatch
{
	private static void Prefix(ref IEnumerable<Creature> targets, string path)
	{
		EyeTargetMoveScope.State value = EyeTargetMoveScope.Current.Value;
		if (value == null || path != "vfx/vfx_attack_slash" || !Entry.PresentationSettings.ActionEffectsEnabled || !EnemyVisualHostAdapter.TryResolveAppliedProfile(((MonsterModel)value.Monster).Creature.GetCreatureNode(), out EnemyReplacementProfile profile))
		{
			return;
		}
		List<Creature> list = new List<Creature>();
		foreach (Creature target in targets)
		{
			if (target.IsAlive && !EnemyTargetAttackPatch.TryPlay(target, profile, EnemyAction.BasicAttack, value.Generation, atEntry: false, null))
			{
				list.Add(target);
			}
		}
		targets = list;
	}
}
