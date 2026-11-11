using System;
using System.IO;
using System.Text.Json;
using Godot;

namespace PengoTarot.Patch.VisualVanilla;

public static class VanillaStyleConfig
{
	private class Data
	{
		public bool TarotVanilla { get; set; }

		public bool PlanetVanilla { get; set; }
	}

	private static readonly string FolderPath = Path.Combine(OS.GetUserDataDir(), "mod_configs", "PengoTarot");

	private static readonly string FilePath = Path.Combine(FolderPath, "VanillaStyleSettings.json");

	private static Data _data = new Data();

	private static bool _loaded;

	public static bool TarotVanilla
	{
		get
		{
			Load();
			return _data.TarotVanilla;
		}
		set
		{
			Load();
			_data.TarotVanilla = value;
			Save();
		}
	}

	public static bool PlanetVanilla
	{
		get
		{
			Load();
			return _data.PlanetVanilla;
		}
		set
		{
			Load();
			_data.PlanetVanilla = value;
			Save();
		}
	}

	private static void Load()
	{
		if (_loaded)
		{
			return;
		}
		_loaded = true;
		try
		{
			if (File.Exists(FilePath))
			{
				Data data = JsonSerializer.Deserialize<Data>(File.ReadAllText(FilePath));
				if (data != null)
				{
					_data = data;
				}
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("[PengoTarot] VanillaStyle load: " + ex.Message);
		}
	}

	private static void Save()
	{
		try
		{
			if (!Directory.Exists(FolderPath))
			{
				Directory.CreateDirectory(FolderPath);
			}
			File.WriteAllText(FilePath, JsonSerializer.Serialize(_data));
		}
		catch (Exception ex)
		{
			GD.PrintErr("[PengoTarot] VanillaStyle save: " + ex.Message);
		}
	}
}
