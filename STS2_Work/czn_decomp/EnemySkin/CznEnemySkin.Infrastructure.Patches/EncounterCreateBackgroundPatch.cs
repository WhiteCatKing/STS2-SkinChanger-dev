using System;
using CznEnemySkin.Domain;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(EncounterModel), "CreateBackground")]
internal static class EncounterCreateBackgroundPatch
{
	private static bool Prefix(EncounterModel __instance, ActModel parentAct, Rng rng, ref NCombatBackground __result)
	{
		if (Entry.EncounterBattlefields.TryResolve(((AbstractModel)__instance).Id.Entry, out EncounterBattlefieldProfile profile))
		{
			if (string.Equals(((AbstractModel)__instance).Id.Entry, "KAISER_CRAB_BOSS", StringComparison.Ordinal))
			{
				return true;
			}
			if (!Entry.Backgrounds.TryCreate(profile.Battlefield, rng, out NCombatBackground background))
			{
				return true;
			}
			__result = background;
			return false;
		}
		if (!ActBackgroundPatchEligibility.TryResolve(__instance, parentAct, out ActPresentationProfile profile2) || !Entry.Backgrounds.TryCreate(profile2.Battlefield, rng, out NCombatBackground background2))
		{
			return true;
		}
		__result = background2;
		return false;
	}

	private static void Postfix(EncounterModel __instance, Rng rng, NCombatBackground __result)
	{
		if (string.Equals(((AbstractModel)__instance).Id.Entry, "KAISER_CRAB_BOSS", StringComparison.Ordinal) && Entry.EncounterBattlefields.TryResolve(((AbstractModel)__instance).Id.Entry, out EncounterBattlefieldProfile profile))
		{
			Entry.KaiserCrabBattlefield.TryApply(__result, profile.Battlefield, rng);
		}
	}
}
