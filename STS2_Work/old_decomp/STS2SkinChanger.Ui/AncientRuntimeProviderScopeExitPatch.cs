using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Events;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NAncientEventLayout), "_ExitTree")]
internal static class AncientRuntimeProviderScopeExitPatch
{
	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix()
	{
		CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunCharacters();
	}
}
