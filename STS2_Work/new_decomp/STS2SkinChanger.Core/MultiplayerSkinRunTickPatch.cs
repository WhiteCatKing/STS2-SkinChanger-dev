using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NRun), "_Process")]
internal static class MultiplayerSkinRunTickPatch
{
	private static void Postfix(double delta)
	{
		MultiplayerSkinSync.Tick(delta);
	}
}
