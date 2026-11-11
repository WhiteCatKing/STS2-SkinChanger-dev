using System;
using System.Text;

namespace STS2SkinChanger.Core;

internal static class BackgroundSkinPolicy
{
	public const string FollowCategoryId = "__follow__";

	public const string ModDefaultId = "__mod_default__";

	private const string ProviderPrefix = "bg";

	private const string Separator = "::";

	public static string ProviderOptionId(string providerId, string battlefieldId)
	{
		return "bg::" + providerId.Trim() + "::" + battlefieldId.Trim();
	}

	public static bool TryParseProviderOptionId(string? optionId, out string providerId, out string battlefieldId)
	{
		providerId = string.Empty;
		battlefieldId = string.Empty;
		if (string.IsNullOrWhiteSpace(optionId))
		{
			return false;
		}
		string[] array = optionId.Split("::");
		if (array.Length != 3 || !array[0].Equals("bg", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(array[1]) || string.IsNullOrWhiteSpace(array[2]))
		{
			return false;
		}
		providerId = array[1].Trim();
		battlefieldId = array[2].Trim();
		return true;
	}

	public static bool IsFollowCategory(string? selection)
	{
		if (!string.IsNullOrWhiteSpace(selection))
		{
			return selection.Equals("__follow__", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public static string DisplayName(string battlefieldId)
	{
		string text = battlefieldId.Trim();
		string text2;
		if (text.StartsWith("b_", StringComparison.OrdinalIgnoreCase))
		{
			text2 = text;
			text = text2.Substring(2, text2.Length - 2);
		}
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		bool flag = true;
		text2 = text;
		foreach (char c in text2)
		{
			if ((c == '-' || c == '_') ? true : false)
			{
				if (stringBuilder.Length > 0)
				{
					if (stringBuilder[stringBuilder.Length - 1] != ' ')
					{
						stringBuilder.Append(' ');
					}
				}
				flag = true;
			}
			else
			{
				stringBuilder.Append(flag ? char.ToUpperInvariant(c) : c);
				flag = false;
			}
		}
		return stringBuilder.ToString().Trim();
	}
}
