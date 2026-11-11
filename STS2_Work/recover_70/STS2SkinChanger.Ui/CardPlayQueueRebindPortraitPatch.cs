using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCardPlayQueue), "UpdateCardBeforeExecution")]
internal static class CardPlayQueueRebindPortraitPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(NCardPlayQueue __instance)
	{
		CardSkinControls.ReapplyQueuedCardPortraits(__instance);
	}
}
