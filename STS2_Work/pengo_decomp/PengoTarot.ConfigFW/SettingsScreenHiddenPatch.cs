using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace PengoTarot.ConfigFW;

[HarmonyPatch(typeof(NSettingsScreen), "OnSubmenuHidden")]
public static class SettingsScreenHiddenPatch
{
	public static void Postfix()
	{
		ConfigFloatingWindow.OnSettingsScreenClosed();
	}
}
