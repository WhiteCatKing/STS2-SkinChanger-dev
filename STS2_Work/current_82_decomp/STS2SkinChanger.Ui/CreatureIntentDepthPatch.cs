using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCreature), "UpdateIntent")]
internal static class CreatureIntentDepthPatch
{
	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(NCreature __instance)
	{
		CreatureIntentDepth.Apply(__instance);
	}
}
