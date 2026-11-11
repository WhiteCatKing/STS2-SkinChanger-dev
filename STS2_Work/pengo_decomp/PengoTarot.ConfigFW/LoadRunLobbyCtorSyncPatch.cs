using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

namespace PengoTarot.ConfigFW;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
public static class LoadRunLobbyCtorSyncPatch
{
	public static void Postfix(LoadRunLobby __instance)
	{
		ConfigFloatingWindow.RegisterSyncForLoadLobby(__instance.NetService);
	}
}
