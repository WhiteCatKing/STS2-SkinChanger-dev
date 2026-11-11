using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NMerchantRoom), "_ExitTree")]
internal static class MerchantRoomPreviewExitTreePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static bool Prefix(NMerchantRoom __instance)
	{
		return !MerchantRuntimeAppearance.IsMerchantPreviewRoot((Node)(object)__instance);
	}

	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(NMerchantRoom __instance)
	{
		if (!MerchantRuntimeAppearance.IsMerchantPreviewRoot((Node)(object)__instance))
		{
			RoomRuntimeScopeOwnership.Release(__instance);
		}
	}
}
