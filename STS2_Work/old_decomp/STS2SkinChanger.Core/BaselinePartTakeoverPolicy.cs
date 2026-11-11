using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class BaselinePartTakeoverPolicy
{
	public static IReadOnlyList<string> ResolveServablePartPaths(IEnumerable<string> candidatePaths, Func<string, bool> belongsToKnownGroup, Func<string, bool> isGroupAnchor, Func<string, bool> existsInBaseline)
	{
		ArgumentNullException.ThrowIfNull(candidatePaths, "candidatePaths");
		List<string> list = new List<string>();
		foreach (string candidatePath in candidatePaths)
		{
			if (belongsToKnownGroup(candidatePath) && !isGroupAnchor(candidatePath) && existsInBaseline(candidatePath))
			{
				list.Add(candidatePath);
			}
		}
		return list;
	}

	public static string? ResolveAdoptedDefault(IReadOnlyList<string> partOptionIds)
	{
		ArgumentNullException.ThrowIfNull(partOptionIds, "partOptionIds");
		if (partOptionIds.Count != 0)
		{
			return partOptionIds.Order<string>(StringComparer.OrdinalIgnoreCase).First();
		}
		return null;
	}
}
