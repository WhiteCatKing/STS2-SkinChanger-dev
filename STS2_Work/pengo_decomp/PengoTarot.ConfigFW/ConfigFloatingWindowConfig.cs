using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;

namespace PengoTarot.ConfigFW;

public static class ConfigFloatingWindowConfig
{
	private class ConfigData
	{
		public int Version { get; set; } = 1;

		public bool TarotEnabled { get; set; } = true;

		public bool PlanetEnabled { get; set; } = true;

		public int TarotBasePriceMin { get; set; } = 175;

		public int TarotBasePriceMax { get; set; } = 200;

		public List<bool> DifficultyFlags { get; set; } = NewFlags();

		public bool ShowInSettingsOnly { get; set; }

		public bool ConfigIntroDismissed { get; set; }

		public bool HextechWarningDismissed { get; set; }

		public int HextechWarningRevision { get; set; }
	}

	private const int CurrentVersion = 1;

	private const int CurrentHextechWarningRevision = 2;

	private const int CurrentHextechResolutionRevision = 3;

	public const int DifficultyFlagCount = 22;

	public const int DefaultEnabledFlagCount = 6;

	public static readonly int[] ColumnSizes;

	private static readonly string FolderPath;

	private static readonly string FilePath;

	private static readonly JsonSerializerOptions Options;

	private static ConfigData _data;

	public static bool TarotEnabled => _data.TarotEnabled;

	public static bool PlanetEnabled => _data.PlanetEnabled;

	public static int TarotBasePriceMin => _data.TarotBasePriceMin;

	public static int TarotBasePriceMax => _data.TarotBasePriceMax;

	public static bool ShowInSettingsOnly => _data.ShowInSettingsOnly;

	public static bool ConfigIntroDismissed => _data.ConfigIntroDismissed;

	public static bool HextechWarningDismissed => _data.HextechWarningRevision >= 2;

	public static bool HasAcknowledgedHextechWarning => _data.HextechWarningDismissed;

	public static bool HextechResolutionDismissed => _data.HextechWarningRevision >= 3;

	static ConfigFloatingWindowConfig()
	{
		ColumnSizes = new int[3] { 7, 8, 7 };
		FolderPath = Path.Combine(OS.GetUserDataDir(), "mod_configs", "PengoTarot");
		FilePath = Path.Combine(FolderPath, "ConfigFloatingWindow.json");
		Options = new JsonSerializerOptions
		{
			WriteIndented = true
		};
		_data = new ConfigData();
		Load();
	}

	private static List<bool> NewFlags()
	{
		List<bool> list = new List<bool>(22);
		for (int i = 0; i < 22; i++)
		{
			list.Add(i < 6);
		}
		return list;
	}

	public static bool GetDifficultyFlag(int index)
	{
		if (index < 0 || index >= _data.DifficultyFlags.Count)
		{
			return false;
		}
		return _data.DifficultyFlags[index];
	}

	public static void SetTarotEnabled(bool value)
	{
		if (_data.TarotEnabled != value)
		{
			_data.TarotEnabled = value;
			Save();
		}
	}

	public static void SetPlanetEnabled(bool value)
	{
		if (_data.PlanetEnabled != value)
		{
			_data.PlanetEnabled = value;
			Save();
		}
	}

	public static void SetDifficultyFlag(int index, bool value)
	{
		if (index >= 0 && index < _data.DifficultyFlags.Count && _data.DifficultyFlags[index] != value)
		{
			_data.DifficultyFlags[index] = value;
			Save();
		}
	}

	public static void SetShowInSettingsOnly(bool value)
	{
		if (_data.ShowInSettingsOnly != value)
		{
			_data.ShowInSettingsOnly = value;
			Save();
		}
	}

	public static void SetConfigIntroDismissed(bool value)
	{
		if (_data.ConfigIntroDismissed != value)
		{
			_data.ConfigIntroDismissed = value;
			Save();
		}
	}

	public static void SetHextechWarningDismissed(bool value)
	{
		int num = (value ? 2 : 0);
		if (_data.HextechWarningRevision != num)
		{
			_data.HextechWarningDismissed = value;
			_data.HextechWarningRevision = num;
			Save();
		}
	}

	public static void SetHextechResolutionDismissed()
	{
		if (_data.HextechWarningRevision < 3)
		{
			_data.HextechWarningDismissed = true;
			_data.HextechWarningRevision = 3;
			Save();
		}
	}

	private static void Load()
	{
		try
		{
			if (!File.Exists(FilePath))
			{
				return;
			}
			using StreamReader streamReader = File.OpenText(FilePath);
			string text = streamReader.ReadToEnd();
			if (string.IsNullOrWhiteSpace(text) || !(JsonNode.Parse(text, null) is JsonObject jsonObject))
			{
				return;
			}
			ConfigData data = new ConfigData();
			TryRead(jsonObject, "Version", delegate(int v)
			{
				data.Version = v;
			});
			TryRead(jsonObject, "TarotEnabled", delegate(bool v)
			{
				data.TarotEnabled = v;
			});
			TryRead(jsonObject, "PlanetEnabled", delegate(bool v)
			{
				data.PlanetEnabled = v;
			});
			TryRead(jsonObject, "TarotBasePriceMin", delegate(int v)
			{
				data.TarotBasePriceMin = v;
			});
			TryRead(jsonObject, "TarotBasePriceMax", delegate(int v)
			{
				data.TarotBasePriceMax = v;
			});
			TryRead(jsonObject, "ShowInSettingsOnly", delegate(bool v)
			{
				data.ShowInSettingsOnly = v;
			});
			TryRead(jsonObject, "ConfigIntroDismissed", delegate(bool v)
			{
				data.ConfigIntroDismissed = v;
			});
			TryRead(jsonObject, "HextechWarningDismissed", delegate(bool v)
			{
				data.HextechWarningDismissed = v;
			});
			TryRead(jsonObject, "HextechWarningRevision", delegate(int v)
			{
				data.HextechWarningRevision = v;
			});
			if (jsonObject["DifficultyFlags"] is JsonArray jsonArray)
			{
				data.DifficultyFlags = new List<bool>(22);
				for (int i = 0; i < 22; i++)
				{
					if (i < jsonArray.Count && jsonArray[i] is JsonValue jsonValue)
					{
						try
						{
							data.DifficultyFlags.Add(jsonValue.GetValue<bool>());
						}
						catch
						{
							data.DifficultyFlags.Add(i < 6);
						}
					}
					else
					{
						data.DifficultyFlags.Add(i < 6);
					}
				}
			}
			else
			{
				data.DifficultyFlags = NewFlags();
			}
			while (data.DifficultyFlags.Count < 22)
			{
				data.DifficultyFlags.Add(item: false);
			}
			if (data.DifficultyFlags.Count > 22)
			{
				data.DifficultyFlags.RemoveRange(22, data.DifficultyFlags.Count - 22);
			}
			_data = data;
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ConfigFloatingWindow] 读取配置失败：" + ex.Message);
		}
	}

	private static void TryRead<T>(JsonObject obj, string key, Action<T> setter)
	{
		try
		{
			if (obj[key] is JsonValue jsonValue)
			{
				setter(jsonValue.GetValue<T>());
			}
		}
		catch
		{
		}
	}

	private static void Save()
	{
		try
		{
			Directory.CreateDirectory(FolderPath);
			File.WriteAllText(FilePath, JsonSerializer.Serialize(_data, Options));
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ConfigFloatingWindow] 写入配置失败：" + ex.Message);
		}
	}
}
