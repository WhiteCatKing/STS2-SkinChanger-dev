using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ModLaunchManager;

internal sealed class AiTranslationDocument
{
	internal sealed record Metadata(string SourceName, string SourceDescription, string Name, string Description);

	private static readonly Dictionary<string, (long Modified, long Length, Metadata? Value)> MetadataCache = new Dictionary<string, (long, long, Metadata)>(StringComparer.Ordinal);

	public int Version { get; set; } = 1;

	public string SourceName { get; set; } = string.Empty;

	public string SourceDescription { get; set; } = string.Empty;

	public string Name { get; set; } = string.Empty;

	public string Description { get; set; } = string.Empty;

	public Dictionary<string, Dictionary<string, string>> Tables { get; set; } = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

	public Dictionary<string, Dictionary<string, string>> Sources { get; set; } = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

	internal int SkippedEntries { get; set; }

	internal string SkippedSummary { get; set; } = string.Empty;

	internal static bool Enabled(string folder)
	{
		return !File.Exists(Path.Combine(folder, "use-original"));
	}

	internal static void SetEnabled(string folder, bool enabled)
	{
		string text = Path.Combine(folder, "use-original");
		ModTranslationFiles.RejectLinks(folder, text);
		if (enabled)
		{
			File.Delete(text);
		}
		else
		{
			ModTranslationFiles.WriteAtomic(text, Array.Empty<byte>());
		}
	}

	internal static string FilePath(string folder, string language)
	{
		ModTranslationFiles.LanguageFolder(folder, language);
		string text = Path.Combine(folder, "ai", language + ".json");
		ModTranslationFiles.RejectLinks(folder, text);
		return text;
	}

	internal static AiTranslationDocument? Read(string folder, string language)
	{
		string path = FilePath(folder, language);
		if (!File.Exists(path))
		{
			return null;
		}
		AiTranslationDocument aiTranslationDocument = JsonSerializer.Deserialize<AiTranslationDocument>(PckLocalizationArchive.Utf8.GetString(ModTranslationFiles.ReadFile(path)).TrimStart('\ufeff'));
		if (aiTranslationDocument == null || aiTranslationDocument.Version != 1 || aiTranslationDocument.Tables == null || aiTranslationDocument.Sources == null || aiTranslationDocument.Tables.Count > 2048 || aiTranslationDocument.Name == null || aiTranslationDocument.Description == null)
		{
			throw new TranslationFileException("invalidJson");
		}
		foreach (var (text2, dictionary2) in aiTranslationDocument.Tables)
		{
			ModTranslationFiles.ValidateRelativePath(language + "/" + text2);
			if (dictionary2 == null || dictionary2.Any((KeyValuePair<string, string> p) => p.Value == null) || !aiTranslationDocument.Sources.TryGetValue(text2, out Dictionary<string, string> value) || value == null || value.Any((KeyValuePair<string, string> p) => p.Value == null))
			{
				throw new TranslationFileException("invalidJson");
			}
		}
		return aiTranslationDocument;
	}

	internal static Metadata? ReadMetadata(string folder, string language)
	{
		string text = FilePath(folder, language);
		FileInfo fileInfo = new FileInfo(text);
		if (!fileInfo.Exists)
		{
			MetadataCache.Remove(text);
			return null;
		}
		if (MetadataCache.TryGetValue(text, out (long, long, Metadata) value) && value.Item1 == fileInfo.LastWriteTimeUtc.Ticks && value.Item2 == fileInfo.Length)
		{
			return value.Item3;
		}
		AiTranslationDocument aiTranslationDocument = Read(folder, language);
		Metadata metadata = ((aiTranslationDocument == null) ? null : new Metadata(aiTranslationDocument.SourceName, aiTranslationDocument.SourceDescription, aiTranslationDocument.Name, aiTranslationDocument.Description));
		if (MetadataCache.Count > 2048)
		{
			MetadataCache.Clear();
		}
		MetadataCache[text] = (fileInfo.LastWriteTimeUtc.Ticks, fileInfo.Length, metadata);
		return metadata;
	}

	internal void Save(string folder, string language)
	{
		byte[] array = JsonSerializer.SerializeToUtf8Bytes(this);
		if (array.Length > 8388608)
		{
			throw new TranslationFileException("tooLarge");
		}
		string text = FilePath(folder, language);
		ModTranslationFiles.WriteAtomic(text, array);
		MetadataCache.Remove(text);
	}
}
