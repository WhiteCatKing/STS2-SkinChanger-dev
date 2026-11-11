using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(RelicModel), "get_IconOutline")]
internal static class FrameworkRelicOutlineTexturePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(600)]
	private static bool Prefix(RelicModel __instance, ref Texture2D __result)
	{
		FrameworkRelicVisualPlan frameworkRelicVisualPlan = FrameworkSkinRuntime.ResolveRelicVisual(__instance, largeIcon: false);
		if ((object)frameworkRelicVisualPlan == null || frameworkRelicVisualPlan.OutlinePath == null)
		{
			return true;
		}
		return FrameworkRelicTextureLoader.Apply(frameworkRelicVisualPlan.OutlinePath, ref __result);
	}
}
