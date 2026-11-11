using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace PengoTarot.ConfigFW;

[HarmonyPatch(typeof(NSettingsScreen), "OnSubmenuShown")]
public static class SettingsScreenShownPatch
{
	public static void Postfix(NSettingsScreen __instance)
	{
		ConfigFloatingWindow.OnSettingsScreenOpened((Control)(object)__instance);
	}
}
