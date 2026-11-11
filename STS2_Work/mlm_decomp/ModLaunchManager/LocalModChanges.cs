using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ModLaunchManager;

internal static class LocalModChanges
{
	internal static readonly StringComparer PathComparer = (OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

	internal static string FolderKey(string path)
	{
		string text;
		try
		{
			text = Path.GetFullPath(path);
		}
		catch
		{
			text = path.Trim();
		}
		return text.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '/');
	}

	internal static HashSet<string> FoldersOf(IEnumerable<string> files)
	{
		HashSet<string> hashSet = new HashSet<string>(PathComparer);
		foreach (string file in files)
		{
			string directoryName = Path.GetDirectoryName(file);
			if (directoryName != null && directoryName.Length > 0)
			{
				hashSet.Add(FolderKey(directoryName));
			}
		}
		return hashSet;
	}

	internal static bool InFolders(string path, IReadOnlySet<string> folders)
	{
		if (!string.IsNullOrWhiteSpace(path))
		{
			return folders.Contains(FolderKey(path));
		}
		return false;
	}

	internal static bool ManifestRemains(string folder, string id)
	{
		try
		{
			if (!Directory.Exists(folder))
			{
				return false;
			}
			foreach (string item in Directory.EnumerateFiles(folder, "*.json", SearchOption.TopDirectoryOnly))
			{
				string json;
				try
				{
					json = File.ReadAllText(item, Encoding.UTF8);
				}
				catch (FileNotFoundException)
				{
					continue;
				}
				catch (DirectoryNotFoundException)
				{
					return false;
				}
				catch (Exception ex3) when (((ex3 is IOException || ex3 is UnauthorizedAccessException) ? 1 : 0) != 0)
				{
					return true;
				}
				if (DeclaredId(json) == id)
				{
					return true;
				}
			}
			return false;
		}
		catch (DirectoryNotFoundException)
		{
			return false;
		}
		catch (Exception ex5) when (((ex5 is IOException || ex5 is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			return true;
		}
	}

	internal static LocalModPairing<TGone, TArrival> Pair<TGone, TArrival>(IReadOnlyList<TGone> gone, Func<TGone, string?> goneId, IReadOnlyList<TArrival> arrivals, Func<TArrival, string?> arrivalId)
	{
		Dictionary<string, int> dictionary = CountIds(gone, goneId);
		Dictionary<string, int> dictionary2 = CountIds(arrivals, arrivalId);
		Dictionary<string, TGone> dictionary3 = new Dictionary<string, TGone>(StringComparer.Ordinal);
		List<TGone> list = new List<TGone>();
		foreach (TGone item in gone)
		{
			string text = goneId(item);
			if (text != null && text.Length > 0 && dictionary[text] == 1 && dictionary2.GetValueOrDefault(text) == 1)
			{
				dictionary3[text] = item;
			}
			else
			{
				list.Add(item);
			}
		}
		List<(TGone, TArrival)> list2 = new List<(TGone, TArrival)>();
		List<TArrival> list3 = new List<TArrival>();
		foreach (TArrival arrival in arrivals)
		{
			string text2 = arrivalId(arrival);
			if (text2 != null && text2.Length > 0 && dictionary3.TryGetValue(text2, out var value))
			{
				list2.Add((value, arrival));
			}
			else
			{
				list3.Add(arrival);
			}
		}
		return new LocalModPairing<TGone, TArrival>(list2, list, list3);
	}

	private static Dictionary<string, int> CountIds<T>(IEnumerable<T> items, Func<T, string?> idOf)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (T item in items)
		{
			string text = idOf(item);
			if (text != null && text.Length > 0)
			{
				dictionary[text] = dictionary.GetValueOrDefault(text) + 1;
			}
		}
		return dictionary;
	}

	private static string? DeclaredId(string json)
	{
		try
		{
			return (JsonNode.Parse(json, null) is JsonObject jsonObject && jsonObject["id"] is JsonValue jsonValue && jsonValue.GetValueKind() == JsonValueKind.String) ? jsonValue.GetValue<string>() : null;
		}
		catch
		{
			return null;
		}
	}
}
