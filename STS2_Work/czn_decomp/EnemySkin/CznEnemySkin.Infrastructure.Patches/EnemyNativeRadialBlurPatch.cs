using System;
using CznEnemySkin.Domain;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NCombatRoom), "RadialBlur", new Type[] { typeof(VfxPosition) })]
internal static class EnemyNativeRadialBlurPatch
{
	private static bool Prefix()
	{
		return !Entry.NativePresentation.ShouldSuppressVfxFeature(EnemyNativeVfxFeature.RadialBlur);
	}
}
