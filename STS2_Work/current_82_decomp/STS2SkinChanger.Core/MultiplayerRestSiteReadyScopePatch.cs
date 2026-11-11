using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
internal static class MultiplayerRestSiteReadyScopePatch
{
	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(NRestSiteCharacter __instance)
	{
		ManagedSlotVisibilityBridge.BindPlayerScene((Node)(object)__instance, __instance.Player);
	}

	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void Prefix(NRestSiteCharacter __instance, out IDisposable? __state)
	{
		__state = ((__instance.Player == null) ? null : MultiplayerSkinSync.BeginPlayerRuntimeScope(__instance.Player, __instance.Player.Character.RestSiteAnimPath));
	}

	private static Exception? Finalizer(Exception? __exception, IDisposable? __state)
	{
		__state?.Dispose();
		return __exception;
	}
}
