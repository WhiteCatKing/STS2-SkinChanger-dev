using HarmonyLib;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Models;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(CombatHistory), "MonsterPerformedMove")]
internal static class EnemyMoveFinishedPatch
{
	[HarmonyPostfix]
	private static void Postfix(MonsterModel monster)
	{
		EnemyAttackGate.MoveFinished(monster);
	}
}
