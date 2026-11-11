using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MegaCrit.Sts2.Core.Debug;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class WorkshopPackagePolicy
{
	public static WorkshopPackageAssessment Assess(string directory, string? gameVersion, IReadOnlyDictionary<string, string> loadedDependencies)
	{
		if (!Directory.Exists(directory))
		{
			return Reject(WorkshopLoadReason.InvalidPackage);
		}
		List<SkinModDescriptor> list = new List<SkinModDescriptor>();
		List<(string, string)> list2 = new List<(string, string)>();
		try
		{
			string[] array = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Take(20001).ToArray();
			if (array.Length > 20000)
			{
				return Reject(WorkshopLoadReason.InvalidPackage);
			}
			foreach (string item2 in array.Where((string path) => Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase)))
			{
				if (new FileInfo(item2).Length > 1048576)
				{
					continue;
				}
				JsonDocument jsonDocument;
				try
				{
					jsonDocument = JsonDocument.Parse(File.ReadAllText(item2).TrimStart('\ufeff'));
				}
				catch (JsonException)
				{
					if (File.Exists(Path.ChangeExtension(item2, ".pck")) || File.Exists(Path.ChangeExtension(item2, ".dll")))
					{
						return Reject(WorkshopLoadReason.InvalidPackage);
					}
					continue;
				}
				using (jsonDocument)
				{
					JsonElement rootElement = jsonDocument.RootElement;
					if (rootElement.ValueKind != JsonValueKind.Object || !rootElement.TryGetProperty("id", out var value) || !rootElement.TryGetProperty("has_pck", out var value2))
					{
						continue;
					}
					string @string = value.GetString();
					JsonElement value3;
					string text = (rootElement.TryGetProperty("pck_name", out value3) ? value3.GetString() : @string);
					if (!SafeName(@string) || !SafeName(text))
					{
						return Reject(WorkshopLoadReason.InvalidPackage);
					}
					if (!rootElement.TryGetProperty("affects_gameplay", out var value4) || value4.GetBoolean())
					{
						return Reject(WorkshopLoadReason.Gameplay);
					}
					if (!value2.GetBoolean())
					{
						return Reject(WorkshopLoadReason.IncompleteResources);
					}
					string directoryName = Path.GetDirectoryName(item2);
					string text2 = Path.Combine(directoryName, text + ".pck");
					if (!File.Exists(text2))
					{
						text2 = SkinPackagePaths.Resolve(directoryName, @string, ".pck");
					}
					if (!File.Exists(text2))
					{
						return Reject(WorkshopLoadReason.InvalidPackage);
					}
					WorkshopLoadReason workshopLoadReason = CheckVersion(rootElement, gameVersion);
					if (workshopLoadReason != 0)
					{
						return Reject(workshopLoadReason);
					}
					if (rootElement.TryGetProperty("dependencies", out var value5) && value5.ValueKind != JsonValueKind.Null)
					{
						if (value5.ValueKind != JsonValueKind.Array)
						{
							return Reject(WorkshopLoadReason.InvalidPackage);
						}
						foreach (JsonElement item3 in value5.EnumerateArray())
						{
							string text3 = ((item3.ValueKind == JsonValueKind.String) ? item3.GetString() : item3.GetProperty("id").GetString());
							if (string.IsNullOrWhiteSpace(text3))
							{
								return Reject(WorkshopLoadReason.InvalidPackage);
							}
							JsonElement value6;
							string item = ((item3.ValueKind == JsonValueKind.Object && item3.TryGetProperty("min_version", out value6)) ? value6.GetString() : null);
							list2.Add((text3, item));
						}
					}
					if (rootElement.TryGetProperty("has_dll", out var value7) && value7.GetBoolean() && !File.Exists(SkinPackagePaths.Resolve(directoryName, @string, ".dll")))
					{
						return Reject(WorkshopLoadReason.InvalidPackage);
					}
					list.Add(new SkinModDescriptor(@string, rootElement.TryGetProperty("name", out var value8) ? (value8.GetString() ?? @string) : @string, text2, AffectsGameplay: false, directoryName));
				}
			}
			if (list.Count == 0 || list.Select((SkinModDescriptor m) => m.Id).Distinct<string>(StringComparer.OrdinalIgnoreCase).Count() != list.Count)
			{
				return Reject(WorkshopLoadReason.InvalidPackage);
			}
			foreach (var dep in list2)
			{
				KeyValuePair<string, string> keyValuePair = loadedDependencies.FirstOrDefault<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => pair.Key.Equals(dep.Item1, StringComparison.OrdinalIgnoreCase));
				if (keyValuePair.Key == null || !MeetsMinimum(keyValuePair.Value, dep.Item2))
				{
					return Reject(WorkshopLoadReason.Dependency);
				}
			}
			if (RequiresCodeAtStartup(directory))
			{
				return Reject(WorkshopLoadReason.StartupCode);
			}
			return new WorkshopPackageAssessment(list.ToArray(), WorkshopLoadReason.None);
		}
		catch (Exception ex2) when (((ex2 is IOException || ex2 is UnauthorizedAccessException || ex2 is JsonException || ex2 is InvalidOperationException || ex2 is ArgumentException || ex2 is KeyNotFoundException) ? 1 : 0) != 0)
		{
			return Reject(WorkshopLoadReason.InvalidPackage);
		}
		static WorkshopPackageAssessment Reject(WorkshopLoadReason reason)
		{
			return new WorkshopPackageAssessment(Array.Empty<SkinModDescriptor>(), reason);
		}
	}

	public static bool RequiresCodeAtStartup(string directory)
	{
		return Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Any(delegate(string path)
		{
			switch (Path.GetExtension(path).ToLowerInvariant())
			{
			case ".dll":
				return !WorkshopCodeInspection.CanOmitExecution(path);
			case ".so":
			case ".dylib":
			case ".gd":
			case ".gdc":
			case ".gdextension":
				return true;
			default:
				return false;
			}
		});
	}

	internal static Dictionary<string, (long Length, DateTime Modified)> Snapshot(string directory)
	{
		string[] array = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Take(20001).ToArray();
		if (array.Length > 20000)
		{
			throw new InvalidDataException("Workshop package contains too many files.");
		}
		return array.ToDictionary<string, string, (long, DateTime)>((string path) => path, delegate(string path)
		{
			FileInfo fileInfo = new FileInfo(path);
			return (Length: fileInfo.Length, LastWriteTimeUtc: fileInfo.LastWriteTimeUtc);
		}, StringComparer.OrdinalIgnoreCase);
	}

	internal static bool Unchanged(string directory, Dictionary<string, (long Length, DateTime Modified)> before)
	{
		Dictionary<string, (long Length, DateTime Modified)> after = Snapshot(directory);
		if (before.Count == after.Count)
		{
			return before.All<KeyValuePair<string, (long, DateTime)>>(delegate(KeyValuePair<string, (long Length, DateTime Modified)> pair)
			{
				if (after.TryGetValue(pair.Key, out (long, DateTime) value))
				{
					(long, DateTime) tuple = value;
					(long, DateTime) value2 = pair.Value;
					if (tuple.Item1 == value2.Item1)
					{
						return tuple.Item2 == value2.Item2;
					}
					return false;
				}
				return false;
			});
		}
		return false;
	}

	private static bool SafeName(string? name)
	{
		if (!string.IsNullOrWhiteSpace(name) && !(name == ".") && !(name == ".."))
		{
			return name.IndexOfAny(new char[3] { '/', '\\', ':' }) < 0;
		}
		return false;
	}

	private static bool MeetsMinimum(string actual, string? minimum)
	{
		if (!string.IsNullOrWhiteSpace(minimum))
		{
			SemanticVersion val = default(SemanticVersion);
			SemanticVersion val2 = default(SemanticVersion);
			if (SemanticVersion.TryFromString(actual, ref val) && SemanticVersion.TryFromString(minimum, ref val2))
			{
				return val.CompareTo(val2) >= 0;
			}
			return false;
		}
		return true;
	}

	private static WorkshopLoadReason CheckVersion(JsonElement j, string? gameVersion)
	{
		SemanticVersion val = default(SemanticVersion);
		SemanticVersion val2 = default(SemanticVersion);
		foreach (JsonProperty item in j.EnumerateObject())
		{
			bool flag = !item.Name.Contains("version", StringComparison.OrdinalIgnoreCase);
			string name;
			if (!flag)
			{
				name = item.Name;
				bool flag2 = ((name == "version" || name == "internal_test_version") ? true : false);
				flag = flag2;
			}
			if (flag)
			{
				continue;
			}
			name = item.Name;
			if (!(name == "min_game_version") && !(name == "max_game_version"))
			{
				return WorkshopLoadReason.VersionRule;
			}
			if (item.Value.ValueKind != JsonValueKind.Null)
			{
				if (gameVersion == null || !SemanticVersion.TryFromString(gameVersion, ref val) || !SemanticVersion.TryFromString(item.Value.GetString(), ref val2))
				{
					return WorkshopLoadReason.VersionRule;
				}
				int num = val.CompareTo(val2);
				if ((item.Name == "min_game_version") ? (num < 0) : (num > 0))
				{
					return WorkshopLoadReason.Version;
				}
			}
		}
		return WorkshopLoadReason.None;
	}
}
