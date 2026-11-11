using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace STS2SkinChanger.Ui;

[HarmonyPatch]
internal static class EventPreviewLifecyclePatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return new global::_003C_003Ez__ReadOnlyArray<MethodBase>(new MethodBase[4]
		{
			AccessTools.Method(typeof(NEventLayout), "_EnterTree", (Type[])null, (Type[])null),
			AccessTools.Method(typeof(NEventLayout), "_ExitTree", (Type[])null, (Type[])null),
			AccessTools.Method(typeof(NEventOptionButton), "_Ready", (Type[])null, (Type[])null),
			AccessTools.Method(typeof(NEventOptionButton), "OnRelease", (Type[])null, (Type[])null)
		});
	}

	private static bool Prefix(Node __instance)
	{
		return !EventCompendiumPreview.IsPreview(__instance);
	}
}
