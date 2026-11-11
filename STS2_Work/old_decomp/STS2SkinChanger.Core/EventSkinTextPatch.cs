using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(LocTable), "GetRawText")]
internal static class EventSkinTextPatch
{
	[HarmonyPriority(0)]
	private static bool Prefix(string ____name, string key, ref string __result)
	{
		if (____name != "events" || SkinService.Catalog == null)
		{
			return true;
		}
		if (!SkinService.Catalog.TryResolveEventText(key, ModLocalization.CurrentLanguage, SkinService.Config.Selections, out string text))
		{
			return true;
		}
		__result = text;
		return false;
	}
}
