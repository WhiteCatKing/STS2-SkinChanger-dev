using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;

namespace PengoTarot.BalatroEffect;

public static class EnchantmentConfig
{
	private class ConfigData
	{
		public int Version { get; set; } = 2;

		public Dictionary<string, Config.CardEffectEntry> Enchantments { get; set; } = new Dictionary<string, Config.CardEffectEntry>();
	}

	private const int CurrentConfigVersion = 2;

	private static readonly string FolderPath = Path.Combine(OS.GetUserDataDir(), "mod_configs", "PengoTarot");

	private static readonly string FilePath = Path.Combine(FolderPath, "BalatroEffectEnchantments.json");

	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		WriteIndented = true
	};

	private static ConfigData _data = new ConfigData();

	private static Dictionary<string, Config.CardEffectEntry> _enchantments => _data.Enchantments;

	public static IEnumerable<string> AllEnchantmentIds => _enchantments.Keys;

	public static event Action? Changed;

	public static void Load()
	{
		try
		{
			if (!File.Exists(FilePath))
			{
				_data = new ConfigData();
				return;
			}
			ConfigData configData = JsonSerializer.Deserialize<ConfigData>(File.ReadAllText(FilePath));
			if (configData == null)
			{
				_data = new ConfigData();
				return;
			}
			if (configData.Version == 1)
			{
				if (configData.Enchantments != null)
				{
					foreach (Config.CardEffectEntry value in configData.Enchantments.Values)
					{
						if (value != null)
						{
							RemapEntry(value);
						}
					}
				}
				configData.Version = 2;
			}
			else if (configData.Version != 2)
			{
				_data = new ConfigData();
				return;
			}
			_data = configData;
			PruneInvalidEntries();
		}
		catch
		{
			_data = new ConfigData();
		}
	}

	private static void RemapEntry(Config.CardEffectEntry e)
	{
		e.Mode = Config.RemapModeV7ToV8(e.Mode);
		e.FullCardEffect = Config.RemapModeV7ToV8(e.FullCardEffect);
		if (e.Parts == null)
		{
			return;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (KeyValuePair<string, int> part in e.Parts)
		{
			dictionary[part.Key] = Config.RemapModeV7ToV8(part.Value);
		}
		e.Parts = dictionary;
	}

	public static void Save()
	{
		try
		{
			if (!Directory.Exists(FolderPath))
			{
				Directory.CreateDirectory(FolderPath);
			}
			File.WriteAllText(FilePath, JsonSerializer.Serialize(_data, Options));
		}
		catch (Exception ex)
		{
			GD.PrintErr("[BalatroEffect] EnchantmentConfig Save failed: " + ex.Message);
		}
	}

	public static Config.CardEffectEntry? GetEntry(string enchantmentId)
	{
		if (string.IsNullOrEmpty(enchantmentId))
		{
			return null;
		}
		if (!_enchantments.TryGetValue(enchantmentId, out Config.CardEffectEntry value))
		{
			return null;
		}
		return value;
	}

	public static bool TryGetEntry(string enchantmentId, out Config.CardEffectEntry? entry)
	{
		entry = GetEntry(enchantmentId);
		return entry != null;
	}

	public static bool HasEffect(string enchantmentId)
	{
		Config.CardEffectEntry entry = GetEntry(enchantmentId);
		if (entry != null)
		{
			return EntryHasEffect(entry);
		}
		return false;
	}

	public static bool HasAnyEffect()
	{
		foreach (Config.CardEffectEntry value in _enchantments.Values)
		{
			if (EntryHasEffect(value))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetEffect(string enchantmentId, string partName)
	{
		Config.CardEffectEntry entry = GetEntry(enchantmentId);
		if (entry == null)
		{
			return 0;
		}
		if (partName == "FullCard")
		{
			return entry.FullCardEffect;
		}
		if (entry.Parts.TryGetValue(partName, out var value))
		{
			return value;
		}
		return 0;
	}

	public static void SetEffect(string enchantmentId, string partName, int effectIndex)
	{
		if (!string.IsNullOrEmpty(enchantmentId))
		{
			if (!_enchantments.TryGetValue(enchantmentId, out Config.CardEffectEntry value))
			{
				value = (_enchantments[enchantmentId] = new Config.CardEffectEntry());
			}
			if (partName == "FullCard")
			{
				value.FullCardEffect = effectIndex;
			}
			else if (effectIndex == 0)
			{
				value.Parts.Remove(partName);
			}
			else
			{
				value.Parts[partName] = effectIndex;
			}
			Config.ResetPerformanceThrottle();
			EnchantmentConfig.Changed?.Invoke();
			Save();
		}
	}

	public static void ClearEffect(string enchantmentId, string? partName = null)
	{
		Config.CardEffectEntry value;
		if (partName == null)
		{
			_enchantments.Remove(enchantmentId);
		}
		else if (_enchantments.TryGetValue(enchantmentId, out value))
		{
			if (partName == "FullCard")
			{
				value.FullCardEffect = 0;
			}
			else
			{
				value.Parts.Remove(partName);
			}
		}
		Config.ResetPerformanceThrottle();
		EnchantmentConfig.Changed?.Invoke();
		Save();
	}

	public static double GetIntensity(string enchantmentId, double defaultValue = 1.0)
	{
		return GetEntry(enchantmentId)?.Intensity ?? defaultValue;
	}

	public static void SetIntensity(string enchantmentId, double value)
	{
		if (!string.IsNullOrEmpty(enchantmentId) && _enchantments.TryGetValue(enchantmentId, out Config.CardEffectEntry value2))
		{
			value2.Intensity = Math.Clamp(value, 0.0, 1.0);
			Config.ResetPerformanceThrottle();
			Save();
		}
	}

	public static string GetEditMode(string enchantmentId)
	{
		Config.CardEffectEntry entry = GetEntry(enchantmentId);
		if (entry == null)
		{
			return "normal";
		}
		return entry.EditMode;
	}

	public static void SetEditMode(string enchantmentId, string newMode)
	{
		if (string.IsNullOrEmpty(enchantmentId))
		{
			return;
		}
		bool flag;
		switch (newMode)
		{
		case "normal":
		case "separately":
		case "fully":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			if (!_enchantments.TryGetValue(enchantmentId, out Config.CardEffectEntry value))
			{
				value = (_enchantments[enchantmentId] = new Config.CardEffectEntry());
			}
			value.EditMode = newMode;
			Config.ResetPerformanceThrottle();
			EnchantmentConfig.Changed?.Invoke();
			Save();
		}
	}

	public static int GetCardEffectMode(string enchantmentId)
	{
		return GetEntry(enchantmentId)?.Mode ?? 0;
	}

	public static void SetCardEffectMode(string enchantmentId, int mode)
	{
		if (!string.IsNullOrEmpty(enchantmentId))
		{
			if (!_enchantments.TryGetValue(enchantmentId, out Config.CardEffectEntry value))
			{
				value = (_enchantments[enchantmentId] = new Config.CardEffectEntry());
			}
			value.Mode = mode;
			Config.ResetPerformanceThrottle();
			EnchantmentConfig.Changed?.Invoke();
			Save();
		}
	}

	public static int GetCardEffectIndex(string enchantmentId)
	{
		Config.CardEffectEntry entry = GetEntry(enchantmentId);
		if (entry == null)
		{
			return 0;
		}
		return EffectiveMode(entry);
	}

	private static int EffectiveMode(Config.CardEffectEntry entry)
	{
		if (entry.FullCardEffect > 0)
		{
			return entry.FullCardEffect;
		}
		if (entry.Parts != null)
		{
			foreach (int value in entry.Parts.Values)
			{
				if (value > 0)
				{
					return value;
				}
			}
		}
		return entry.Mode;
	}

	private static bool EntryHasEffect(Config.CardEffectEntry entry)
	{
		if (entry.FullCardEffect > 0)
		{
			return true;
		}
		if (entry.Parts != null)
		{
			foreach (int value in entry.Parts.Values)
			{
				if (value > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void PruneInvalidEntries()
	{
		List<string> list = new List<string>();
		foreach (var (item, cardEffectEntry2) in _enchantments)
		{
			if (cardEffectEntry2.FullCardEffect == 0 && (cardEffectEntry2.Parts == null || cardEffectEntry2.Parts.Count == 0))
			{
				list.Add(item);
			}
		}
		foreach (string item2 in list)
		{
			_enchantments.Remove(item2);
		}
	}

	public static string ExportCardPreset(string enchantmentId)
	{
		Config.CardEffectEntry entry = GetEntry(enchantmentId);
		return JsonSerializer.Serialize(new
		{
			EnchantmentId = enchantmentId,
			EditMode = (entry?.EditMode ?? "normal"),
			Mode = (entry?.Mode ?? 0),
			Intensity = (entry?.Intensity ?? 1.0),
			Parts = entry?.Parts,
			FullCardEffect = (entry?.FullCardEffect ?? 0)
		}, Options);
	}

	public static bool ImportCardPreset(string enchantmentId, string json)
	{
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(json);
			JsonElement rootElement = jsonDocument.RootElement;
			JsonElement value;
			string editMode = (rootElement.TryGetProperty("EditMode", out value) ? (value.GetString() ?? "normal") : "normal");
			JsonElement value2;
			int mode = (rootElement.TryGetProperty("Mode", out value2) ? value2.GetInt32() : 0);
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			if (rootElement.TryGetProperty("Parts", out var value3))
			{
				foreach (JsonProperty item in value3.EnumerateObject())
				{
					dictionary[item.Name] = item.Value.GetInt32();
				}
			}
			JsonElement value4;
			int fullCardEffect = (rootElement.TryGetProperty("FullCardEffect", out value4) ? value4.GetInt32() : 0);
			Config.CardEffectEntry cardEffectEntry = new Config.CardEffectEntry
			{
				EditMode = editMode,
				Mode = mode,
				Parts = dictionary,
				FullCardEffect = fullCardEffect
			};
			if (rootElement.TryGetProperty("Intensity", out var value5))
			{
				cardEffectEntry.Intensity = Math.Clamp(value5.GetDouble(), 0.0, 1.0);
			}
			_enchantments[enchantmentId] = cardEffectEntry;
			if (cardEffectEntry.FullCardEffect == 0 && (cardEffectEntry.Parts == null || cardEffectEntry.Parts.Count == 0))
			{
				_enchantments.Remove(enchantmentId);
			}
			Config.ResetPerformanceThrottle();
			EnchantmentConfig.Changed?.Invoke();
			Save();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool ApplyAuthorPreset(string json)
	{
		try
		{
			ConfigData configData = JsonSerializer.Deserialize<ConfigData>(json);
			if (configData == null || configData.Enchantments == null)
			{
				return false;
			}
			foreach (KeyValuePair<string, Config.CardEffectEntry> enchantment in configData.Enchantments)
			{
				if (enchantment.Value != null)
				{
					_enchantments[enchantment.Key] = enchantment.Value;
				}
			}
			PruneInvalidEntries();
			Config.ResetPerformanceThrottle();
			EnchantmentConfig.Changed?.Invoke();
			Save();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool LoadAuthorPreset(string path)
	{
		if (!FileAccess.FileExists(path))
		{
			return false;
		}
		FileAccess val = FileAccess.Open(path, (ModeFlags)1);
		try
		{
			return ApplyAuthorPreset(val.GetAsText(false));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
