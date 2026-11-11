using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class CardPortraitResultPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(CardModel __instance, ref Texture2D __result)
	{
		CardSkinControls.ReplacePortrait(__instance, ref __result);
	}
}
