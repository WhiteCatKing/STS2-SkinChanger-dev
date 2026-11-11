using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCard), "ActivateRewardScreenGlow")]
internal static class StatefulRewardGlowPatch
{
	private static bool Prefix(NCard __instance)
	{
		StatefulCardArtRuntime.Runtime runtime = StatefulCardArtRuntime.For(__instance.Model);
		if (runtime != null && runtime.Enabled)
		{
			return !runtime.Setting("HideCardRewardRarityGlow", fallback: true);
		}
		return true;
	}
}
