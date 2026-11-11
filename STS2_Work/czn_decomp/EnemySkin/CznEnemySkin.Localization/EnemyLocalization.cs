using System;
using System.Collections.Generic;
using System.Globalization;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Localization;

namespace CznEnemySkin.Localization;

internal static class EnemyLocalization
{
	private const string MonsterTable = "monsters";

	private const string SettingsTable = "settings_ui";

	private const string KeyPrefix = "CZN_ENEMY_SKIN.";

	private static readonly HashSet<string> ReportedMissingKeys = new HashSet<string>(StringComparer.Ordinal);

	public static bool TryGetEnemyName(EnemyReplacementProfile profile, out string name)
	{
		return TryGet("monsters", "CZN_ENEMY_SKIN." + profile.Source.OfficialNameKey, out name);
	}

	public static string GetToggleLabel(EnemyReplacementProfile profile)
	{
		string enemyNameOrSourceId = GetEnemyNameOrSourceId(profile);
		return Format(GetUi("bestiary.toggle_format", "Replace with {0}"), enemyNameOrSourceId);
	}

	public static string GetStatus(bool enabled)
	{
		if (!enabled)
		{
			return GetUi("bestiary.disabled", "Disabled");
		}
		return GetUi("bestiary.enabled", "Enabled");
	}

	public static string GetToggleTooltip()
	{
		return GetUi("bestiary.toggle_tooltip", "Toggle the Chaos Zero Nightmare appearance replacement for this enemy.");
	}

	public static string GetSettingsText(string key, string fallback)
	{
		return GetUi(key, fallback);
	}

	public static string GetPreviewAction(EnemyBestiaryPreviewAction action)
	{
		return action switch
		{
			EnemyBestiaryPreviewAction.Hatch => GetUi("bestiary.action.hatch", "Hatch"), 
			EnemyBestiaryPreviewAction.Summon => GetUi("bestiary.action.summon", "Summon"), 
			EnemyBestiaryPreviewAction.Awaken => GetUi("bestiary.action.awaken", "Awaken"), 
			_ => GetUi("bestiary.action.entrance", "Entrance"), 
		};
	}

	public static string FormatBestiaryDisplayName(EnemyReplacementProfile profile, string originalName)
	{
		if (!TryGetEnemyName(profile, out string name))
		{
			return originalName;
		}
		return Format(GetUi("bestiary.name_format", "{0} ({1})"), name, originalName);
	}

	private static string GetEnemyNameOrSourceId(EnemyReplacementProfile profile)
	{
		if (!TryGetEnemyName(profile, out string name))
		{
			return profile.Source.MonsterId;
		}
		return name;
	}

	private static string GetUi(string key, string fallback)
	{
		if (!TryGet("settings_ui", "CZN_ENEMY_SKIN." + key, out string value))
		{
			return fallback;
		}
		return value;
	}

	private static bool TryGet(string tableName, string key, out string value)
	{
		try
		{
			value = LocManager.Instance.GetTable(tableName).GetRawText(key);
			if (!string.IsNullOrWhiteSpace(value))
			{
				return true;
			}
		}
		catch (Exception ex)
		{
			ReportMissingOnce(tableName, key, ex.Message);
		}
		value = string.Empty;
		return false;
	}

	private static string Format(string format, params object[] values)
	{
		try
		{
			LocManager instance = LocManager.Instance;
			return string.Format(((instance != null) ? instance.CultureInfo : null) ?? CultureInfo.InvariantCulture, format, values);
		}
		catch (FormatException ex)
		{
			ReportMissingOnce("settings_ui", format, ex.Message);
			return string.Join(" ", values);
		}
	}

	private static void ReportMissingOnce(string tableName, string key, string error)
	{
		string item = tableName + ":" + key;
		if (ReportedMissingKeys.Add(item))
		{
			Entry.Log.Warn("Enemy localization fallback used " + $"[table={tableName}, key={key}, error={error}].", 1);
		}
	}
}
