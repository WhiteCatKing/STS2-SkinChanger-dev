using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(NRemoteLobbyPlayer), "_Ready")]
internal static class PortraitLobbyRowPatch
{
	[HarmonyPostfix]
	private static void Postfix(NRemoteLobbyPlayer __instance)
	{
		PortraitSync.Timer((Node)(object)__instance, delegate
		{
			PortraitSync.RefreshLobbyRow(__instance);
		});
	}
}
