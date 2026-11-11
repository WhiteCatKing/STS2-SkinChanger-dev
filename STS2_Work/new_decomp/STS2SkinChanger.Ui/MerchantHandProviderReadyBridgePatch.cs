using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NMerchantHand), "_Ready")]
internal static class MerchantHandProviderReadyBridgePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void Prefix(NMerchantHand __instance, out string? __state)
	{
		__state = MerchantRuntimeAppearance.BeginSelectedMerchantNodeReady((Node)(object)__instance);
	}

	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(NMerchantHand __instance, string? __state)
	{
		MerchantRuntimeAppearance.CompleteSelectedMerchantNodeReady((Node)(object)__instance, __state);
	}
}
