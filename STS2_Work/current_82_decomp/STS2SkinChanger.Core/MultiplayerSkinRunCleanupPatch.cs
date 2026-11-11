using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(RunManager), "CleanUp")]
internal static class MultiplayerSkinRunCleanupPatch
{
	private static void Prefix()
	{
		MultiplayerSkinSync.EndMultiplayerRunSession();
	}
}
