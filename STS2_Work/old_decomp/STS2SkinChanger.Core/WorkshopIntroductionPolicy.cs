using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace STS2SkinChanger.Core;

internal static class WorkshopIntroductionPolicy
{
	private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(80L, 0L);

	public static string Summary(string? text, int maxCharacters = 360)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return "";
		}
		text = text.Substring(0, Math.Min(text.Length, 32000)).Replace("\r\n", "\n").Replace('\r', '\n');
		try
		{
			text = Regex.Replace(text, "\\[(img|previewyoutube|previewicon)(?:=[^\\]]*)?\\].*?\\[/\\1\\]", "", RegexOptions.IgnoreCase | RegexOptions.Singleline, RegexTimeout);
			text = Regex.Replace(text, "\\[/?(?:h[1-6]|list|olist|\\*)\\]", "\n", RegexOptions.IgnoreCase, RegexTimeout);
			text = Regex.Replace(text, "\\[/?[a-z][^\\]\\n]*\\]", "", RegexOptions.IgnoreCase, RegexTimeout);
			text = Regex.Replace(text, "[\\t ]+", " ", RegexOptions.None, RegexTimeout);
			text = Regex.Replace(text, "\\n[ \\n]+", "\n\n", RegexOptions.None, RegexTimeout).Trim();
		}
		catch (RegexMatchTimeoutException)
		{
			return "";
		}
		int[] array = StringInfo.ParseCombiningCharacters(text);
		int num = Math.Clamp(maxCharacters, 1, 1000);
		if (array.Length > num)
		{
			return text.Substring(0, array[num - 1]).TrimEnd() + "…";
		}
		return text;
	}

	public static string[] Images(string cover, IEnumerable<string> previews)
	{
		string[] array = previews.Where((string url) => !string.IsNullOrWhiteSpace(url)).Distinct<string>(StringComparer.Ordinal).Take(64)
			.ToArray();
		if (array.Length == 0)
		{
			if (!string.IsNullOrWhiteSpace(cover))
			{
				return new string[1] { cover };
			}
			return Array.Empty<string>();
		}
		return array;
	}
}
