using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class RelicIconOutlineSkinPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(RelicModel __instance, ref Texture2D __result)
	{
		string packedIconPath = __instance.PackedIconPath;
		string text = packedIconPath.Replace("/relic_atlas.sprites/", "/relic_outline_atlas.sprites/", StringComparison.OrdinalIgnoreCase);
		if (!text.Equals(packedIconPath, StringComparison.OrdinalIgnoreCase))
		{
			RelicSkinResources.Replace(text, ref __result);
		}
	}
}
