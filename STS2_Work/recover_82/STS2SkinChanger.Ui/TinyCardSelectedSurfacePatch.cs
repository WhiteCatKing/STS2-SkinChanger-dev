using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NTinyCard), "SetCard")]
internal static class TinyCardSelectedSurfacePatch
{
	[HarmonyPriority(0)]
	private static void Postfix(NTinyCard __instance, CardModel card)
	{
		CardSurfaceView.BindTinyCard(__instance, card);
	}
}
