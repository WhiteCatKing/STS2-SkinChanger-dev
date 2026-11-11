using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NCharacterSelectScreen), "_Process")]
internal static class MultiplayerSkinLobbyTickPatch
{
	private static void Postfix(double delta)
	{
		MultiplayerSkinSync.Tick(delta);
	}
}
