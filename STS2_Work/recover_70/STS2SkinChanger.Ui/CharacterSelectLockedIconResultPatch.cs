using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class CharacterSelectLockedIconResultPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(CharacterModel __instance, ref CompressedTexture2D __result)
	{
		ContextualSkinControls.ReplaceCharacterSelectTexture(__instance, locked: true, ref __result);
	}
}
