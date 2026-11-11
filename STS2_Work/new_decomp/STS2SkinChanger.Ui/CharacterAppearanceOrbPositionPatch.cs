using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Ui;

[HarmonyPatch]
internal static class CharacterAppearanceOrbPositionPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(NCreature), "SetOrbManagerPosition", (Type[])null, (Type[])null);
	}

	[HarmonyPriority(0)]
	private static void Postfix(NCreature __instance)
	{
		CharacterAppearanceRuntime.CorrectOrbPositionForCharacterTransform(__instance);
	}
}
