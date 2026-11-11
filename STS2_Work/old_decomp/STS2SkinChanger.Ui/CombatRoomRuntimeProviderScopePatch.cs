using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCombatRoom), "Create")]
internal static class CombatRoomRuntimeProviderScopePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void Prefix(ICombatRoomVisuals visuals)
	{
		CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnCombatRoom(visuals);
	}
}
