using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(CombatManager), "EndEnemyTurn")]
internal static class EnemyTurnEndPatch
{
	[HarmonyPrefix]
	private static void Prefix()
	{
		EnemyAttackGate.EndTurn();
	}
}
