using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NMuteInBackgroundHandler), "_Notification")]
internal static class SkinPopupBackgroundMutePatch
{
	private static bool Prefix(NMuteInBackgroundHandler __instance, int what)
	{
		return !ContextualSkinControls.ShouldIgnoreBackgroundMute(__instance, what);
	}
}
