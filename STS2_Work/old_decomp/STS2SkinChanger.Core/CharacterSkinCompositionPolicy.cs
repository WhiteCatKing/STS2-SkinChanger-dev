using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace STS2SkinChanger.Core;

internal static class CharacterSkinCompositionPolicy
{
	public const string IdPrefix = "composition:";

	public const int MaxNameLength = 40;

	public static string CreateId()
	{
		return "composition:" + Guid.NewGuid().ToString("N");
	}

	public static string CreateSessionId(string groupId, IEnumerable<string> sourceOptionIds)
	{
		StringBuilder signature = new StringBuilder();
		AppendToken(groupId);
		foreach (string sourceOptionId in sourceOptionIds)
		{
			AppendToken(sourceOptionId);
		}
		return "composition:session:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(signature.ToString()))).ToLowerInvariant();
		void AppendToken(string value)
		{
			string text = value.Trim().ToLowerInvariant();
			signature.Append(text.Length).Append(':').Append(text);
		}
	}

	public static List<CharacterSkinComposition> Normalize(IEnumerable<CharacterSkinComposition>? compositions)
	{
		List<CharacterSkinComposition> list = new List<CharacterSkinComposition>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (CharacterSkinComposition item in compositions ?? Array.Empty<CharacterSkinComposition>())
		{
			if (item == null || string.IsNullOrWhiteSpace(item.GroupId))
			{
				continue;
			}
			string text = item.Id?.Trim() ?? string.Empty;
			if (!text.StartsWith("composition:", StringComparison.OrdinalIgnoreCase) || !hashSet.Add(text))
			{
				do
				{
					text = CreateId();
				}
				while (!hashSet.Add(text));
			}
			list.Add(new CharacterSkinComposition
			{
				Id = text,
				GroupId = item.GroupId.Trim(),
				Name = TrimName(item.Name),
				SourceOptionIds = NormalizeOptionIds(item.SourceOptionIds),
				HideSources = item.HideSources
			});
		}
		return list;
	}

	public static string UniqueName(string? requestedName, IEnumerable<string> existingNames, string defaultBaseName)
	{
		HashSet<string> hashSet = existingNames.Where((string name) => !string.IsNullOrWhiteSpace(name)).Select(TrimName).ToHashSet<string>(StringComparer.CurrentCultureIgnoreCase);
		string text = TrimName(requestedName);
		string text2 = TrimName(defaultBaseName);
		if (string.IsNullOrWhiteSpace(text2))
		{
			text2 = "Combined Skin";
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			return FirstAvailableNumberedName(text2, 1, hashSet);
		}
		if (!hashSet.Contains(text))
		{
			return text;
		}
		return FirstAvailableNumberedName(text, 2, hashSet);
	}

	public static IReadOnlyList<string> VisibleRawOptionIds(string groupId, IEnumerable<string> rawOptionIds, IEnumerable<CharacterSkinComposition>? compositions)
	{
		HashSet<string> hidden = (compositions ?? Array.Empty<CharacterSkinComposition>()).Where((CharacterSkinComposition composition) => composition != null && composition.HideSources && composition.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase)).SelectMany((CharacterSkinComposition composition) => composition.SourceOptionIds ?? new List<string>()).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		return (from optionId in NormalizeOptionIds(rawOptionIds)
			where !hidden.Contains(optionId)
			select optionId).ToArray();
	}

	public static IReadOnlyList<string> ResolveAvailableSourceIds(IEnumerable<string>? sourceOptionIds, IEnumerable<string> availableOptionIds)
	{
		HashSet<string> hashSet = availableOptionIds.Where((string optionId) => !string.IsNullOrWhiteSpace(optionId)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		return NormalizeOptionIds(sourceOptionIds).Where(hashSet.Contains).ToArray();
	}

	public static bool CanComposeCharacterGroup(bool isKnownCharacterGroup, bool hasRecognizedAppearanceAssets, bool hasRuntimeProvider)
	{
		if (!hasRecognizedAppearanceAssets)
		{
			return isKnownCharacterGroup && hasRuntimeProvider;
		}
		return true;
	}

	public static IReadOnlyDictionary<string, string> MergeSelectionUpdates(IEnumerable<IReadOnlyDictionary<string, string>> updateSets)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (IReadOnlyDictionary<string, string> updateSet in updateSets)
		{
			foreach (KeyValuePair<string, string> item in updateSet)
			{
				dictionary[item.Key] = item.Value;
			}
		}
		return dictionary;
	}

	public static IReadOnlyList<string> BuildProviderPriority(IEnumerable<string> providerIds)
	{
		return Enumerable.Reverse(NormalizeOptionIds(providerIds)).ToArray();
	}

	public static bool ShouldApplyAfterSave(bool isNewComposition, bool wasSelected)
	{
		return isNewComposition || wasSelected;
	}

	public static string ResolveDisplayName(string name, bool isComposition, Func<string, string> localizeRawName)
	{
		if (!isComposition)
		{
			return localizeRawName(name);
		}
		return name;
	}

	public static ResolvedCharacterSkinComposition<T> ResolveAssets<T>(IEnumerable<string>? sourceOptionIds, IReadOnlyDictionary<string, CharacterSkinCompositionSource<T>> availableSources, Func<string, string> canonicalizeTarget)
	{
		ArgumentNullException.ThrowIfNull(availableSources, "availableSources");
		ArgumentNullException.ThrowIfNull(canonicalizeTarget, "canonicalizeTarget");
		IReadOnlyList<string> readOnlyList = ResolveAvailableSourceIds(sourceOptionIds, availableSources.Keys);
		Dictionary<string, T> dictionary = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
		string text = null;
		foreach (string item in readOnlyList)
		{
			CharacterSkinCompositionSource<T> characterSkinCompositionSource = availableSources[item];
			if (text == null && characterSkinCompositionSource.HasDynamicBehavior)
			{
				text = characterSkinCompositionSource.OptionId;
			}
			foreach (KeyValuePair<string, T> asset in characterSkinCompositionSource.Assets)
			{
				string text2 = canonicalizeTarget(asset.Key);
				if (!string.IsNullOrWhiteSpace(text2))
				{
					dictionary.TryAdd(text2, asset.Value);
				}
			}
		}
		return new ResolvedCharacterSkinComposition<T>(readOnlyList, dictionary, text);
	}

	private static List<string> NormalizeOptionIds(IEnumerable<string>? optionIds)
	{
		return (from optionId in optionIds ?? Array.Empty<string>()
			where !string.IsNullOrWhiteSpace(optionId)
			select optionId.Trim()).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
	}

	private static string TrimName(string? name)
	{
		string text = name?.Trim() ?? string.Empty;
		if (text.Length > 40)
		{
			return text.Substring(0, 40).TrimEnd();
		}
		return text;
	}

	private static string FirstAvailableNumberedName(string baseName, int firstNumber, IReadOnlySet<string> existing)
	{
		for (int i = firstNumber; i < int.MaxValue; i++)
		{
			string text = " " + i;
			int num = Math.Max(0, 40 - text.Length);
			string text2 = ((baseName.Length <= num) ? baseName : baseName.Substring(0, num).TrimEnd()) + text;
			if (!existing.Contains(text2))
			{
				return text2;
			}
		}
		throw new InvalidOperationException("无法为合并皮肤生成唯一名称。");
	}
}
