using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NFakeMerchant), "_ExitTree")]
internal static class FakeMerchantPreviewExitTreePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static bool Prefix(NFakeMerchant __instance)
	{
		return !MerchantRuntimeAppearance.IsMerchantPreviewRoot((Node)(object)__instance);
	}

	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(NFakeMerchant __instance)
	{
		if (!MerchantRuntimeAppearance.IsMerchantPreviewRoot((Node)(object)__instance))
		{
			RoomRuntimeScopeOwnership.Release(__instance);
		}
	}
}
