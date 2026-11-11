using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class MultiplayerOrbIconScopePatch
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
			string text = "res://images/orbs/" + ((AbstractModel)__instance).Id.Entry.ToLowerInvariant() + ".png";
			__state = new MultiplayerRestSiteCreateScopePatch.RoomScope(MultiplayerSkinSync.BeginPlayerRuntimeScope(owner, text), RemoteRoomSceneLease.BeginTexture(owner, text, "充能球图标"));
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
