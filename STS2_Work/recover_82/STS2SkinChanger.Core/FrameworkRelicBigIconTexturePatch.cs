using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(RelicModel), "get_BigIcon")]
internal static class FrameworkRelicBigIconTexturePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(600)]
	private static bool Prefix(RelicModel __instance, ref Texture2D __result)
	{
		FrameworkRelicVisualPlan frameworkRelicVisualPlan = FrameworkSkinRuntime.ResolveRelicVisual(__instance, largeIcon: true);
		if (frameworkRelicVisualPlan == null)
		{
			return true;
		}
		return FrameworkRelicTextureLoader.Apply(frameworkRelicVisualPlan.IconPath, ref __result);
	}
}
