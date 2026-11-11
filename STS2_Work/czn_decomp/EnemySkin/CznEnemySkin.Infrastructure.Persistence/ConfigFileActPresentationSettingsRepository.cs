using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Application;
using CznEnemySkin.Domain;
using Godot;

namespace CznEnemySkin.Infrastructure.Persistence;

internal sealed class ConfigFileActPresentationSettingsRepository : IActPresentationSettingsRepository
{
	private const string ConfigDirectory = "user://CznModConfig/CznEnemySkin";

	private const string ProfilesSection = "act_profiles";

	private readonly Dictionary<string, bool> _values = new Dictionary<string, bool>(StringComparer.Ordinal);

	private readonly Action<string> _warn;

	public ConfigFileActPresentationSettingsRepository(IEnumerable<ActPresentationProfile> profiles, Action<string> warn)
	{
		_warn = warn ?? throw new ArgumentNullException("warn");
		foreach (ActPresentationProfile profile in profiles)
		{
			if (_values.TryGetValue(profile.SettingId, out var value))
			{
				if (value != profile.EnabledByDefault)
				{
					throw new InvalidOperationException("Act routes sharing setting id '" + profile.SettingId + "' must share a default value.");
				}
			}
			else
			{
				_values.Add(profile.SettingId, profile.EnabledByDefault);
			}
		}
		Load();
	}

	public bool IsEnabled(string profileId, bool defaultValue)
	{
		if (!_values.TryGetValue(profileId, out var value))
		{
			return defaultValue;
		}
		return value;
	}

	public void SetEnabled(string profileId, bool enabled)
	{
		_values[profileId] = enabled;
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
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Invalid comparison between Unknown and I8
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
				_warn($"Could not load act presentation settings ({val2}); " + "using defaults.");
				return;
			}
			string[] array = _values.Keys.ToArray();
			foreach (string text in array)
			{
				bool flag = _values[text];
				Variant value = val.GetValue("act_profiles", text, Variant.From<bool>(ref flag));
				if ((long)((Variant)(ref value)).VariantType == 1)
				{
					_values[text] = ((Variant)(ref value)).AsBool();
				}
				else
				{
					_warn("Ignored non-boolean act presentation setting '" + text + "'.");
				}
			}
		}
		catch (Exception ex)
		{
			_warn("Failed to load act presentation settings; using defaults: " + ex.Message);
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
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Invalid comparison between Unknown and I8
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Error val = DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath("user://CznModConfig/CznEnemySkin"));
			if ((int)val != 0 && (long)val != 32)
			{
				_warn("Could not create act presentation settings directory: " + ((object)(Error)(ref val)/*cast due to .constrained prefix*/).ToString());
				return;
			}
			ConfigFile val2 = new ConfigFile();
			Error val3 = val2.Load("user://CznModConfig/CznEnemySkin/settings.cfg");
			if ((int)val3 != 0 && (long)val3 != 7)
			{
				_warn("Could not preserve existing settings while saving act " + $"presentation settings: {val3}");
				return;
			}
			foreach (var (text2, flag2) in _values)
			{
				val2.SetValue("act_profiles", text2, Variant.op_Implicit(flag2));
			}
			Error val4 = val2.Save("user://CznModConfig/CznEnemySkin/settings.cfg");
			if ((int)val4 != 0)
			{
				_warn("Could not save act presentation settings: " + ((object)(Error)(ref val4)/*cast due to .constrained prefix*/).ToString());
			}
		}
		catch (Exception ex)
		{
			_warn("Failed to save act presentation settings: " + ex.Message);
		}
	}
}
