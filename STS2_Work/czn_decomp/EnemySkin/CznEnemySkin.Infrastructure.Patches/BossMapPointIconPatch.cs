using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NBossMapPoint), "_Ready")]
internal static class BossMapPointIconPatch
{
	private static void Postfix(NBossMapPoint __instance)
	{
		Entry.BossIcons.ApplyMapPoint(__instance);
	}
}
