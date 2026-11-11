using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CznEnemySkin.Application;
using CznEnemySkin.Localization;

namespace CznEnemySkin.Configuration;

internal static class CznModConfigRegistration
{
	private const string RegistryDataKey = "CznModConfig.SharedSettingsExtensions.v1";

	private const string PresentationProviderId = "CznEnemySkin.Presentation";

	private const string MusicProviderId = "CznEnemySkin.Music";

	internal static void Register()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["api_version"] = 1;
		dictionary["category_id"] = "enemy_skin";
		dictionary["category_order"] = 300;
		dictionary["category_label"] = (Func<string>)(() => Text("config.category", "Enemy Appearance"));
		dictionary["group_order"] = 100;
		dictionary["group_label"] = (Func<string>)(() => Text("config.presentation_header", "Enemy Presentation"));
		dictionary["reset"] = new Action(Entry.PresentationSettings.Reset);
		dictionary["controls"] = new object[3]
		{
			CreateToggle(),
			CreateActionEffectsToggle(),
			CreateBrightnessSlider()
		};
		Dictionary<string, object> value = dictionary;
		dictionary = new Dictionary<string, object>();
		dictionary["api_version"] = 1;
		dictionary["category_id"] = "enemy_skin";
		dictionary["category_order"] = 300;
		dictionary["category_label"] = (Func<string>)(() => Text("config.category", "Enemy Appearance"));
		dictionary["group_order"] = 200;
		dictionary["group_label"] = (Func<string>)(() => Text("config.music_header", "Music"));
		dictionary["reset"] = new Action(Entry.Music.Reset);
		dictionary["controls"] = new object[2]
		{
			CreateBgmSourceChoice(),
			CreateBossBgmSourceChoice()
		};
		Dictionary<string, object> value2 = dictionary;
		lock (string.Intern("CznModConfig.SharedSettingsExtensions.v1"))
		{
			ConcurrentDictionary<string, object> concurrentDictionary = AppDomain.CurrentDomain.GetData("CznModConfig.SharedSettingsExtensions.v1") as ConcurrentDictionary<string, object>;
			if (concurrentDictionary == null)
			{
				concurrentDictionary = new ConcurrentDictionary<string, object>(StringComparer.Ordinal);
				AppDomain.CurrentDomain.SetData("CznModConfig.SharedSettingsExtensions.v1", concurrentDictionary);
			}
			concurrentDictionary["CznEnemySkin.Presentation"] = value;
			concurrentDictionary["CznEnemySkin.Music"] = value2;
		}
	}

	private static Dictionary<string, object> CreateBossBgmSourceChoice()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["type"] = "choice";
		dictionary["id"] = "boss_bgm_source";
		dictionary["order"] = 200;
		dictionary["label"] = (Func<string>)(() => Text("config.boss_bgm_source", "Boss BGM"));
		dictionary["description"] = (Func<string>)(() => Text("config.boss_bgm_source_description", "Use curated Boss music when available; otherwise use CZN music."));
		dictionary["get"] = (Func<int>)(() => (int)Entry.Music.BossBgmSource);
		dictionary["set"] = (Action<int>)delegate(int value)
		{
			if (Enum.IsDefined(typeof(BossMusicSource), value))
			{
				Entry.Music.SetBossMusicSource((BossMusicSource)value);
			}
		};
		dictionary["options"] = new object[2]
		{
			CreateChoiceOption(0, "config.boss_bgm_source_curated", "Curated (Default)"),
			CreateChoiceOption(1, "config.boss_bgm_source_czn", "Chaos Zero Nightmare")
		};
		return dictionary;
	}

	private static Dictionary<string, object> CreateBgmSourceChoice()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["type"] = "choice";
		dictionary["id"] = "bgm_source";
		dictionary["order"] = 100;
		dictionary["label"] = (Func<string>)(() => Text("config.bgm_source", "BGM Source"));
		dictionary["description"] = (Func<string>)(() => Text("config.bgm_source_description", "Choose the music used on the map and in combat."));
		dictionary["get"] = (Func<int>)(() => (int)Entry.Music.BgmSource);
		dictionary["set"] = (Action<int>)delegate(int value)
		{
			if (Enum.IsDefined(typeof(RunMusicSource), value))
			{
				Entry.Music.SetBgmSource((RunMusicSource)value);
			}
		};
		dictionary["options"] = new object[2]
		{
			CreateChoiceOption(0, "config.bgm_source_czn", "Chaos Zero Nightmare"),
			CreateChoiceOption(1, "config.bgm_source_sts2", "STS2 Original")
		};
		return dictionary;
	}

	private static Dictionary<string, object> CreateChoiceOption(int id, string key, string fallback)
	{
		return new Dictionary<string, object>
		{
			["id"] = id,
			["label"] = (Func<string>)(() => Text(key, fallback))
		};
	}

	private static Dictionary<string, object> CreateToggle()
	{
		return new Dictionary<string, object>
		{
			["type"] = "toggle",
			["id"] = "show_original_enemy_name",
			["order"] = 100,
			["label"] = (Func<string>)(() => Text("config.show_original_enemy_name", "Show Original Enemy Name")),
			["description"] = (Func<string>)(() => Text("config.show_original_enemy_name_description", "Shows the STS2 enemy name below its replacement name.")),
			["get"] = (Func<bool>)(() => Entry.PresentationSettings.ShowOriginalEnemyName),
			["set"] = new Action<bool>(Entry.PresentationSettings.SetShowOriginalEnemyName)
		};
	}

	private static Dictionary<string, object> CreateActionEffectsToggle()
	{
		return new Dictionary<string, object>
		{
			["type"] = "toggle",
			["id"] = "action_effects_enabled",
			["order"] = 150,
			["label"] = (Func<string>)(() => Text("config.action_effects_enabled", "Enemy Attack / Skill Effects")),
			["description"] = (Func<string>)(() => Text("config.action_effects_enabled_description", "Enabled by default. Toggles added CZN enemy attack and skill effects. Body animations, audio, entrance, hit and death effects are preserved.")),
			["get"] = (Func<bool>)(() => Entry.PresentationSettings.ActionEffectsEnabled),
			["set"] = new Action<bool>(Entry.PresentationSettings.SetActionEffectsEnabled)
		};
	}

	private static Dictionary<string, object> CreateBrightnessSlider()
	{
		return new Dictionary<string, object>
		{
			["type"] = "slider",
			["id"] = "effect_brightness",
			["order"] = 200,
			["label"] = (Func<string>)(() => Text("config.effect_brightness", "Enemy Effect Brightness")),
			["description"] = (Func<string>)(() => Text("config.effect_brightness_description", "Adjusts replacement effects. The initial value is 100%; choose the level you prefer.")),
			["get"] = (Func<double>)(() => (double)Entry.PresentationSettings.EffectBrightness * 100.0),
			["set"] = (Action<double>)delegate(double value)
			{
				Entry.PresentationSettings.SetEffectBrightness((float)(value / 100.0));
			},
			["min"] = 0.0,
			["max"] = 100.0,
			["step"] = 5.0,
			["suffix"] = "%"
		};
	}

	private static string Text(string key, string fallback)
	{
		return EnemyLocalization.GetSettingsText(key, fallback);
	}
}
