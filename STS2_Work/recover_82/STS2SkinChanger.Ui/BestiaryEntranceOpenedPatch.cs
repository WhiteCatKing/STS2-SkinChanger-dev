using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCompendiumSubmenu), "OnSubmenuOpened")]
internal static class BestiaryEntranceOpenedPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(NCompendiumSubmenu __instance)
	{
		BestiaryEntranceSwitcher.ScanCompendium(__instance);
	}
}
