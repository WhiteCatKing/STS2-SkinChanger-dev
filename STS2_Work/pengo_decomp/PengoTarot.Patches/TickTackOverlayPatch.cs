using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NCreature), "_Ready")]
public static class TickTackOverlayPatch
{
	internal const string FlashSfx = "event:/sfx/ui/clicks/ui_checkbox_on";

	internal const float MinFlashInterval = 0.1f;

	internal const float MaxFlashInterval = 0.5f;

	internal const float OverlayAlpha = 0.25f;

	private static void Postfix(NCreature __instance)
	{
		TickTackOverlayDriver tickTackOverlayDriver = new TickTackOverlayDriver(__instance);
		GodotTreeExtensions.AddChildSafely((Node)(object)__instance, (Node)(object)tickTackOverlayDriver);
	}
}
