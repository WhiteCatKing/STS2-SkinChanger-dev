using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class ModManifestReader
{
	private static readonly MethodInfo? ReadModManifestMethod = AccessTools.Method(typeof(ModManager), "ReadModManifest", new Type[2]
	{
		typeof(string),
		typeof(ModSource)
	}, (Type[])null);

	internal static bool IsAvailable => ReadModManifestMethod != null;

	internal static Mod? Read(string manifestFile, ModSource source)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		object? obj = ReadModManifestMethod?.Invoke(null, new object[2] { manifestFile, source });
		return (Mod?)((obj is Mod) ? obj : null);
	}

	internal static string? FindManifestFile(Mod mod)
	{
		try
		{
			if (string.IsNullOrWhiteSpace(mod.path) || !Directory.Exists(mod.path))
			{
				return null;
			}
			string text = mod.manifest?.id;
			foreach (string item in Directory.EnumerateFiles(mod.path, "*.json", SearchOption.TopDirectoryOnly))
			{
				string text2 = TryReadId(item);
				if (text2 != null && (text == null || string.Equals(text2, text, StringComparison.Ordinal)))
				{
					return item;
				}
			}
			return null;
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not scan {mod.path} for a manifest: {ex.Message}");
			return null;
		}
	}

	private static string? TryReadId(string file)
	{
		try
		{
			object result;
			if (JsonNode.Parse(File.ReadAllText(file, Encoding.UTF8), null) is JsonObject jsonObject)
			{
				JsonNode? jsonNode = jsonObject["id"];
				if (jsonNode != null && jsonNode.GetValueKind() == JsonValueKind.String)
				{
					result = jsonObject["id"].GetValue<string>();
					goto IL_005a;
				}
			}
			result = null;
			goto IL_005a;
			IL_005a:
			return (string?)result;
		}
		catch
		{
			return null;
		}
	}

	internal static bool RefreshInPlace(Mod mod, out string? error)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		error = null;
		try
		{
			if (!IsAvailable)
			{
				error = "the vanilla manifest reader is unavailable";
				return false;
			}
			if (string.IsNullOrWhiteSpace(mod.path) || !Directory.Exists(mod.path))
			{
				error = "the mod folder no longer exists";
				return false;
			}
			string text = mod.manifest?.id;
			string text2 = FindManifestFile(mod);
			if (text2 == null)
			{
				error = ((text == null) ? "no readable manifest was found" : ("no manifest declaring id '" + text + "' was found"));
				return false;
			}
			Mod val = Read(text2, mod.modSource);
			if (val == null || val.manifest == null)
			{
				error = "'" + Path.GetFileName(text2) + "' could not be parsed as a manifest";
				return false;
			}
			Adopt(mod, val);
			return true;
		}
		catch (TargetInvocationException ex)
		{
			error = (ex.InnerException ?? ex).Message;
			return false;
		}
		catch (Exception ex2)
		{
			error = ex2.Message;
			return false;
		}
	}

	internal static void Adopt(Mod mod, Mod parsed)
	{
		mod.manifest = parsed.manifest;
		mod.version = null;
		MergeErrors(mod, parsed.errors);
	}

	private static void MergeErrors(Mod mod, List<LocString>? parsedErrors)
	{
		if (parsedErrors == null || parsedErrors.Count <= 0)
		{
			return;
		}
		if (mod.errors == null)
		{
			mod.errors = new List<LocString>();
		}
		foreach (LocString error in parsedErrors)
		{
			if (mod.errors.All((LocString existing) => !string.Equals(existing.LocEntryKey, error.LocEntryKey, StringComparison.Ordinal)))
			{
				mod.errors.Add(error);
			}
		}
	}
}
