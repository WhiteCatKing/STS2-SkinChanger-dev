using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(MonsterModel), "PerformMove")]
internal static class EnemyMovePresentationScopePatch
{
	private static void Prefix(MonsterModel __instance, out IDisposable? __state)
	{
		__state = Entry.NativePresentation.Capture(__instance);
	}

	private static void Postfix(IDisposable? __state)
	{
		__state?.Dispose();
	}
}
