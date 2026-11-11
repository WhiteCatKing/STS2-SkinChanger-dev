using System;
using CznEnemySkin.Domain;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NHorizontalLinesVfx), "Create", new Type[]
{
	typeof(Color),
	typeof(double),
	typeof(bool)
})]
internal static class EnemyNativeHorizontalLinesPatch
{
	private static bool Prefix(ref NHorizontalLinesVfx? __result)
	{
		if (!Entry.NativePresentation.ShouldSuppressVfxFeature(EnemyNativeVfxFeature.HorizontalLines))
		{
			return true;
		}
		__result = null;
		return false;
	}
}
