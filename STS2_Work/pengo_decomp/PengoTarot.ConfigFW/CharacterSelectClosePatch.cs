using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace PengoTarot.ConfigFW;

[HarmonyPatch(typeof(NCharacterSelectScreen), "OnSubmenuClosed")]
public static class CharacterSelectClosePatch
{
	public static void Postfix(NCharacterSelectScreen __instance)
	{
		ConfigFloatingWindow.OnCharacterSelectClosed(__instance);
	}
}
