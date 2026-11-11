using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ModLaunchManager;

internal static class WorkshopBranchFiles
{
	internal static IReadOnlyDictionary<string, string> Validate(string directory, IEnumerable<string> expectedIds)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		Stack<string> stack = new Stack<string>();
		stack.Push(Path.GetFullPath(directory));
		int num = 0;
		string result;
		while (stack.TryPop(out result))
		{
			if ((File.GetAttributes(result) & FileAttributes.ReparsePoint) != 0)
			{
				throw new InvalidDataException("Linked Workshop directories cannot be activated");
			}
			foreach (string item in Directory.EnumerateFileSystemEntries(result))
			{
				if (++num > 20000)
				{
					throw new InvalidDataException("Workshop item contains too many files");
				}
				FileAttributes attributes = File.GetAttributes(item);
				if ((attributes & FileAttributes.ReparsePoint) != 0)
				{
					throw new InvalidDataException("Linked Workshop files cannot be activated");
				}
				if ((attributes & FileAttributes.Directory) != 0)
				{
					stack.Push(item);
				}
				else
				{
					if (!item.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || new FileInfo(item).Length > 1048576)
					{
						continue;
					}
					JsonDocument jsonDocument;
					try
					{
						jsonDocument = JsonDocument.Parse(File.ReadAllText(item, Encoding.UTF8));
					}
					catch (JsonException)
					{
						continue;
					}
					using (jsonDocument)
					{
						JsonElement rootElement = jsonDocument.RootElement;
						if (rootElement.ValueKind != JsonValueKind.Object || !rootElement.TryGetProperty("id", out var value) || value.ValueKind != JsonValueKind.String)
						{
							continue;
						}
						string @string = value.GetString();
						if (@string == null || @string.Length <= 0)
						{
							continue;
						}
						bool flag = @string.IndexOfAny(new char[3] { '/', '\\', ':' }) >= 0;
						if (!flag)
						{
							bool flag2 = ((@string == "." || @string == "..") ? true : false);
							flag = flag2;
						}
						if (flag || !dictionary.TryAdd(@string, item))
						{
							throw new InvalidDataException("Duplicate or unsafe Workshop mod ID");
						}
						(string, string)[] array = new(string, string)[2]
						{
							("has_dll", ".dll"),
							("has_pck", ".pck")
						};
						for (int i = 0; i < array.Length; i++)
						{
							var (propertyName, text) = array[i];
							if (rootElement.TryGetProperty(propertyName, out var value2) && value2.ValueKind == JsonValueKind.True && (!File.Exists(Path.Combine(result, @string + text)) || new FileInfo(Path.Combine(result, @string + text)).Length == 0L))
							{
								throw new InvalidDataException("Workshop payload is missing for " + @string);
							}
						}
					}
				}
			}
		}
		HashSet<string> hashSet = new HashSet<string>(expectedIds, StringComparer.Ordinal);
		if (hashSet.Count == 0 || !hashSet.SetEquals(dictionary.Keys))
		{
			throw new InvalidDataException("The downloaded version contains a different set of mods");
		}
		return dictionary;
	}

	internal static string CacheFolder(string root, ulong item, string folder)
	{
		if (item == 0L || !Guid.TryParseExact(folder, "N", out var _))
		{
			throw new InvalidDataException("Invalid branch cache locator");
		}
		return Path.Combine(Path.GetFullPath(root), item.ToString(CultureInfo.InvariantCulture), folder);
	}
}
