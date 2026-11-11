using System;
using CznEnemySkin.Domain;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NLineBurstVfx), "Create", new Type[] { typeof(Creature) })]
internal static class EnemyNativeLineBurstPatch
{
	private static bool Prefix(ref NLineBurstVfx? __result)
	{
		if (!Entry.NativePresentation.ShouldSuppressVfxFeature(EnemyNativeVfxFeature.LineBurst))
		{
			return true;
		}
		__result = null;
		return false;
	}
}
