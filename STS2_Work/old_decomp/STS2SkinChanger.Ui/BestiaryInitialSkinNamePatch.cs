using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NBestiary), "OnSubmenuOpened")]
internal static class BestiaryInitialSkinNamePatch
{
	[HarmonyPostfix]
	private static void Postfix(NBestiary __instance)
	{
		ContextualSkinControls.ScheduleInitialBestiaryMonsterNameRefresh(__instance);
	}
}
