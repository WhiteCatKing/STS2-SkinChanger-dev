using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(EncounterModel), "GetAssetPaths")]
internal static class EncounterGetAssetPathsPatch
{
	private static void Postfix(EncounterModel __instance, IRunState runState, ref IEnumerable<string> __result)
	{
		ActPresentationProfile profile2;
		IReadOnlyList<string> paths2;
		if (Entry.EncounterBattlefields.TryResolve(((AbstractModel)__instance).Id.Entry, out EncounterBattlefieldProfile profile) && Entry.Backgrounds.TryGetPreloadPaths(profile.Battlefield, out IReadOnlyList<string> paths))
		{
			__result = __result.Concat(paths).Distinct<string>(StringComparer.Ordinal);
		}
		else if (ActBackgroundPatchEligibility.TryResolve(__instance, runState.Act, out profile2) && Entry.Backgrounds.TryGetPreloadPaths(profile2.Battlefield, out paths2))
		{
			__result = __result.Concat(paths2).Distinct<string>(StringComparer.Ordinal);
		}
	}
}
