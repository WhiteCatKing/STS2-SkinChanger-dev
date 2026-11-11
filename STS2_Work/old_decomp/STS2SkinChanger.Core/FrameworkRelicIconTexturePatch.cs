using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(RelicModel), "get_Icon")]
internal static class FrameworkRelicIconTexturePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(600)]
	private static bool Prefix(RelicModel __instance, ref Texture2D __result)
	{
		FrameworkRelicVisualPlan frameworkRelicVisualPlan = FrameworkSkinRuntime.ResolveRelicVisual(__instance, largeIcon: false);
		if (frameworkRelicVisualPlan == null)
		{
			return true;
		}
		return FrameworkRelicTextureLoader.Apply(frameworkRelicVisualPlan.IconPath, ref __result);
	}
}
