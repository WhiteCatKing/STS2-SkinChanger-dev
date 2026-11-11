using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(StartRunLobby), "CleanUp")]
internal static class MultiplayerSkinLobbyCleanupPatch
{
	private static void Prefix(bool disconnectSession)
	{
		if (disconnectSession)
		{
			MultiplayerSkinSync.ResetConnectionState();
		}
	}
}
