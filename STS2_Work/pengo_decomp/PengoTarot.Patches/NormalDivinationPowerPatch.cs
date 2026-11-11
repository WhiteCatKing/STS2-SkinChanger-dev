using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Divination;
using PengoTarot.Powers;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(Hook), "BeforeCombatStart")]
public static class NormalDivinationPowerPatch
{
	private const int JusticeFlag = 11;

	private const int HangedManFlag = 12;

	private const int DeathFlag = 13;

	[HarmonyPostfix]
	private static void Postfix(IRunState runState, ICombatState? combatState, ref Task __result)
	{
		__result = ApplyAfterOriginalAsync(__result, runState, combatState);
	}

	private static async Task ApplyAfterOriginalAsync(Task originalTask, IRunState runState, ICombatState? combatState)
	{
		await originalTask;
		if (!RunManager.Instance.IsInProgress || combatState == null)
		{
			return;
		}
		MapCoord? currentMapCoord = runState.CurrentMapCoord;
		if (!currentMapCoord.HasValue)
		{
			return;
		}
		MapCoord valueOrDefault = currentMapCoord.GetValueOrDefault();
		MapPoint currentMapPoint = runState.CurrentMapPoint;
		if (currentMapPoint == null || (int)currentMapPoint.PointType != 5)
		{
			return;
		}
		List<int> markedFlagsAt = TarotMarkerSystem.GetMarkedFlagsAt(valueOrDefault, runState.CurrentActIndex);
		bool justice = markedFlagsAt.Contains(11);
		bool hanged = markedFlagsAt.Contains(12);
		bool death = markedFlagsAt.Contains(13);
		if (!justice && !hanged && !death)
		{
			return;
		}
		ThrowingPlayerChoiceContext context = new ThrowingPlayerChoiceContext();
		foreach (Player player in combatState.Players)
		{
			if (justice && player.Creature.GetPower<TarJusticeReversedPower>() == null)
			{
				await PowerCmd.Apply<TarJusticeReversedPower>((PlayerChoiceContext)(object)context, player.Creature, 1m, player.Creature, (CardModel)null, false);
			}
			if (hanged && player.Creature.GetPower<TarHangedManReversedPower>() == null)
			{
				await PowerCmd.Apply<TarHangedManReversedPower>((PlayerChoiceContext)(object)context, player.Creature, 1m, player.Creature, (CardModel)null, false);
			}
			if (death && player.Creature.GetPower<TarDeathReversedPower>() == null)
			{
				await PowerCmd.Apply<TarDeathReversedPower>((PlayerChoiceContext)(object)context, player.Creature, 1m, player.Creature, (CardModel)null, false);
			}
		}
	}
}
