using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCombatCardPile), "AnimOut")]
internal static class StatefulExhaustHidePatch
{
	private static bool Prefix(NCombatCardPile __instance)
	{
		if (__instance is NExhaustPileButton)
		{
			return !(((Node)__instance).GetNodeOrNull<StatefulExhaustIcon>(NodePath.op_Implicit("SCStatefulExhaust"))?.Active ?? false);
		}
		return true;
	}
}
