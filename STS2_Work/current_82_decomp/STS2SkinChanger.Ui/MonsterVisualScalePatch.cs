using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCreatureVisuals), "SetScaleAndHue")]
internal static class MonsterVisualScalePatch
{
	[HarmonyPriority(0)]
	private static void Postfix(NCreatureVisuals __instance)
	{
		ContextualSkinControls.ReapplyMonsterScaleAfterGameScale(__instance);
	}
}
