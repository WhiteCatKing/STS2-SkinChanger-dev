using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NCombatRoom), "SetUpBackground")]
internal static class BackgroundSkinRoomPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(NCombatRoom __instance, IRunState state)
	{
		BackgroundSkinService.ApplyToRoom(__instance, state);
	}
}
