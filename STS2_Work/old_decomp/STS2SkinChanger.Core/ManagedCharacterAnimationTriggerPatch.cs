using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NCreature), "SetAnimationTrigger")]
internal static class ManagedCharacterAnimationTriggerPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(NCreature __instance, string trigger)
	{
		ManagedCharacterAnimationBridge.TryDrive(__instance, trigger);
	}
}
