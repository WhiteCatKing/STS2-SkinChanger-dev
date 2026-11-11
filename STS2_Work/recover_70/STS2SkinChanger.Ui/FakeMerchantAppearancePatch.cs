using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NFakeMerchant), "_Ready")]
internal static class FakeMerchantAppearancePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static bool Prefix(NFakeMerchant __instance)
	{
		if (MerchantRuntimeAppearance.IsMerchantPreviewRoot((Node)(object)__instance))
		{
			return false;
		}
		SkinService.MountFakeMerchantEventArt();
		long lease = CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunContext(new _003C_003Ez__ReadOnlySingleElementList<string>("fake_merchant_monster"), "假商人场景", null);
		RoomRuntimeScopeOwnership.Record(__instance, lease);
		return true;
	}

	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(NFakeMerchant __instance)
	{
		if (MerchantRuntimeAppearance.IsMerchantPreviewRoot((Node)(object)__instance))
		{
			PartTakeoverVisualScope.Reconcile((Node)(object)__instance, "fake_merchant_monster");
			return;
		}
		try
		{
			string selectedFullRuntimeProvider = SkinService.GetSelectedFullRuntimeProvider("fake_merchant_monster");
			ManagedSkinModLoader.ReplaySelectedRoomReadyBehaviors((Node)(object)__instance, selectedFullRuntimeProvider);
		}
		catch (Exception ex)
		{
			ModLog.Warn("重放假商人外观初始化失败：" + ex.GetBaseException().Message);
		}
	}
}
