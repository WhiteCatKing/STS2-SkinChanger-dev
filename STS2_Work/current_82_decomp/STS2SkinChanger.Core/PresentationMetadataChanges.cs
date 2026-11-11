using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class PresentationMetadataChanges
{
	internal static PresentationMetadataChange<T>[] Capture<T>(IReadOnlyDictionary<string, T> before, IReadOnlyDictionary<string, T> after)
	{
		T value;
		T value2;
		return (from key in before.Keys.Concat(after.Keys).Distinct<string>(StringComparer.Ordinal)
			select new PresentationMetadataChange<T>(key, before.TryGetValue(key, out value), value, after.TryGetValue(key, out value2), value2) into change
			where change.HadOriginal != change.HasApplied || !EqualityComparer<T>.Default.Equals(change.Original, change.Applied)
			select change).ToArray();
	}

	internal static bool CanRestore<T>(PresentationMetadataChange<T> change, bool hasCurrent, T? current)
	{
		if (change.HasApplied == hasCurrent)
		{
			if (hasCurrent)
			{
				return EqualityComparer<T>.Default.Equals(current, change.Applied);
			}
			return true;
		}
		return false;
	}
}
