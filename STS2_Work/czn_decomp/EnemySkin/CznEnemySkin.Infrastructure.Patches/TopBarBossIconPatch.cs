using HarmonyLib;
using MegaCrit.sts2.Core.Nodes.TopBar;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NTopBarBossIcon), "RefreshBossIcon")]
internal static class TopBarBossIconPatch
{
	private static void Postfix(NTopBarBossIcon __instance)
	{
		Entry.BossIcons.ApplyTopBar(__instance);
	}
}
