using System;
using System.Buffers.Binary;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;

namespace STS2SkinChanger.Catalog;

internal static class CardArtPackScanPolicy
{
	public const string BundleFormat = "card_art_bundle";

	public const string PackSuffix = ".cardartpack.json";

	public const int MaxEntryChars = 67108864;

	public const int MaxMetadataBytes = 65536;

	private static readonly byte[] PngSignature = new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 };

	[GeneratedRegex("\"source_path\"\\s*:\\s*\"(?<v>[^\"]+)\"")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	public static Regex SourcePathRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__SourcePathRegex_0.Instance;
	}

	[GeneratedRegex("\"display_mode\"\\s*:\\s*\"(?<v>[^\"]*)\"")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex DisplayModeRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__DisplayModeRegex_1.Instance;
	}

	[GeneratedRegex("\"type\"\\s*:\\s*\"(?<v>[^\"]*)\"")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex TypeRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__TypeRegex_2.Instance;
	}

	[GeneratedRegex("\"png_base64\"\\s*:\\s*\"(?<v>[A-Za-z0-9+/=]*)\"")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	public static Regex RenderedRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RenderedRegex_3.Instance;
	}

	[GeneratedRegex("\"edit_source_png_base64\"\\s*:\\s*\"(?<v>[A-Za-z0-9+/=]*)\"")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex EditSourceRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__EditSourceRegex_4.Instance;
	}

	[GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex SafeIdRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__SafeIdRegex_5.Instance;
	}

	public static bool IsBundleHeader(ReadOnlySpan<char> head)
	{
		return head.Contains("\"card_art_bundle\"", StringComparison.Ordinal);
	}

	public static bool TryReadPngSize(byte[] png, out int width, out int height)
	{
		width = 0;
		height = 0;
		if (png.Length < 24 || !png.AsSpan(0, PngSignature.Length).SequenceEqual(PngSignature))
		{
			return false;
		}
		width = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16, 4));
		height = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(20, 4));
		if (width > 0)
		{
			return height > 0;
		}
		return false;
	}

	public static CardPresentationDefinition? BuildCardPresentation(CardArtPackCard card)
	{
		if (!card.IsCardShaped)
		{
			return null;
		}
		bool? portraitVisible = true;
		bool? portraitBorderVisible = false;
		bool? bannerVisible = true;
		bool? textBackgroundVisible = true;
		return new CardPresentationDefinition(UseAncientLayout: false, null, null, null, null, null, null, null, null, null, null, null, null, bannerVisible, textBackgroundVisible, portraitBorderVisible, null, null, null, null, null, null, UseFullFrameArt: false, UseExpandedPortraitLayout: true, null, portraitVisible, null, null, null, null, null, null, null);
	}

	public static string NormalizePackId(string? declaredId, string fileName)
	{
		string text;
		if (!fileName.EndsWith(".cardartpack.json", StringComparison.OrdinalIgnoreCase))
		{
			text = Path.GetFileNameWithoutExtension(fileName);
		}
		else
		{
			int length = ".cardartpack.json".Length;
			text = fileName.Substring(0, fileName.Length - length);
		}
		string text2 = text;
		string text3 = (string.IsNullOrWhiteSpace(declaredId) ? text2 : declaredId);
		if (!SafeIdRegex().IsMatch(text3))
		{
			return "card_art_pack";
		}
		return text3;
	}

	public static IReadOnlyList<CardArtPackCard> ParseCards(string bundleText)
	{
		Dictionary<string, CardArtPackCard> dictionary = new Dictionary<string, CardArtPackCard>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in SplitEntries(bundleText).Entries)
		{
			Add(dictionary, item);
		}
		return dictionary.Values.OrderBy<CardArtPackCard, string>((CardArtPackCard card) => card.SourcePath, StringComparer.OrdinalIgnoreCase).ToArray();
	}

	private static void Add(IDictionary<string, CardArtPackCard> target, string entry)
	{
		Match match = SourcePathRegex().Match(entry);
		if (match == null || !match.Success)
		{
			return;
		}
		string value = match.Groups["v"].Value;
		string text = StemOf(value);
		if (string.IsNullOrWhiteSpace(value) || text.Length == 0)
		{
			return;
		}
		string text2 = NonEmpty(EditSourceRegex(), entry);
		string text3 = NonEmpty(RenderedRegex(), entry);
		string text4 = text2 ?? text3;
		if (text4 == null)
		{
			return;
		}
		byte[] array = Decode(text4);
		if (array != null && array.AsSpan().StartsWith(PngSignature))
		{
			CardArtPackCard cardArtPackCard = new CardArtPackCard(value, text, array, Value(DisplayModeRegex(), entry) ?? "original", Value(TypeRegex(), entry) ?? "static", text2 != null);
			if (!target.TryGetValue(value, out CardArtPackCard value2) || ReplacesExisting(value2, cardArtPackCard))
			{
				target[value] = cardArtPackCard;
			}
		}
	}

	public static bool ReplacesExisting(CardArtPackCard existing, CardArtPackCard candidate)
	{
		if (candidate.UsesEditSource == existing.UsesEditSource)
		{
			return candidate.Bytes.Length > existing.Bytes.Length;
		}
		return candidate.UsesEditSource;
	}

	private static string? NonEmpty(Regex regex, string entry)
	{
		Match match = regex.Match(entry);
		if (match == null || !match.Success || match.Groups["v"].Value.Length <= 0)
		{
			return null;
		}
		return match.Groups["v"].Value;
	}

	public static (IReadOnlyList<string> Entries, string Remainder) SplitEntries(string window)
	{
		List<string> list = new List<string>();
		List<int> list2 = new List<int>(4);
		bool flag = false;
		int num = 0;
		int num3;
		for (int i = 0; i < window.Length; i++)
		{
			char c = window[i];
			if (flag)
			{
				switch (c)
				{
				case '\\':
					i++;
					break;
				case '"':
					flag = false;
					break;
				}
				continue;
			}
			switch (c)
			{
			case '"':
				flag = true;
				break;
			case '{':
				list2.Add(i);
				break;
			case '}':
				if (list2.Count != 0)
				{
					int num2 = list2[list2.Count - 1];
					list2.RemoveAt(list2.Count - 1);
					num = i + 1;
					num3 = num2;
					string text = window.Substring(num3, num - num3);
					if (IsEntryObject(text))
					{
						list.Add(text);
					}
				}
				break;
			}
		}
		num3 = num;
		return (Entries: list, Remainder: window.Substring(num3, window.Length - num3));
	}

	private static bool IsEntryObject(ReadOnlySpan<char> candidate)
	{
		if (candidate.Contains("\"source_path\"", StringComparison.Ordinal))
		{
			return !candidate.Contains("\"overrides\"", StringComparison.Ordinal);
		}
		return false;
	}

	private static string? Value(Regex regex, string entry)
	{
		Match match = regex.Match(entry);
		if (match == null || !match.Success)
		{
			return null;
		}
		return match.Groups["v"].Value;
	}

	private static byte[]? Decode(string base64)
	{
		try
		{
			return (base64.Length == 0) ? null : Convert.FromBase64String(base64);
		}
		catch (FormatException)
		{
			return null;
		}
	}

	private static string StemOf(string sourcePath)
	{
		int num = sourcePath.LastIndexOf('/');
		string text;
		if (num < 0)
		{
			text = sourcePath;
		}
		else
		{
			int num2 = num + 1;
			text = sourcePath.Substring(num2, sourcePath.Length - num2);
		}
		string text2 = text;
		int num3 = text2.LastIndexOf('.');
		if (num3 <= 0)
		{
			return text2;
		}
		return text2.Substring(0, num3);
	}
}
