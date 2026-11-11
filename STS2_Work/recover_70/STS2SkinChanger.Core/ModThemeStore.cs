#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace STS2SkinChanger.Core;

internal static class ModThemeStore
{
	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true
	};

	public static ModThemeSettings Load(string path)
	{
		string[] array = new string[2]
		{
			path,
			path + ".bak"
		};
		foreach (string path2 in array)
		{
			if (File.Exists(path2))
			{
				try
				{
					return Decode(File.ReadAllText(path2));
				}
				catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) ? 1 : 0) != 0)
				{
					Trace.TraceWarning("读取界面主题失败，将尝试备份/默认主题：" + ex.Message);
				}
			}
		}
		return new ModThemeSettings();
	}

	private static ModThemeSettings Decode(string json)
	{
		ModThemeSettings modThemeSettings = (JsonSerializer.Deserialize<ModThemeSettings>(json, Options) ?? new ModThemeSettings()).Normalize();
		using JsonDocument jsonDocument = JsonDocument.Parse(json);
		if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object)
		{
			return modThemeSettings;
		}
		HashSet<string> hashSet = (from p in jsonDocument.RootElement.EnumerateObject()
			select p.Name).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		return modThemeSettings with
		{
			SelectionHoverColor = (hashSet.Contains("SelectionHoverColor") ? modThemeSettings.SelectionHoverColor : modThemeSettings.SelectionColor),
			SelectionHoverOpacity = (hashSet.Contains("SelectionHoverOpacity") ? modThemeSettings.SelectionHoverOpacity : modThemeSettings.SelectionOpacity),
			SelectionHoverBlur = (hashSet.Contains("SelectionHoverBlur") ? modThemeSettings.SelectionHoverBlur : modThemeSettings.SelectionBlur),
			DropdownSelectionHoverColor = (hashSet.Contains("DropdownSelectionHoverColor") ? modThemeSettings.DropdownSelectionHoverColor : (hashSet.Contains("DropdownHoverColor") ? modThemeSettings.DropdownHoverColor : modThemeSettings.HoverColor)),
			DropdownSelectionHoverOpacity = (hashSet.Contains("DropdownSelectionHoverOpacity") ? modThemeSettings.DropdownSelectionHoverOpacity : (hashSet.Contains("DropdownHoverOpacity") ? modThemeSettings.DropdownHoverOpacity : modThemeSettings.ButtonOpacity)),
			DropdownColor = (hashSet.Contains("DropdownColor") ? modThemeSettings.DropdownColor : modThemeSettings.PanelColor),
			DropdownOpacity = (hashSet.Contains("DropdownOpacity") ? modThemeSettings.DropdownOpacity : modThemeSettings.PanelOpacity),
			DropdownHoverColor = (hashSet.Contains("DropdownHoverColor") ? modThemeSettings.DropdownHoverColor : modThemeSettings.HoverColor),
			DropdownHoverOpacity = (hashSet.Contains("DropdownHoverOpacity") ? modThemeSettings.DropdownHoverOpacity : modThemeSettings.ButtonOpacity),
			DropdownSelectionColor = (hashSet.Contains("DropdownSelectionColor") ? modThemeSettings.DropdownSelectionColor : modThemeSettings.SelectionColor),
			DropdownSelectionOpacity = (hashSet.Contains("DropdownSelectionOpacity") ? modThemeSettings.DropdownSelectionOpacity : modThemeSettings.SelectionOpacity),
			DropdownBorderColor = (hashSet.Contains("DropdownBorderColor") ? modThemeSettings.DropdownBorderColor : modThemeSettings.BorderColor),
			DropdownBorderWidth = (hashSet.Contains("DropdownBorderWidth") ? modThemeSettings.DropdownBorderWidth : modThemeSettings.BorderWidth),
			DropdownCornerRadius = (hashSet.Contains("DropdownCornerRadius") ? modThemeSettings.DropdownCornerRadius : modThemeSettings.CornerRadius)
		};
	}

	public static void Save(string path, ModThemeSettings settings)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
		string text = path + ".tmp";
		try
		{
			File.WriteAllText(text, JsonSerializer.Serialize(settings.Normalize(), Options));
			File.Move(text, path, overwrite: true);
		}
		finally
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
		}
		try
		{
			File.Copy(path, path + ".bak", overwrite: true);
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			Trace.TraceWarning("主题已保存，但备份更新失败：" + ex.Message);
		}
	}
}
