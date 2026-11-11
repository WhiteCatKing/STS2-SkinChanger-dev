using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NMapScreen), "Open")]
internal static class MerchantRoomPrewarmPatch
{
	[HarmonyPostfix]
	private static void Postfix()
	{
		SkinService.PrewarmRuntimeSceneOverlay("merchant", "res://scenes/rooms/merchant_room.tscn");
	}
}
