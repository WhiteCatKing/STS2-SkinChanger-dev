using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NMerchantRoom), "_EnterTree")]
internal static class MerchantRoomPreviewEnterTreePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static bool Prefix(NMerchantRoom __instance)
	{
		return !MerchantRuntimeAppearance.IsMerchantPreviewRoot((Node)(object)__instance);
	}
}
