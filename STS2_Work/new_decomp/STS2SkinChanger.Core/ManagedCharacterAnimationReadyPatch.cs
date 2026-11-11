using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NCreature), "_Ready")]
internal static class ManagedCharacterAnimationReadyPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(NCreature __instance)
	{
		Creature entity = __instance.Entity;
		ManagedCharacterAnimationBridge.TryDrive(__instance, (entity != null && entity.IsDead) ? "Dead" : "Idle");
	}
}
