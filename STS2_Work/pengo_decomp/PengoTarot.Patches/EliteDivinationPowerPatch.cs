using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Divination;
using PengoTarot.Powers;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(Hook), "BeforeCombatStart")]
public static class EliteDivinationPowerPatch
{
	private const int ChariotFlag = 7;

	private const int StrengthFlag = 8;

	private const int HermitFlag = 9;

	private const decimal HermitPlatingRatio = 0.1m;

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
		if (currentMapPoint == null || (int)currentMapPoint.PointType != 6)
		{
			return;
		}
		List<int> markedFlagsAt = TarotMarkerSystem.GetMarkedFlagsAt(valueOrDefault, runState.CurrentActIndex);
		bool chariot = markedFlagsAt.Contains(7);
		bool strength = markedFlagsAt.Contains(8);
		bool hermit = markedFlagsAt.Contains(9);
		if (!chariot && !strength && !hermit)
		{
			return;
		}
		ThrowingPlayerChoiceContext context = new ThrowingPlayerChoiceContext();
		foreach (Creature enemy in combatState.Enemies)
		{
			if (chariot && enemy.GetPower<TarChariotReversedPower>() == null)
			{
				await PowerCmd.Apply<TarChariotReversedPower>((PlayerChoiceContext)(object)context, enemy, 1m, enemy, (CardModel)null, false);
			}
			if (strength && enemy.GetPower<TarStrengthReversedPower>() == null)
			{
				await PowerCmd.Apply<TarStrengthReversedPower>((PlayerChoiceContext)(object)context, enemy, 1m, enemy, (CardModel)null, false);
			}
			if (hermit && enemy.GetPower<TarHermitReversedPower>() == null)
			{
				await PowerCmd.Apply<TarHermitReversedPower>((PlayerChoiceContext)(object)context, enemy, decimal.Round((decimal)enemy.MaxHp * 0.1m), enemy, (CardModel)null, false);
			}
		}
	}
}
