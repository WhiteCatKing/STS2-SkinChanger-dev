using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.sts2.Core.Nodes.TopBar;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(NTopBarPortrait), "Initialize")]
internal static class PortraitTopBarPatch
{
	[HarmonyPostfix]
	private static void Postfix(NTopBarPortrait __instance, Player player)
	{
		PortraitSync.Timer((Node)(object)__instance, delegate
		{
			foreach (TextureRect item in PortraitSync.Descendants<TextureRect>((Node)(object)__instance))
			{
				PortraitSync.RunIcon(item, player);
			}
		});
	}
}
