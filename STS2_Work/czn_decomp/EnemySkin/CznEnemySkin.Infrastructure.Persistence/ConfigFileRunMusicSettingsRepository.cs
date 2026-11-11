using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Application;
using CznEnemySkin.Domain;
using Godot;

namespace CznEnemySkin.Infrastructure.Persistence;

internal sealed class ConfigFileRunMusicSettingsRepository : IRunMusicSettingsRepository
{
	private const string ConfigDirectory = "user://CznModConfig/CznEnemySkin";

	private const string MusicSection = "music";

	private const string BgmSourceKey = "bgm_source";

	private const string BossBgmSourceKey = "boss_bgm_source";

	private const string ProfilesSection = "run_music_profiles";

	private const string OverridesSection = "run_music_track_overrides";

	private readonly Dictionary<string, bool> _enabled = new Dictionary<string, bool>(StringComparer.Ordinal);

	private readonly Dictionary<string, string?> _overrides = new Dictionary<string, string>(StringComparer.Ordinal);

	private readonly Action<string> _warn;

	private RunMusicSource _bgmSource;

	private BossMusicSource _bossMusicSource;

	public ConfigFileRunMusicSettingsRepository(IEnumerable<ActMapMusicProfile> profiles, Action<string> warn)
	{
		_warn = warn ?? throw new ArgumentNullException("warn");
		foreach (ActMapMusicProfile profile in profiles)
		{
			_enabled.Add(profile.Id, profile.EnabledByDefault);
			_overrides.Add(profile.Id, null);
		}
		Load();
	}

	public RunMusicSource GetBgmSource()
	{
		return _bgmSource;
	}

	public void SetBgmSource(RunMusicSource source)
	{
		if (Enum.IsDefined(source))
		{
			_bgmSource = source;
			Save();
		}
	}

	public BossMusicSource GetBossMusicSource()
	{
		return _bossMusicSource;
	}

	public void SetBossMusicSource(BossMusicSource source)
	{
		if (Enum.IsDefined(source))
		{
			_bossMusicSource = source;
			Save();
		}
	}

	public bool IsEnabled(string profileId, bool defaultValue)
	{
		if (!_enabled.TryGetValue(profileId, out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public void SetEnabled(string profileId, bool enabled)
	{
		_enabled[profileId] = enabled;
		Save();
	}

	public string? GetTrackOverride(string profileId)
	{
		if (!_overrides.TryGetValue(profileId, out string value))
		{
			return null;
		}
		return value;
	}

	public void SetTrackOverride(string profileId, string? trackId)
	{
		_overrides[profileId] = (string.IsNullOrWhiteSpace(trackId) ? null : trackId.Trim());
		Save();
	}

	private void Load()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I8
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Invalid comparison between Unknown and I8
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Invalid comparison between Unknown and I8
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Invalid comparison between Unknown and I8
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Invalid comparison between Unknown and I8
		try
		{
			ConfigFile val = new ConfigFile();
			Error val2 = val.Load("user://CznModConfig/CznEnemySkin/settings.cfg");
			if ((long)val2 == 7)
			{
				Save();
				return;
			}
			if ((int)val2 != 0)
			{
				_warn($"Could not load run-music settings ({val2}); using defaults.");
				return;
			}
			string text = SourceValue(RunMusicSource.Czn);
			Variant value = val.GetValue("music", "bgm_source", Variant.From<string>(ref text));
			if ((long)((Variant)(ref value)).VariantType == 4 && TryParseSource(((Variant)(ref value)).AsString(), out var source))
			{
				_bgmSource = source;
			}
			else
			{
				_warn("Ignored invalid BGM source setting; using CZN music.");
				_bgmSource = RunMusicSource.Czn;
			}
			text = BossSourceValue(BossMusicSource.Curated);
			Variant value2 = val.GetValue("music", "boss_bgm_source", Variant.From<string>(ref text));
			if ((long)((Variant)(ref value2)).VariantType == 4 && TryParseBossSource(((Variant)(ref value2)).AsString(), out var source2))
			{
				_bossMusicSource = source2;
			}
			else
			{
				_warn("Ignored invalid Boss BGM source setting; using curated music.");
				_bossMusicSource = BossMusicSource.Curated;
			}
			string[] array = _enabled.Keys.ToArray();
			foreach (string text2 in array)
			{
				bool flag = _enabled[text2];
				Variant value3 = val.GetValue("run_music_profiles", text2, Variant.From<bool>(ref flag));
				if ((long)((Variant)(ref value3)).VariantType == 1)
				{
					_enabled[text2] = ((Variant)(ref value3)).AsBool();
				}
				else
				{
					_warn("Ignored non-boolean run-music setting '" + text2 + "'.");
				}
				Variant value4 = val.GetValue("run_music_track_overrides", text2, Variant.From<string>(ref string.Empty));
				if ((long)((Variant)(ref value4)).VariantType == 4)
				{
					string text3 = ((Variant)(ref value4)).AsString().Trim();
					_overrides[text2] = ((text3.Length == 0) ? null : text3);
				}
				else
				{
					_warn("Ignored non-string run-music override '" + text2 + "'.");
				}
			}
		}
		catch (Exception ex)
		{
			_warn("Failed to load run-music settings; using defaults: " + ex.Message);
		}
	}

	private void Save()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I8
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Invalid comparison between Unknown and I8
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Error val = DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath("user://CznModConfig/CznEnemySkin"));
			if ((int)val != 0 && (long)val != 32)
			{
				_warn("Could not create run-music settings directory: " + ((object)(Error)(ref val)/*cast due to .constrained prefix*/).ToString());
				return;
			}
			ConfigFile val2 = new ConfigFile();
			Error val3 = val2.Load("user://CznModConfig/CznEnemySkin/settings.cfg");
			if ((int)val3 != 0 && (long)val3 != 7)
			{
				_warn("Could not preserve existing settings while saving " + $"run-music settings: {val3}");
				return;
			}
			val2.SetValue("music", "bgm_source", Variant.op_Implicit(SourceValue(_bgmSource)));
			val2.SetValue("music", "boss_bgm_source", Variant.op_Implicit(BossSourceValue(_bossMusicSource)));
			foreach (var (text2, flag2) in _enabled)
			{
				val2.SetValue("run_music_profiles", text2, Variant.op_Implicit(flag2));
				val2.SetValue("run_music_track_overrides", text2, Variant.op_Implicit(_overrides[text2] ?? string.Empty));
			}
			Error val4 = val2.Save("user://CznModConfig/CznEnemySkin/settings.cfg");
			if ((int)val4 != 0)
			{
				_warn("Could not save run-music settings: " + ((object)(Error)(ref val4)/*cast due to .constrained prefix*/).ToString());
			}
		}
		catch (Exception ex)
		{
			_warn("Failed to save run-music settings: " + ex.Message);
		}
	}

	private static string SourceValue(RunMusicSource source)
	{
		if (source == RunMusicSource.Sts2)
		{
			return "sts2";
		}
		return "czn";
	}

	private static bool TryParseSource(string value, out RunMusicSource source)
	{
		string text = value.Trim().ToLowerInvariant();
		if (!(text == "czn"))
		{
			if (text == "sts2")
			{
				source = RunMusicSource.Sts2;
				return true;
			}
			source = RunMusicSource.Czn;
			return false;
		}
		source = RunMusicSource.Czn;
		return true;
	}

	private static string BossSourceValue(BossMusicSource source)
	{
		if (source == BossMusicSource.Czn)
		{
			return "czn";
		}
		return "curated";
	}

	private static bool TryParseBossSource(string value, out BossMusicSource source)
	{
		string text = value.Trim().ToLowerInvariant();
		if (!(text == "curated"))
		{
			if (text == "czn")
			{
				source = BossMusicSource.Czn;
				return true;
			}
			source = BossMusicSource.Curated;
			return false;
		}
		source = BossMusicSource.Curated;
		return true;
	}
}
