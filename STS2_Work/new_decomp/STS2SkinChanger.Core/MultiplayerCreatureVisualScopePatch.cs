using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(Creature), "CreateVisuals")]
internal static class MultiplayerCreatureVisualScopePatch
{
	[HarmonyPriority(800)]
	private static void Prefix(Creature __instance, out IDisposable? __state)
	{
		__state = MultiplayerSkinSync.BeginCreatureRuntimeScope(__instance);
	}

	[HarmonyPriority(0)]
	private static void Postfix(Creature __instance, ref NCreatureVisuals? __result, IDisposable? __state)
	{
		if (__state != null)
		{
			ContextualSkinControls.ReplaceCreatedCreatureVisuals(__instance, ref __result);
		}
		ContextualSkinControls.ApplySelectedCreatureVisualPostfix(__instance, ref __result);
	}

	private static Exception? Finalizer(Exception? __exception, IDisposable? __state)
	{
		__state?.Dispose();
		return __exception;
	}
}
