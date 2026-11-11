using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace STS2SkinChanger.Core;

internal static class CardPresetMigrationPolicy
{
	internal static CardPresetMigrationResult Run(SkinConfig config, IReadOnlyDictionary<string, string> categories, IReadOnlyDictionary<string, string> cardGroups)
	{
		if (categories.Count == 0)
		{
			return new CardPresetMigrationResult(0, 0);
		}
		int archived = ((config.CardPresetMigrationRepairVersion < 1) ? ArchiveRecognizableCopies(config, categories) : 0);
		config.CardPresetMigrationRepairVersion = 1;
		CardSkinPreset[] array = config.CardSkinPresets.Where((CardSkinPreset p) => string.IsNullOrWhiteSpace(p.CategoryId)).ToArray();
		List<CardSkinPreset> list = config.CardSkinPresets.Where((CardSkinPreset p) => !string.IsNullOrWhiteSpace(p.CategoryId)).ToList();
		int num = 0;
		CardSkinPreset[] array2 = array;
		foreach (CardSkinPreset cardSkinPreset in array2)
		{
			CardSkinPreset cardSkinPreset2 = cardSkinPreset.Clone();
			foreach (KeyValuePair<string, string> group2 in categories)
			{
				Dictionary<string, string> dictionary = cardSkinPreset.Selections.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> p) => Belongs(p.Key, group2.Key)).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value, StringComparer.OrdinalIgnoreCase);
				cardSkinPreset.CardSkinPriorities.TryGetValue(group2.Key, out List<CardSkinPriorityEntry> value);
				if ((value == null || value.Count == 0) && dictionary.Count == 0)
				{
					cardSkinPreset2.CardSkinPriorities.Remove(group2.Key);
					continue;
				}
				string name = group2.Value + "-" + cardSkinPreset.Name;
				int num2 = 2;
				while (list.Any((CardSkinPreset p) => string.Equals(p.CategoryId, group2.Key, StringComparison.OrdinalIgnoreCase) && p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
				{
					name = group2.Value + "-" + cardSkinPreset.Name + "-" + num2++;
				}
				list.Add(new CardSkinPreset
				{
					Name = name,
					CategoryId = group2.Key,
					CardSkinPriorities = new Dictionary<string, List<CardSkinPriorityEntry>>(StringComparer.OrdinalIgnoreCase) { [group2.Key] = value?.ToList() ?? new List<CardSkinPriorityEntry>() },
					Selections = dictionary,
					AllOriginal = cardSkinPreset.AllOriginal
				});
				cardSkinPreset2.CardSkinPriorities.Remove(group2.Key);
				foreach (string key3 in dictionary.Keys)
				{
					cardSkinPreset2.Selections.Remove(key3);
				}
				if (cardSkinPreset.Name.Equals(config.ActiveCardSkinPreset, StringComparison.OrdinalIgnoreCase))
				{
					config.ActiveCardSkinPresets.TryAdd(group2.Key, name);
				}
				num++;
			}
			if (cardSkinPreset2.CardSkinPriorities.Count > 0 || cardSkinPreset2.Selections.Count > 0)
			{
				list.Add(cardSkinPreset2);
			}
		}
		config.CardSkinPresets = list;
		if (!list.Any((CardSkinPreset p) => string.IsNullOrWhiteSpace(p.CategoryId) && p.Name == config.ActiveCardSkinPreset))
		{
			config.ActiveCardSkinPreset = null;
		}
		return new CardPresetMigrationResult(num, archived);
		bool Belongs(string key, string group)
		{
			if (!key.Equals("cards:" + group, StringComparison.OrdinalIgnoreCase))
			{
				return string.Equals(cardGroups.GetValueOrDefault(key), group, StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
	}

	private static int ArchiveRecognizableCopies(SkinConfig config, IReadOnlyDictionary<string, string> categories)
	{
		CardSkinPreset[] array = (from item in (from preset in config.CardSkinPresets
				select (Preset: preset, Origin: OriginalName(preset)) into item
				where item.Origin != null
				group item by item.Preset.CategoryId.ToLowerInvariant() + "\n" + JsonSerializer.Serialize(item.Preset.CardSkinPriorities.Values.FirstOrDefault() ?? new List<CardSkinPriorityEntry>()) + "\n" + JsonSerializer.Serialize(item.Preset.Selections.OrderBy<KeyValuePair<string, string>, string>((KeyValuePair<string, string> pair) => pair.Key, StringComparer.OrdinalIgnoreCase)) into @group
				where @group.Select(((CardSkinPreset Preset, string Origin) item) => item.Origin).Distinct<string>(StringComparer.OrdinalIgnoreCase).Count() >= 2
				select @group).SelectMany((IGrouping<string, (CardSkinPreset Preset, string Origin)> @group) => @group)
			select item.Preset).ToArray();
		CardSkinPreset[] array2 = array;
		foreach (CardSkinPreset cardSkinPreset in array2)
		{
			config.ArchivedLegacyCardSkinPresets.Add(cardSkinPreset.Clone());
			config.CardSkinPresets.Remove(cardSkinPreset);
			if (string.Equals(config.ActiveCardSkinPresets.GetValueOrDefault(cardSkinPreset.CategoryId), cardSkinPreset.Name, StringComparison.OrdinalIgnoreCase))
			{
				config.ActiveCardSkinPresets.Remove(cardSkinPreset.CategoryId);
			}
			foreach (CharacterSkinBundle characterSkinBundle in config.CharacterSkinBundles)
			{
				CharacterSkinBundlePolicy.RemovePresetReference(characterSkinBundle, monsterPreset: false, cardSkinPreset.CategoryId, cardSkinPreset.Name);
			}
		}
		return array.Length;
		string? OriginalName(CardSkinPreset preset)
		{
			if (preset.CategoryId == null || preset.AllOriginal || BundlePresetPolicy.IsOwned(preset.Name) || config.CharacterSkinBundles.Any((CharacterSkinBundle bundle) => string.Equals(bundle.CardPresetNames.GetValueOrDefault(preset.CategoryId), preset.Name, StringComparison.OrdinalIgnoreCase)) || preset.Selections.Keys.Any((string key) => !key.Equals("cards:" + preset.CategoryId, StringComparison.OrdinalIgnoreCase)) || preset.CardSkinPriorities.Keys.Any((string key) => !key.Equals(preset.CategoryId, StringComparison.OrdinalIgnoreCase)))
			{
				return null;
			}
			string[] array3 = new string[2]
			{
				preset.CategoryId,
				categories.GetValueOrDefault(preset.CategoryId)
			};
			foreach (string text in array3)
			{
				if (!string.IsNullOrEmpty(text) && preset.Name.StartsWith(text + "-", StringComparison.OrdinalIgnoreCase))
				{
					string name = preset.Name;
					int num = text.Length + 1;
					string origin = name.Substring(num, name.Length - num);
					if (origin.Length > 0 && config.CardSkinPresets.Any((CardSkinPreset other) => !string.Equals(other.CategoryId, preset.CategoryId, StringComparison.OrdinalIgnoreCase) && other.Name.Equals(origin, StringComparison.OrdinalIgnoreCase)))
					{
						return origin;
					}
				}
			}
			return null;
		}
	}
}
