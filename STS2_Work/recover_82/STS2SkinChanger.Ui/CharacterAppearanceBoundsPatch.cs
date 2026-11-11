using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Ui;

[HarmonyPatch]
internal static class CharacterAppearanceBoundsPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(NCreature), "UpdateBounds", new Type[1] { typeof(Node) }, (Type[])null);
	}

	[HarmonyPriority(800)]
	private static void Postfix(NCreature __instance, Node boundsContainer)
	{
		CharacterAppearanceRuntime.CorrectBoundsForVisualTransforms(__instance, boundsContainer);
	}
}
