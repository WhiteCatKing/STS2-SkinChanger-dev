using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NFakeMerchant), "_Ready")]
internal static class FakeMerchantEventFramePatch
{
	private static void Postfix(NFakeMerchant __instance)
	{
		FakeMerchantEventFrame.Schedule(__instance);
	}
}
