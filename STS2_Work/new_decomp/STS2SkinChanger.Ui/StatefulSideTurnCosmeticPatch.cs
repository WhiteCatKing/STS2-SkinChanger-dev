using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Singleton;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(AbstractModel), "AfterSideTurnStart")]
internal static class StatefulSideTurnCosmeticPatch
{
	private static void Postfix(AbstractModel __instance)
	{
		if (__instance is MultiplayerScalingModel)
		{
			StatefulCombatEffects.ClearCurrent();
		}
	}
}
