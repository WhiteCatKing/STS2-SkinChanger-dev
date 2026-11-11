using CznEnemySkin.Domain;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NBestiaryEntry), "_Ready")]
internal static class BestiaryEntryReadyPatch
{
	private static void Postfix(NBestiaryEntry __instance)
	{
		if (BestiaryProfileResolver.TryResolve(__instance, out EnemyReplacementProfile profile))
		{
			BestiaryProfileResolver.RefreshEntryLabel(__instance, profile);
		}
	}
}
