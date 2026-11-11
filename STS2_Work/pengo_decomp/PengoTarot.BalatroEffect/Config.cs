using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;

namespace PengoTarot.BalatroEffect;

public static class Config
{
	private class ConfigData
	{
		public int Version { get; set; } = 8;

		public Dictionary<string, CardEffectEntry> Cards { get; set; } = new Dictionary<string, CardEffectEntry>();

		public bool GlobalDynamicEffect { get; set; }

		public bool EnableShaderInNonCombat { get; set; }

		public bool ShownTestWarning { get; set; }
	}

	private class ConfigDataV5
	{
		public int Version { get; set; } = 5;

		public Dictionary<string, CardEffectEntry> Cards { get; set; } = new Dictionary<string, CardEffectEntry>();

		public Dictionary<int, double> IntensitySettings { get; set; } = new Dictionary<int, double>();

		public bool GlobalDynamicEffect { get; set; }

		public bool EnableShaderInNonCombat { get; set; }

		public bool ShownTestWarning { get; set; }
	}

	public class CardEffectEntry
	{
		public string EditMode { get; set; } = "normal";

		public int Mode { get; set; }

		public double Intensity { get; set; } = 1.0;

		public Dictionary<string, int> Parts { get; set; } = new Dictionary<string, int>();

		public int FullCardEffect { get; set; }
	}

	private sealed class CardModeCache
	{
		public Dictionary<string, int> NormalParts = new Dictionary<string, int>();

		public Dictionary<string, int> SeparateParts = new Dictionary<string, int>();

		public int FullCardEffect;
	}

	private const int CurrentConfigVersion = 8;

	private static readonly string FolderPath = Path.Combine(OS.GetUserDataDir(), "mod_configs", "PengoTarot");

	private static readonly string FilePath = Path.Combine(FolderPath, "BalatroEffectSettings.json");

	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		WriteIndented = true
	};

	private static bool _performanceThrottled = false;

	public const string ModeNormal = "normal";

	public const string ModeSeparately = "separately";

	public const string ModeFully = "fully";

	private static ConfigData _data = new ConfigData();

	public static readonly string[] AllPartNames = new string[8] { "Portrait", "Frame", "TitleBanner", "TypePlaque", "PortraitBorder", "EnergyIcon", "StarIcon", "Enchantment" };

	private static readonly Dictionary<string, CardModeCache> _modeCache = new Dictionary<string, CardModeCache>();

	private static readonly Dictionary<string, string> _cardEnchantOverlay = new Dictionary<string, string>();

	private static readonly HashSet<string> _previewEnchantCards = new HashSet<string>();

	public static bool IsPerformanceThrottled => _performanceThrottled;

	private static Dictionary<string, CardEffectEntry> _cards => _data.Cards;

	public static bool GlobalDynamicEffect
	{
		get
		{
			if (!IsPerformanceThrottled)
			{
				return _data.GlobalDynamicEffect;
			}
			return false;
		}
		set
		{
			_data.GlobalDynamicEffect = value;
			ResetPerformanceThrottle();
			Save();
		}
	}

	public static bool EnableShaderInNonCombat
	{
		get
		{
			if (!IsPerformanceThrottled)
			{
				return _data.EnableShaderInNonCombat;
			}
			return false;
		}
		set
		{
			_data.EnableShaderInNonCombat = value;
			ResetPerformanceThrottle();
			Save();
		}
	}

	public static bool ShownTestWarning
	{
		get
		{
			return _data.ShownTestWarning;
		}
		set
		{
			_data.ShownTestWarning = value;
			ResetPerformanceThrottle();
			Save();
		}
	}

	public static event Action? PerformanceThrottled;

	internal static void SetPerformanceThrottled(bool value)
	{
		if (_performanceThrottled != value)
		{
			_performanceThrottled = value;
			if (value)
			{
				Config.PerformanceThrottled?.Invoke();
			}
		}
	}

	internal static void ResetPerformanceThrottle()
	{
		_performanceThrottled = false;
	}

	private static CardModeCache GetModeCache(string cardId)
	{
		if (!_modeCache.TryGetValue(cardId, out CardModeCache value))
		{
			value = (_modeCache[cardId] = new CardModeCache());
		}
		return value;
	}

	public static string GetEditMode(string cardId)
	{
		if (IsPerformanceThrottled)
		{
			return "normal";
		}
		if (TryGetOverlay(cardId, out string enchantmentId, out bool preview))
		{
			CardEffectEntry entry = EnchantmentConfig.GetEntry(enchantmentId);
			if (preview)
			{
				if (entry == null)
				{
					return "normal";
				}
				return entry.EditMode;
			}
			if (entry != null)
			{
				return entry.EditMode;
			}
		}
		if (!_cards.TryGetValue(cardId, out CardEffectEntry value))
		{
			return "normal";
		}
		return value.EditMode;
	}

	public static void SetEditMode(string cardId, string newMode)
	{
		if (string.IsNullOrEmpty(cardId))
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
			if (!_cards.TryGetValue(cardId, out CardEffectEntry value))
			{
				value = (_cards[cardId] = new CardEffectEntry());
			}
			if (!(value.EditMode == newMode))
			{
				SaveCurrentModeToCache(cardId, value);
				RestoreModeFromCache(cardId, value, newMode);
				value.EditMode = newMode;
				ResetPerformanceThrottle();
				Save();
			}
		}
	}

	private static void SaveCurrentModeToCache(string cardId, CardEffectEntry entry)
	{
		CardModeCache modeCache = GetModeCache(cardId);
		string editMode = entry.EditMode;
		if (!(editMode == "separately"))
		{
			if (editMode == "fully")
			{
				modeCache.FullCardEffect = entry.FullCardEffect;
			}
			else
			{
				modeCache.NormalParts = new Dictionary<string, int>(entry.Parts);
			}
		}
		else
		{
			modeCache.SeparateParts = new Dictionary<string, int>(entry.Parts);
		}
	}

	private static void RestoreModeFromCache(string cardId, CardEffectEntry entry, string mode)
	{
		CardModeCache modeCache = GetModeCache(cardId);
		if (!(mode == "separately"))
		{
			if (mode == "fully")
			{
				entry.Parts = new Dictionary<string, int>();
				if (modeCache.FullCardEffect > 0)
				{
					entry.FullCardEffect = modeCache.FullCardEffect;
				}
				return;
			}
			entry.FullCardEffect = 0;
			if (modeCache.NormalParts.Count > 0)
			{
				entry.Parts = new Dictionary<string, int>(modeCache.NormalParts);
			}
			else if (entry.Parts.Count > 0)
			{
				modeCache.NormalParts = new Dictionary<string, int>(entry.Parts);
			}
		}
		else
		{
			entry.FullCardEffect = 0;
			if (modeCache.SeparateParts.Count > 0)
			{
				entry.Parts = new Dictionary<string, int>(modeCache.SeparateParts);
			}
			else if (entry.Parts.Count > 0)
			{
				modeCache.SeparateParts = new Dictionary<string, int>(entry.Parts);
			}
		}
	}

	public static void Load()
	{
		try
		{
			if (!File.Exists(FilePath))
			{
				ResetAll();
				ApplyAllAuthorPresets();
				return;
			}
			string json = File.ReadAllText(FilePath);
			ConfigData configData = JsonSerializer.Deserialize<ConfigData>(json);
			if (configData == null)
			{
				ResetAll();
				return;
			}
			bool flag = configData.Version != 8;
			if (flag)
			{
				configData = UpgradeToCurrent(json);
				if (configData == null)
				{
					ResetAll();
					return;
				}
			}
			_data = configData;
			PruneInvalidEntries();
			if (flag)
			{
				ApplyAllAuthorPresets();
			}
			ResetPerformanceThrottle();
		}
		catch
		{
			ResetAll();
		}
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
			GD.PrintErr("[BalatroEffect] Save failed: " + ex.Message);
		}
	}

	private static bool TryGetOverlay(string cardId, out string enchantmentId, out bool preview)
	{
		if (_cardEnchantOverlay.TryGetValue(cardId, out string value))
		{
			enchantmentId = value;
			preview = _previewEnchantCards.Contains(cardId);
			return true;
		}
		enchantmentId = "";
		preview = false;
		return false;
	}

	internal static void SetCardEnchantmentOverlay(string cardId, string? enchantmentId, bool previewOnly)
	{
		if (string.IsNullOrEmpty(enchantmentId))
		{
			_cardEnchantOverlay.Remove(cardId);
			_previewEnchantCards.Remove(cardId);
			return;
		}
		_cardEnchantOverlay[cardId] = enchantmentId;
		if (previewOnly)
		{
			_previewEnchantCards.Add(cardId);
		}
		else
		{
			_previewEnchantCards.Remove(cardId);
		}
	}

	internal static void ClearCardEnchantmentOverlay(string cardId)
	{
		_cardEnchantOverlay.Remove(cardId);
		_previewEnchantCards.Remove(cardId);
	}

	internal static void ClearAllEnchantOverlays()
	{
		_cardEnchantOverlay.Clear();
		_previewEnchantCards.Clear();
	}

	public static int GetCardEffectMode(string cardId)
	{
		if (IsPerformanceThrottled)
		{
			return 0;
		}
		if (TryGetOverlay(cardId, out string enchantmentId, out bool preview))
		{
			CardEffectEntry entry = EnchantmentConfig.GetEntry(enchantmentId);
			if (preview)
			{
				return entry?.Mode ?? 0;
			}
			if (entry != null)
			{
				return entry.Mode;
			}
		}
		if (!_cards.TryGetValue(cardId, out CardEffectEntry value))
		{
			return 0;
		}
		return value.Mode;
	}

	public static void SetCardEffectMode(string cardId, int mode)
	{
		if (!_cards.ContainsKey(cardId))
		{
			_cards[cardId] = new CardEffectEntry();
		}
		_cards[cardId].Mode = mode;
		ResetPerformanceThrottle();
		Save();
	}

	public static int GetEffect(string cardId, string partName)
	{
		if (IsPerformanceThrottled)
		{
			return 0;
		}
		if (TryGetOverlay(cardId, out string enchantmentId, out bool preview))
		{
			CardEffectEntry entry = EnchantmentConfig.GetEntry(enchantmentId);
			if (preview)
			{
				if (entry == null)
				{
					return 0;
				}
				if (partName == "FullCard")
				{
					return entry.FullCardEffect;
				}
				if (!entry.Parts.TryGetValue(partName, out var value))
				{
					return 0;
				}
				return value;
			}
			if (entry != null)
			{
				if (partName == "FullCard")
				{
					if (entry.FullCardEffect <= 0)
					{
						return GetCardEffectCore(cardId, partName);
					}
					return entry.FullCardEffect;
				}
				int cardEffectCore = GetCardEffectCore(cardId, partName);
				if (!entry.Parts.TryGetValue(partName, out var value2) || value2 <= 0)
				{
					return cardEffectCore;
				}
				return value2;
			}
		}
		return GetCardEffectCore(cardId, partName);
	}

	private static int GetCardEffectCore(string cardId, string partName)
	{
		if (_cards.TryGetValue(cardId, out CardEffectEntry value))
		{
			if (partName == "FullCard")
			{
				return value.FullCardEffect;
			}
			if (value.Parts.TryGetValue(partName, out var value2))
			{
				return value2;
			}
		}
		return 0;
	}

	public static void SetEffect(string cardId, string partName, int effectIndex)
	{
		if (!_cards.ContainsKey(cardId))
		{
			_cards[cardId] = new CardEffectEntry();
		}
		CardEffectEntry cardEffectEntry = _cards[cardId];
		if (partName == "FullCard")
		{
			cardEffectEntry.FullCardEffect = effectIndex;
		}
		else if (effectIndex == 0)
		{
			cardEffectEntry.Parts.Remove(partName);
		}
		else
		{
			cardEffectEntry.Parts[partName] = effectIndex;
		}
		ResetPerformanceThrottle();
		Save();
	}

	public static void ClearEffect(string cardId, string? partName = null)
	{
		CardEffectEntry value;
		if (partName == null)
		{
			_cards.Remove(cardId);
		}
		else if (_cards.TryGetValue(cardId, out value))
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
		ResetPerformanceThrottle();
		Save();
	}

	public static double GetIntensity(string cardId, double defaultValue = 1.0)
	{
		if (IsPerformanceThrottled)
		{
			return 0.0;
		}
		if (TryGetOverlay(cardId, out string enchantmentId, out bool preview))
		{
			CardEffectEntry entry = EnchantmentConfig.GetEntry(enchantmentId);
			if (preview)
			{
				return entry?.Intensity ?? defaultValue;
			}
			if (entry != null && EnchantmentConfig.HasEffect(enchantmentId))
			{
				return entry.Intensity;
			}
		}
		return GetCardIntensityCore(cardId, defaultValue);
	}

	private static double GetCardIntensityCore(string cardId, double defaultValue)
	{
		if (!_cards.TryGetValue(cardId, out CardEffectEntry value))
		{
			return defaultValue;
		}
		return value.Intensity;
	}

	public static void SetIntensity(string cardId, double value)
	{
		if (!string.IsNullOrEmpty(cardId) && _cards.TryGetValue(cardId, out CardEffectEntry value2))
		{
			value2.Intensity = Math.Clamp(value, 0.0, 1.0);
			ResetPerformanceThrottle();
			Save();
		}
	}

	public static int GetCardEffectIndex(string cardId)
	{
		if (IsPerformanceThrottled)
		{
			return 0;
		}
		if (TryGetOverlay(cardId, out string enchantmentId, out bool preview))
		{
			CardEffectEntry entry = EnchantmentConfig.GetEntry(enchantmentId);
			if (preview)
			{
				if (entry == null)
				{
					return 0;
				}
				return EnchantmentConfig.GetCardEffectIndex(enchantmentId);
			}
			if (entry != null)
			{
				return EnchantmentConfig.GetCardEffectIndex(enchantmentId);
			}
		}
		if (!_cards.TryGetValue(cardId, out CardEffectEntry value))
		{
			return 0;
		}
		return EffectiveMode(value);
	}

	private static void PruneInvalidEntries()
	{
		List<string> list = new List<string>();
		foreach (var (item, cardEffectEntry2) in _cards)
		{
			if (cardEffectEntry2.FullCardEffect == 0 && (cardEffectEntry2.Parts == null || cardEffectEntry2.Parts.Count == 0))
			{
				list.Add(item);
			}
		}
		foreach (string item2 in list)
		{
			_cards.Remove(item2);
		}
	}

	private static int EffectiveMode(CardEffectEntry entry)
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

	public static bool HasAnyEffect()
	{
		if (IsPerformanceThrottled)
		{
			return false;
		}
		if (_cards == null)
		{
			return false;
		}
		foreach (CardEffectEntry value in _cards.Values)
		{
			if (value.FullCardEffect > 0)
			{
				return true;
			}
			if (value.Parts != null && value.Parts.Values.Any((int v) => v > 0))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasEffect(string cardId)
	{
		if (IsPerformanceThrottled)
		{
			return false;
		}
		if (TryGetOverlay(cardId, out string enchantmentId, out bool preview))
		{
			if (preview)
			{
				return EnchantmentConfig.HasEffect(enchantmentId);
			}
			if (!CardHasEffectCore(cardId))
			{
				return EnchantmentConfig.HasEffect(enchantmentId);
			}
			return true;
		}
		return CardHasEffectCore(cardId);
	}

	private static bool CardHasEffectCore(string cardId)
	{
		if (!_cards.TryGetValue(cardId, out CardEffectEntry value))
		{
			return false;
		}
		return EntryHasEffect(value);
	}

	public static CardEffectEntry? GetEntry(string cardId)
	{
		if (IsPerformanceThrottled)
		{
			return null;
		}
		if (!_cards.TryGetValue(cardId, out CardEffectEntry value))
		{
			return null;
		}
		return value;
	}

	public static void ReplaceCardEffects(string cardId, CardEffectEntry source)
	{
		if (source != null && !string.IsNullOrEmpty(cardId))
		{
			CardEffectEntry cardEffectEntry = new CardEffectEntry
			{
				EditMode = source.EditMode,
				Mode = source.Mode,
				Intensity = source.Intensity,
				Parts = new Dictionary<string, int>(source.Parts),
				FullCardEffect = source.FullCardEffect
			};
			if (!EntryHasEffect(cardEffectEntry))
			{
				_cards.Remove(cardId);
			}
			else
			{
				_cards[cardId] = cardEffectEntry;
			}
			ResetPerformanceThrottle();
			Save();
		}
	}

	private static bool EntryHasEffect(CardEffectEntry entry)
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

	public static bool IsValidCardPresetJson(string json)
	{
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(json);
			JsonElement rootElement = jsonDocument.RootElement;
			if (rootElement.TryGetProperty("FullCardEffect", out var value) && value.GetInt32() > 0)
			{
				return true;
			}
			if (rootElement.TryGetProperty("Parts", out var value2))
			{
				foreach (JsonProperty item in value2.EnumerateObject())
				{
					if (item.Value.GetInt32() > 0)
					{
						return true;
					}
				}
			}
			return false;
		}
		catch
		{
			return false;
		}
	}

	public static bool IsValidPresetJson(string json)
	{
		try
		{
			ConfigData configData = JsonSerializer.Deserialize<ConfigData>(json);
			if (configData == null)
			{
				return false;
			}
			bool flag = configData.Version == 8;
			if (!flag)
			{
				int version = configData.Version;
				bool flag2 = (uint)(version - 5) <= 2u;
				flag = flag2;
			}
			return flag;
		}
		catch
		{
			return false;
		}
	}

	public static string ExportPreset()
	{
		return JsonSerializer.Serialize(new ConfigData
		{
			Version = 8,
			Cards = _cards,
			GlobalDynamicEffect = _data.GlobalDynamicEffect,
			EnableShaderInNonCombat = _data.EnableShaderInNonCombat,
			ShownTestWarning = _data.ShownTestWarning
		}, Options);
	}

	public static bool ImportPreset(string json)
	{
		try
		{
			ConfigData configData = JsonSerializer.Deserialize<ConfigData>(json);
			if (configData == null)
			{
				return false;
			}
			if (configData.Version != 8)
			{
				configData = UpgradeToCurrent(json);
				if (configData == null)
				{
					return false;
				}
			}
			if (configData.Cards != null)
			{
				foreach (KeyValuePair<string, CardEffectEntry> card in configData.Cards)
				{
					if (card.Value != null)
					{
						_cards[card.Key] = card.Value;
					}
				}
			}
			PruneInvalidEntries();
			ResetPerformanceThrottle();
			Save();
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static string ExportCardPreset(string cardId)
	{
		CardEffectEntry value;
		CardEffectEntry cardEffectEntry = (_cards.TryGetValue(cardId, out value) ? value : null);
		return JsonSerializer.Serialize(new
		{
			CardId = cardId,
			EditMode = (cardEffectEntry?.EditMode ?? "normal"),
			Mode = (cardEffectEntry?.Mode ?? 0),
			Intensity = (cardEffectEntry?.Intensity ?? 1.0),
			Parts = cardEffectEntry?.Parts,
			FullCardEffect = (cardEffectEntry?.FullCardEffect ?? 0)
		}, Options);
	}

	public static bool ImportCardPreset(string cardId, string json)
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
			CardEffectEntry cardEffectEntry = new CardEffectEntry
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
			_cards[cardId] = cardEffectEntry;
			if (cardEffectEntry.FullCardEffect == 0 && (cardEffectEntry.Parts == null || cardEffectEntry.Parts.Count == 0))
			{
				_cards.Remove(cardId);
			}
			ResetPerformanceThrottle();
			Save();
			return true;
		}
		catch
		{
			return false;
		}
	}

	private static string InferEditMode(CardEffectEntry entry)
	{
		if (entry.FullCardEffect > 0)
		{
			return "fully";
		}
		int num = 0;
		if (entry.Parts != null)
		{
			foreach (int value in entry.Parts.Values)
			{
				if (value > 0)
				{
					if (num == 0)
					{
						num = value;
					}
					else if (value != num)
					{
						return "separately";
					}
				}
			}
		}
		return "normal";
	}

	private static ConfigData? UpgradeToCurrent(string json)
	{
		ConfigData configData = JsonSerializer.Deserialize<ConfigData>(json);
		if (configData == null)
		{
			return null;
		}
		if (configData.Version == 5)
		{
			configData = V5ToV6(JsonSerializer.Deserialize<ConfigDataV5>(json) ?? new ConfigDataV5());
		}
		if (configData.Version == 6)
		{
			V6ToV7(configData);
		}
		if (configData.Version == 7)
		{
			V7ToV8(configData);
		}
		if (configData.Version != 8)
		{
			return null;
		}
		return configData;
	}

	private static ConfigData V5ToV6(ConfigDataV5 v5)
	{
		Dictionary<string, CardEffectEntry> dictionary = v5.Cards ?? new Dictionary<string, CardEffectEntry>();
		if (v5.IntensitySettings != null)
		{
			foreach (CardEffectEntry value2 in dictionary.Values)
			{
				if (value2 != null)
				{
					int num = EffectiveMode(value2);
					if (num > 0 && v5.IntensitySettings.TryGetValue(num, out var value))
					{
						value2.Intensity = value;
					}
				}
			}
		}
		return new ConfigData
		{
			Version = 6,
			Cards = dictionary,
			GlobalDynamicEffect = v5.GlobalDynamicEffect,
			EnableShaderInNonCombat = v5.EnableShaderInNonCombat,
			ShownTestWarning = v5.ShownTestWarning
		};
	}

	private static void V6ToV7(ConfigData data)
	{
		if (data.Cards != null)
		{
			foreach (CardEffectEntry value in data.Cards.Values)
			{
				if (value != null)
				{
					value.EditMode = InferEditMode(value);
				}
			}
		}
		data.Version = 7;
	}

	private static void V7ToV8(ConfigData data)
	{
		if (data.Cards != null)
		{
			foreach (CardEffectEntry value in data.Cards.Values)
			{
				if (value != null)
				{
					RemapEntry(value);
				}
			}
		}
		data.Version = 8;
	}

	private static void RemapEntry(CardEffectEntry e)
	{
		e.Mode = RemapModeV7ToV8(e.Mode);
		e.FullCardEffect = RemapModeV7ToV8(e.FullCardEffect);
		if (e.Parts == null)
		{
			return;
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (KeyValuePair<string, int> part in e.Parts)
		{
			dictionary[part.Key] = RemapModeV7ToV8(part.Value);
		}
		e.Parts = dictionary;
	}

	internal static int RemapModeV7ToV8(int mode)
	{
		return mode switch
		{
			2 => 5, 
			5 => 2, 
			6 => 7, 
			_ => mode, 
		};
	}

	public static void ResetAll()
	{
		_data = new ConfigData();
		ResetPerformanceThrottle();
		Save();
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
			return ImportPreset(val.GetAsText(false));
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void ApplyAllAuthorPresets()
	{
		LoadAuthorPreset("res://balatroeffect/Assets/author_preset.json");
		EnchantmentConfig.LoadAuthorPreset("res://balatroeffect/Assets/author_enchant_preset.json");
	}
}
