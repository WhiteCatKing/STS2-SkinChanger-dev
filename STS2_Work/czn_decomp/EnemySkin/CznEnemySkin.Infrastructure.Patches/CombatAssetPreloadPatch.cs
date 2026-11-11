using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.TestSupport;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(PreloadManager), "GetCombatAssetPaths")]
internal static class CombatAssetPreloadPatch
{
	private static void Postfix(EncounterModel encounter, ref IEnumerable<string> __result)
	{
		if (TestMode.IsOn)
		{
			return;
		}
		try
		{
			HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
			foreach (var monstersWithSlot in encounter.MonstersWithSlots)
			{
				MonsterModel item = monstersWithSlot.Item1;
				if (!Entry.Replacements.TryResolveMonster(((AbstractModel)item).Id.Entry, out EnemyReplacementProfile profile))
				{
					continue;
				}
				foreach (string item2 in EnemyPresentationAssetPaths.ForProfile(profile, Entry.PresentationSettings.ActionEffectsEnabled))
				{
					hashSet.Add(item2);
				}
			}
			if (hashSet.Count != 0)
			{
				__result = __result.Concat(hashSet);
				Entry.EffectMaterials.WarmUpMaterials();
				Entry.Log.Info("Queued replacement presentation assets for the combat preload [encounter=" + ((AbstractModel)encounter).Id.Entry + ", " + $"paths={hashSet.Count}].", 1);
			}
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Could not extend the combat asset preload: " + ex.Message, 1);
		}
	}
}
