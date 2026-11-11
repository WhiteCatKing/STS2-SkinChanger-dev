using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NRun), "_Ready")]
internal static class InRunCharacterAppearanceRuntimePatch
{
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void Prefix(NRun __instance)
	{
		CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunCharacters(__instance);
	}

	private static void Postfix(NRun __instance)
	{
		MultiplayerSkinSync.AttachToRun();
		if (((Node)__instance).GetNodeOrNull<CharacterAppearanceRuntimeNode>(NodePath.op_Implicit("SkinChangerAppearanceRuntime")) == null)
		{
			CharacterAppearanceRuntimeNode characterAppearanceRuntimeNode = new CharacterAppearanceRuntimeNode();
			((Node)characterAppearanceRuntimeNode).Name = StringName.op_Implicit("SkinChangerAppearanceRuntime");
			((Node)characterAppearanceRuntimeNode).ProcessMode = (ProcessModeEnum)3;
			((Node)__instance).AddChild((Node)(object)characterAppearanceRuntimeNode, false, (InternalMode)0);
		}
	}
}
