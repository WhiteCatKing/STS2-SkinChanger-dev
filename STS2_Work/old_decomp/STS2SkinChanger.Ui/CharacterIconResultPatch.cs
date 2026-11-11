using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class CharacterIconResultPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(CharacterModel __instance, ref Control __result)
	{
		ContextualSkinControls.ReplaceCharacterIcon(__instance, ref __result);
	}
}
