using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(VfxCmd), "PlayOnCreatureCenter", new Type[]
{
	typeof(Creature),
	typeof(string)
})]
internal static class EnemyNativeCenteredVfxPatch
{
	private static bool Prefix(Creature target, string path)
	{
		return !Entry.NativePresentation.ShouldSuppressVfxPath(path);
	}
}
