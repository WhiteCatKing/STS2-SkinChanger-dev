using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(StartRunLobby), "ChangeCharacter")]
internal static class MultiplayerLobbyCharacterChangedPatch
{
	private static void Postfix(StartRunLobby __instance, ulong playerId, bool isRandomCharacterResolution)
	{
		try
		{
			MultiplayerSkinSync.OnLobbyCharacterChanged(__instance, playerId, isRandomCharacterResolution);
		}
		catch (Exception ex)
		{
			ModLog.Warn("大厅角色改变后的皮肤同步失败，将在进入对局后重试：" + ex.GetBaseException().Message);
		}
	}
}
