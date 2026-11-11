using System.Reflection;
using System.Runtime.Loader;
using HarmonyLib;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(AssemblyLoadContext), "LoadFromAssemblyPath")]
internal static class NativeFrameworkAssemblyLoadPatch
{
	private static bool Prefix(AssemblyLoadContext __instance, string __0, ref Assembly __result)
	{
		if (!NativeFrameworkAssemblyLoader.IsOriginalFrameworkPath(__0))
		{
			return true;
		}
		if (!NativeFrameworkAssemblyLoader.TryLoadCompatible(__instance, __0, out Assembly assembly))
		{
			return true;
		}
		__result = assembly;
		return false;
	}
}
