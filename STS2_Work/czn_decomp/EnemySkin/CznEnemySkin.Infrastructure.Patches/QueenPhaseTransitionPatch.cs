using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(Queen), "AfterDeath")]
internal static class QueenPhaseTransitionPatch
{
	private static void Postfix(Queen __instance, Creature creature)
	{
		if (creature.Monster is TorchHeadAmalgam && ((MonsterModel)__instance).Creature.IsAlive)
		{
			QueenAnimatorAdapter.TryEnterPhaseTwo(__instance);
		}
	}
}
