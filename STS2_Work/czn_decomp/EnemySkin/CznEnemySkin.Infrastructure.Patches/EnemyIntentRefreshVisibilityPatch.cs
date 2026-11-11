using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NCreature), "UpdateIntent")]
internal static class EnemyIntentRefreshVisibilityPatch
{
	private static void Postfix(NCreature __instance)
	{
		EnemyIntentVisibilityGuard.QueueRefresh(__instance);
	}
}
