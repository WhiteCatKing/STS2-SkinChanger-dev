using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Godot;

namespace ModLaunchManager;

internal static class ProfileStore
{
	internal const string FileName = "mod_launch_profiles.json";

	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true
	};

	internal static string Path => LauncherDataPaths.File("mod_launch_profiles.json");

	internal static ProfileDocument Load()
	{
		try
		{
			string path = LauncherDataPaths.ReadFile("mod_launch_profiles.json");
			if (!File.Exists(path))
			{
				return new ProfileDocument();
			}
			return JsonSerializer.Deserialize<ProfileDocument>(File.ReadAllText(path, Encoding.UTF8), Options) ?? new ProfileDocument();
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] could not read profiles: " + ex.Message);
			return new ProfileDocument();
		}
	}

	internal static PlanResult Save(ProfileDocument document)
	{
		try
		{
			File.WriteAllText(Path, JsonSerializer.Serialize(document, Options), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			ConfigCloudChanges.Notify();
			return PlanResult.Ok(MlmLocManager.Get("profiles.storage.saved"));
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not write profiles: {ex}");
			return PlanResult.Error(MlmLocManager.Get("profiles.storage.saveFailed", ("error", ex.Message)));
		}
	}
}
