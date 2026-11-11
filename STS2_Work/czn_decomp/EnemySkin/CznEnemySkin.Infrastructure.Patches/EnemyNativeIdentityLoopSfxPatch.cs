using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(SfxCmd), "PlayLoop", new Type[]
{
	typeof(Creature),
	typeof(string),
	typeof(string),
	typeof(float)
})]
internal static class EnemyNativeIdentityLoopSfxPatch
{
	private static bool Prefix(string sfx)
	{
		return !Entry.NativePresentation.ShouldSuppressAudio(sfx);
	}
}
