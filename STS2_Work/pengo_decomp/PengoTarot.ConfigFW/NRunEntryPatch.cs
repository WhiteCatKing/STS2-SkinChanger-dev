using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace PengoTarot.ConfigFW;

[HarmonyPatch(typeof(NRun), "_Ready")]
public static class NRunEntryPatch
{
	public static void Postfix(NRun __instance)
	{
		ConfigFloatingWindow.OnRunReady(__instance);
	}
}
