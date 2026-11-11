using System;
using System.Diagnostics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NMerchantRoom), "Create")]
internal static class MerchantRoomCreateAppearancePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void Prefix(out long __state)
	{
		__state = CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunContext(new _003C_003Ez__ReadOnlySingleElementList<string>("merchant"), "商店", null);
	}

	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(ref NMerchantRoom? __result, long __state)
	{
		if (__result != null && GodotObject.IsInstanceValid((GodotObject)(object)__result))
		{
			RoomRuntimeScopeOwnership.Record(__result, __state);
			long timestamp = Stopwatch.GetTimestamp();
			if (!MerchantRuntimeAppearance.TryCreateCurrentMerchantRoom(__result, out NMerchantRoom currentRoom, out string error) || currentRoom == null)
			{
				ModLog.Warn("保留游戏预加载的商店房间：" + error);
				MerchantRuntimeAppearance.ScopePartTakeover((Node)(object)__result);
				return;
			}
			NMerchantRoom? obj = __result;
			__result = currentRoom;
			RoomRuntimeScopeOwnership.Record(currentRoom, __state);
			((GodotObject)obj).Free();
			MerchantRuntimeAppearance.ScopePartTakeover((Node)(object)currentRoom);
			TimeSpan elapsedTime = Stopwatch.GetElapsedTime(timestamp);
			ModLog.Info("商店房间已绕过启动预加载缓存并按当前选择创建：" + SkinService.Config.GetSelection("merchant") + $"（重建耗时={elapsedTime.TotalMilliseconds:F1} ms）");
		}
	}
}
