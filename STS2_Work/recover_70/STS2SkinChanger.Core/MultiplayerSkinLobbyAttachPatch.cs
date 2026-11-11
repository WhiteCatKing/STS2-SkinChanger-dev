using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NCharacterSelectScreen), "AfterInitialized")]
internal static class MultiplayerSkinLobbyAttachPatch
{
	private static void Postfix(NCharacterSelectScreen __instance)
	{
		MultiplayerSkinSync.AttachToLobby(__instance.Lobby);
	}
}
