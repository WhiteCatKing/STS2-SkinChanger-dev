using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class CharacterIconTextureResultPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(CharacterModel __instance, ref Texture2D __result)
	{
		string text = ((AbstractModel)__instance).Id.Entry.ToLowerInvariant();
		string resourcePath = ContextualSkinControls.CanonicalImagePath("ui/top_panel/character_icon_" + text + ".png");
		ContextualSkinControls.ReplaceCharacterTexture(__instance, resourcePath, ref __result);
	}
}
