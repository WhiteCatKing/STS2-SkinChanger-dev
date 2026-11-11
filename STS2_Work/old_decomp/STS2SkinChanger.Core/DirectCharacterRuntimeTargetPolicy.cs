using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class DirectCharacterRuntimeTargetPolicy
{
	public static IReadOnlySet<string> ResolveTargets(IEnumerable<(string FieldName, string Value)> declarations, IEnumerable<string> knownCharacterGroupIds)
	{
		Dictionary<string, string> dictionary = knownCharacterGroupIds.Where((string value) => !string.IsNullOrWhiteSpace(value)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToDictionary<string, string, string>((string value) => value, (string value) => value, StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (var declaration in declarations)
		{
			if (IsTargetCharacterField(declaration.FieldName) && !string.IsNullOrWhiteSpace(declaration.Value) && dictionary.TryGetValue(declaration.Value.Trim(), out var value2))
			{
				hashSet.Add(value2);
			}
		}
		return hashSet;
	}

	internal static bool IsTargetCharacterField(string? fieldName)
	{
		if (string.IsNullOrWhiteSpace(fieldName))
		{
			return false;
		}
		string text = new string(fieldName.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
		bool flag = ((text == "characterid" || text == "characterentry") ? true : false);
		if (!flag && !text.StartsWith("targetcharacter", StringComparison.Ordinal) && !text.StartsWith("skincharacter", StringComparison.Ordinal))
		{
			return text.StartsWith("runtimeskincharacter", StringComparison.Ordinal);
		}
		return true;
	}
}
