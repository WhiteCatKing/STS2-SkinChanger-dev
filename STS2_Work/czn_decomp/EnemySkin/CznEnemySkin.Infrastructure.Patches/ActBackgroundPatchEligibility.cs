using System;
using System.Reflection;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace CznEnemySkin.Infrastructure.Patches;

internal static class ActBackgroundPatchEligibility
{
	private static readonly MethodInfo? HasCustomBackgroundGetter = AccessTools.PropertyGetter(typeof(EncounterModel), "HasCustomBackground");

	private static bool _reportedMissingGetter;

	public static bool TryResolve(EncounterModel encounter, ActModel act, out ActPresentationProfile profile)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (!ActRoomScopeClassifier.TryClassify(encounter.RoomType, encounter.IsWeak, out var scope) || HasCustomBackground(encounter))
		{
			profile = null;
			return false;
		}
		return Entry.ActPresentations.TryResolve(((AbstractModel)act).Id.Entry, scope, out profile);
	}

	private static bool HasCustomBackground(EncounterModel encounter)
	{
		if (HasCustomBackgroundGetter == null)
		{
			if (!_reportedMissingGetter)
			{
				_reportedMissingGetter = true;
				Entry.Log.Warn("STS2 HasCustomBackground getter was not found; act background replacement is disabled conservatively.", 1);
			}
			return true;
		}
		try
		{
			object obj = HasCustomBackgroundGetter.Invoke(encounter, null);
			return obj is bool && (bool)obj;
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Could not inspect encounter background ownership; keeping the STS2 background [encounter=" + ((AbstractModel)encounter).Id.Entry + ", error=" + ex.GetBaseException().Message + "].", 1);
			return true;
		}
	}
}
