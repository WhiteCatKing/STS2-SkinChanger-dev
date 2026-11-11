using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class FrameworkRelicOutlinePatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return FrameworkRelicPackedIconPatch.RelicGetters("PackedIconOutlinePath");
	}

	[HarmonyPrefix]
	private static bool Prefix(RelicModel __instance, ref string __result)
	{
		return FrameworkRelicPackedIconPatch.Apply(__instance, ref __result, "PackedIconOutlinePath");
	}
}
