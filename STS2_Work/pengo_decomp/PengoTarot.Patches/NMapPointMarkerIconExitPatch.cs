using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NNormalMapPoint), "_ExitTree")]
public static class NMapPointMarkerIconExitPatch
{
	public static void Postfix(NNormalMapPoint __instance)
	{
		NMapPointMarkerIconPatch.OnExit(__instance);
	}
}
