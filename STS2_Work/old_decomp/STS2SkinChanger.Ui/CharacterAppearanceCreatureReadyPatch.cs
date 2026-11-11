using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCreature), "_Ready")]
internal static class CharacterAppearanceCreatureReadyPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(NCreature __instance)
	{
		CharacterAppearanceRuntime.OnCreatureReady(__instance);
	}
}
