using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCardLibrary), "OnSubmenuOpened")]
internal static class CardLibrarySkinOpenedPatch
{
	private static void Prefix(NCardLibrary __instance)
	{
		CardSkinControls.ResetAvailabilityFilter(__instance);
	}

	private static void Postfix(NCardLibrary __instance)
	{
		CardSkinControls.SyncToSelectedFilter(__instance);
	}
}
