#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace STS2SkinChanger.Core;

internal sealed class ModThemePresets
{
	private sealed class PresetFile
	{
		public int Version { get; init; }

		public List<ModThemePreset>? Presets { get; init; }
	}

	public const string DefaultId = "default";

	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true
	};

	private readonly string _path;

	private List<ModThemePreset> _users;

	public IReadOnlyList<ModThemePreset> Presets
	{
		get
		{
			ModThemePreset modThemePreset = new ModThemePreset("default", "", new ModThemeSettings());
			List<ModThemePreset> users = _users;
			int num = 0;
			ModThemePreset[] array = new ModThemePreset[1 + users.Count];
			array[num] = modThemePreset;
			num++;
			Span<ModThemePreset> span = CollectionsMarshal.AsSpan(users);
			span.CopyTo(new Span<ModThemePreset>(array).Slice(num, span.Length));
			num += span.Length;
			return new global::_003C_003Ez__ReadOnlyArray<ModThemePreset>(array);
		}
	}

	public ModThemePresets(string path)
	{
		_path = path;
		_users = Load(path);
	}

	public string? FindMatchingId(ModThemeSettings current, string? preferredId)
	{
		ModThemeSettings normalized = current.Normalize();
		ModThemePreset[] source = Presets.Where((ModThemePreset p) => p.Settings == normalized).ToArray();
		object obj = source.FirstOrDefault((ModThemePreset p) => p.Id == preferredId)?.Id;
		if (obj == null)
		{
			ModThemePreset? modThemePreset = source.FirstOrDefault();
			if ((object)modThemePreset == null)
			{
				return null;
			}
			obj = modThemePreset.Id;
		}
		return (string?)obj;
	}

	public string Create(string name, ModThemeSettings settings)
	{
		ModThemePreset modThemePreset = new ModThemePreset(Guid.NewGuid().ToString("N"), ValidName(name), settings.Normalize());
		List<ModThemePreset> users = _users;
		int num = 1 + users.Count;
		List<ModThemePreset> list = new List<ModThemePreset>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<ModThemePreset> span = CollectionsMarshal.AsSpan(list);
		int num2 = 0;
		Span<ModThemePreset> span2 = CollectionsMarshal.AsSpan(users);
		span2.CopyTo(span.Slice(num2, span2.Length));
		num2 += span2.Length;
		span[num2] = modThemePreset;
		Save(list);
		return modThemePreset.Id;
	}

	public ModThemePreset ImportCode(string code, string importedName)
	{
		ModThemeSettings settings = ThemePresetCode.Decode(code);
		string text = importedName.Trim();
		int length = text.Length;
		if ((length > 100 || length == 0) ? true : false)
		{
			throw new ArgumentException("Invalid imported preset name.", "importedName");
		}
		HashSet<string> hashSet = _users.Select((ModThemePreset p) => p.Name).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		string text2 = text;
		int num = 2;
		while (hashSet.Contains(text2))
		{
			string text3 = $" ({num})";
			text2 = text.Substring(0, Math.Min(text.Length, 100 - text3.Length)) + text3;
			num++;
		}
		return new ModThemePreset(Create(text2, settings), text2, settings);
	}

	public string ExportCode(string id)
	{
		return ThemePresetCode.Encode((Presets.FirstOrDefault((ModThemePreset p) => p.Id == id) ?? throw new ArgumentException("Preset not found.", "id")).Settings);
	}

	public bool Rename(string id, string name)
	{
		int num = _users.FindIndex((ModThemePreset p) => p.Id == id);
		if (num < 0)
		{
			return false;
		}
		List<ModThemePreset> list = new List<ModThemePreset>(_users);
		list[num] = list[num]with
		{
			Name = ValidName(name, id)
		};
		Save(list);
		return true;
	}

	public bool Overwrite(string id, ModThemeSettings settings)
	{
		int num = _users.FindIndex((ModThemePreset p) => p.Id == id);
		if (num < 0)
		{
			return false;
		}
		List<ModThemePreset> list = new List<ModThemePreset>(_users);
		list[num] = list[num]with
		{
			Settings = settings.Normalize()
		};
		Save(list);
		return true;
	}

	public bool Delete(string id)
	{
		if (!_users.Any((ModThemePreset p) => p.Id == id))
		{
			return false;
		}
		Save(_users.Where((ModThemePreset p) => p.Id != id).ToList());
		return true;
	}

	private string ValidName(string name, string? excluding = null)
	{
		string trimmed = name.Trim();
		int length = trimmed.Length;
		bool flag = ((length > 100 || length == 0) ? true : false);
		if (flag || _users.Any((ModThemePreset p) => p.Id != excluding && p.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
		{
			throw new ArgumentException("Preset name must be unique and contain 1–100 characters.", "name");
		}
		return trimmed;
	}

	private static List<ModThemePreset> Load(string path)
	{
		string[] array = new string[2]
		{
			path,
			path + ".bak"
		};
		foreach (string path2 in array)
		{
			if (!File.Exists(path2))
			{
				continue;
			}
			try
			{
				PresetFile presetFile = JsonSerializer.Deserialize<PresetFile>(File.ReadAllText(path2), Options);
				if (presetFile == null || presetFile.Version != 1 || presetFile.Presets == null)
				{
					throw new JsonException("Invalid theme preset file.");
				}
				HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal) { "default" };
				HashSet<string> hashSet2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				List<ModThemePreset> list = new List<ModThemePreset>();
				foreach (ModThemePreset preset in presetFile.Presets)
				{
					if (!(preset?.Settings == null) && !string.IsNullOrWhiteSpace(preset.Id) && !string.IsNullOrWhiteSpace(preset.Name))
					{
						string text = preset.Name.Trim();
						if (text.Length <= 100 && !hashSet.Contains(preset.Id) && !hashSet2.Contains(text))
						{
							hashSet.Add(preset.Id);
							hashSet2.Add(text);
							list.Add(preset with
							{
								Name = text,
								Settings = preset.Settings.Normalize()
							});
						}
					}
				}
				return list;
			}
			catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) ? 1 : 0) != 0)
			{
				Trace.TraceWarning("读取主题预设失败，将尝试备份/默认预设：" + ex.Message);
			}
		}
		return new List<ModThemePreset>();
	}

	private void Save(List<ModThemePreset> next)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path)));
		string text = _path + ".tmp";
		try
		{
			File.WriteAllText(text, JsonSerializer.Serialize(new PresetFile
			{
				Version = 1,
				Presets = next
			}, Options));
			File.Move(text, _path, overwrite: true);
		}
		finally
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
		}
		_users = next;
		try
		{
			File.Copy(_path, _path + ".bak", overwrite: true);
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			Trace.TraceWarning("主题预设已保存，但备份更新失败：" + ex.Message);
		}
	}
}
