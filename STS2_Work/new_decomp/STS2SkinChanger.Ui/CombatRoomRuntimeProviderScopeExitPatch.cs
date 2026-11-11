using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCombatRoom), "_ExitTree")]
internal static class CombatRoomRuntimeProviderScopeExitPatch
{
	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix()
	{
		CharacterAppearanceRuntime.ClearCombatRuntimeGroups();
	}
}
