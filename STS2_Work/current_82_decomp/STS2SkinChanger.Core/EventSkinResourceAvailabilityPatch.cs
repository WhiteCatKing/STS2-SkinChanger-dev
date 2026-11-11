using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class EventSkinResourceAvailabilityPatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return new global::_003C_003Ez__ReadOnlyArray<MethodBase>(new MethodBase[2]
		{
			AccessTools.PropertyGetter(typeof(EventModel), "HasVfx"),
			AccessTools.PropertyGetter(typeof(EventModel), "HasPhobiaModePortrait")
		});
	}

	private static bool Prefix(EventModel __instance, MethodBase __originalMethod, ref bool __result)
	{
		if (!EventSkinRuntime.IsManaged(__instance))
		{
			return true;
		}
		string text = ((AbstractModel)__instance).Id.Entry.ToLowerInvariant();
		string groupId = EventSkinPolicy.GroupId(text);
		string path = ((__originalMethod.Name == "get_HasVfx") ? ("res://scenes/vfx/events/" + text + "_vfx.tscn") : ("res://images/events/" + text + "_phobia_mode.png"));
		__result = SkinService.Catalog.HasEventResource(groupId, SkinService.Config.GetSelection(groupId), path);
		return false;
	}
}
