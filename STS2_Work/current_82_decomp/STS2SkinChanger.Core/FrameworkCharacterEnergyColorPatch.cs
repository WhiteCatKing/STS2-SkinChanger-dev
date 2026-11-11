using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class FrameworkCharacterEnergyColorPatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return (from type in AccessTools.AllTypes()
			where !type.IsAbstract && typeof(CharacterModel).IsAssignableFrom(type)
			select AccessTools.PropertyGetter(type, "EnergyLabelOutlineColor") into method
			where method != null
			select method).Cast<MethodBase>().Distinct();
	}

	[HarmonyPrefix]
	[HarmonyPriority(600)]
	private static bool Prefix(CharacterModel __instance, ref Color __result)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!FrameworkSkinRuntime.TryGetCharacterColor(__instance, "EnergyLabelColor", out var color))
		{
			return true;
		}
		__result = color;
		return false;
	}
}
