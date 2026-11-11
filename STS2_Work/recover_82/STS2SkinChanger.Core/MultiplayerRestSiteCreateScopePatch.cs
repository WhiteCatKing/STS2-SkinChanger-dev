using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NRestSiteCharacter), "Create")]
internal static class MultiplayerRestSiteCreateScopePatch
{
	internal sealed class RoomScope(IDisposable? playerScope, IDisposable? sceneLease) : IDisposable
	{
		public void Dispose()
		{
			playerScope?.Dispose();
			sceneLease?.Dispose();
		}
	}

	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void Prefix(Player player, out IDisposable? __state)
	{
		string restSiteAnimPath = player.Character.RestSiteAnimPath;
		__state = new RoomScope(MultiplayerSkinSync.BeginPlayerRuntimeScope(player, restSiteAnimPath), RemoteRoomSceneLease.Begin(player, restSiteAnimPath, "火堆"));
	}

	private static Exception? Finalizer(Exception? __exception, IDisposable? __state)
	{
		__state?.Dispose();
		return __exception;
	}
}
