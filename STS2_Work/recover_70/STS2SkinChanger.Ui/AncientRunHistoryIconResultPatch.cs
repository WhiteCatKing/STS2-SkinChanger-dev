using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class AncientRunHistoryIconResultPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(AncientEventModel __instance, ref Texture2D __result)
	{
		string text = ((AbstractModel)__instance).Id.Entry.ToLowerInvariant();
		string imagePath = ImageHelper.GetImagePath("ui/run_history/" + text + ".png");
		AncientCompendiumEntry.ReplaceAncientIcon(__instance, imagePath, ref __result);
	}
}
