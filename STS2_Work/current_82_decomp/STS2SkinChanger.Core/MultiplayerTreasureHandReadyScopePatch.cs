using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NHandImage), "_Ready")]
internal static class MultiplayerTreasureHandReadyScopePatch
{
	[HarmonyPostfix]
	private static void Postfix(NHandImage __instance)
	{
		MultiplayerTreasureHandAppearance.Remember(__instance, null);
	}

	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void Prefix(NHandImage __instance, out IDisposable? __state)
	{
		__state = ((__instance.Player == null) ? null : MultiplayerSkinSync.BeginPlayerSelectionScope(__instance.Player.NetId));
	}

	private static Exception? Finalizer(Exception? __exception, IDisposable? __state)
	{
		__state?.Dispose();
		return __exception;
	}
}
