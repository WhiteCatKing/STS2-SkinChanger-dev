using System;
using CznEnemySkin.Domain;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(MonsterModel), "CreateVisuals")]
internal static class MonsterModelCreateVisualsPatch
{
	private static bool Prefix(MonsterModel __instance, ref NCreatureVisuals __result)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (!Entry.Replacements.TryResolveMonster(((AbstractModel)__instance).Id.Entry, out EnemyReplacementProfile profile) || !Entry.VisualHostAdapter.TryCreateReplacement(profile, ResolveCombatVisualOffset(__instance, profile), out NCreatureVisuals replacement))
		{
			return true;
		}
		__result = replacement;
		Entry.ActionPresentation.Prewarm(profile);
		return false;
	}

	private static Vector2 ResolveCombatVisualOffset(MonsterModel monster, EnemyReplacementProfile profile)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (profile.Id != "mountain_god_for_ceremonial_beast")
		{
			return Vector2.Zero;
		}
		try
		{
			EncounterModel encounter = monster.CombatState.Encounter;
			string text = ((encounter != null) ? ((AbstractModel)encounter).Id.Entry : null);
			if (text == null || !string.Equals(text, "CEREMONIAL_BEAST_BOSS", StringComparison.Ordinal) || !Entry.EncounterBattlefields.TryResolve(text, out EncounterBattlefieldProfile profile2) || !string.Equals(profile2.Battlefield.Source.BattlefieldId, "b_laino_boss_1", StringComparison.Ordinal))
			{
				return Vector2.Zero;
			}
			return new Vector2(-80f, -40f);
		}
		catch (InvalidOperationException)
		{
			return Vector2.Zero;
		}
	}
}
