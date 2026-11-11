using System;

namespace STS2SkinChanger.Core;

internal static class SkinSources
{
	public const string Steam = "steam";

	public const string GitHub = "github";

	public static readonly string[] Options = new string[2] { "steam", "github" };

	public static string Normalize(string? value)
	{
		if (!string.Equals(value, "github", StringComparison.OrdinalIgnoreCase))
		{
			return "steam";
		}
		return "github";
	}

	public static int IndexOf(string? value)
	{
		return (Normalize(value) == "github") ? 1 : 0;
	}

	public static string OptionAt(int index)
	{
		if (index < 0 || index >= Options.Length)
		{
			return "steam";
		}
		return Options[index];
	}

	public static string Name(string language, string? value)
	{
		return GitHubSkinText.ForLanguage(language, (!(Normalize(value) == "github")) ? GitHubSkinTextKey.Steam : GitHubSkinTextKey.GitHub);
	}
}
