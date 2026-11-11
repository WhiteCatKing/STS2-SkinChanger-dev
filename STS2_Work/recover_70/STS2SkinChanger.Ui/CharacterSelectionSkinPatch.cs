using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCharacterSelectScreen), "SelectCharacter")]
internal static class CharacterSelectionSkinPatch
{
	[HarmonyPriority(800)]
	private static void Prefix(NCharacterSelectScreen __instance)
	{
		ContextualSkinControls.CaptureCharacterBackgroundHostLayout(__instance);
	}

	[HarmonyPriority(800)]
	private static void Postfix(NCharacterSelectScreen __instance, CharacterModel characterModel)
	{
		ContextualSkinControls.ShowCharacter(__instance, characterModel);
	}
}
