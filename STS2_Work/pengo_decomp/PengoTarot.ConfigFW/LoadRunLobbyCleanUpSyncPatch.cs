using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

namespace PengoTarot.ConfigFW;

[HarmonyPatch(typeof(LoadRunLobby), "CleanUp")]
public static class LoadRunLobbyCleanUpSyncPatch
{
	public static void Postfix(LoadRunLobby __instance)
	{
		ConfigFloatingWindow.OnLoadLobbyClosed(__instance.NetService);
	}
}
