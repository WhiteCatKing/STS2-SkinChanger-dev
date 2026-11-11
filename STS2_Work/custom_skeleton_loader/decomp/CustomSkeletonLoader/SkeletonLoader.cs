using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CustomSkeletonLoader;

internal static class SkeletonLoader
{
	private static readonly ConcurrentDictionary<string, MegaSkeletonDataResource?> _cache;

	private static readonly Dictionary<string, string> _atlasPathCache;

	internal static readonly string SkeletonsPath;

	internal static readonly string ReplaceRootPath;

	static SkeletonLoader()
	{
		_cache = new ConcurrentDictionary<string, MegaSkeletonDataResource>();
		_atlasPathCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		SkeletonsPath = ResolveSkeletonsPath();
		ReplaceRootPath = Path.Combine(SkeletonsPath, "ReplaceResources") + Path.DirectorySeparatorChar;
		RefreshConfigCache();
		ScanAvailableAtlases();
	}

	internal static void RefreshConfigCache()
	{
	}

	private static string ResolveSkeletonsPath()
	{
		string location = typeof(SkeletonLoader).Assembly.Location;
		string directoryName = Path.GetDirectoryName(location);
		if (!string.IsNullOrEmpty(directoryName))
		{
			ModLogger.Debug("[CustomSkeletonLoader] Mod Directory: " + directoryName);
			return directoryName + Path.DirectorySeparatorChar;
		}
		ModLogger.Warn("[CustomSkeletonLoader] Could not get DLL directory, falling back to current dir");
		return Path.Combine(Directory.GetCurrentDirectory(), "mods", "CustomSkeletonLoader") + Path.DirectorySeparatorChar;
	}

	private static void ScanAvailableAtlases()
	{
		if (!Directory.Exists(ReplaceRootPath))
		{
			ModLogger.Warn("[CustomSkeletonLoader] Replace directory not found: " + ReplaceRootPath);
			return;
		}
		try
		{
			string[] files = Directory.GetFiles(ReplaceRootPath, "*.atlas", SearchOption.AllDirectories);
			string[] array = files;
			foreach (string text in array)
			{
				string key = Path.GetFileNameWithoutExtension(text).ToLowerInvariant();
				_atlasPathCache[key] = text;
			}
			ModLogger.Debug($"[CustomSkeletonLoader] Scanned {_atlasPathCache.Count} atlas files from {ReplaceRootPath}");
		}
		catch (Exception ex)
		{
			ModLogger.Warn("[CustomSkeletonLoader] Error scanning atlas files: " + ex.Message);
		}
	}

	internal static string ResolveNameFromPath(string path)
	{
		string text = Path.GetFileNameWithoutExtension(path);
		if (text.EndsWith("_skel_data", StringComparison.OrdinalIgnoreCase))
		{
			string text2 = text;
			int length = "_skel_data".Length;
			text = text2.Substring(0, text2.Length - length);
		}
		if (text.Equals("merchant_hand", StringComparison.OrdinalIgnoreCase) || text.Equals("merchanthand", StringComparison.OrdinalIgnoreCase))
		{
			return "merchanthand";
		}
		return text;
	}

	internal static MegaSkeletonDataResource? TryBuildMegaSkeletonDataResource(string name)
	{
		return GetSkeleton(name);
	}

	internal static MegaSkeletonDataResource? GetSkeleton(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		string text = name.ToLowerInvariant();
		if (_cache.TryGetValue(text, out MegaSkeletonDataResource value) && value != null)
		{
			return value;
		}
		MegaSkeletonDataResource val = LoadSkeleton(text);
		if (val != null)
		{
			_cache.TryAdd(text, val);
		}
		return val;
	}

	private static MegaSkeletonDataResource? LoadSkeleton(string name)
	{
		ModLogger.Debug("[CustomSkeletonLoader] Building resource for: " + name);
		string text = FindAtlasFromCache(name);
		if (text == null)
		{
			ModLogger.Debug("[CustomSkeletonLoader] No atlas found for: " + name);
			return null;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
		string directoryName = Path.GetDirectoryName(text);
		string directory = directoryName ?? ReplaceRootPath;
		string text2 = FindSkeletonFile(directory, fileNameWithoutExtension);
		if (text2 == null)
		{
			ModLogger.Debug("[CustomSkeletonLoader] No skel file found for: " + fileNameWithoutExtension);
			return null;
		}
		ModLogger.Debug($"[CustomSkeletonLoader] Matched: {name} -> atlas={text}, skel={text2}");
		return BuildSkeletonDataResource(name, text, text2);
	}

	private static string? FindAtlasFromCache(string name)
	{
		string text = name.ToLowerInvariant();
		if (_atlasPathCache.TryGetValue(text, out string value))
		{
			return value;
		}
		string text2 = text.Replace("_", "");
		if (_atlasPathCache.TryGetValue(text2, out string value2))
		{
			ModLogger.Debug("[CustomSkeletonLoader] Fallback match (underscore removed): " + name + " -> " + text2);
			return value2;
		}
		string text3 = ExtractCharacterName(text);
		if (!string.IsNullOrEmpty(text3))
		{
			if (_atlasPathCache.TryGetValue(text3 + "_shop", out string value3))
			{
				return value3;
			}
			if (_atlasPathCache.TryGetValue(text3, out string value4))
			{
				return value4;
			}
		}
		return null;
	}

	private static string? FindSkeletonFile(string directory, string baseName)
	{
		string text = Path.Combine(directory, baseName + ".skel");
		if (File.Exists(text))
		{
			return text;
		}
		if (baseName.EndsWith("_shop", StringComparison.OrdinalIgnoreCase))
		{
			int length = "_shop".Length;
			string text2 = baseName.Substring(0, baseName.Length - length);
			string text3 = Path.Combine(directory, text2 + ".skel");
			if (File.Exists(text3))
			{
				return text3;
			}
		}
		return null;
	}

	private static string ExtractCharacterName(string lowerName)
	{
		if (lowerName.StartsWith("characterselect_"))
		{
			string text = lowerName;
			int length = "characterselect_".Length;
			return text.Substring(length, text.Length - length);
		}
		if (lowerName.EndsWith("_merchant"))
		{
			string text = lowerName;
			int length = "_merchant".Length;
			return text.Substring(0, text.Length - length);
		}
		int num = lowerName.IndexOf("_merchant");
		if (num > 0)
		{
			return lowerName.Substring(0, num);
		}
		if (lowerName == "merchant_hand")
		{
			return "merchanthand";
		}
		return string.Empty;
	}

	private static MegaSkeletonDataResource? BuildSkeletonDataResource(string name, string atlasPath, string skelPath)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I8
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Invalid comparison between Unknown and I8
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Expected O, but got Unknown
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		GodotObject val = null;
		GodotObject val2 = null;
		GodotObject val3 = null;
		try
		{
			Variant val4 = ClassDB.Instantiate(StringName.op_Implicit("SpineAtlasResource"));
			val = ((Variant)(ref val4)).AsGodotObject();
			if (val == null)
			{
				ModLogger.Error("[CustomSkeletonLoader] Failed to instantiate SpineAtlasResource");
				return null;
			}
			Error val5 = (Error)(int)val.Call(StringName.op_Implicit("load_from_atlas_file"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(atlasPath) });
			if ((long)val5 > 0L)
			{
				ModLogger.Warn($"[CustomSkeletonLoader] Failed to load atlas for {name}: {val5}");
				return null;
			}
			val4 = ClassDB.Instantiate(StringName.op_Implicit("SpineSkeletonFileResource"));
			val2 = ((Variant)(ref val4)).AsGodotObject();
			if (val2 == null)
			{
				ModLogger.Error("[CustomSkeletonLoader] Failed to instantiate SpineSkeletonFileResource");
				return null;
			}
			Error val6 = (Error)(int)val2.Call(StringName.op_Implicit("load_from_file"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(skelPath) });
			if ((long)val6 > 0L)
			{
				ModLogger.Warn($"[CustomSkeletonLoader] Failed to load skeleton for {name}: {val6}");
				return null;
			}
			val4 = ClassDB.Instantiate(StringName.op_Implicit("SpineSkeletonDataResource"));
			val3 = ((Variant)(ref val4)).AsGodotObject();
			if (val3 == null)
			{
				ModLogger.Error("[CustomSkeletonLoader] Failed to instantiate SpineSkeletonDataResource");
				return null;
			}
			val3.Set(StringName.op_Implicit("atlas_res"), Variant.op_Implicit(val));
			val3.Set(StringName.op_Implicit("skeleton_file_res"), Variant.op_Implicit(val2));
			ModLogger.Debug("[CustomSkeletonLoader] Successfully loaded custom skeleton for '" + name + "'");
			MegaSkeletonDataResource result = new MegaSkeletonDataResource(Variant.op_Implicit(val3));
			val = null;
			val2 = null;
			val3 = null;
			return result;
		}
		catch (Exception ex)
		{
			ModLogger.Warn("[CustomSkeletonLoader] Exception loading '" + name + "': " + ex.Message);
			return null;
		}
		finally
		{
			try
			{
				if (val != null)
				{
					val.Call(StringName.op_Implicit("free"), Array.Empty<Variant>());
				}
				if (val2 != null)
				{
					val2.Call(StringName.op_Implicit("free"), Array.Empty<Variant>());
				}
				if (val3 != null)
				{
					val3.Call(StringName.op_Implicit("free"), Array.Empty<Variant>());
				}
			}
			catch (Exception ex2)
			{
				ModLogger.Debug("[CustomSkeletonLoader] Resource cleanup warning: " + ex2.Message);
			}
		}
	}
}
