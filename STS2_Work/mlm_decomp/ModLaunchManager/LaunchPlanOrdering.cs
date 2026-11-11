using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLaunchManager;

internal static class LaunchPlanOrdering
{
	internal static void StablePartitionEnabledFirst<T>(List<T> entries, Func<T, bool> isEnabled)
	{
		List<T> collection = entries.Where(isEnabled).Concat(entries.Where((T entry) => !isEnabled(entry))).ToList();
		entries.Clear();
		entries.AddRange(collection);
	}

	internal static IOrderedEnumerable<T> OrderForExplicitOrganization<T>(IEnumerable<T> candidates, Func<T, bool> isEnabled, Func<T, int> category, Func<T, string> group, Func<T, string> name, Func<T, string> id, Func<T, int> currentRank)
	{
		return candidates.OrderBy((T candidate) => (!isEnabled(candidate)) ? 1 : 0).ThenBy(category).ThenBy((T candidate) => string.IsNullOrWhiteSpace(group(candidate)))
			.ThenBy<T, string>(group, StringComparer.OrdinalIgnoreCase)
			.ThenBy<T, string>(name, StringComparer.OrdinalIgnoreCase)
			.ThenBy<T, string>(id, StringComparer.OrdinalIgnoreCase)
			.ThenBy(currentRank);
	}
}
