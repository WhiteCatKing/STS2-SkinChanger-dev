using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCardLibrary), "_Ready")]
internal static class CardLibrarySkinReadyPatch
{
	private static void Postfix(NCardLibrary __instance)
	{
		CardSkinControls.Attach(__instance);
	}
}
