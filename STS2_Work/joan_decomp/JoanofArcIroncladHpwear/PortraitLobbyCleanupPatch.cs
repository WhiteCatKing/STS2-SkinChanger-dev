using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(NCharacterSelectScreen), "CleanUpLobby")]
internal static class PortraitLobbyCleanupPatch
{
	[HarmonyPostfix]
	private static void Postfix()
	{
		PortraitNetwork.Reset();
	}
}
