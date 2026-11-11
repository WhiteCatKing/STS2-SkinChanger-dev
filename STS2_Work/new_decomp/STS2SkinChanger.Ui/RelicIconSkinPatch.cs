using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class RelicIconSkinPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(RelicModel __instance, ref Texture2D __result)
	{
		RelicSkinResources.Replace(__instance.PackedIconPath, ref __result);
	}
}
