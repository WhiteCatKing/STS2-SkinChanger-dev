using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(EventModel), "CreateVfx")]
internal static class EventSkinVfxPatch
{
	private static bool Prefix(EventModel __instance, ref Node2D __result)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!EventSkinRuntime.IsManaged(__instance))
		{
			return true;
		}
		string text = "res://scenes/vfx/events/" + ((AbstractModel)__instance).Id.Entry.ToLowerInvariant() + "_vfx.tscn";
		try
		{
			__result = SkinService.InstantiateRuntimeScene<Node2D>(EventSkinPolicy.GroupId(((AbstractModel)__instance).Id.Entry), text, (Action?)null);
			((GodotObject)__result).SetMeta(StringName.op_Implicit("sc_event_vfx_source"), Variant.op_Implicit(text));
			return false;
		}
		catch (Exception ex)
		{
			ModLog.Error("事件特效加载失败 " + ((AbstractModel)__instance).Id.Entry + "：" + ex.Message);
			return true;
		}
	}
}
