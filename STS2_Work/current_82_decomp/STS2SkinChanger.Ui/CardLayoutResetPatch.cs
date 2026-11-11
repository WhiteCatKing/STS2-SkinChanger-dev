using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2SkinChanger.Ui;

[HarmonyPatch]
internal static class CardLayoutResetPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(NCard), "Reload", (Type[])null, (Type[])null);
	}

	[HarmonyPriority(800)]
	private static void Prefix(NCard __instance)
	{
		CardSkinControls.RestoreBaselineLayout(__instance);
	}
}
