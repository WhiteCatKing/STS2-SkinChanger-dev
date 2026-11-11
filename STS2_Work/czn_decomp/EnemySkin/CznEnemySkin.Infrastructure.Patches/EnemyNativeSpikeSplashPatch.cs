using System;
using CznEnemySkin.Domain;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NSpikeSplashVfx), "Create", new Type[]
{
	typeof(Creature),
	typeof(VfxColor)
})]
internal static class EnemyNativeSpikeSplashPatch
{
	private static bool Prefix(ref NSpikeSplashVfx? __result)
	{
		if (!Entry.NativePresentation.ShouldSuppressVfxFeature(EnemyNativeVfxFeature.SpikeSplash))
		{
			return true;
		}
		__result = null;
		return false;
	}
}
