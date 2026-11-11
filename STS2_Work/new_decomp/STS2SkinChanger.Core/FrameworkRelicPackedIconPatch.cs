using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class FrameworkRelicPackedIconPatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return RelicGetters("PackedIconPath");
	}

	[HarmonyPrefix]
	private static bool Prefix(RelicModel __instance, ref string __result)
	{
		return Apply(__instance, ref __result, "PackedIconPath");
	}

	internal static IEnumerable<MethodBase> RelicGetters(string propertyName)
	{
		return (from method in new MethodInfo[1] { AccessTools.PropertyGetter(typeof(RelicModel), propertyName) }.Concat(from type in AccessTools.AllTypes()
				where !type.IsAbstract && typeof(RelicModel).IsAssignableFrom(type)
				select AccessTools.DeclaredPropertyGetter(type, propertyName))
			where method != null
			select method).Cast<MethodBase>().Distinct();
	}

	internal static bool Apply(RelicModel model, ref string result, string propertyName)
	{
		if (!FrameworkSkinRuntime.TryGetRelicResource(model, propertyName, out string path))
		{
			return true;
		}
		result = path;
		return false;
	}
}
