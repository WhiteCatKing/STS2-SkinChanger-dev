using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCard), "_ExitTree")]
internal static class StatefulCardTreeExitPatch
{
	private static void Prefix(NCard __instance)
	{
		StatefulCardArtView.Release(__instance);
	}
}
