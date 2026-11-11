using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCard), "UpdateVisuals")]
internal static class CardPreviewModeCapturePatch
{
	[HarmonyPriority(800)]
	private static void Prefix(NCard __instance, CardPreviewMode previewMode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		CardSkinControls.RememberPreviewMode(__instance, previewMode);
	}
}
