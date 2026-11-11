using System;
using Godot;

namespace JoanofArcIroncladHpwear;

public static class WearSettingsRuntime
{
	private static readonly WearSettings Defaults = new WearSettings();

	private static bool pressed;

	public static Func<WearSettings>? Provider { get; set; }

	public static Action<bool>? PersistEnabled { get; set; }

	public static Action<int>? PersistAppearance { get; set; }

	public static WearSettings Current
	{
		get
		{
			try
			{
				return Provider?.Invoke() ?? Defaults;
			}
			catch
			{
				return Defaults;
			}
		}
	}

	public static string PreviewSummary(WearSettings settings)
	{
		int value = Math.Clamp(settings.PercentThreshold, 0, 100);
		int value2 = Math.Clamp(settings.AbsoluteThreshold, 0, 999);
		string value3 = (settings.UseAbsoluteHp ? $"{value2} HP" : $"{value}%");
		return $"换装{(settings.WearEnabled ? "开启" : "关闭")}；分界 {value3}；高于分界第1档，等于或低于第2档。";
	}

	public static void LogPreview()
	{
		ModEntry.TryInfo("[wear-stage] " + PreviewSummary(Current));
	}

	internal static void TickHotkey()
	{
		UpdateHotkey(BindingMatches(Current.SafetyHotkey, (Key key) => Input.IsKeyPressed(key) || Input.IsPhysicalKeyPressed(key)));
	}

	internal static bool BindingMatches(string? binding, Func<Key, bool> isDown)
	{
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(binding))
		{
			return false;
		}
		bool flag = false;
		string[] array = binding.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		foreach (string text in array)
		{
			Key result;
			switch (text.ToLowerInvariant())
			{
			case "ctrl":
			case "control":
				result = (Key)4194326;
				break;
			case "alt":
				result = (Key)4194328;
				break;
			case "shift":
				result = (Key)4194325;
				break;
			case "meta":
			case "command":
			case "cmd":
			case "win":
			case "super":
				result = (Key)4194327;
				break;
			default:
				if (flag || !Enum.TryParse<Key>(text, ignoreCase: true, out result) || (int)result == 0)
				{
					return false;
				}
				flag = true;
				break;
			}
			if (!isDown(result))
			{
				return false;
			}
		}
		return flag;
	}

	internal static void UpdateHotkey(bool down)
	{
		WearSettings current = Current;
		if (down && !pressed)
		{
			bool flag = !current.WearEnabled;
			if (PersistEnabled != null)
			{
				PersistEnabled(flag);
			}
			else
			{
				current.WearEnabled = flag;
			}
		}
		pressed = down;
	}
}
