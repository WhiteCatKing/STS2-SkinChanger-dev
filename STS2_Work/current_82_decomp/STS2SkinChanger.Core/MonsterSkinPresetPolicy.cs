using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class MonsterSkinPresetPolicy
{
	internal static MonsterSkinPresetSnapshot Capture(string categoryId, IReadOnlyCollection<string> categoryGroupIds, IReadOnlyList<MonsterSkinPresetPriorityState> priority, IReadOnlyDictionary<string, string> selections)
	{
		HashSet<string> scopedIds = categoryGroupIds.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		return new MonsterSkinPresetSnapshot(categoryId, priority.ToArray(), selections.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => scopedIds.Contains(pair.Key)).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key, (KeyValuePair<string, string> pair) => pair.Value, StringComparer.OrdinalIgnoreCase));
	}

	internal static Dictionary<string, string> Apply(MonsterSkinPresetSnapshot preset, IReadOnlyDictionary<string, string> currentSelections)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(currentSelections, StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, string> selection in preset.Selections)
		{
			dictionary[selection.Key] = selection.Value;
		}
		return dictionary;
	}
}
