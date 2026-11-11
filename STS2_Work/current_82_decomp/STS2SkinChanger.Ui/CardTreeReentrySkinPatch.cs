using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCard), "_EnterTree")]
internal static class CardTreeReentrySkinPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(NCard __instance)
	{
		CardSkinControls.ReapplyCardAfterTreeEntry(__instance);
	}
}
