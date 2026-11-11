using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace PengoTarot.ConfigFW;

[HarmonyPatch(typeof(NSettingsScreen), "OnSubmenuClosed")]
public static class SettingsScreenClosePatch
{
	public static void Postfix()
	{
		ConfigFloatingWindow.OnSettingsScreenClosed();
	}
}
