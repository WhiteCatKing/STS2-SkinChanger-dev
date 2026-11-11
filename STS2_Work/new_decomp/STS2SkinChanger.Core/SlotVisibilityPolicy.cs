using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class SlotVisibilityPolicy
{
	private static readonly string[] RavenSelectSlots = new string[11]
	{
		"tougu mianju 0", "tougu mianju 1", "tougu mianju 2", "mianju yanjing 0", "mianju yanjing 6", "mianju yanjing 2", "mianju yanjing 3", "mianju yanjing 4", "mianju yanjing 5", "mianju yanjing 1",
		"mianju yinying 0"
	};

	private static readonly string[] RavenCombatSlots = new string[4] { "mianjv", "gumian", "ATK mianju yanjing", "dujiaoshou_27" };

	private static readonly string[] RavenRestSlots = new string[2] { "mianju", "mianju  yanjing" };

	public static string[] ResolveSlots(IEnumerable<string> sourceSlots, IEnumerable<string> availableSlots)
	{
		HashSet<string> hashSet = sourceSlots.ToHashSet<string>(StringComparer.Ordinal);
		HashSet<string> hashSet2 = availableSlots.ToHashSet<string>(StringComparer.Ordinal);
		if (hashSet.Count == 0)
		{
			return Array.Empty<string>();
		}
		if (hashSet.IsSubsetOf(hashSet2))
		{
			return hashSet.ToArray();
		}
		if (!hashSet.SetEquals(RavenSelectSlots))
		{
			return Array.Empty<string>();
		}
		if (RavenCombatSlots.All(hashSet2.Contains) && hashSet2.Contains("head21") && hashSet2.Contains("ATK tou"))
		{
			return RavenCombatSlots.ToArray();
		}
		if (RavenRestSlots.All(hashSet2.Contains) && hashSet2.Contains("head") && hashSet2.Contains("kouzhao"))
		{
			return RavenRestSlots.ToArray();
		}
		return Array.Empty<string>();
	}

	public static SlotVisibilitySelection[] GetSelections(SkinConfig config, string groupId, string providerId)
	{
		return config.SlotVisibilitySelections.Where((SlotVisibilitySelection state) => state.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase) && state.ProviderId.Equals(providerId, StringComparison.OrdinalIgnoreCase)).ToArray();
	}

	public static string[] GetHiddenSourceSlots(SkinConfig config, string groupId, string providerId)
	{
		return (from state in GetSelections(config, groupId, providerId)
			where state.Hidden
			select state).SelectMany((SlotVisibilitySelection state) => state.SourceSlots).Distinct<string>(StringComparer.Ordinal).ToArray();
	}

	public static List<SlotVisibilitySelection> Normalize(IEnumerable<SlotVisibilitySelection>? selections)
	{
		return (from state in (selections ?? Array.Empty<SlotVisibilitySelection>()).Where(delegate(SlotVisibilitySelection state)
			{
				if (state != null && !string.IsNullOrWhiteSpace(state.GroupId) && !string.IsNullOrWhiteSpace(state.ProviderId) && !string.IsNullOrWhiteSpace(state.ToggleId))
				{
					string[] sourceSlots = state.SourceSlots;
					if (sourceSlots != null)
					{
						int num = sourceSlots.Length;
						if (num > 0)
						{
							return num <= 256;
						}
					}
					return false;
				}
				return false;
			})
			select state with
			{
				SourceSlots = state.SourceSlots.Where((string slot) => !string.IsNullOrWhiteSpace(slot)).Distinct<string>(StringComparer.Ordinal).ToArray()
			} into state
			where state.SourceSlots.Length != 0
			select state).DistinctBy((SlotVisibilitySelection state) => (state.GroupId.ToLowerInvariant(), state.ProviderId.ToLowerInvariant(), ToggleId: state.ToggleId)).ToList();
	}
}
