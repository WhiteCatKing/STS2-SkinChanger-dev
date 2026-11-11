using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using PengoTarot.Data.Judgement;

namespace PengoTarot.Patch.Relic;

[HarmonyPatch(typeof(TreasureRoomRelicSynchronizer), "OnRoomExited")]
public static class WheelTreasureExitPatch
{
	private static void Postfix()
	{
		WheelOfFortuneRelicCopyState.ResetTreasure();
	}
}
