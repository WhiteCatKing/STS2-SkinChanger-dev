using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NRestSiteRoom), "_Ready")]
internal static class RestSitePlayerAppearancePatch
{
	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(NRestSiteRoom __instance)
	{
		string selectedLocalCharacterProvider = MerchantRuntimeAppearance.GetSelectedLocalCharacterProvider();
		ManagedSkinModLoader.ReplaySelectedRoomReadyBehaviors((Node)(object)__instance, selectedLocalCharacterProvider);
	}
}
