using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NCreature), "_Ready")]
internal static class MultiplayerCreatureReadyScopePatch
{
	[HarmonyPriority(800)]
	private static void Prefix(NCreature __instance, out IDisposable? __state)
	{
		__state = ((__instance.Entity == null) ? null : MultiplayerSkinSync.BeginCreatureRuntimeScope(__instance.Entity));
	}

	[HarmonyPriority(0)]
	private static void Postfix(NCreature __instance)
	{
		if (__instance.Entity != null)
		{
			CharacterAppearanceRuntime.ReplaySelectedCreatureNodeReady(__instance);
		}
	}

	private static Exception? Finalizer(Exception? __exception, IDisposable? __state)
	{
		__state?.Dispose();
		return __exception;
	}
}
