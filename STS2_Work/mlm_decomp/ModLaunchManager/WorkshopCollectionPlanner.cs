using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLaunchManager;

internal static class WorkshopCollectionPlanner
{
	internal static IReadOnlyList<LaunchEntry> InstalledMods(LaunchPlan plan, ulong itemId)
	{
		return plan.Entries.Where(delegate(LaunchEntry entry)
		{
			string text = entry.Mod.manifest?.id;
			return text != null && text.Length > 0 && PhysicalModIdentity.WorkshopItemIdFor(entry.Mod) == itemId;
		}).ToList();
	}

	internal static IReadOnlyDictionary<ulong, ulong> Clashes(LaunchPlan plan, IEnumerable<ulong> selected)
	{
		Dictionary<ulong, ulong> dictionary = new Dictionary<ulong, ulong>();
		Dictionary<string, ulong> dictionary2 = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
		foreach (ulong item in selected.Distinct())
		{
			foreach (string item2 in (from entry in InstalledMods(plan, item)
				select entry.Id).Distinct<string>(StringComparer.OrdinalIgnoreCase))
			{
				if (dictionary2.TryGetValue(item2, out var value) && value != item)
				{
					dictionary.TryAdd(item, value);
				}
				else
				{
					dictionary2[item2] = item;
				}
			}
		}
		return dictionary;
	}

	internal static LaunchProfile Build(string name, ulong collectionId, IEnumerable<ulong> selectedInOrder, LaunchPlan plan)
	{
		LaunchProfile launchProfile = new LaunchProfile
		{
			Name = name,
			SourceCollection = collectionId
		};
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (ulong item in selectedInOrder)
		{
			foreach (LaunchEntry item2 in InstalledMods(plan, item))
			{
				if (hashSet.Add(item2.Id))
				{
					launchProfile.Mods.Add(new ProfileMod
					{
						Id = item2.Id,
						Source = $"workshop:{item}",
						Enabled = true
					});
				}
			}
		}
		return launchProfile;
	}

	internal static string SuggestName(IReadOnlyList<LaunchProfile> profiles, ulong collectionId, string title, string fallback)
	{
		LaunchProfile launchProfile = profiles.FirstOrDefault((LaunchProfile profile) => profile.SourceCollection == collectionId);
		if (launchProfile != null)
		{
			return launchProfile.Name;
		}
		string text = title.Trim();
		if (text.Length == 0)
		{
			text = fallback;
		}
		if (text.Length > 48)
		{
			text = text.Substring(0, 48).TrimEnd();
		}
		string name = text;
		int num = 2;
		while (profiles.Any((LaunchProfile profile) => string.Equals(profile.Name, name, StringComparison.OrdinalIgnoreCase)))
		{
			name = $"{text} ({num})";
			num++;
		}
		return name;
	}
}
