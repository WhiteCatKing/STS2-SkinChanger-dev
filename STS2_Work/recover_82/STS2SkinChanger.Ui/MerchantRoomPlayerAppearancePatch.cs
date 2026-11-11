using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NMerchantRoom), "_Ready")]
internal static class MerchantRoomPlayerAppearancePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static bool Prefix(NMerchantRoom __instance)
	{
		return !MerchantRuntimeAppearance.IsMerchantPreviewRoot((Node)(object)__instance);
	}

	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(NMerchantRoom __instance)
	{
		if (MerchantRuntimeAppearance.IsMerchantPreviewRoot((Node)(object)__instance))
		{
			PartTakeoverVisualScope.Reconcile((Node)(object)__instance, "merchant");
			return;
		}
		Player localPlayer = CharacterAppearanceRuntime.GetLocalPlayer();
		if (localPlayer == null || __instance.PlayerVisuals.Count == 0)
		{
			return;
		}
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)localPlayer.Character).Id.Entry, ((object)localPlayer.Character).GetType().Name);
		if (skinGroup != null)
		{
			MerchantRuntimeAppearance.ApplyLocalPlayerTransform(__instance.PlayerVisuals[0], skinGroup.Id);
			string selectedLocalCharacterProvider = MerchantRuntimeAppearance.GetSelectedLocalCharacterProvider();
			foreach (var item in ManagedSkinModLoader.ReplaySelectedRoomReadyBehaviors((Node)(object)__instance, selectedLocalCharacterProvider))
			{
				MerchantRuntimeAppearance.TrackProviderRoots(__instance.PlayerVisuals[0], item.AddedRoots);
			}
			MerchantRuntimeAppearance.ApplyLocalPlayerTransform(__instance.PlayerVisuals[0], skinGroup.Id);
		}
		PartTakeoverVisualScope.Reconcile((Node)(object)__instance, "merchant");
	}
}
