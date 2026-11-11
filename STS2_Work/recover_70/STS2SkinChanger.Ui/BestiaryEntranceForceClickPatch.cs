using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NClickableControl), "ForceClick")]
internal static class BestiaryEntranceForceClickPatch
{
	private static void Prefix(NClickableControl __instance)
	{
		BestiaryEntranceSwitcher.RememberReleased(__instance);
	}
}
