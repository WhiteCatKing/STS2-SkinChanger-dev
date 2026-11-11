using System.CodeDom.Compiler;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;

namespace ModLaunchManager;

internal static class WorkshopCollectionLink
{
	[GeneratedRegex("steamcommunity\\.com/(?:sharedfiles|workshop)/filedetails/?\\?(?:[^#\\s]*?&)?id=(\\d{1,20})", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.26416")]
	private static Regex PageLink()
	{
		return _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__PageLink_0.Instance;
	}

	[GeneratedRegex("steam://url/CommunityFilePage/(\\d{1,20})", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.26416")]
	private static Regex ClientLink()
	{
		return _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__ClientLink_1.Instance;
	}

	internal static bool TryParse(string? text, out ulong itemId)
	{
		itemId = 0uL;
		string text2 = text?.Trim() ?? string.Empty;
		if (text2.Length == 0)
		{
			return false;
		}
		if (text2.All(char.IsAsciiDigit))
		{
			if (ulong.TryParse(text2, out itemId))
			{
				return itemId != 0;
			}
			return false;
		}
		Match match = PageLink().Match(text2);
		if (!match.Success)
		{
			match = ClientLink().Match(text2);
		}
		if (match.Success && ulong.TryParse(match.Groups[1].Value, out itemId))
		{
			return itemId != 0;
		}
		return false;
	}
}
