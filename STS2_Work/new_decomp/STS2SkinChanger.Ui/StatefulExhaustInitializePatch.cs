using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NExhaustPileButton), "Initialize")]
internal static class StatefulExhaustInitializePatch
{
	private static void Postfix(NExhaustPileButton __instance)
	{
		if (((Node)__instance).GetNodeOrNull<StatefulExhaustIcon>(NodePath.op_Implicit("SCStatefulExhaust")) == null)
		{
			StatefulExhaustIcon statefulExhaustIcon = new StatefulExhaustIcon();
			((Node)statefulExhaustIcon).Name = StringName.op_Implicit("SCStatefulExhaust");
			statefulExhaustIcon.Button = __instance;
			((Node)__instance).AddChild((Node)(object)statefulExhaustIcon, false, (InternalMode)0);
		}
	}
}
