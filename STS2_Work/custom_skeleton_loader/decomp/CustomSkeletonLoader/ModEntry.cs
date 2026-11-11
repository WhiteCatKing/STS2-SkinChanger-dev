using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace CustomSkeletonLoader;

[ModInitializer("Init")]
public static class ModEntry
{
	private static Harmony? _harmony;

	public static void Init()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		ModConfigIntegration.Register();
		ModLogger.Debug("[CustomSkeletonLoader] Initializing...");
		ModLogger.Debug("[CustomSkeletonLoader] Skeletons path: " + SkeletonLoader.SkeletonsPath);
		_harmony = new Harmony("com.lamali.customskeletonloader");
		_harmony.PatchAll();
		IEnumerable<MethodBase> patchedMethods = _harmony.GetPatchedMethods();
		int num = 0;
		foreach (MethodBase item in patchedMethods)
		{
			num++;
			ModLogger.Debug("[CustomSkeletonLoader] Patched: " + item.DeclaringType?.Name + "." + item.Name);
		}
		Log.Info($"[CustomSkeletonLoader] Loaded successfully! {num} methods patched.", 2);
	}
}
