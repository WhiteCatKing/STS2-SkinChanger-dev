using System;
using System.Threading;
using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NMonsterDeathVfx), "Create", new Type[]
{
	typeof(NCreature),
	typeof(CancellationToken)
})]
internal static class EnemyOfficialDeathEffectFadePatch
{
	private static bool Prefix(NCreature creatureNode, ref NMonsterDeathVfx? __result)
	{
		if (!EnemyVisualHostAdapter.WasBodyHiddenForOfficialDeathEffect(creatureNode))
		{
			return true;
		}
		__result = null;
		return false;
	}
}
