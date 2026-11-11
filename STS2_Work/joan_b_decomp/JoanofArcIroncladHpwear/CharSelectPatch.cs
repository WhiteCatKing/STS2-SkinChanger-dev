using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(NCharacterSelectScreen), "SelectCharacter")]
internal static class CharSelectPatch
{
	[HarmonyPostfix]
	private static void Postfix(NCharacterSelectScreen __instance, CharacterModel characterModel)
	{
		CharSelectCycle.OnCharacterSelected(__instance, characterModel);
	}
}
