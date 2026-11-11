using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(LagavulinMatriarch), "AfterDamageReceived")]
internal static class LagavulinMatriarchPhaseTransitionPatch
{
	private static void Prefix(LagavulinMatriarch __instance, out bool __state)
	{
		__state = __instance.IsShellAwake;
	}

	private static void Postfix(LagavulinMatriarch __instance, bool __state)
	{
		if (!__state && __instance.IsShellAwake)
		{
			LagavulinMatriarchAnimatorAdapter.TryBeginPhaseTwo(__instance);
		}
	}
}
