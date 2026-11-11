using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NCombatRoom), "AdjustCreatureScaleForAspectRatio")]
internal static class EnemyIntentLayoutVisibilityPatch
{
	private static void Postfix(NCombatRoom __instance)
	{
		foreach (NCreature creatureNode in __instance.CreatureNodes)
		{
			EnemyIntentVisibilityGuard.QueueRefresh(creatureNode);
		}
	}
}
