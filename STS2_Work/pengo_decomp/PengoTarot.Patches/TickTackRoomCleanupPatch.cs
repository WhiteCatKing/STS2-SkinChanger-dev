using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Powers;

namespace PengoTarot.Patches;

[HarmonyPatch]
public static class TickTackRoomCleanupPatch
{
	[HarmonyPatch(typeof(RunManager), "ExitCurrentRoom")]
	[HarmonyPrefix]
	private static void BeforeRoomExit()
	{
		TickTackPower.StopAllEffectsForRoomTransition();
	}
}
