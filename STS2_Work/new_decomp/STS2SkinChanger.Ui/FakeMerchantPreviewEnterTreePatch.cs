using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NFakeMerchant), "_EnterTree")]
internal static class FakeMerchantPreviewEnterTreePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static bool Prefix(NFakeMerchant __instance)
	{
		return !MerchantRuntimeAppearance.IsMerchantPreviewRoot((Node)(object)__instance);
	}
}
