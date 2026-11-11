using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(SfxCmd), "PlayDamage", new Type[]
{
	typeof(MonsterModel),
	typeof(int)
})]
internal static class EnemyNativeDamageSfxPatch
{
	private static bool Prefix(MonsterModel? monster)
	{
		return !Entry.NativePresentation.ShouldSuppressDamageSfx(monster);
	}
}
