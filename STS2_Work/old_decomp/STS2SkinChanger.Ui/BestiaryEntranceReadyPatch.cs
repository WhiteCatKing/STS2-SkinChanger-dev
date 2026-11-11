using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCompendiumSubmenu), "_Ready")]
internal static class BestiaryEntranceReadyPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(NCompendiumSubmenu __instance)
	{
		BestiaryEntranceSwitcher.ScanCompendium(__instance);
	}
}
