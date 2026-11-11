using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.RunHistoryScreen;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NMapPointHistoryEntry), "_Ready")]
internal static class RunHistoryBossIconPatch
{
	private static void Postfix(NMapPointHistoryEntry __instance)
	{
		Entry.BossIcons.ApplyRunHistory(__instance);
	}
}
