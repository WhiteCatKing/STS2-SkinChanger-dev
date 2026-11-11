using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class FrameworkOrbDarkenedColorPatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return (from type in AccessTools.AllTypes()
			where !type.IsAbstract && typeof(OrbModel).IsAssignableFrom(type)
			select AccessTools.DeclaredPropertyGetter(type, "DarkenedColor") into method
			where method != null
			select method).Cast<MethodBase>();
	}

	[HarmonyPrefix]
	private static bool Prefix(OrbModel __instance, ref Color __result)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!FrameworkSkinRuntime.TryGetOrbColor(__instance, "CustomDarkenedColor", out var color))
		{
			return true;
		}
		__result = color;
		return false;
	}
}
