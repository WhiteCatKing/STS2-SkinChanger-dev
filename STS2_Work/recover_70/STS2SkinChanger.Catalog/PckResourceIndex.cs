using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Catalog;

internal sealed class PckResourceIndex : IDisposable
{
	private readonly Dictionary<string, string> _importedToSource;

	public SkinModDescriptor Mod { get; }

	public PckArchive Archive { get; }

	public Dictionary<string, ResourceAsset> Assets { get; } = new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase);

	private PckResourceIndex(SkinModDescriptor mod, PckArchive archive, Dictionary<string, string> importedToSource)
	{
		Mod = mod;
		Archive = archive;
		_importedToSource = importedToSource;
	}

	public static PckResourceIndex Build(SkinModDescriptor mod, PckArchive archive, Dictionary<string, string> importedToSource, Func<string, bool>? remapFilter)
	{
		PckResourceIndex pckResourceIndex = new PckResourceIndex(mod, archive, importedToSource);
		foreach (string item in archive.Paths.Where(IsRemapPath))
		{
			if (remapFilter == null || remapFilter(item))
			{
				pckResourceIndex.AddRemap(item);
			}
		}
		foreach (string item2 in archive.Paths.Where(IsDirectAnimationResource))
		{
			string sourcePath = SkinCatalog.NormalizeTakeoverPath(item2);
			pckResourceIndex.GetAsset(sourcePath).AddFile(archive, item2);
		}
		foreach (string item3 in archive.Paths.Where(IsImportedPath))
		{
			if (importedToSource.TryGetValue(item3, out string value))
			{
				pckResourceIndex.GetAsset(value).AddFile(archive, item3);
			}
		}
		foreach (string item4 in archive.Paths.Where(IsDirectCharacterImageResource))
		{
			string text = SkinCatalog.NormalizeTakeoverPath(item4);
			if (SkinCatalog.IsCharacterImageResourceForProvider(mod.ResourceNamespaceId, text))
			{
				pckResourceIndex.GetAsset(text).AddFile(archive, item4);
			}
		}
		return pckResourceIndex;
	}

	public ResourceAsset? TryBuildAsset(string sourcePath)
	{
		if (Assets.TryGetValue(sourcePath, out ResourceAsset value))
		{
			return value;
		}
		string text = SkinCatalog.NormalizeTakeoverPath(sourcePath);
		if (!text.Equals(sourcePath, StringComparison.OrdinalIgnoreCase) && Assets.TryGetValue(text, out value))
		{
			return value;
		}
		string[] array = new string[2] { ".import", ".remap" };
		foreach (string text2 in array)
		{
			string text3 = sourcePath + text2;
			if (Archive.Contains(text3))
			{
				AddRemap(text3);
				return Assets.GetValueOrDefault(text);
			}
		}
		if (Archive.Contains(sourcePath))
		{
			ResourceAsset asset = GetAsset(text);
			asset.AddFile(Archive, sourcePath);
			return asset;
		}
		return null;
	}

	public void Dispose()
	{
		Archive.Dispose();
	}

	private void AddRemap(string remapPath)
	{
		string text2;
		if (!remapPath.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
		{
			string text = remapPath;
			text2 = text.Substring(0, text.Length - 6);
		}
		else
		{
			string text = remapPath;
			text2 = text.Substring(0, text.Length - 7);
		}
		string path = text2;
		path = SkinCatalog.NormalizeTakeoverPath(path);
		ResourceAsset asset = GetAsset(path);
		asset.AddFile(Archive, remapPath);
		string @string = Encoding.UTF8.GetString(Archive.ReadFile(remapPath));
		foreach (Match item in RemapTargetRegex().Matches(@string))
		{
			string value = item.Groups[1].Value;
			_importedToSource[value] = path;
			if (Archive.Contains(value))
			{
				asset.AddFile(Archive, value);
			}
		}
		Match match = DestFilesArrayRegex().Match(@string);
		if (!match.Success)
		{
			return;
		}
		foreach (Match item2 in QuotedPathRegex().Matches(match.Value))
		{
			string value2 = item2.Groups[1].Value;
			_importedToSource[value2] = path;
			if (Archive.Contains(value2))
			{
				asset.AddFile(Archive, value2);
			}
		}
	}

	private ResourceAsset GetAsset(string sourcePath)
	{
		if (!Assets.TryGetValue(sourcePath, out ResourceAsset value))
		{
			value = new ResourceAsset(sourcePath);
			Assets.Add(sourcePath, value);
		}
		return value;
	}

	private static bool IsRemapPath(string path)
	{
		if (!path.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
		{
			return path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool IsImportedPath(string path)
	{
		return path.StartsWith("res://.godot/imported/", StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsDirectAnimationResource(string path)
	{
		if (SkinCatalog.NormalizeTakeoverPath(path).StartsWith("res://animations/", StringComparison.OrdinalIgnoreCase) || SkinCatalog.NormalizeTakeoverPath(path).StartsWith("res://backgrounds/", StringComparison.OrdinalIgnoreCase) || SkinCatalog.NormalizeTakeoverPath(path).StartsWith("res://scenes/", StringComparison.OrdinalIgnoreCase))
		{
			if (!path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase))
			{
				return path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		return false;
	}

	private static bool IsDirectCharacterImageResource(string path)
	{
		string extension = Path.GetExtension(path);
		if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	[GeneratedRegex("path(?:\\.[a-z0-9_]+)?\\s*=\\s*\"(res://[^\"]+)\"", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex RemapTargetRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RemapTargetRegex_45.Instance;
	}

	[GeneratedRegex("dest_files\\s*=\\s*\\[[^\\]]*\\]", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex DestFilesArrayRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__DestFilesArrayRegex_46.Instance;
	}

	[GeneratedRegex("\"(res://[^\"]+)\"", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex QuotedPathRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__ResourcePathRegex_41.Instance;
	}
}
