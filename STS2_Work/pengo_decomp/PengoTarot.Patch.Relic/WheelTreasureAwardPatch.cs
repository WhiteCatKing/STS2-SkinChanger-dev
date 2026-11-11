using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using PengoTarot.Data;
using PengoTarot.Data.Judgement;

namespace PengoTarot.Patch.Relic;

[HarmonyPatch(typeof(NTreasureRoomRelicCollection), "AnimateRelicAwards")]
public static class WheelTreasureAwardPatch
{
	private static void Postfix(List<RelicPickingResult> results, ref Task __result)
	{
		__result = Wrap(__result, results);
	}

	private static async Task Wrap(Task task, List<RelicPickingResult> results)
	{
		int copies = WheelOfFortuneRelicCopyState.TreasureCopies;
		await task;
		foreach (RelicPickingResult result in results)
		{
			if ((int)result.type != 3 && result.player != null && TarotImmediateEffects.CanWheelOfFortuneCopy(result.relic))
			{
				for (int i = 0; i < copies; i++)
				{
					await RelicCmd.Obtain(ModelDb.GetById<RelicModel>(((AbstractModel)result.relic).Id).ToMutable(), result.player, -1);
				}
			}
		}
		WheelOfFortuneRelicCopyState.ResetTreasure();
	}
}
