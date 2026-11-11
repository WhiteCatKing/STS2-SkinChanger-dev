using System;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NCreature), "SetDefaultScaleTo", new Type[]
{
	typeof(float),
	typeof(float)
})]
internal static class TestSubjectRespawnScalePatch
{
	private static bool Prefix(NCreature __instance, float size)
	{
		Creature entity = __instance.Entity;
		if (((entity != null) ? entity.Monster : null) is TestSubject && EnemyVisualHostAdapter.TryResolveAppliedProfile(__instance, out EnemyReplacementProfile profile) && string.Equals(profile.Id, "deathchrome_for_test_subject", StringComparison.Ordinal))
		{
			if (!Mathf.IsEqualApprox(size, 1.1f))
			{
				return !Mathf.IsEqualApprox(size, 1.2f);
			}
			return false;
		}
		return true;
	}
}
