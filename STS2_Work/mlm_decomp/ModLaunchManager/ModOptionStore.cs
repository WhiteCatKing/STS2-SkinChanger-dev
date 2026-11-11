using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Godot;

namespace ModLaunchManager;

internal static class ModOptionStore
{
	internal const string FileName = "mod_launch_options.json";

	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	internal static string Path => LauncherDataPaths.File("mod_launch_options.json");

	internal static ModOptionDocument Load()
	{
		try
		{
			string path = LauncherDataPaths.ReadFile("mod_launch_options.json");
			if (!File.Exists(path))
			{
				return new ModOptionDocument();
			}
			ModOptionDocument? modOptionDocument = JsonSerializer.Deserialize<ModOptionDocument>(File.ReadAllText(path, Encoding.UTF8), Options);
			ModOptionDocument modOptionDocument2 = new ModOptionDocument();
			foreach (var (text2, modOptionEntry2) in modOptionDocument?.Mods ?? new Dictionary<string, ModOptionEntry>())
			{
				if (!string.IsNullOrWhiteSpace(text2) && modOptionEntry2 != null && !modOptionEntry2.IsEmpty)
				{
					modOptionDocument2.Mods[text2.Trim()] = modOptionEntry2;
				}
			}
			return modOptionDocument2;
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] could not read launch options: " + ex.Message);
			return new ModOptionDocument();
		}
	}

	internal static PlanResult Save(ModOptionDocument document)
	{
		try
		{
			File.WriteAllText(Path, JsonSerializer.Serialize(document, Options), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			ConfigCloudChanges.Notify();
			return PlanResult.Ok(MlmLocManager.Get("options.storage.saved"));
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not write launch options: {ex}");
			return PlanResult.Error(MlmLocManager.Get("options.storage.saveFailed", ("error", ex.Message)));
		}
	}
}
