using System;
using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(Creature), "LoseHpInternal")]
internal static class SlimedBerserkerPhaseThresholdPatch
{
	private static void Postfix(Creature __instance, ValueProp props, DamageResult __result)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		MonsterModel monster = __instance.Monster;
		SlimedBerserker val = (SlimedBerserker)(object)((monster is SlimedBerserker) ? monster : null);
		if (val != null)
		{
			SlimedBerserkerAnimatorAdapter.TryEnterPhaseTwo(val, __result.UnblockedDamage > 0 && !((Enum)props).HasFlag((Enum)(object)(ValueProp)16));
		}
	}
}
