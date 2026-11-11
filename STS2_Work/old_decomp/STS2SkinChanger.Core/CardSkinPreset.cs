using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal sealed class CardSkinPreset
{
	public bool AllOriginal { get; set; }

	public string Name { get; set; } = string.Empty;

	public string? CategoryId { get; set; }

	public Dictionary<string, List<CardSkinPriorityEntry>> CardSkinPriorities { get; set; } = new Dictionary<string, List<CardSkinPriorityEntry>>(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, string> Selections { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	internal CardSkinPreset Clone()
	{
		return new CardSkinPreset
		{
			Name = Name,
			CategoryId = CategoryId,
			AllOriginal = AllOriginal,
			CardSkinPriorities = CardSkinPriorities.ToDictionary<KeyValuePair<string, List<CardSkinPriorityEntry>>, string, List<CardSkinPriorityEntry>>((KeyValuePair<string, List<CardSkinPriorityEntry>> p) => p.Key, (KeyValuePair<string, List<CardSkinPriorityEntry>> p) => p.Value.ToList(), StringComparer.OrdinalIgnoreCase),
			Selections = new Dictionary<string, string>(Selections, StringComparer.OrdinalIgnoreCase)
		};
	}
}
