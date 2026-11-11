using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCard), "ReloadOverlay")]
internal static class CardBuiltInOverlayPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(NCard __instance)
	{
		CardModel model = __instance.Model;
		if (model != null && model.HasBuiltInOverlay)
		{
			CardSkinControls.ApplyBuiltInOverlay(__instance, ExternalCardVisualBridge.GetOwnership(__instance).Frame ? null : SkinService.GetCardPresentation(__instance.Model));
		}
	}
}
