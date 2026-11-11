using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace PengoTarot.ConfigFW;

[HarmonyPatch(typeof(NCharacterSelectScreen), "OnSubmenuOpened")]
public static class CharacterSelectEntryPatch
{
	public static void Postfix(NCharacterSelectScreen __instance)
	{
		ConfigFloatingWindow.OnCharacterSelectOpened(__instance);
	}
}
