using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class WaterfallGiantRewardAnimationQueryPatch
{
	private static void Postfix(NCreature __instance, ref bool __result)
	{
		if (__result)
		{
			Creature entity = __instance.Entity;
			MonsterModel obj = ((entity != null) ? entity.Monster : null);
			WaterfallGiant val = (WaterfallGiant)(object)((obj is WaterfallGiant) ? obj : null);
			if (val != null && WaterfallGiantAnimatorAdapter.IsTerminalSequence(val))
			{
				__result = false;
			}
		}
	}
}
