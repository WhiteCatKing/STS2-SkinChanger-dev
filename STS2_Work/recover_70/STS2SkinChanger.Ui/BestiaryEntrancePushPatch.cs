using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NSubmenuStack), "Push")]
internal static class BestiaryEntrancePushPatch
{
	[HarmonyPriority(800)]
	private static bool Prefix(NSubmenuStack __instance, NSubmenu screen)
	{
		return !BestiaryEntranceSwitcher.TryRedirectBestiaryOpen(__instance, screen);
	}
}
