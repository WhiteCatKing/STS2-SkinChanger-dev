using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class VisualSelectionOverlayPolicy
{
	internal static IReadOnlyDictionary<string, string> Merge(IReadOnlyDictionary<string, string> persistentSelections, IReadOnlyDictionary<string, string>? previewSelections, IReadOnlyDictionary<string, string>? scopedSelections)
	{
		if ((previewSelections == null || previewSelections.Count == 0) && (scopedSelections == null || scopedSelections.Count == 0))
		{
			return persistentSelections;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>(persistentSelections, StringComparer.OrdinalIgnoreCase);
		Overlay(dictionary, previewSelections);
		Overlay(dictionary, scopedSelections);
		return dictionary;
	}

	internal static IReadOnlySet<string> AffectedGroups(IEnumerable<string> previousPreviewGroups, IEnumerable<string> nextPreviewGroups)
	{
		return previousPreviewGroups.Concat(nextPreviewGroups).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	private static void Overlay(IDictionary<string, string> target, IReadOnlyDictionary<string, string>? overlay)
	{
		if (overlay == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> item in overlay)
		{
			target[item.Key] = item.Value;
		}
	}
}
