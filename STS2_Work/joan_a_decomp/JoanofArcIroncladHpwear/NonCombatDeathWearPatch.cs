using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(CreatureCmd), "KillWithoutCheckingWinCondition")]
internal static class NonCombatDeathWearPatch
{
	[HarmonyPostfix]
	private static void Postfix(Creature creature, ref Task __result)
	{
		if (__result != null && ((creature != null) ? creature.Player : null) != null)
		{
			__result = AfterDeath(__result, creature);
		}
	}

	private static async Task AfterDeath(Task original, Creature creature)
	{
		await original;
		try
		{
			if (creature.IsDead)
			{
				HpWear.OnNonCombatDeath(creature);
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("noncombat death wear skipped: " + ex.Message, 1);
		}
	}
}
