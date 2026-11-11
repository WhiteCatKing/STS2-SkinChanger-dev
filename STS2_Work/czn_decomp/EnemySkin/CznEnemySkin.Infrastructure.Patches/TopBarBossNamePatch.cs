using HarmonyLib;
using MegaCrit.sts2.Core.Nodes.TopBar;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NTopBarBossIcon), "OnFocus")]
internal static class TopBarBossNamePatch
{
	private static bool Prefix(NTopBarBossIcon __instance)
	{
		return !Entry.BossIcons.TryShowTopBarHoverTip(__instance);
	}
}
