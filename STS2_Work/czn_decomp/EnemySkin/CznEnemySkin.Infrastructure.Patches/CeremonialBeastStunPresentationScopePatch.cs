using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(CeremonialBeast), "SetStunned")]
internal static class CeremonialBeastStunPresentationScopePatch
{
	private static void Prefix(CeremonialBeast __instance, out IDisposable? __state)
	{
		__state = Entry.NativePresentation.Capture((MonsterModel)(object)__instance);
	}

	private static void Postfix(IDisposable? __state)
	{
		__state?.Dispose();
	}
}
