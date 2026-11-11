using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CustomSkeletonLoader.Patches;

[HarmonyPatch(typeof(AssetCache))]
internal static class AssetCacheLoadAssetPatch
{
	[HarmonyPatch("LoadAsset")]
	[HarmonyPrefix]
	internal static bool Prefix(string path, ref Resource __result)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		if (!path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		string text = SkeletonLoader.ResolveNameFromPath(path);
		ModLogger.Debug("[CustomSkeletonLoader] Cache intercepting: " + path + " -> " + text);
		if (string.IsNullOrEmpty(text))
		{
			return true;
		}
		SkeletonRegistry.Record(text, path);
		MegaSkeletonDataResource val = SkeletonLoader.TryBuildMegaSkeletonDataResource(text);
		if (val == null)
		{
			return true;
		}
		__result = (Resource)((MegaSpineBinding)val).BoundObject;
		ModLogger.Debug("[CustomSkeletonLoader] Cache replaced: " + text);
		return false;
	}
}
