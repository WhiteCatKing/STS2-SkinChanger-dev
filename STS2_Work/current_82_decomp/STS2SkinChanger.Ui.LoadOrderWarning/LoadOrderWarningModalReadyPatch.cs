using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace STS2SkinChanger.Ui.LoadOrderWarning;

[HarmonyPatch(typeof(NModalContainer), "_Ready")]
internal static class LoadOrderWarningModalReadyPatch
{
	private static void Postfix()
	{
		LoadOrderWarningController.TryShow();
	}
}
