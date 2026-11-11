using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class BundlePresetPolicy
{
	private const string Prefix = "__bundle_preset__:";

	internal static string PresetKey(CharacterSkinBundle bundle)
	{
		return bundle.PresetKey;
	}

	internal static bool IsOwned(string key)
	{
		return key.StartsWith("__bundle_preset__:", StringComparison.Ordinal);
	}

	internal static Dictionary<string, string> CardSelections(CardSkinPreset preset, string groupId)
	{
		if (!preset.AllOriginal)
		{
			return new Dictionary<string, string>(preset.Selections, StringComparer.OrdinalIgnoreCase);
		}
		return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["cards:" + groupId] = "__base__" };
	}

	internal static Dictionary<string, string> MonsterSelections(MonsterSkinPreset preset, IEnumerable<string> groups)
	{
		if (!preset.AllOriginal)
		{
			return new Dictionary<string, string>(preset.Selections, StringComparer.OrdinalIgnoreCase);
		}
		return groups.ToDictionary<string, string, string>((string id) => id, (string _) => "__base__", StringComparer.OrdinalIgnoreCase);
	}

	internal static string DisplayName(SkinConfig config, string key, Func<string, string> characterName)
	{
		CharacterSkinBundle characterSkinBundle = config.CharacterSkinBundles.FirstOrDefault((CharacterSkinBundle bundle) => PresetKey(bundle) == key);
		if (characterSkinBundle != null)
		{
			return characterName(characterSkinBundle.CharacterGroupId) + "-" + characterSkinBundle.Name;
		}
		return key;
	}

	internal static string ResolveReference(CharacterSkinBundle bundle, string? requested, IEnumerable<string> availableNames)
	{
		if (string.IsNullOrWhiteSpace(requested) || !availableNames.Contains<string>(requested, StringComparer.OrdinalIgnoreCase))
		{
			return PresetKey(bundle);
		}
		return requested;
	}

	internal static IEnumerable<string> HiddenSources(SkinConfig config, string groupId)
	{
		return (from bundle in config.CharacterSkinBundles
			where bundle.HideSources && bundle.CharacterGroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase) && bundle.CharacterOptionId != "__base__"
			select bundle.CharacterOptionId).Distinct<string>(StringComparer.OrdinalIgnoreCase);
	}

	internal static void Synchronize(SkinConfig config, IEnumerable<string> cardCategories, IEnumerable<string> monsterCategories)
	{
		string[] array = cardCategories.ToArray();
		string[] array2 = monsterCategories.ToArray();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (CharacterSkinBundle characterSkinBundle in config.CharacterSkinBundles)
		{
			string[] array4;
			if (!hashSet.Add(characterSkinBundle.Id))
			{
				string previousKey = PresetKey(characterSkinBundle);
				characterSkinBundle.Id = Guid.NewGuid().ToString("N");
				hashSet.Add(characterSkinBundle.Id);
				Dictionary<string, string>[] array3 = new Dictionary<string, string>[2] { characterSkinBundle.CardPresetNames, characterSkinBundle.MonsterPresetNames };
				foreach (Dictionary<string, string> dictionary in array3)
				{
					array4 = (from pair in dictionary
						where pair.Value == previousKey
						select pair.Key).ToArray();
					foreach (string key in array4)
					{
						dictionary[key] = PresetKey(characterSkinBundle);
					}
				}
			}
			string key2 = PresetKey(characterSkinBundle);
			array4 = array;
			foreach (string category in array4)
			{
				if (!config.CardSkinPresets.Any((CardSkinPreset p) => p.Name == key2 && string.Equals(p.CategoryId, category, StringComparison.OrdinalIgnoreCase)))
				{
					config.CardSkinPresets.Add(new CardSkinPreset
					{
						Name = key2,
						CategoryId = category,
						AllOriginal = true
					});
				}
			}
			array4 = array2;
			foreach (string category2 in array4)
			{
				if (!config.MonsterSkinPresets.Any((MonsterSkinPreset p) => p.Name == key2 && p.CategoryId.Equals(category2, StringComparison.OrdinalIgnoreCase)))
				{
					config.MonsterSkinPresets.Add(new MonsterSkinPreset
					{
						Name = key2,
						CategoryId = category2,
						AllOriginal = true
					});
				}
			}
		}
		foreach (CharacterSkinBundle characterSkinBundle2 in config.CharacterSkinBundles)
		{
			string[] array4 = array;
			foreach (string category3 in array4)
			{
				characterSkinBundle2.CardPresetNames[category3] = ResolveReference(characterSkinBundle2, characterSkinBundle2.CardPresetNames.GetValueOrDefault(category3), from p in config.CardSkinPresets
					where string.Equals(p.CategoryId, category3, StringComparison.OrdinalIgnoreCase)
					select p.Name);
			}
			array4 = array2;
			foreach (string category4 in array4)
			{
				characterSkinBundle2.MonsterPresetNames[category4] = ResolveReference(characterSkinBundle2, characterSkinBundle2.MonsterPresetNames.GetValueOrDefault(category4), from p in config.MonsterSkinPresets
					where p.CategoryId.Equals(category4, StringComparison.OrdinalIgnoreCase)
					select p.Name);
			}
		}
	}

	internal static void InitializeDraft(CharacterSkinBundle bundle, IEnumerable<string> cards, IEnumerable<string> monsters)
	{
		string value = PresetKey(bundle);
		foreach (string card in cards)
		{
			bundle.CardPresetNames[card] = value;
		}
		foreach (string monster in monsters)
		{
			bundle.MonsterPresetNames[monster] = value;
		}
	}

	internal static void RemoveOwnedPresets(SkinConfig config, CharacterSkinBundle bundle)
	{
		string key = PresetKey(bundle);
		config.CardSkinPresets.RemoveAll((CardSkinPreset p) => p.Name == key);
		config.MonsterSkinPresets.RemoveAll((MonsterSkinPreset p) => p.Name == key);
		Dictionary<string, string>[] array = new Dictionary<string, string>[2] { config.ActiveCardSkinPresets, config.ActiveMonsterSkinPresets };
		foreach (Dictionary<string, string> dictionary in array)
		{
			string[] array2 = (from p in dictionary
				where p.Value == key
				select p.Key).ToArray();
			foreach (string key2 in array2)
			{
				dictionary.Remove(key2);
			}
		}
		foreach (CharacterSkinBundle item in config.CharacterSkinBundles.Where((CharacterSkinBundle other) => other.Id != bundle.Id))
		{
			array = new Dictionary<string, string>[2] { item.CardPresetNames, item.MonsterPresetNames };
			foreach (Dictionary<string, string> dictionary2 in array)
			{
				string[] array2 = (from p in dictionary2
					where p.Value == key
					select p.Key).ToArray();
				foreach (string key3 in array2)
				{
					dictionary2[key3] = PresetKey(item);
				}
			}
		}
	}
}
