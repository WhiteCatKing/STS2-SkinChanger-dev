using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class EventSkinPriorityPolicy
{
	public static List<EventSkinPriorityEntry> Entries(IEnumerable<EventSkinPriorityEntry> stored, IEnumerable<string> available)
	{
		List<EventSkinPriorityEntry> list = stored.Where((EventSkinPriorityEntry e) => e != null && !string.IsNullOrWhiteSpace(e.OptionId)).DistinctBy<EventSkinPriorityEntry, string>((EventSkinPriorityEntry e) => e.OptionId, StringComparer.OrdinalIgnoreCase).ToList();
		foreach (string id in available)
		{
			if (!list.Any((EventSkinPriorityEntry e) => e.OptionId.Equals(id, StringComparison.OrdinalIgnoreCase)))
			{
				list.Add(new EventSkinPriorityEntry(id, Enabled: true));
			}
		}
		return list;
	}

	public static string Resolve(IEnumerable<EventSkinPriorityEntry> entries, IEnumerable<string> available)
	{
		string[] source = available.ToArray();
		foreach (EventSkinPriorityEntry entry in entries.Where((EventSkinPriorityEntry e) => e.Enabled))
		{
			string text = source.FirstOrDefault((string id) => id.Equals(entry.OptionId, StringComparison.OrdinalIgnoreCase));
			if (text != null)
			{
				return text;
			}
		}
		return "__base__";
	}
}
