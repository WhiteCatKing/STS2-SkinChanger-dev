using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class CharacterGroupEvidencePolicy
{
	public static IReadOnlySet<string> ResolveEligibleGroups(IEnumerable<string> candidateGroupIds, IEnumerable<string> anchoredGroupIds)
	{
		HashSet<string> hashSet = candidateGroupIds.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet2 = anchoredGroupIds.Where(hashSet.Contains).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (hashSet2.Count != 0)
		{
			return hashSet2;
		}
		return hashSet;
	}

	public static bool IsIconOnlySupplement(bool presentsOnlySupplementAssets, bool replacesCharacterVisualsAtRuntime)
	{
		if (presentsOnlySupplementAssets)
		{
			return !replacesCharacterVisualsAtRuntime;
		}
		return false;
	}
}
