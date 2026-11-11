using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Godot;

namespace ModLaunchManager;

internal static class MlmLocManager
{
	internal const string SystemPreference = "system";

	internal const float MinimumFontScale = 0.85f;

	internal const float MaximumFontScale = 1.25f;

	internal const string FileName = "mod_launch_manager_settings.json";

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true
	};

	private static readonly IReadOnlyDictionary<string, int> GameLocaleOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = 0,
		["zhs"] = 1,
		["zht"] = 2,
		["deu"] = 3,
		["esp"] = 4,
		["fra"] = 5,
		["ind"] = 6,
		["ita"] = 7,
		["jpn"] = 8,
		["kor"] = 9,
		["pol"] = 10,
		["ptb"] = 11,
		["rus"] = 12,
		["spa"] = 13,
		["tha"] = 14,
		["tur"] = 15
	};

	private static readonly HashSet<string> ReportedFormatErrors = new HashSet<string>(StringComparer.Ordinal);

	private static bool _tableLoaded;

	private static MlmLocTable _table = MlmLocTable.Empty();

	private static IReadOnlyList<MlmLocaleInfo> _availableLocales = Array.Empty<MlmLocaleInfo>();

	private static bool _preferencesLoaded;

	private static LauncherPreferences _preferences = new LauncherPreferences();

	internal static string Preference
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.Language;
		}
	}

	internal static IReadOnlyList<MlmLocaleInfo> AvailableLocales
	{
		get
		{
			EnsureTableLoaded();
			return _availableLocales;
		}
	}

	internal static LauncherTheme ThemePreference
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.Theme;
		}
	}

	internal static int SaveProfileId
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.SaveProfileId ?? 1;
		}
	}

	internal static string BackgroundPreference
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.Background;
		}
	}

	internal static bool AutoUnlockAll
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.AutoUnlockAll;
		}
	}

	internal static float FontScale
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.FontScale;
		}
	}

	internal static bool ShowAffectsGameplayColumn
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.ShowAffectsGameplayColumn;
		}
	}

	internal static bool GroupListView
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.GroupListView;
		}
	}

	internal static bool FiltersCollapsed
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.FiltersCollapsed;
		}
	}

	internal static bool KeepDisabledInPlace
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.KeepDisabledInPlace;
		}
	}

	internal static IReadOnlyList<string> CollapsedGroups
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.CollapsedGroups ?? new List<string>();
		}
	}

	internal static bool CloudSync
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.CloudSync;
		}
	}

	internal static bool ShowRandomModButton
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.ShowRandomModButton;
		}
	}

	internal static bool ShowVanillaLaunchButton
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.ShowVanillaLaunchButton;
		}
	}

	internal static int AcknowledgedUpdateRevision
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.AcknowledgedUpdateRevision;
		}
	}

	internal static string EffectiveLocale
	{
		get
		{
			EnsurePreferencesLoaded();
			if (!(_preferences.Language == "system"))
			{
				return MatchAvailableLocale(_preferences.Language);
			}
			return MatchAvailableLocale(DetectSystemLocale());
		}
	}

	internal static bool ModIsolation
	{
		get
		{
			EnsurePreferencesLoaded();
			return _preferences.ModIsolation;
		}
	}

	internal static LauncherTheme EffectiveTheme
	{
		get
		{
			EnsurePreferencesLoaded();
			if (_preferences.Theme != 0)
			{
				return _preferences.Theme;
			}
			return DetectSystemTheme();
		}
	}

	private static string SettingsPath => LauncherDataPaths.File("mod_launch_manager_settings.json");

	internal static string Get(string key, params (string Name, object? Value)[] arguments)
	{
		EnsureTableLoaded();
		Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.Ordinal);
		for (int i = 0; i < arguments.Length; i++)
		{
			var (key2, value) = arguments[i];
			dictionary.Add(key2, value);
		}
		try
		{
			return _table.Format(EffectiveLocale, key, dictionary);
		}
		catch (Exception ex) when (((ex is FormatException || ex is ArgumentException) ? 1 : 0) != 0)
		{
			if (ReportedFormatErrors.Add(key))
			{
				GD.PushWarning($"{"[ModLaunchManager]"} localization formatting failed for '{key}': {ex.Message}");
			}
			return _table.GetRaw(EffectiveLocale, key);
		}
	}

	internal static string PreferenceLabel(string preference)
	{
		EnsureTableLoaded();
		if (string.Equals(preference, "system", StringComparison.OrdinalIgnoreCase))
		{
			return Get("settings.language.system");
		}
		if (!_table.TryGetLocale(preference, out MlmLocaleInfo info))
		{
			return preference;
		}
		return info.DisplayName;
	}

	internal static string ThemeLabel(LauncherTheme theme)
	{
		return theme switch
		{
			LauncherTheme.System => Get("settings.theme.system"), 
			LauncherTheme.Fluent => Get("settings.theme.fluent"), 
			LauncherTheme.FluentDark => Get("settings.theme.fluentDark"), 
			LauncherTheme.LiquidGlass => Get("settings.theme.liquidGlass"), 
			LauncherTheme.LiquidGlassDark => Get("settings.theme.liquidGlassDark"), 
			_ => Get("settings.theme.system"), 
		};
	}

	internal static bool SetPreference(string preference, out string? error)
	{
		EnsurePreferencesLoaded();
		string text = NormalizePreference(preference);
		_preferences.Language = ((text == "system" || _table.TryGetLocale(text, out MlmLocaleInfo _)) ? text : "system");
		return Save(out error);
	}

	internal static bool SetThemePreference(LauncherTheme theme, out string? error)
	{
		EnsurePreferencesLoaded();
		_preferences.Theme = (Enum.IsDefined(theme) ? theme : LauncherTheme.System);
		return Save(out error);
	}

	internal static bool SetBackgroundPreference(string selection, out string? error)
	{
		EnsurePreferencesLoaded();
		_preferences.Background = (string.IsNullOrWhiteSpace(selection) ? "auto" : selection.Trim());
		return Save(out error);
	}

	internal static bool SetSaveProfileId(int profileId, out string? error)
	{
		EnsurePreferencesLoaded();
		if ((profileId < 1 || profileId > 3) ? true : false)
		{
			throw new ArgumentOutOfRangeException("profileId", profileId, $"Game save profile must be between 1 and {3}.");
		}
		_preferences.SaveProfileId = profileId;
		return Save(out error);
	}

	internal static bool SetFontScale(float scale, out string? error)
	{
		EnsurePreferencesLoaded();
		_preferences.FontScale = NormalizeFontScale(scale);
		return Save(out error);
	}

	internal static bool SetAutoUnlockAll(bool enabled, out string? error)
	{
		EnsurePreferencesLoaded();
		_preferences.AutoUnlockAll = enabled;
		return Save(out error);
	}

	internal static bool SetShowAffectsGameplayColumn(bool show, out string? error)
	{
		EnsurePreferencesLoaded();
		_preferences.ShowAffectsGameplayColumn = show;
		return Save(out error);
	}

	internal static bool SetGroupListView(bool listView, out string? error)
	{
		EnsurePreferencesLoaded();
		_preferences.GroupListView = listView;
		return Save(out error);
	}

	internal static bool SetFiltersCollapsed(bool collapsed, out string? error)
	{
		EnsurePreferencesLoaded();
		_preferences.FiltersCollapsed = collapsed;
		return Save(out error);
	}

	internal static bool SetKeepDisabledInPlace(bool keep, out string? error)
	{
		EnsurePreferencesLoaded();
		_preferences.KeepDisabledInPlace = keep;
		return Save(out error);
	}

	internal static bool SetCollapsedGroups(IEnumerable<string> groups, out string? error)
	{
		EnsurePreferencesLoaded();
		_preferences.CollapsedGroups = groups.Where((string group) => !string.IsNullOrWhiteSpace(group)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
		return Save(out error);
	}

	internal static bool SetCloudSync(bool enabled, out string? error)
	{
		EnsurePreferencesLoaded();
		_preferences.CloudSync = enabled;
		return Save(out error);
	}

	internal static bool SetModIsolation(bool enabled, out string? error)
	{
		EnsurePreferencesLoaded();
		_preferences.ModIsolation = enabled;
		return Save(out error);
	}

	internal static bool SetShowRandomModButton(bool show, out string? error)
	{
		EnsurePreferencesLoaded();
		_preferences.ShowRandomModButton = show;
		return Save(out error);
	}

	internal static bool SetShowVanillaLaunchButton(bool show, out string? error)
	{
		EnsurePreferencesLoaded();
		_preferences.ShowVanillaLaunchButton = show;
		return Save(out error);
	}

	internal static bool AcknowledgeUpdate(int revision, out string? error)
	{
		EnsurePreferencesLoaded();
		int acknowledgedUpdateRevision = _preferences.AcknowledgedUpdateRevision;
		if (revision <= acknowledgedUpdateRevision)
		{
			error = null;
			return true;
		}
		_preferences.AcknowledgedUpdateRevision = revision;
		if (Save(out error))
		{
			return true;
		}
		_preferences.AcknowledgedUpdateRevision = acknowledgedUpdateRevision;
		return false;
	}

	internal static void ReloadPreferences()
	{
		_preferencesLoaded = false;
		_preferences = new LauncherPreferences();
	}

	private static void EnsureTableLoaded()
	{
		if (_tableLoaded)
		{
			return;
		}
		_tableLoaded = true;
		try
		{
			_table = MlmLocTable.LoadFromAssembly(typeof(MlmLocManager).Assembly);
			_availableLocales = _table.Locales.OrderBy((MlmLocaleInfo locale) => GameLocaleOrder.GetValueOrDefault(locale.GameLanguage, int.MaxValue)).ThenBy<MlmLocaleInfo, string>((MlmLocaleInfo locale) => locale.EnglishName, StringComparer.OrdinalIgnoreCase).ToArray();
		}
		catch (Exception ex)
		{
			_table = MlmLocTable.Empty();
			_availableLocales = _table.Locales;
			GD.PushWarning("[ModLaunchManager] embedded localization could not be loaded: " + ex.Message);
		}
	}

	private static bool Save(out string? error)
	{
		try
		{
			File.WriteAllText(SettingsPath, JsonSerializer.Serialize(_preferences, JsonOptions), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			error = null;
			ConfigCloudChanges.Notify();
			return true;
		}
		catch (Exception ex)
		{
			error = ex.Message;
			GD.PushWarning($"{"[ModLaunchManager]"} could not save launcher preferences: {ex}");
			return false;
		}
	}

	private static void EnsurePreferencesLoaded()
	{
		if (_preferencesLoaded)
		{
			return;
		}
		EnsureTableLoaded();
		_preferencesLoaded = true;
		try
		{
			string path = LauncherDataPaths.ReadFile("mod_launch_manager_settings.json");
			LauncherPreferences launcherPreferences = (File.Exists(path) ? (JsonSerializer.Deserialize<LauncherPreferences>(File.ReadAllText(path, Encoding.UTF8), JsonOptions) ?? new LauncherPreferences()) : new LauncherPreferences());
			launcherPreferences.Language = NormalizePreference(launcherPreferences.Language);
			if (launcherPreferences.Language != "system" && !_table.TryGetLocale(launcherPreferences.Language, out MlmLocaleInfo _))
			{
				launcherPreferences.Language = "system";
			}
			if (!Enum.IsDefined(launcherPreferences.Theme))
			{
				launcherPreferences.Theme = LauncherTheme.System;
			}
			if (string.IsNullOrWhiteSpace(launcherPreferences.Background))
			{
				launcherPreferences.Background = "auto";
			}
			launcherPreferences.FontScale = NormalizeFontScale(launcherPreferences.FontScale);
			int num;
			switch (launcherPreferences.SaveProfileId)
			{
			default:
				num = 1;
				break;
			case 1:
			case 2:
			case 3:
				num = launcherPreferences.SaveProfileId.Value;
				break;
			}
			int num2 = num;
			int? num3 = SaveProfileSwitcher.ReadCurrentProfileId();
			int num4 = num3 ?? num2;
			bool num5 = launcherPreferences.SaveProfileId != num4;
			launcherPreferences.SaveProfileId = num4;
			_preferences = launcherPreferences;
			if (num5)
			{
				Save(out string _);
				if (num3.HasValue)
				{
					GD.Print($"{"[ModLaunchManager]"} synchronized launcher save profile to game save profile {num4}.");
				}
			}
		}
		catch (Exception ex)
		{
			_preferences = new LauncherPreferences
			{
				SaveProfileId = (SaveProfileSwitcher.ReadCurrentProfileId() ?? 1)
			};
			GD.PushWarning("[ModLaunchManager] could not read launcher preferences: " + ex.Message);
		}
	}

	private static string NormalizePreference(string? preference)
	{
		string value = preference?.Trim().Replace('_', '-') ?? string.Empty;
		if (value.Length == 0 || value.Equals("System", StringComparison.OrdinalIgnoreCase))
		{
			return "system";
		}
		if (value.Equals("Chinese", StringComparison.OrdinalIgnoreCase))
		{
			return "zh-Hans";
		}
		if (value.Equals("English", StringComparison.OrdinalIgnoreCase))
		{
			return "en";
		}
		EnsureTableLoaded();
		if (_table.TryGetLocale(value, out MlmLocaleInfo info))
		{
			return info.Id;
		}
		return _availableLocales.FirstOrDefault((MlmLocaleInfo locale) => locale.GameLanguage.Equals(value, StringComparison.OrdinalIgnoreCase))?.Id ?? value;
	}

	private static float NormalizeFontScale(float scale)
	{
		if (!float.IsFinite(scale))
		{
			return 1f;
		}
		return MathF.Round(Math.Clamp(scale, 0.85f, 1.25f) * 20f) / 20f;
	}

	private static string MatchAvailableLocale(string requested)
	{
		EnsureTableLoaded();
		string text = requested.Trim().Replace('_', '-');
		int num = 1;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		CollectionsMarshal.AsSpan(list)[0] = text;
		List<string> list2 = list;
		string text2 = text.ToLowerInvariant();
		if (text2.StartsWith("zh", StringComparison.Ordinal))
		{
			bool flag = text2.Contains("hant", StringComparison.Ordinal) || text2.EndsWith("-tw", StringComparison.Ordinal) || text2.EndsWith("-hk", StringComparison.Ordinal) || text2.EndsWith("-mo", StringComparison.Ordinal);
			list2.Add(flag ? "zh-Hant" : "zh-Hans");
		}
		else if (text2.StartsWith("es", StringComparison.Ordinal))
		{
			list2.Add(text2.EndsWith("-es", StringComparison.Ordinal) ? "es-ES" : "es-419");
		}
		else if (text2.StartsWith("pt", StringComparison.Ordinal))
		{
			list2.Add("pt-BR");
		}
		int num2 = text.IndexOf('-');
		if (num2 > 0)
		{
			list2.Add(text.Substring(0, num2));
		}
		foreach (string item in list2.Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			string locale = NormalizePreference(item);
			if (_table.TryGetLocale(locale, out MlmLocaleInfo info))
			{
				return info.Id;
			}
		}
		return _table.SourceLocale.Id;
	}

	private static LauncherTheme DetectSystemTheme()
	{
		try
		{
			if (DisplayServer.IsDarkModeSupported())
			{
				return (!DisplayServer.IsDarkMode()) ? LauncherTheme.Fluent : LauncherTheme.FluentDark;
			}
		}
		catch
		{
		}
		return LauncherTheme.FluentDark;
	}

	private static string DetectSystemLocale()
	{
		string text = string.Empty;
		try
		{
			text = OS.GetLocale();
		}
		catch
		{
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		return CultureInfo.CurrentUICulture.Name;
	}
}
