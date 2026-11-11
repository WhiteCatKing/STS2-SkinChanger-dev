using System;
using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class WaterfallGiantRewardDelayPatch
{
	private static void Postfix(MonsterModel __instance, ref float __result)
	{
		WaterfallGiant val = (WaterfallGiant)(object)((__instance is WaterfallGiant) ? __instance : null);
		if (val != null && WaterfallGiantAnimatorAdapter.IsTerminalSequence(val))
		{
			__result = Math.Max(__result, WaterfallGiantAnimatorAdapter.GetRewardDelayOverride());
		}
	}
}
