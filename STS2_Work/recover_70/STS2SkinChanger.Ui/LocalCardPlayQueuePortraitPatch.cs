using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCardPlayQueue), "OnLocalCardPlayed")]
internal static class LocalCardPlayQueuePortraitPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(NCardPlayQueue __instance)
	{
		CardSkinControls.ReapplyQueuedCardPortraits(__instance);
	}
}
