using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NTinyCard), "Set")]
internal static class TinyCardAnonymousSurfacePatch
{
	private static void Prefix(NTinyCard __instance)
	{
		CardSurfaceView.ForgetTinyCard(__instance);
	}
}
