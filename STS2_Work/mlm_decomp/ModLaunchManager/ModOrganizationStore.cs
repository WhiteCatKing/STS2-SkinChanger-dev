using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Godot;

namespace ModLaunchManager;

internal static class ModOrganizationStore
{
	internal const string FileName = "mod_launch_organization.json";

	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true
	};

	internal static string Path => LauncherDataPaths.File("mod_launch_organization.json");

	internal static ModOrganizationDocument Load()
	{
		try
		{
			string path = LauncherDataPaths.ReadFile("mod_launch_organization.json");
			if (!File.Exists(path))
			{
				return new ModOrganizationDocument();
			}
			ModOrganizationDocument modOrganizationDocument = JsonSerializer.Deserialize<ModOrganizationDocument>(File.ReadAllText(path, Encoding.UTF8), Options);
			ModOrganizationDocument modOrganizationDocument2 = new ModOrganizationDocument();
			foreach (string item in modOrganizationDocument?.Groups ?? new List<string>())
			{
				string text = item?.Trim() ?? string.Empty;
				if (text.Length > 0 && !modOrganizationDocument2.Groups.Contains<string>(text, StringComparer.OrdinalIgnoreCase))
				{
					modOrganizationDocument2.Groups.Add(text);
				}
			}
			string key2;
			foreach (KeyValuePair<string, string> item2 in modOrganizationDocument?.Parents ?? new Dictionary<string, string>())
			{
				item2.Deconstruct(out var key, out key2);
				string text2 = key;
				string text3 = key2;
				string text4 = text2?.Trim() ?? string.Empty;
				string text5 = text3?.Trim() ?? string.Empty;
				if (text4.Length > 0 && text5.Length > 0 && !string.Equals(text4, text5, StringComparison.OrdinalIgnoreCase))
				{
					modOrganizationDocument2.Parents[text4] = text5;
				}
			}
			foreach (KeyValuePair<string, ModOrganizationEntry> item3 in modOrganizationDocument?.Mods ?? new Dictionary<string, ModOrganizationEntry>())
			{
				item3.Deconstruct(out key2, out var value);
				string text6 = key2;
				ModOrganizationEntry modOrganizationEntry = value;
				if (string.IsNullOrWhiteSpace(text6) || modOrganizationEntry == null)
				{
					continue;
				}
				string text7 = modOrganizationEntry.Alias?.Trim() ?? string.Empty;
				string text8 = modOrganizationEntry.Group?.Trim() ?? string.Empty;
				string text9 = modOrganizationEntry.Description?.Trim() ?? string.Empty;
				if (text7.Length > 0 || text8.Length > 0 || text9.Length > 0 || modOrganizationEntry.IgnorePriority)
				{
					if (text8.Length > 0 && !modOrganizationDocument2.Groups.Contains<string>(text8, StringComparer.OrdinalIgnoreCase))
					{
						modOrganizationDocument2.Groups.Add(text8);
					}
					modOrganizationDocument2.Mods[text6.Trim()] = new ModOrganizationEntry
					{
						Alias = text7,
						Group = text8,
						Description = text9,
						IgnorePriority = modOrganizationEntry.IgnorePriority
					};
				}
			}
			return modOrganizationDocument2;
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] could not read mod organization: " + ex.Message);
			return new ModOrganizationDocument();
		}
	}

	internal static PlanResult Save(ModOrganizationDocument document)
	{
		try
		{
			File.WriteAllText(Path, JsonSerializer.Serialize(document, Options), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			BaseLibSourceAliases.RefreshAliases(document);
			ConfigCloudChanges.Notify();
			return PlanResult.Ok(MlmLocManager.Get("organization.storage.saved"));
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not write mod organization: {ex}");
			return PlanResult.Error(MlmLocManager.Get("organization.storage.saveFailed", ("error", ex.Message)));
		}
	}
}
