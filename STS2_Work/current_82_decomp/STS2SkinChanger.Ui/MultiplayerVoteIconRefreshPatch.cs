using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NMultiplayerVoteContainer), "RefreshPlayerVotes")]
internal static class MultiplayerVoteIconRefreshPatch
{
	private static void Postfix(NMultiplayerVoteContainer __instance)
	{
		ContextualSkinControls.RefreshMultiplayerVoteIcons(__instance);
	}
}
