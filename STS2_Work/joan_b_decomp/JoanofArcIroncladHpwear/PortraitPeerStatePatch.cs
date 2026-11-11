using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(NMultiplayerPlayerState), "_Ready")]
internal static class PortraitPeerStatePatch
{
	[HarmonyPostfix]
	private static void Postfix(NMultiplayerPlayerState __instance)
	{
		PortraitSync.Timer((Node)(object)__instance, delegate
		{
			PortraitSync.RunIcon(((Node)__instance).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%CharacterIcon")), __instance.Player);
		});
	}
}
