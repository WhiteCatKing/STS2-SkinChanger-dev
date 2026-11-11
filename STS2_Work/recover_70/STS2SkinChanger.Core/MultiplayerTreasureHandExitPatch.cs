using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NHandImage), "_ExitTree")]
internal static class MultiplayerTreasureHandExitPatch
{
	private static void Postfix(NHandImage __instance)
	{
		MultiplayerTreasureHandAppearance.Forget(__instance);
	}
}
