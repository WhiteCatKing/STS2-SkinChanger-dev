using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(NCreature), "StartDeathAnim")]
internal static class DeathReplayPatch
{
	[HarmonyPrefix]
	private static void Prefix(NCreature __instance)
	{
		try
		{
			HpWear.PrepareDeath(__instance);
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("death preparation failed: " + ex.Message, 1);
		}
	}
}
