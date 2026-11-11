using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NInspectCardScreen), "_Ready")]
internal static class CardInspectSkinReadyPatch
{
	private static void Postfix(NInspectCardScreen __instance)
	{
		CardInspectSkinControls.Attach(__instance);
	}
}
