using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class FrameworkCardEnergyColorPatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return (from type in AccessTools.AllTypes()
			where !type.IsAbstract && typeof(CardPoolModel).IsAssignableFrom(type)
			select AccessTools.PropertyGetter(type, "EnergyOutlineColor") into method
			where method != null
			select method).Cast<MethodBase>().Distinct();
	}

	[HarmonyPrefix]
	[HarmonyPriority(600)]
	private static bool Prefix(CardPoolModel __instance, ref Color __result)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!FrameworkSkinRuntime.TryGetPoolColor(__instance, "EnergyLabelOutlineColor", out var color))
		{
			return true;
		}
		__result = color;
		return false;
	}
}
