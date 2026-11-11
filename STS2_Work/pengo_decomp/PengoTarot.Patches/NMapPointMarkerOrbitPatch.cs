using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NNormalMapPoint), "_Process")]
public static class NMapPointMarkerOrbitPatch
{
	public static void Postfix(NNormalMapPoint __instance, double delta)
	{
		NMapPointMarkerIconPatch.OnProcess(__instance, delta);
	}
}
