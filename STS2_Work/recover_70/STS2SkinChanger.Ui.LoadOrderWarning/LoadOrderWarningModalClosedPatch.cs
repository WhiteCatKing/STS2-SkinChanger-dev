using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace STS2SkinChanger.Ui.LoadOrderWarning;

[HarmonyPatch(typeof(NModalContainer), "Clear")]
internal static class LoadOrderWarningModalClosedPatch
{
	private static void Postfix()
	{
		LoadOrderWarningController.TryShow();
	}
}
