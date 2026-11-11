using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class RuntimeResourceRetentionPolicy
{
	public static IReadOnlySet<string> SelectTransientCombatGroups(IEnumerable<string> visibleGroupIds, IEnumerable<string> persistentGroupIds)
	{
		HashSet<string> persistent = persistentGroupIds.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		return visibleGroupIds.Where((string groupId) => !persistent.Contains(groupId)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}
}
