using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(OrbModel), "get_SpritePath")]
internal static class FrameworkOrbSpritePatch
{
	[HarmonyPrefix]
	private static bool Prefix(OrbModel __instance, ref string __result)
	{
		if (!FrameworkSkinRuntime.TryGetOrbResource(__instance, "CustomSpritePath", out string path))
		{
			return true;
		}
		__result = path;
		return false;
	}
}
