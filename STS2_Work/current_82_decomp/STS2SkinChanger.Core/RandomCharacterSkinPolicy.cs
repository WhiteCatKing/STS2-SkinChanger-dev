using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class RandomCharacterSkinPolicy
{
	public const string OptionId = "__random_character_skin__";

	public static bool IsRandom(string? id)
	{
		return string.Equals(id, "__random_character_skin__", StringComparison.OrdinalIgnoreCase);
	}

	public static bool IsCandidate(string? id)
	{
		if (!string.IsNullOrWhiteSpace(id) && !IsRandom(id))
		{
			return WorkshopCatalogPolicy.IsSkinChoice(id);
		}
		return false;
	}

	public static string Draw(IEnumerable<string> visibleOptions, Func<int, int> next)
	{
		string[] array = visibleOptions.Where(IsCandidate).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		if (array.Length != 0)
		{
			return array[next(array.Length)];
		}
		return "__base__";
	}
}
