using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NNormalMapPoint), "_Ready")]
public static class NMapPointMarkerIconReadyPatch
{
	public static void Postfix(NNormalMapPoint __instance)
	{
		NMapPointMarkerIconPatch.OnReady(__instance);
	}
}
