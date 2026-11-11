using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(OrbModel), "get_IconPath")]
internal static class FrameworkOrbIconPatch
{
	[HarmonyPrefix]
	private static bool Prefix(OrbModel __instance, ref string __result)
	{
		return Apply(__instance, ref __result, "CustomIconPath");
	}

	private static bool Apply(OrbModel model, ref string result, string propertyName)
	{
		if (!FrameworkSkinRuntime.TryGetOrbResource(model, propertyName, out string path))
		{
			return true;
		}
		result = path;
		return false;
	}
}
