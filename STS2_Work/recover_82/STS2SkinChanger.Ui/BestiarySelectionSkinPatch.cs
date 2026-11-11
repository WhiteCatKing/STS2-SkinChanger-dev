using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NBestiary), "SelectMonster")]
internal static class BestiarySelectionSkinPatch
{
	private static void Postfix(NBestiary __instance, NBestiaryEntry entry)
	{
		if (!ContextualSkinControls.IsRefreshingMonsterDisplay)
		{
			ContextualSkinControls.ShowMonster(__instance, entry);
		}
	}
}
