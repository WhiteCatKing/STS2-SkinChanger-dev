using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NEventLayout), "SetPortrait")]
internal static class EventSkinPortraitTrackingPatch
{
	private static void Postfix(NEventLayout __instance, Texture2D portrait, Texture2D? phobiaModePortrait)
	{
		EventSkinRuntime.RememberPortrait(__instance, portrait, phobiaModePortrait);
	}
}
