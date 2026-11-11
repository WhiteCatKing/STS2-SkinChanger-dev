using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Godot;

namespace ModLaunchManager;

internal static class ModListTransfer
{
	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true
	};

	private const int MaxDocumentBytes = 4194304;

	internal static string DefaultFileName => $"mod-list-{DateTime.Now:yyyyMMdd-HHmm}.json";

	internal static string FallbackExportFolder => LauncherDataPaths.Directory("mod_list_exports");

	internal static ModListDocument Capture(LaunchPlan plan, string gameVersion)
	{
		LaunchEntry launchEntry = plan.Entries.FirstOrDefault((LaunchEntry entry) => entry.IsManager);
		ModListDocument modListDocument = new ModListDocument();
		modListDocument.Name = MlmLocManager.Get("transfer.export.defaultName", ("enabled", plan.EnabledCount));
		modListDocument.ExportedAt = DateTime.UtcNow.ToString("O");
		modListDocument.GameVersion = gameVersion;
		modListDocument.ManagerVersion = launchEntry?.Mod.manifest?.version ?? string.Empty;
		modListDocument.Mods = plan.Entries.Select((LaunchEntry entry) => new ModListItem
		{
			Id = entry.Id,
			Name = entry.Name,
			Source = SourceTag(entry),
			Enabled = entry.Enabled
		}).ToList();
		return modListDocument;
	}

	internal static string Serialize(ModListDocument document)
	{
		return JsonSerializer.Serialize(document, Options);
	}

	internal static bool TryParse(string payload, out LaunchProfile profile, out string error)
	{
		ModListDocument document;
		return TryParse(payload, out document, out profile, out error);
	}

	internal static bool TryParse(string payload, out ModListDocument document, out LaunchProfile profile, out string error)
	{
		document = new ModListDocument();
		profile = new LaunchProfile();
		error = string.Empty;
		if (string.IsNullOrWhiteSpace(payload))
		{
			error = MlmLocManager.Get("transfer.parse.empty");
			return false;
		}
		if (Encoding.UTF8.GetByteCount(payload) > 4194304)
		{
			error = MlmLocManager.Get("transfer.parse.tooLarge");
			return false;
		}
		ModListDocument modListDocument;
		try
		{
			modListDocument = JsonSerializer.Deserialize<ModListDocument>(payload, Options);
		}
		catch (JsonException ex)
		{
			error = MlmLocManager.Get("transfer.parse.notJson", ("error", ex.Message));
			return false;
		}
		if (modListDocument == null || !string.Equals(modListDocument.Kind, "mod-launch-manager.mod-list", StringComparison.Ordinal))
		{
			error = MlmLocManager.Get("transfer.parse.wrongKind");
			return false;
		}
		int formatVersion = modListDocument.FormatVersion;
		if ((formatVersion < 1 || formatVersion > 1) ? true : false)
		{
			error = MlmLocManager.Get("transfer.parse.unsupportedVersion", ("version", modListDocument.FormatVersion));
			return false;
		}
		List<ProfileMod> list = new List<ProfileMod>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (ModListItem item in modListDocument.Mods ?? new List<ModListItem>())
		{
			string text = item?.Id?.Trim() ?? string.Empty;
			if (text.Length != 0 && hashSet.Add(text))
			{
				list.Add(new ProfileMod
				{
					Id = text,
					Source = (item.Source?.Trim() ?? string.Empty),
					Enabled = item.Enabled
				});
			}
		}
		if (list.Count == 0)
		{
			error = MlmLocManager.Get("transfer.parse.noMods");
			return false;
		}
		profile = new LaunchProfile
		{
			Name = (string.IsNullOrWhiteSpace(modListDocument.Name) ? MlmLocManager.Get("transfer.import.unnamed") : modListDocument.Name.Trim()),
			Mods = list
		};
		document = modListDocument;
		return true;
	}

	internal static bool TryWorkshopItemId(string? source, out ulong itemId)
	{
		itemId = 0uL;
		if (source != null && source.StartsWith("workshop:", StringComparison.OrdinalIgnoreCase) && ulong.TryParse(source.Substring("workshop:".Length), out itemId))
		{
			return itemId != 0;
		}
		return false;
	}

	internal static bool TryWrite(string path, string payload, out string error)
	{
		error = string.Empty;
		try
		{
			string directoryName = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			string text = path + ".tmp";
			File.WriteAllText(text, payload, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			File.Move(text, path);
			return true;
		}
		catch (Exception ex)
		{
			error = ex.Message;
			GD.PushWarning($"{"[ModLaunchManager]"} could not write a mod list export: {ex}");
			return false;
		}
	}

	internal static bool TryRead(string path, out string payload, out string error)
	{
		payload = string.Empty;
		error = string.Empty;
		try
		{
			FileInfo fileInfo = new FileInfo(path);
			if (!fileInfo.Exists)
			{
				error = MlmLocManager.Get("transfer.import.fileMissing");
				return false;
			}
			if (fileInfo.Length > 4194304)
			{
				error = MlmLocManager.Get("transfer.parse.tooLarge");
				return false;
			}
			payload = File.ReadAllText(path, Encoding.UTF8);
			return true;
		}
		catch (Exception ex)
		{
			error = ex.Message;
			return false;
		}
	}

	private static string SourceTag(LaunchEntry entry)
	{
		ulong num = PhysicalModIdentity.WorkshopItemIdFor(entry.Mod);
		if (num == 0)
		{
			return "local";
		}
		return $"workshop:{num}";
	}
}
