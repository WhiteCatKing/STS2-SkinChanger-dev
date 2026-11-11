using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NetClientGameService), "Initialize")]
internal static class SkinChangerClientConnectionResetPatch
{
	private static void Prefix()
	{
		MultiplayerSkinSync.ResetConnectionState(clearOnlineCache: true);
	}
}
