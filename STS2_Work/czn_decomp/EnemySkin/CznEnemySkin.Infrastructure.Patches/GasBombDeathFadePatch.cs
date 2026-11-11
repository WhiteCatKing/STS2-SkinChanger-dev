using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class GasBombDeathFadePatch
{
	private const string ProfileId = "devoured_being_for_gas_bomb";

	private static void Postfix(GasBomb __instance, ref bool __result)
	{
		NCreature creatureNode = ((MonsterModel)__instance).Creature.GetCreatureNode();
		if (creatureNode != null && EnemyVisualHostAdapter.TryResolveAppliedProfile(creatureNode, out EnemyReplacementProfile profile) && profile.Id == "devoured_being_for_gas_bomb")
		{
			__result = true;
		}
	}
}
