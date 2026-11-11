using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NMultiplayerPlayerState), "_Ready")]
internal static class MultiplayerPlayerStateIconScopePatch
{
	private static void Prefix(NMultiplayerPlayerState __instance, out IDisposable? __state)
	{
		__state = ((__instance.Player == null) ? null : MultiplayerSkinSync.BeginPlayerSelectionScope(__instance.Player.NetId));
	}

	private static void Postfix(NMultiplayerPlayerState __instance, IDisposable? __state)
	{
		__state?.Dispose();
		if (__instance.Player != null)
		{
			MultiplayerSkinSync.RequestIconRefresh(__instance.Player.NetId);
		}
	}
}
