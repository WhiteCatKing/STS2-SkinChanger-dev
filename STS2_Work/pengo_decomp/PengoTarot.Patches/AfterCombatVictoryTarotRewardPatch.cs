using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Divination;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(Hook), "AfterCombatVictory")]
public static class AfterCombatVictoryTarotRewardPatch
{
	private static void Postfix(IRunState runState, ICombatState? combatState, CombatRoom room)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (RunManager.Instance.IsInProgress)
		{
			MapCoord? currentMapCoord = runState.CurrentMapCoord;
			if (currentMapCoord.HasValue)
			{
				MapCoord valueOrDefault = currentMapCoord.GetValueOrDefault();
				Log.Info($"[PengoTarot] [DivinationReward] victory peer={RunManager.Instance.NetService.Type} act={runState.CurrentActIndex} coord={valueOrDefault} room={((AbstractRoom)room).RoomType}", 2);
				List<Player> players = ((IPlayerCollection)runState).Players.ToList();
				TarotMarkerSystem.OnMarkedCombatVictory(valueOrDefault, runState.CurrentActIndex, room, players);
			}
		}
	}
}
