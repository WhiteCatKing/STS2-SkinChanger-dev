using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCardLibrary), "UpdateCardPoolFilter")]
internal static class CardLibraryPoolSkinPatch
{
	private static void Postfix(NCardLibrary __instance, NCardPoolFilter filter)
	{
		CardSkinControls.ShowForFilter(__instance, filter);
	}
}
