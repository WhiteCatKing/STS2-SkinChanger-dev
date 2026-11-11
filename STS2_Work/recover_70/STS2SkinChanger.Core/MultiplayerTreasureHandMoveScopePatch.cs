using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class MultiplayerTreasureHandMoveScopePatch
{
	[HarmonyPostfix]
	private static void Postfix(NHandImage __instance, RelicPickingFightMove move)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerTreasureHandAppearance.Remember(__instance, move);
	}

	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(NHandImage), "SetTextureToFightMove", (Type[])null, (Type[])null) ?? throw new MissingMethodException(typeof(NHandImage).FullName, "SetTextureToFightMove");
	}

	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void Prefix(NHandImage __instance, out IDisposable? __state)
	{
		__state = ((__instance.Player == null) ? null : MultiplayerSkinSync.BeginPlayerSelectionScope(__instance.Player.NetId));
	}

	private static Exception? Finalizer(Exception? __exception, IDisposable? __state)
	{
		__state?.Dispose();
		return __exception;
	}
}
