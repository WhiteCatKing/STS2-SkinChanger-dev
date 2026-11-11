using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NBossMapPoint), "_Ready")]
internal static class BossMapPointSkinPresentationPatch
{
	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(NBossMapPoint __instance)
	{
		CharacterAppearanceRuntime.RefreshBossMapPointPresentation(__instance);
	}
}
