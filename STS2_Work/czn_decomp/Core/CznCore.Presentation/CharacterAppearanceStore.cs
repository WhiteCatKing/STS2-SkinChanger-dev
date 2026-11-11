using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CznCore.Presentation;

internal sealed class CharacterAppearanceStore(string path)
{
	internal const int RetainedRuns = 20;

	private string RunsPath => path + ".runs";

	internal Dictionary<string, CharacterAppearanceChoice> LoadPreferences()
	{
		return Read(path);
	}

	internal Dictionary<string, CharacterAppearanceChoice> LoadRuns()
	{
		return Read(RunsPath);
	}

	internal void SavePreferences(IReadOnlyDictionary<string, CharacterAppearanceChoice> values)
	{
		Write(path, values);
	}

	internal void SaveRuns(IReadOnlyDictionary<string, CharacterAppearanceChoice> values)
	{
		Write(RunsPath, values);
	}

	internal static void Prune(Dictionary<string, CharacterAppearanceChoice> runs, List<string> order, string? keep)
	{
		while (order.Count > 20)
		{
			string run = order[0];
			order.RemoveAt(0);
			if (run == keep)
			{
				order.Add(run);
				continue;
			}
			string[] array = runs.Keys.Where((string k) => RunOf(k) == run).ToArray();
			foreach (string key in array)
			{
				runs.Remove(key);
			}
		}
	}

	internal static void MigrateLegacy(string legacy, string path)
	{
		try
		{
			if (!File.Exists(path) && File.Exists(legacy))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.Move(legacy, path);
				if (File.Exists(legacy + ".runs"))
				{
					File.Move(legacy + ".runs", path + ".runs");
				}
			}
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Appearance data could not be migrated: " + ex.Message);
		}
	}

	internal static string RunOf(string key)
	{
		return key.Substring(0, Math.Max(0, key.IndexOf('/')));
	}

	internal static string RunKey(string identity, ulong netId, string slot)
	{
		return identity + "/" + netId.ToString(CultureInfo.InvariantCulture) + "/" + slot;
	}

	private static Dictionary<string, CharacterAppearanceChoice> Read(string file)
	{
		try
		{
			if (File.Exists(file))
			{
				return JsonSerializer.Deserialize<Dictionary<string, CharacterAppearanceChoice>>(File.ReadAllText(file)) ?? new Dictionary<string, CharacterAppearanceChoice>(StringComparer.Ordinal);
			}
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Appearance data could not be read: " + file + " " + ex.Message);
		}
		return new Dictionary<string, CharacterAppearanceChoice>(StringComparer.Ordinal);
	}

	private static void Write(string file, IReadOnlyDictionary<string, CharacterAppearanceChoice> values)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(file));
			File.WriteAllText(file + ".tmp", JsonSerializer.Serialize(values));
			File.Move(file + ".tmp", file, overwrite: true);
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Appearance data could not be saved: " + file + " " + ex.Message);
		}
	}
}
