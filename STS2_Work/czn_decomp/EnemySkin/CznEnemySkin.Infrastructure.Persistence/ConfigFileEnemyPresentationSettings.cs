using System;
using Godot;

namespace CznEnemySkin.Infrastructure.Persistence;

internal sealed class ConfigFileEnemyPresentationSettings
{
	internal const string ConfigPath = "user://CznModConfig/CznEnemySkin/presentation.cfg";

	internal const bool DefaultShowOriginalEnemyName = true;

	internal const bool DefaultActionEffectsEnabled = true;

	internal const float DefaultEffectBrightness = 1f;

	internal const float MinimumEffectBrightness = 0f;

	internal const float MaximumEffectBrightness = 1f;

	private const string ConfigDirectory = "user://CznModConfig/CznEnemySkin";

	private const string PresentationSection = "presentation";

	private const string ShowOriginalNameKey = "show_original_enemy_name";

	private const string ActionEffectsEnabledKey = "action_effects_enabled";

	private const string EffectBrightnessKey = "effect_brightness";

	private readonly Action<string> _warn;

	public bool ShowOriginalEnemyName { get; private set; } = true;

	public bool ActionEffectsEnabled { get; private set; } = true;

	public float EffectBrightness { get; private set; } = 1f;

	public event Action<bool>? ActionEffectsEnabledChanged;

	public event Action<float>? EffectBrightnessChanged;

	public ConfigFileEnemyPresentationSettings(Action<string> warn)
	{
		_warn = warn ?? throw new ArgumentNullException("warn");
		Load();
	}

	public void SetActionEffectsEnabled(bool value)
	{
		if (ActionEffectsEnabled != value)
		{
			ActionEffectsEnabled = value;
			Save();
			this.ActionEffectsEnabledChanged?.Invoke(value);
		}
	}

	public void SetShowOriginalEnemyName(bool value)
	{
		if (ShowOriginalEnemyName != value)
		{
			ShowOriginalEnemyName = value;
			Save();
		}
	}

	public void SetEffectBrightness(float value)
	{
		float num = Mathf.Clamp(value, 0f, 1f);
		if (!Mathf.IsEqualApprox(EffectBrightness, num))
		{
			EffectBrightness = num;
			Save();
			this.EffectBrightnessChanged?.Invoke(EffectBrightness);
		}
	}

	public void Reset()
	{
		bool num = !ActionEffectsEnabled;
		bool flag = !Mathf.IsEqualApprox(EffectBrightness, 1f);
		ShowOriginalEnemyName = true;
		EffectBrightness = 1f;
		ActionEffectsEnabled = true;
		Save();
		if (num)
		{
			this.ActionEffectsEnabledChanged?.Invoke(ActionEffectsEnabled);
		}
		if (flag)
		{
			this.EffectBrightnessChanged?.Invoke(EffectBrightness);
		}
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
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Invalid comparison between Unknown and I8
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Invalid comparison between Unknown and I8
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Invalid comparison between Unknown and I8
		try
		{
			ConfigFile val = new ConfigFile();
			Error val2 = val.Load("user://CznModConfig/CznEnemySkin/presentation.cfg");
			if ((long)val2 == 7)
			{
				Save();
				return;
			}
			if ((int)val2 != 0)
			{
				_warn($"Could not load enemy presentation settings ({val2}); " + "using defaults.");
				return;
			}
			bool flag = true;
			Variant value = val.GetValue("presentation", "show_original_enemy_name", Variant.From<bool>(ref flag));
			if ((long)((Variant)(ref value)).VariantType == 1)
			{
				ShowOriginalEnemyName = ((Variant)(ref value)).AsBool();
			}
			else
			{
				_warn("Ignored non-boolean enemy presentation setting 'show_original_enemy_name'.");
			}
			flag = true;
			Variant value2 = val.GetValue("presentation", "action_effects_enabled", Variant.From<bool>(ref flag));
			if ((long)((Variant)(ref value2)).VariantType == 1)
			{
				ActionEffectsEnabled = ((Variant)(ref value2)).AsBool();
			}
			else
			{
				_warn("Ignored non-boolean enemy presentation setting 'action_effects_enabled'.");
			}
			float num = 1f;
			Variant value3 = val.GetValue("presentation", "effect_brightness", Variant.From<float>(ref num));
			Type variantType = ((Variant)(ref value3)).VariantType;
			if (((long)(variantType - 2) <= 1L) ? true : false)
			{
				EffectBrightness = Mathf.Clamp((float)((Variant)(ref value3)).AsDouble(), 0f, 1f);
			}
			else
			{
				_warn("Ignored non-numeric enemy presentation setting 'effect_brightness'.");
			}
		}
		catch (Exception ex)
		{
			_warn("Failed to load enemy presentation settings; using defaults: " + ex.Message);
		}
	}

	private void Save()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I8
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Error val = DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath("user://CznModConfig/CznEnemySkin"));
			if ((int)val != 0 && (long)val != 32)
			{
				_warn("Could not create enemy presentation settings directory: " + ((object)(Error)(ref val)/*cast due to .constrained prefix*/).ToString());
				return;
			}
			ConfigFile val2 = new ConfigFile();
			val2.SetValue("presentation", "show_original_enemy_name", Variant.op_Implicit(ShowOriginalEnemyName));
			val2.SetValue("presentation", "effect_brightness", Variant.op_Implicit(EffectBrightness));
			val2.SetValue("presentation", "action_effects_enabled", Variant.op_Implicit(ActionEffectsEnabled));
			Error val3 = val2.Save("user://CznModConfig/CznEnemySkin/presentation.cfg");
			if ((int)val3 != 0)
			{
				_warn($"Could not save enemy presentation settings: {val3}");
			}
		}
		catch (Exception ex)
		{
			_warn("Failed to save enemy presentation settings: " + ex.Message);
		}
	}
}
