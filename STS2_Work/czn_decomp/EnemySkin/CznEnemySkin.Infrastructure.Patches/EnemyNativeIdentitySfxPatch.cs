using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(SfxCmd), "Play", new Type[]
{
	typeof(string),
	typeof(float)
})]
internal static class EnemyNativeIdentitySfxPatch
{
	private static bool Prefix(string sfx)
	{
		return !Entry.NativePresentation.ShouldSuppressAudio(sfx);
	}
}
