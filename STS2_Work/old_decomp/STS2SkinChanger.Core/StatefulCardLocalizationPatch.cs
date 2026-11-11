using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(LocManager), "SetLanguage")]
internal static class StatefulCardLocalizationPatch
{
	private static void Postfix()
	{
		StatefulCardArtRuntime.RefreshLocalization();
	}
}
