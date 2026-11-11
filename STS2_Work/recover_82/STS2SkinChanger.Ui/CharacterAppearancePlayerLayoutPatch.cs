using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCombatRoom), "PositionPlayersAndPets")]
internal static class CharacterAppearancePlayerLayoutPatch
{
	[HarmonyPriority(800)]
	private static void Prefix(float scaling, bool fullyCenterPlayers)
	{
		CharacterAppearanceRuntime.CapturePlayerAndPetLayout(scaling, fullyCenterPlayers);
	}
}
