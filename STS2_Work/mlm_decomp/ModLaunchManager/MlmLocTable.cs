using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class MlmLocTable
{
	private sealed record MlmLocEntry(string Text, IReadOnlySet<string> Arguments);

	private sealed record MlmLocaleBundle(MlmLocaleInfo Info, IReadOnlyList<string> Fallbacks, IReadOnlyDictionary<string, string> Strings);

	private sealed class SourceDocument
	{
		[JsonPropertyName("formatVersion")]
		public int FormatVersion { get; set; }

		[JsonPropertyName("revision")]
		public int Revision { get; set; }

		[JsonPropertyName("sourceLocale")]
		public SourceLocaleDocument? SourceLocale { get; set; }

		[JsonPropertyName("entries")]
		public Dictionary<string, SourceEntryDocument>? Entries { get; set; }
	}

	private sealed class SourceLocaleDocument
	{
		[JsonPropertyName("id")]
		public string? Id { get; set; }

		[JsonPropertyName("displayName")]
		public string? DisplayName { get; set; }

		[JsonPropertyName("englishName")]
		public string? EnglishName { get; set; }

		[JsonPropertyName("gameLanguage")]
		public string? GameLanguage { get; set; }

		[JsonPropertyName("culture")]
		public string? Culture { get; set; }
	}

	private sealed class SourceEntryDocument
	{
		[JsonPropertyName("text")]
		public string? Text { get; set; }

		[JsonPropertyName("context")]
		public string? Context { get; set; }

		[JsonPropertyName("args")]
		public Dictionary<string, ArgumentDocument>? Arguments { get; set; }
	}

	private sealed class ArgumentDocument
	{
		[JsonPropertyName("type")]
		public string? Type { get; set; }

		[JsonPropertyName("description")]
		public string? Description { get; set; }
	}

	private sealed class LocaleDocument
	{
		[JsonPropertyName("formatVersion")]
		public int FormatVersion { get; set; }

		[JsonPropertyName("locale")]
		public string? Locale { get; set; }

		[JsonPropertyName("displayName")]
		public string? DisplayName { get; set; }

		[JsonPropertyName("englishName")]
		public string? EnglishName { get; set; }

		[JsonPropertyName("gameLanguage")]
		public string? GameLanguage { get; set; }

		[JsonPropertyName("culture")]
		public string? Culture { get; set; }

		[JsonPropertyName("fallback")]
		public List<string>? Fallback { get; set; }

		[JsonPropertyName("sourceRevision")]
		public int SourceRevision { get; set; }

		[JsonPropertyName("strings")]
		public Dictionary<string, string>? Strings { get; set; }
	}

	internal const int CurrentFormatVersion = 1;

	internal const string SourceResourceName = "ModLaunchManager.Localization.table.json";

	internal const string LocaleResourcePrefix = "ModLaunchManager.Localization.locales.";

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = false,
		AllowTrailingCommas = false,
		ReadCommentHandling = JsonCommentHandling.Disallow
	};

	private readonly IReadOnlyDictionary<string, MlmLocEntry> _sourceEntries;

	private readonly IReadOnlyDictionary<string, MlmLocaleBundle> _localeBundles;

	private readonly IReadOnlyDictionary<string, MlmLocaleInfo> _localeInfo;

	internal int Revision { get; }

	internal MlmLocaleInfo SourceLocale { get; }

	internal IReadOnlyList<MlmLocaleInfo> Locales { get; }

	private MlmLocTable(int revision, MlmLocaleInfo sourceLocale, IReadOnlyDictionary<string, MlmLocEntry> sourceEntries, IReadOnlyDictionary<string, MlmLocaleBundle> localeBundles)
	{
		Revision = revision;
		SourceLocale = sourceLocale;
		_sourceEntries = sourceEntries;
		_localeBundles = localeBundles;
		Dictionary<string, MlmLocaleInfo> dictionary = new Dictionary<string, MlmLocaleInfo>(StringComparer.OrdinalIgnoreCase) { [sourceLocale.Id] = sourceLocale };
		foreach (MlmLocaleBundle value in localeBundles.Values)
		{
			dictionary.Add(value.Info.Id, value.Info);
		}
		_localeInfo = new ReadOnlyDictionary<string, MlmLocaleInfo>(dictionary);
		Locales = dictionary.Values.ToArray();
	}

	internal static MlmLocTable Empty()
	{
		return new MlmLocTable(0, new MlmLocaleInfo("en", "English", "English", "eng", "en", 0), new ReadOnlyDictionary<string, MlmLocEntry>(new Dictionary<string, MlmLocEntry>(StringComparer.Ordinal)), new ReadOnlyDictionary<string, MlmLocaleBundle>(new Dictionary<string, MlmLocaleBundle>(StringComparer.OrdinalIgnoreCase)));
	}

	internal static MlmLocTable LoadFromAssembly(Assembly assembly)
	{
		string[] manifestResourceNames = assembly.GetManifestResourceNames();
		if (!manifestResourceNames.Contains<string>("ModLaunchManager.Localization.table.json", StringComparer.Ordinal))
		{
			throw new InvalidDataException("Embedded localization source 'ModLaunchManager.Localization.table.json' was not found.");
		}
		string sourceJson = ReadResourceUtf8(assembly, "ModLaunchManager.Localization.table.json");
		(string, string)[] localeResources = (from name in manifestResourceNames.Where((string name) => name.StartsWith("ModLaunchManager.Localization.locales.", StringComparison.Ordinal) && name.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).OrderBy<string, string>((string name) => name, StringComparer.Ordinal)
			select (name: name, ReadResourceUtf8(assembly, name))).ToArray();
		return Load(sourceJson, localeResources);
	}

	internal static MlmLocTable Load(string sourceJson, IEnumerable<(string Name, string Json)> localeResources)
	{
		SourceDocument sourceDocument = Deserialize<SourceDocument>(sourceJson, "source table");
		if (sourceDocument.FormatVersion != 1)
		{
			throw new InvalidDataException($"Unsupported source localization format {sourceDocument.FormatVersion}; expected {1}.");
		}
		if (sourceDocument.Revision <= 0)
		{
			throw new InvalidDataException("Source localization revision must be greater than zero.");
		}
		SourceLocaleDocument sourceLocaleDocument = sourceDocument.SourceLocale ?? throw new InvalidDataException("Source localization metadata is missing.");
		MlmLocaleInfo mlmLocaleInfo = CreateLocaleInfo(sourceLocaleDocument.Id, sourceLocaleDocument.DisplayName, sourceLocaleDocument.EnglishName, sourceLocaleDocument.GameLanguage, sourceLocaleDocument.Culture, sourceDocument.Revision, "source table");
		Dictionary<string, MlmLocEntry> dictionary = new Dictionary<string, MlmLocEntry>(StringComparer.Ordinal);
		string key;
		foreach (KeyValuePair<string, SourceEntryDocument> item in sourceDocument.Entries ?? throw new InvalidDataException("Source localization entries are missing."))
		{
			item.Deconstruct(out key, out var value2);
			string text = key;
			SourceEntryDocument sourceEntryDocument = value2;
			ValidateKey(text, "source table");
			string text2 = RequireText(sourceEntryDocument.Text, "source key '" + text + "'");
			HashSet<string> placeholderNames = MlmLocFormatter.GetPlaceholderNames(text2);
			IEnumerable<string> enumerable = sourceEntryDocument.Arguments?.Keys;
			HashSet<string> hashSet = new HashSet<string>(enumerable ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
			if (!placeholderNames.SetEquals(hashSet))
			{
				throw new InvalidDataException($"Source key '{text}' declares [{string.Join(", ", hashSet)}] but uses [{string.Join(", ", placeholderNames)}].");
			}
			dictionary.Add(text, new MlmLocEntry(text2, placeholderNames));
		}
		Dictionary<string, MlmLocaleBundle> dictionary2 = new Dictionary<string, MlmLocaleBundle>(StringComparer.OrdinalIgnoreCase);
		foreach (var localeResource in localeResources)
		{
			string resourceName = localeResource.Name;
			LocaleDocument localeDocument = Deserialize<LocaleDocument>(localeResource.Json, resourceName);
			if (localeDocument.FormatVersion != 1)
			{
				throw new InvalidDataException($"{resourceName} uses localization format {localeDocument.FormatVersion}; expected {1}.");
			}
			MlmLocaleInfo mlmLocaleInfo2 = CreateLocaleInfo(localeDocument.Locale, localeDocument.DisplayName, localeDocument.EnglishName, localeDocument.GameLanguage, localeDocument.Culture, localeDocument.SourceRevision, resourceName);
			if (string.Equals(mlmLocaleInfo2.Id, mlmLocaleInfo.Id, StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidDataException(resourceName + " duplicates the source locale '" + mlmLocaleInfo.Id + "'.");
			}
			if (localeDocument.SourceRevision <= 0 || localeDocument.SourceRevision > sourceDocument.Revision)
			{
				throw new InvalidDataException($"{resourceName} has invalid sourceRevision {localeDocument.SourceRevision}; the current source revision is {sourceDocument.Revision}.");
			}
			Dictionary<string, string> dictionary3 = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (KeyValuePair<string, string> item2 in localeDocument.Strings ?? throw new InvalidDataException(resourceName + " has no strings object."))
			{
				item2.Deconstruct(out key, out var value3);
				string text3 = key;
				string value4 = value3;
				ValidateKey(text3, resourceName);
				if (!dictionary.TryGetValue(text3, out var value5))
				{
					throw new InvalidDataException(resourceName + " contains unknown key '" + text3 + "'.");
				}
				string text4 = RequireText(value4, resourceName + " key '" + text3 + "'");
				if (!MlmLocFormatter.GetPlaceholderNames(text4).SetEquals(value5.Arguments))
				{
					throw new InvalidDataException($"{resourceName} key '{text3}' must use placeholders [{string.Join(", ", value5.Arguments)}].");
				}
				if (mlmLocaleInfo2.Id.StartsWith("zh-", StringComparison.OrdinalIgnoreCase) && HasCjkLatinBoundarySpace(text4))
				{
					throw new InvalidDataException(resourceName + " key '" + text3 + "' contains a space between Chinese and Latin text.");
				}
				dictionary3.Add(text3, text4);
			}
			List<string> fallback = localeDocument.Fallback;
			List<string>? list;
			if (fallback == null || fallback.Count <= 0)
			{
				int num = 1;
				list = new List<string>(num);
				CollectionsMarshal.SetCount(list, num);
				CollectionsMarshal.AsSpan(list)[0] = mlmLocaleInfo.Id;
			}
			else
			{
				list = localeDocument.Fallback;
			}
			string[] fallbacks = list.Select((string value) => RequireText(value, resourceName + " fallback")).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
			dictionary2.Add(mlmLocaleInfo2.Id, new MlmLocaleBundle(mlmLocaleInfo2, fallbacks, new ReadOnlyDictionary<string, string>(dictionary3)));
		}
		ValidateFallbacks(mlmLocaleInfo.Id, dictionary2);
		return new MlmLocTable(sourceDocument.Revision, mlmLocaleInfo, new ReadOnlyDictionary<string, MlmLocEntry>(dictionary), new ReadOnlyDictionary<string, MlmLocaleBundle>(dictionary2));
	}

	internal bool TryGetLocale(string locale, out MlmLocaleInfo info)
	{
		return _localeInfo.TryGetValue(locale, out info);
	}

	internal string GetRaw(string locale, string key)
	{
		if (!_sourceEntries.TryGetValue(key, out MlmLocEntry value))
		{
			return "[" + key + "]";
		}
		if (string.Equals(locale, SourceLocale.Id, StringComparison.OrdinalIgnoreCase))
		{
			return value.Text;
		}
		return ResolveTranslation(locale, key, new HashSet<string>(StringComparer.OrdinalIgnoreCase)) ?? value.Text;
	}

	internal string Format(string locale, string key, IReadOnlyDictionary<string, object?> arguments)
	{
		string raw = GetRaw(locale, key);
		MlmLocaleInfo info;
		string name = (TryGetLocale(locale, out info) ? info.Culture : SourceLocale.Culture);
		CultureInfo formatProvider;
		try
		{
			formatProvider = CultureInfo.GetCultureInfo(name);
		}
		catch (CultureNotFoundException)
		{
			formatProvider = CultureInfo.InvariantCulture;
		}
		return MlmLocFormatter.Format(raw, arguments, formatProvider);
	}

	private string? ResolveTranslation(string locale, string key, HashSet<string> visited)
	{
		if (!visited.Add(locale))
		{
			return null;
		}
		if (string.Equals(locale, SourceLocale.Id, StringComparison.OrdinalIgnoreCase))
		{
			return _sourceEntries[key].Text;
		}
		if (!_localeBundles.TryGetValue(locale, out MlmLocaleBundle value))
		{
			return null;
		}
		if (value.Strings.TryGetValue(key, out string value2))
		{
			return value2;
		}
		foreach (string fallback in value.Fallbacks)
		{
			string text = ResolveTranslation(fallback, key, visited);
			if (text != null)
			{
				return text;
			}
		}
		return null;
	}

	private static T Deserialize<T>(string json, string source) where T : class
	{
		try
		{
			return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? throw new InvalidDataException(source + " deserialized to null.");
		}
		catch (JsonException ex)
		{
			throw new InvalidDataException("Could not parse " + source + ": " + ex.Message, ex);
		}
	}

	private static MlmLocaleInfo CreateLocaleInfo(string? id, string? displayName, string? englishName, string? gameLanguage, string? culture, int sourceRevision, string source)
	{
		string text = RequireText(id, source + " locale");
		if (!IsLocaleId(text))
		{
			throw new InvalidDataException(source + " has invalid BCP-47 locale '" + text + "'.");
		}
		return new MlmLocaleInfo(text, RequireText(displayName, source + " displayName"), RequireText(englishName, source + " englishName"), RequireText(gameLanguage, source + " gameLanguage"), RequireText(culture, source + " culture"), sourceRevision);
	}

	private static void ValidateFallbacks(string sourceLocale, IReadOnlyDictionary<string, MlmLocaleBundle> bundles)
	{
		foreach (MlmLocaleBundle value2 in bundles.Values)
		{
			foreach (string fallback in value2.Fallbacks)
			{
				if (!string.Equals(fallback, sourceLocale, StringComparison.OrdinalIgnoreCase) && !bundles.ContainsKey(fallback))
				{
					throw new InvalidDataException($"Locale '{value2.Info.Id}' references unavailable fallback '{fallback}'.");
				}
			}
			Visit(value2.Info.Id, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
		}
		void Visit(string locale, HashSet<string> path)
		{
			if (!string.Equals(locale, sourceLocale, StringComparison.OrdinalIgnoreCase))
			{
				if (!path.Add(locale))
				{
					throw new InvalidDataException("Localization fallback cycle contains '" + locale + "'.");
				}
				if (bundles.TryGetValue(locale, out MlmLocaleBundle value))
				{
					foreach (string fallback2 in value.Fallbacks)
					{
						Visit(fallback2, path);
					}
				}
				path.Remove(locale);
			}
		}
	}

	private static string ReadResourceUtf8(Assembly assembly, string resourceName)
	{
		using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidDataException("Embedded resource '" + resourceName + "' could not be opened.");
		using StreamReader streamReader = new StreamReader(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), detectEncodingFromByteOrderMarks: true);
		return streamReader.ReadToEnd();
	}

	private static void ValidateKey(string key, string source)
	{
		if (string.IsNullOrWhiteSpace(key) || key.Any(char.IsWhiteSpace) || !key.Contains('.'))
		{
			throw new InvalidDataException(source + " contains invalid localization key '" + key + "'.");
		}
	}

	private static string RequireText(string? value, string field)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new InvalidDataException(field + " must not be empty.");
		}
		return value;
	}

	private static bool IsLocaleId(string value)
	{
		string[] array = value.Split('-');
		bool flag = array.Length == 0;
		if (!flag)
		{
			int length = array[0].Length;
			bool flag2 = ((length < 2 || length > 3) ? true : false);
			flag = flag2;
		}
		if (flag || array[0].Any((char character) => !char.IsAsciiLetter(character)))
		{
			return false;
		}
		return array.Skip(1).All(delegate(string part)
		{
			int length2 = part.Length;
			return length2 >= 2 && length2 <= 8 && part.All(char.IsAsciiLetterOrDigit);
		});
	}

	private static bool HasCjkLatinBoundarySpace(string value)
	{
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if ((c != '\t' && c != ' ') || 1 == 0)
			{
				continue;
			}
			int num = i - 1;
			int num2 = i + 1;
			while (true)
			{
				bool flag = num >= 0;
				if (flag)
				{
					c = value[num];
					bool flag2 = ((c == '\t' || c == ' ') ? true : false);
					flag = flag2;
				}
				if (flag)
				{
					num--;
					continue;
				}
				break;
			}
			while (true)
			{
				bool flag = num2 < value.Length;
				if (flag)
				{
					c = value[num2];
					bool flag2 = ((c == '\t' || c == ' ') ? true : false);
					flag = flag2;
				}
				if (!flag)
				{
					break;
				}
				num2++;
			}
			if (num >= 0 && num2 < value.Length && ((IsCjk(value[num]) && IsLatinOrDigit(value[num2])) || (IsLatinOrDigit(value[num]) && IsCjk(value[num2]))))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsLatinOrDigit(char character)
	{
		if (!char.IsAsciiLetter(character))
		{
			return char.IsAsciiDigit(character);
		}
		return true;
	}

	private static bool IsCjk(char character)
	{
		if (character >= '㐀')
		{
			if (character >= '豈')
			{
				if (character <= '\ufaff')
				{
					goto IL_0046;
				}
			}
			else if (character >= '가')
			{
				if (character <= '\ud7af')
				{
					goto IL_0046;
				}
			}
			else if (character <= '鿿')
			{
				goto IL_0046;
			}
		}
		else if (character >= '\u3040' && character <= 'ヿ')
		{
			goto IL_0046;
		}
		return false;
		IL_0046:
		return true;
	}
}
