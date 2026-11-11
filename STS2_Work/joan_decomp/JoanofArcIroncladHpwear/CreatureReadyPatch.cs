using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(NCreature), "_Ready")]
internal static class CreatureReadyPatch
{
	[HarmonyPostfix]
	private static void Postfix(NCreature __instance)
	{
		try
		{
			HpWear.OnCreatureReady(__instance);
		}
		catch (Exception value)
		{
			ModEntry.Log.Error($"creature visual initialization failed: {value}", 1);
		}
	}
}
