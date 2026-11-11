using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(VfxCmd), "PlayOnCreature", new Type[]
{
	typeof(Creature),
	typeof(string)
})]
internal static class EnemyNativeCreatureVfxPatch
{
	private static bool Prefix(Creature target, string path)
	{
		return !Entry.NativePresentation.ShouldSuppressVfxPath(path);
	}
}
