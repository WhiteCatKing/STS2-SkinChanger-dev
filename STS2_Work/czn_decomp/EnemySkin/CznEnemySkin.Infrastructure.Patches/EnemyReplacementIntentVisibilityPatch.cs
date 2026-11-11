using System;
using CznEnemySkin.Infrastructure.Runtime;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NCreature), "UpdateBounds", new Type[] { typeof(Node) })]
internal static class EnemyReplacementIntentVisibilityPatch
{
	private static void Postfix(NCreature __instance)
	{
		try
		{
			EnemyIntentVisibilityGuard.OnBoundsUpdated(__instance);
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Could not apply replacement enemy intent visibility guard: " + ex.Message, 1);
		}
	}
}
