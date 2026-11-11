using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace STS2SkinChanger.Ui.LoadOrderWarning;

[HarmonyPatch(typeof(NMainMenu), "_Ready")]
internal static class MainMenuLoadOrderWarningPatch
{
	private static void Postfix()
	{
		LoadOrderWarningController.Schedule();
	}
}
