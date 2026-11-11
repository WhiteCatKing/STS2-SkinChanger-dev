using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(NMultiplayerVoteContainer), "RefreshPlayerVotes")]
internal static class PortraitVotePatch
{
	[HarmonyPostfix]
	private static void Postfix(NMultiplayerVoteContainer __instance)
	{
		PortraitSync.TrackVotePortraits(__instance);
	}
}
