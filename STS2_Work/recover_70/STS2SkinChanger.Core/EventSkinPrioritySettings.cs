using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal sealed class EventSkinPrioritySettings
{
	public Dictionary<string, List<string>> RegionGroups { get; set; } = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, List<EventSkinPriorityEntry>> Priorities { get; set; } = new Dictionary<string, List<EventSkinPriorityEntry>>(StringComparer.OrdinalIgnoreCase);

	public List<string> ManualGroups { get; set; } = new List<string>();

	public List<string> KnownGroups { get; set; } = new List<string>();

	public bool IsFollowing(string groupId)
	{
		if (RegionGroups.Values.Any((List<string> groups) => groups.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase)))
		{
			return !ManualGroups.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase);
		}
		return false;
	}

	public EventSkinPrioritySettings Clone()
	{
		return new EventSkinPrioritySettings
		{
			RegionGroups = RegionGroups.ToDictionary<KeyValuePair<string, List<string>>, string, List<string>>((KeyValuePair<string, List<string>> p) => p.Key, (KeyValuePair<string, List<string>> p) => p.Value.ToList(), StringComparer.OrdinalIgnoreCase),
			Priorities = Priorities.ToDictionary<KeyValuePair<string, List<EventSkinPriorityEntry>>, string, List<EventSkinPriorityEntry>>((KeyValuePair<string, List<EventSkinPriorityEntry>> p) => p.Key, (KeyValuePair<string, List<EventSkinPriorityEntry>> p) => p.Value.ToList(), StringComparer.OrdinalIgnoreCase),
			ManualGroups = ManualGroups.ToList(),
			KnownGroups = KnownGroups.ToList()
		};
	}

	public void Normalize(Func<string, bool>? isManagedGroup = null)
	{
		if (isManagedGroup == null)
		{
			isManagedGroup = EventSkinPolicy.IsEventGroup;
		}
		RegionGroups = (RegionGroups ?? new Dictionary<string, List<string>>()).Where<KeyValuePair<string, List<string>>>((KeyValuePair<string, List<string>> p) => !string.IsNullOrWhiteSpace(p.Key)).ToDictionary<KeyValuePair<string, List<string>>, string, List<string>>((KeyValuePair<string, List<string>> p) => p.Key, (KeyValuePair<string, List<string>> p) => (p.Value ?? new List<string>()).Where((string id) => !string.IsNullOrWhiteSpace(id) && isManagedGroup(id)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);
		Priorities = (Priorities ?? new Dictionary<string, List<EventSkinPriorityEntry>>()).Where<KeyValuePair<string, List<EventSkinPriorityEntry>>>((KeyValuePair<string, List<EventSkinPriorityEntry>> p) => !string.IsNullOrWhiteSpace(p.Key)).ToDictionary<KeyValuePair<string, List<EventSkinPriorityEntry>>, string, List<EventSkinPriorityEntry>>((KeyValuePair<string, List<EventSkinPriorityEntry>> p) => p.Key, (KeyValuePair<string, List<EventSkinPriorityEntry>> p) => EventSkinPriorityPolicy.Entries(p.Value ?? new List<EventSkinPriorityEntry>(), Array.Empty<string>()), StringComparer.OrdinalIgnoreCase);
		ManualGroups = (ManualGroups ?? new List<string>()).Where((string id) => !string.IsNullOrWhiteSpace(id) && isManagedGroup(id)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
		KnownGroups = (KnownGroups ?? new List<string>()).Where((string id) => !string.IsNullOrWhiteSpace(id) && isManagedGroup(id)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
	}

	public void Register(IReadOnlyDictionary<string, List<string>> regions, IReadOnlyDictionary<string, string> selections)
	{
		RegionGroups = regions.ToDictionary<KeyValuePair<string, List<string>>, string, List<string>>((KeyValuePair<string, List<string>> p) => p.Key, (KeyValuePair<string, List<string>> p) => p.Value.ToList(), StringComparer.OrdinalIgnoreCase);
		foreach (string item in regions.Values.SelectMany((List<string> ids) => ids).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			if (!KnownGroups.Contains<string>(item, StringComparer.OrdinalIgnoreCase))
			{
				if (selections.TryGetValue(item, out string value) && value != "__base__" && !ManualGroups.Contains<string>(item, StringComparer.OrdinalIgnoreCase))
				{
					ManualGroups.Add(item);
				}
				KnownGroups.Add(item);
			}
		}
	}
}
