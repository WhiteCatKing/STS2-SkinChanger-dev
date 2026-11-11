using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCard), "set_Model")]
internal static class CardLayoutModelRebindPatch
{
	[HarmonyPriority(800)]
	private static void Prefix(NCard __instance, CardModel? value)
	{
		if (__instance.Model != value)
		{
			CardSkinControls.ReleasePresentationForReuse(__instance);
		}
	}
}
