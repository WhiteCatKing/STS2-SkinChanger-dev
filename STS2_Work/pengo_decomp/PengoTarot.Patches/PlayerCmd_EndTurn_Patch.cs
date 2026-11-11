using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(PlayerCmd))]
public static class PlayerCmd_EndTurn_Patch
{
	[HarmonyPrefix]
	[HarmonyPatch("EndTurn")]
	public static bool Prefix(Player player, bool canBackOut, Func<Task>? actionDuringEnemyTurn)
	{
		CombatManager instance = CombatManager.Instance;
		if (instance.IsPlayerReadyToEndTurn(player))
		{
			(AccessTools.Field(typeof(CombatManager), "PlayerEndedTurn")?.GetValue(instance) as Action<Player, bool>)?.Invoke(player, canBackOut);
			return false;
		}
		return true;
	}
}
