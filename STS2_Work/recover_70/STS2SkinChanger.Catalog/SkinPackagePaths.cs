using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace STS2SkinChanger.Catalog;

internal static class SkinPackagePaths
{
	public static string Resolve(string root, string id, string extension)
	{
		bool flag = !SafeName(id);
		if (!flag)
		{
			bool flag2 = ((extension == ".dll" || extension == ".pck") ? true : false);
			flag = !flag2;
		}
		if (flag)
		{
			throw new ArgumentException("无效的皮肤资源文件名。");
		}
		string result = Path.Combine(root, id + extension);
		if (File.Exists(Path.Combine(root, id + ".dll")) || File.Exists(Path.Combine(root, id + ".pck")))
		{
			return result;
		}
		try
		{
			IReadOnlyList<string> readOnlyList = FindManifests(root, id);
			if (readOnlyList.Count != 1)
			{
				return result;
			}
			string path = readOnlyList[0];
			using JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(path).TrimStart('\ufeff'));
			JsonElement rootElement = jsonDocument.RootElement;
			if (!rootElement.TryGetProperty("affects_gameplay", out var value) || value.ValueKind != JsonValueKind.False)
			{
				return result;
			}
			JsonElement value2;
			bool flag3 = rootElement.TryGetProperty("has_dll", out value2) && value2.ValueKind == JsonValueKind.True;
			JsonElement value3;
			bool flag4 = rootElement.TryGetProperty("has_pck", out value3) && value3.ValueKind == JsonValueKind.True;
			if ((!flag3 && !flag4) || (extension == ".dll" && !flag3) || (extension == ".pck" && !flag4))
			{
				return result;
			}
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			if (!SafeName(fileNameWithoutExtension) || (flag3 && !File.Exists(Path.Combine(root, fileNameWithoutExtension + ".dll"))) || (flag4 && !File.Exists(Path.Combine(root, fileNameWithoutExtension + ".pck"))))
			{
				return result;
			}
			return Path.Combine(root, fileNameWithoutExtension + extension);
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) ? 1 : 0) != 0)
		{
			return result;
		}
	}

	internal static IReadOnlyList<string> FindManifests(string root, string id)
	{
		if (!Directory.Exists(root))
		{
			return Array.Empty<string>();
		}
		List<string> list = new List<string>();
		foreach (string item in from path in Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly)
			where Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase)
			select path)
		{
			if (new FileInfo(item).Length > 1048576)
			{
				continue;
			}
			try
			{
				using JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(item).TrimStart('\ufeff'));
				if (jsonDocument.RootElement.ValueKind == JsonValueKind.Object && jsonDocument.RootElement.TryGetProperty("id", out var value) && value.ValueKind == JsonValueKind.String && string.Equals(value.GetString(), id, StringComparison.OrdinalIgnoreCase))
				{
					list.Add(item);
				}
			}
			catch (JsonException)
			{
			}
		}
		return list;
	}

	private static bool SafeName(string value)
	{
		bool flag = !string.IsNullOrWhiteSpace(value);
		if (flag)
		{
			bool flag2 = ((value == "." || value == "..") ? true : false);
			flag = !flag2;
		}
		if (flag && value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0)
		{
			return value.IndexOfAny(new char[3] { '/', '\\', ':' }) < 0;
		}
		return false;
	}
}
