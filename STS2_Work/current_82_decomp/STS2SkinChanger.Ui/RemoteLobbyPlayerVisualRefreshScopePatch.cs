using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NRemoteLobbyPlayer), "RefreshVisuals")]
internal static class RemoteLobbyPlayerVisualRefreshScopePatch
{
	private static void Prefix(NRemoteLobbyPlayer __instance, out IDisposable? __state)
	{
		__state = MultiplayerSkinSync.BeginPlayerSelectionScope(__instance.PlayerId);
	}

	private static void Postfix(NRemoteLobbyPlayer __instance, IDisposable? __state)
	{
		__state?.Dispose();
		MultiplayerSkinSync.RequestIconRefresh(__instance.PlayerId);
	}
}
