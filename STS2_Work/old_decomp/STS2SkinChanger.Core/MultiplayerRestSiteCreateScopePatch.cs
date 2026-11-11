using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NRestSiteCharacter), "Create")]
internal static class MultiplayerRestSiteCreateScopePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void Prefix(Player player, out IDisposable? __state)
	{
		__state = MultiplayerSkinSync.BeginPlayerRuntimeScope(player, player.Character.RestSiteAnimPath);
	}

	private static Exception? Finalizer(Exception? __exception, IDisposable? __state)
	{
		__state?.Dispose();
		return __exception;
	}
}
