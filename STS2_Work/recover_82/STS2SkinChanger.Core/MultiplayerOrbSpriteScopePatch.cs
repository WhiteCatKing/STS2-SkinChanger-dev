using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(OrbModel), "CreateSprite")]
internal static class MultiplayerOrbSpriteScopePatch
{
	[HarmonyPriority(800)]
	private static void Prefix(OrbModel __instance, out IDisposable? __state)
	{
		try
		{
			Player owner = __instance.Owner;
			if (((owner != null) ? owner.Character : null) == null)
			{
				__state = null;
				return;
			}
			string scenePath = ContextualSkinControls.CanonicalScenePath("orbs/orb_visuals/" + ((AbstractModel)__instance).Id.Entry.ToLowerInvariant());
			__state = new MultiplayerRestSiteCreateScopePatch.RoomScope(MultiplayerSkinSync.BeginPlayerRuntimeScope(owner, scenePath), RemoteRoomSceneLease.Begin(owner, scenePath, "充能球"));
		}
		catch
		{
			__state = null;
		}
	}

	private static Exception? Finalizer(Exception? __exception, IDisposable? __state)
	{
		__state?.Dispose();
		return __exception;
	}
}
