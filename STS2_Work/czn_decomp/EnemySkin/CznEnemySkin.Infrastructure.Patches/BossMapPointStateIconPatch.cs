using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NBossMapPoint), "RefreshColorInstantly")]
internal static class BossMapPointStateIconPatch
{
	private static void Postfix(NBossMapPoint __instance)
	{
		Entry.BossIcons.RefreshMapPointState(__instance);
	}
}
