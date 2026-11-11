using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class SkinPriorityDisplayPolicy
{
	internal const float UncheckedAlpha = 0.55f;

	private const int RankedEnabled = 0;

	private const int RankedDisabled = 1;

	private const int Unranked = 2;

	internal static IReadOnlyList<SkinPriorityState> States<T>(IEnumerable<T> entries, Func<T, string> optionId, Func<T, bool> enabled)
	{
		return entries.Select((T entry) => new SkinPriorityState(optionId(entry), enabled(entry))).ToArray();
	}

	internal static IReadOnlyList<T> Project<T>(IReadOnlyList<T> options, Func<T, string> optionId, IReadOnlyList<SkinPriorityState> entries)
	{
		if (options.Count == 0 || entries.Count == 0)
		{
			return options;
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (T option in options)
		{
			hashSet.Add(optionId(option));
		}
		Dictionary<string, (int Rank, int Order)> rank = new Dictionary<string, (int, int)>(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < entries.Count; i++)
		{
			SkinPriorityState skinPriorityState = entries[i];
			if (!string.IsNullOrWhiteSpace(skinPriorityState.OptionId) && hashSet.Contains(skinPriorityState.OptionId))
			{
				rank.TryAdd(skinPriorityState.OptionId, ((!skinPriorityState.Enabled) ? 1 : 0, i));
			}
		}
		(int, int) value;
		(int, int) value2;
		return (from candidate in options.Select((T option, int index) => (Option: option, Index: index))
			orderby (!rank.TryGetValue(optionId(candidate.Option), out value)) ? 2 : value.Item1, (!rank.TryGetValue(optionId(candidate.Option), out value2)) ? candidate.Index : value2.Item2
			select candidate.Option).ToArray();
	}

	internal static bool IsUnchecked(IReadOnlyList<SkinPriorityState>? entries, string optionId)
	{
		if (entries == null || string.IsNullOrWhiteSpace(optionId))
		{
			return false;
		}
		foreach (SkinPriorityState entry in entries)
		{
			if (entry.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase))
			{
				return !entry.Enabled;
			}
		}
		return false;
	}

	internal static Func<string, bool> UncheckedLookup(IReadOnlyList<SkinPriorityState>? entries)
	{
		if (entries == null || entries.Count == 0)
		{
			return (string _) => false;
		}
		HashSet<string> recorded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> uncheckedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (SkinPriorityState entry in entries)
		{
			if (!string.IsNullOrWhiteSpace(entry.OptionId))
			{
				recorded.Add(entry.OptionId);
				if (!entry.Enabled)
				{
					uncheckedIds.Add(entry.OptionId);
				}
			}
		}
		return (string optionId) => !string.IsNullOrWhiteSpace(optionId) && recorded.Contains(optionId) && uncheckedIds.Contains(optionId);
	}

	internal static float RowAlpha(bool excludedFromRandom, bool uncheckedForCategory)
	{
		return (excludedFromRandom ? 0.5f : 1f) * (uncheckedForCategory ? 0.55f : 1f);
	}
}
