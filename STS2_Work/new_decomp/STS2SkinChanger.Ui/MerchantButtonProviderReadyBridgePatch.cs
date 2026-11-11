using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NMerchantButton), "_Ready")]
internal static class MerchantButtonProviderReadyBridgePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void Prefix(NMerchantButton __instance, out string? __state)
	{
		__state = MerchantRuntimeAppearance.BeginSelectedMerchantNodeReady((Node)(object)__instance);
	}

	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(NMerchantButton __instance, string? __state)
	{
		PartTakeoverVisualScope.RememberMerchantFigure((Node)(object)__instance);
		MerchantRuntimeAppearance.CompleteSelectedMerchantNodeReady((Node)(object)__instance, __state);
	}
}
