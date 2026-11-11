using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(EncounterModel), "CreateBackground")]
internal static class BackgroundSkinCreatePatch
{
	[HarmonyPriority(0)]
	private static void Postfix(EncounterModel __instance, ActModel parentAct, Rng rng, ref NCombatBackground __result)
	{
		BackgroundSkinService.ApplyToResult(__instance, parentAct, rng, ref __result);
	}
}
