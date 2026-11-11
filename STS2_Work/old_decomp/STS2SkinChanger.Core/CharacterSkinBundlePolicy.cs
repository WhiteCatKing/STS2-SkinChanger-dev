using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class CharacterSkinBundlePolicy
{
	private const string SelectionOptionPrefix = "__skin_bundle__:";

	internal static string CreateSelectionOptionId(string name)
	{
		return "__skin_bundle__:" + Uri.EscapeDataString(name.Trim());
	}

	internal static bool TryGetSelectionBundleName(string? optionId, out string name)
	{
		name = string.Empty;
		if (string.IsNullOrWhiteSpace(optionId) || !optionId.StartsWith("__skin_bundle__:", StringComparison.Ordinal))
		{
			return false;
		}
		try
		{
			int length = "__skin_bundle__:".Length;
			name = Uri.UnescapeDataString(optionId.Substring(length, optionId.Length - length)).Trim();
			return name.Length > 0;
		}
		catch (UriFormatException)
		{
			name = string.Empty;
			return false;
		}
	}

	internal static string CreateSelectionDisplayName(string name)
	{
		return "[P] " + name.Trim();
	}

	internal static bool IsValidCharacterOptionReference(string? optionId)
	{
		string name;
		if (!string.IsNullOrWhiteSpace(optionId))
		{
			return !TryGetSelectionBundleName(optionId, out name);
		}
		return false;
	}

	internal static bool TryEnterApplication(string groupId, string bundleName, IReadOnlySet<string> activeApplications, out HashSet<string> nextApplications)
	{
		nextApplications = new HashSet<string>(activeApplications, StringComparer.OrdinalIgnoreCase);
		return nextApplications.Add(groupId.Trim() + "\n" + bundleName.Trim());
	}

	internal static bool ChangesOutsideRequestedGroups(IReadOnlyDictionary<string, string> original, IReadOnlyDictionary<string, string> staged, IEnumerable<string> protectedGroups, IReadOnlySet<string> requestedGroups)
	{
		return protectedGroups.Any((string id) => !requestedGroups.Contains(id) && !string.Equals(original.GetValueOrDefault(id, "__base__"), staged.GetValueOrDefault(id, "__base__"), StringComparison.OrdinalIgnoreCase));
	}

	internal static List<CharacterSkinBundle> Normalize(IEnumerable<CharacterSkinBundle>? bundles)
	{
		return (from bundle in bundles ?? Array.Empty<CharacterSkinBundle>()
			where bundle != null && !string.IsNullOrWhiteSpace(bundle.Name) && !string.IsNullOrWhiteSpace(bundle.CharacterGroupId)
			select new CharacterSkinBundle
			{
				Id = (string.IsNullOrWhiteSpace(bundle.Id) ? Guid.NewGuid().ToString("N") : bundle.Id.Trim()),
				HideSources = bundle.HideSources,
				Name = bundle.Name.Trim(),
				CharacterGroupId = bundle.CharacterGroupId.Trim().ToLowerInvariant(),
				CharacterOptionId = (string.IsNullOrWhiteSpace(bundle.CharacterOptionId) ? "__base__" : bundle.CharacterOptionId.Trim()),
				CardMode = NormalizeMode(bundle.CardMode),
				MonsterMode = NormalizeMode(bundle.MonsterMode),
				CardModPriority = NormalizePriority(bundle.CardModPriority),
				MonsterModPriority = NormalizePriority(bundle.MonsterModPriority),
				CardPresetNames = NormalizeReferences(bundle.CardPresetNames),
				MonsterPresetNames = NormalizeReferences(bundle.MonsterPresetNames)
			}).DistinctBy<CharacterSkinBundle, string>((CharacterSkinBundle bundle) => bundle.CharacterGroupId + "\n" + bundle.Name, StringComparer.OrdinalIgnoreCase).ToList();
	}

	internal static void RenamePresetReference(CharacterSkinBundle bundle, bool monsterPreset, string categoryId, string previousName, string newName)
	{
		Dictionary<string, string> dictionary = (monsterPreset ? bundle.MonsterPresetNames : bundle.CardPresetNames);
		if (dictionary.TryGetValue(categoryId, out var value) && value.Equals(previousName, StringComparison.OrdinalIgnoreCase))
		{
			dictionary[categoryId] = newName;
		}
	}

	internal static void RemovePresetReference(CharacterSkinBundle bundle, bool monsterPreset, string categoryId, string presetName)
	{
		Dictionary<string, string> dictionary = (monsterPreset ? bundle.MonsterPresetNames : bundle.CardPresetNames);
		if (dictionary.TryGetValue(categoryId, out var value) && value.Equals(presetName, StringComparison.OrdinalIgnoreCase))
		{
			dictionary[categoryId] = bundle.PresetKey;
		}
	}

	internal static CharacterSkinBundle Clone(CharacterSkinBundle bundle)
	{
		return new CharacterSkinBundle
		{
			Id = bundle.Id,
			HideSources = bundle.HideSources,
			Name = bundle.Name,
			CharacterGroupId = bundle.CharacterGroupId,
			CharacterOptionId = bundle.CharacterOptionId,
			CardMode = NormalizeMode(bundle.CardMode),
			MonsterMode = NormalizeMode(bundle.MonsterMode),
			CardModPriority = NormalizePriority(bundle.CardModPriority),
			MonsterModPriority = NormalizePriority(bundle.MonsterModPriority),
			CardPresetNames = new Dictionary<string, string>(bundle.CardPresetNames, StringComparer.OrdinalIgnoreCase),
			MonsterPresetNames = new Dictionary<string, string>(bundle.MonsterPresetNames, StringComparer.OrdinalIgnoreCase)
		};
	}

	private static BundleContentMode NormalizeMode(BundleContentMode mode)
	{
		if (mode != BundleContentMode.ModPriority)
		{
			return BundleContentMode.MultiplePresets;
		}
		return mode;
	}

	internal static List<BundleModPriorityEntry> NormalizePriority(IEnumerable<BundleModPriorityEntry>? entries)
	{
		return (from entry in entries ?? Array.Empty<BundleModPriorityEntry>()
			where entry != null && !string.IsNullOrWhiteSpace(entry.OptionId)
			select entry with
			{
				OptionId = entry.OptionId.Trim()
			}).DistinctBy<BundleModPriorityEntry, string>((BundleModPriorityEntry entry) => entry.OptionId, StringComparer.OrdinalIgnoreCase).ToList();
	}

	internal static List<BundleModPriorityEntry> CompletePriority(IEnumerable<BundleModPriorityEntry>? stored, IEnumerable<string> available)
	{
		List<BundleModPriorityEntry> list = NormalizePriority(stored);
		HashSet<string> hashSet = list.Select((BundleModPriorityEntry entry) => entry.OptionId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in available)
		{
			if (hashSet.Add(item))
			{
				list.Add(new BundleModPriorityEntry(item, Enabled: false));
			}
		}
		return list;
	}

	internal static List<BundleModPriorityEntry> ProjectPriority(IReadOnlyList<BundleModPriorityEntry> entries, IEnumerable<string> categorySources)
	{
		HashSet<string> ids = categorySources.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		List<BundleModPriorityEntry> list = entries.Where((BundleModPriorityEntry entry) => ids.Contains(entry.OptionId)).ToList();
		HashSet<string> included = list.Select((BundleModPriorityEntry entry) => entry.OptionId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		list.AddRange(from id in ids
			where !included.Contains(id)
			select new BundleModPriorityEntry(id, Enabled: false));
		return list;
	}

	private static Dictionary<string, string> NormalizeReferences(IReadOnlyDictionary<string, string>? references)
	{
		return (references ?? new Dictionary<string, string>()).Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value)).DistinctBy<KeyValuePair<string, string>, string>((KeyValuePair<string, string> pair) => pair.Key.Trim(), StringComparer.OrdinalIgnoreCase).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key.Trim().ToLowerInvariant(), (KeyValuePair<string, string> pair) => pair.Value.Trim(), StringComparer.OrdinalIgnoreCase);
	}
}
