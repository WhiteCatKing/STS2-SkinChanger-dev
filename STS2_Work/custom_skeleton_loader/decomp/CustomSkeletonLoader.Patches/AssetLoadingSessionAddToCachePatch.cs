using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CustomSkeletonLoader.Patches;

[HarmonyPatch(typeof(AssetLoadingSession))]
internal static class AssetLoadingSessionAddToCachePatch
{
	[HarmonyPatch("AddToCache")]
	[HarmonyPrefix]
	internal static void Prefix(ref Resource? resource, string path)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		if (resource == null || ((GodotObject)resource).GetClass() != "SpineSkeletonDataResource")
		{
			return;
		}
		string text = SkeletonLoader.ResolveNameFromPath(path);
		ModLogger.Debug("[CustomSkeletonLoader] Session intercepting: " + path + " -> " + text);
		if (!string.IsNullOrEmpty(text))
		{
			SkeletonRegistry.Record(text, path, resource);
			MegaSkeletonDataResource val = SkeletonLoader.TryBuildMegaSkeletonDataResource(text);
			if (val != null)
			{
				resource = (Resource)((MegaSpineBinding)val).BoundObject;
				ModLogger.Debug("[CustomSkeletonLoader] Session replaced: " + text);
			}
		}
	}
}
