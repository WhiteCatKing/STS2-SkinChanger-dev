using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NClickableControl), "OnReleaseHandler")]
internal static class BestiaryEntranceReleasePatch
{
	private static void Prefix(NClickableControl __instance)
	{
		BestiaryEntranceSwitcher.RememberReleased(__instance);
	}
}
