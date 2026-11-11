using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NAncientBgContainer), "OnWindowChange")]
internal static class ManagedAncientStaticBackgroundWindowPatch
{
	private static void Postfix(NAncientBgContainer __instance)
	{
		ManagedAncientStaticBackground.FitChildren(__instance);
	}
}
