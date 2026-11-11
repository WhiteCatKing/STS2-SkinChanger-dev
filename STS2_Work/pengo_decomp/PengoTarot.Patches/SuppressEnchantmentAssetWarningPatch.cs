using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Logging;

namespace PengoTarot.Patches;

[HarmonyPatch]
internal static class SuppressEnchantmentAssetWarningPatch
{
	private static readonly Lazy<FieldInfo?> CacheField = new Lazy<FieldInfo>(() => typeof(AssetCache).GetField("_cache", BindingFlags.Instance | BindingFlags.NonPublic));

	private static readonly Lazy<FieldInfo?> MissedCacheField = new Lazy<FieldInfo>(() => typeof(AssetCache).GetField("_missedCacheAssets", BindingFlags.Instance | BindingFlags.NonPublic));

	private static readonly HashSet<string> SuppressedPathPrefixes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "res://images/enchantments/tar_", "res://images/enchantments/planet_" };

	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(AssetCache), "LoadAsset", (Type[])null, (Type[])null);
	}

	private static bool ShouldSuppress(string path)
	{
		return SuppressedPathPrefixes.Any((string prefix) => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
	}

	[HarmonyPrefix]
	private static bool Prefix(AssetCache __instance, string path, ref Resource __result)
	{
		if (!ShouldSuppress(path))
		{
			return true;
		}
		try
		{
			ConcurrentDictionary<string, Resource> concurrentDictionary = CacheField.Value?.GetValue(__instance) as ConcurrentDictionary<string, Resource>;
			if (concurrentDictionary != null && concurrentDictionary.TryGetValue(path, out var value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
			{
				__result = value;
				return false;
			}
			Resource val = ResourceLoader.Load<Resource>(path, (string)null, (CacheMode)1);
			if (val == null)
			{
				Log.Warn("[PengoTarot] Failed to load enchantment icon: " + path, 2);
				__result = null;
				return false;
			}
			if (concurrentDictionary != null)
			{
				concurrentDictionary[path] = val;
			}
			(MissedCacheField.Value?.GetValue(__instance) as HashSet<string>)?.Add(path);
			__result = val;
			return false;
		}
		catch (Exception ex)
		{
			Log.Warn("[PengoTarot] SuppressEnchantmentWarning error for " + path + ": " + ex.Message, 2);
			return true;
		}
	}
}
