using System;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(WaterfallGiant), "TriggerAboutToBlowState")]
internal static class WaterfallGiantPhaseTransitionPatch
{
	private static void Prefix(WaterfallGiant __instance)
	{
		try
		{
			NCreature creatureNode = ((MonsterModel)__instance).Creature.GetCreatureNode();
			if (creatureNode != null && EnemyVisualHostAdapter.TryResolveAppliedProfile(creatureNode, out EnemyReplacementProfile _))
			{
				WaterfallGiantAnimatorAdapter.MarkTerminalSequence(__instance);
				creatureNode.SetAnimationTrigger("AboutToBlow");
			}
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Waterfall Giant phase transition adapter failed: " + ex.Message, 1);
		}
	}
}
