using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(PreloadManager), "LoadRoomAssets")]
internal static class RoomBoundaryCacheReleasePatch
{
	private static void Prefix()
	{
		try
		{
			Entry.ActionPresentation.ReleaseCachedResources();
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Could not release cached enemy presentation resources: " + ex.Message, 1);
		}
	}
}
