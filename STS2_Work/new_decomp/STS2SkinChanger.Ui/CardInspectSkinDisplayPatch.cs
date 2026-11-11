using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NInspectCardScreen), "UpdateCardDisplay")]
internal static class CardInspectSkinDisplayPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(NInspectCardScreen __instance)
	{
		CardInspectSkinControls.Sync(__instance);
	}
}
