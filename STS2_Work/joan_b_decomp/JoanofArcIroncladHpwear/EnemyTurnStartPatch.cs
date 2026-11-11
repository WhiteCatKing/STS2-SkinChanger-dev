using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(CombatManager), "ExecuteEnemyTurn")]
internal static class EnemyTurnStartPatch
{
	[HarmonyPrefix]
	private static void Prefix(CombatManager __instance)
	{
		try
		{
			EnemyAttackGate.Snapshot(__instance.DebugOnlyGetState());
		}
		catch (Exception ex)
		{
			ModEntry.TryInfo("enemy gate snapshot: " + ex.Message);
		}
	}
}
