using System.Threading.Tasks;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.TestSupport;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(DecimillipedeSegment), "AnimSegmentsAttack")]
internal static class DecimillipedeGroupedAttackPatch
{
	private static bool Prefix(DecimillipedeSegment __instance, ref Task __result)
	{
		if (TestMode.IsOn)
		{
			return true;
		}
		EnemyAction? enemyAction = ((MonsterState)((MonsterModel)__instance).NextMove).Id switch
		{
			"WRITHE_MOVE" => EnemyAction.BasicAttack, 
			"BULK_MOVE" => EnemyAction.Buff, 
			"CONSTRICT_MOVE" => EnemyAction.Cast, 
			_ => null, 
		};
		if (!enemyAction.HasValue)
		{
			return true;
		}
		if (!DecimillipedePresentationAdapter.TryCreateCombatPresentation(__instance, enemyAction.Value, out Task presentation))
		{
			return true;
		}
		__result = presentation;
		return false;
	}
}
