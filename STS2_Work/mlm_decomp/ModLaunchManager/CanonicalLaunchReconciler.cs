using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLaunchManager;

internal static class CanonicalLaunchReconciler
{
	internal static CanonicalReconciliation Reconcile(IReadOnlyList<CanonicalLaunchEntry> canonical, IReadOnlyList<CanonicalLiveDescriptor> live, bool keepDisabledInPlace = false)
	{
		CanonicalReconciliation canonicalReconciliation = new CanonicalReconciliation();
		Dictionary<int, int> bindings = new Dictionary<int, int>();
		HashSet<int> boundLive = new HashSet<int>();
		List<int> list = (from index in Enumerable.Range(0, canonical.Count)
			where IsUsable(canonical[index])
			select index).ToList();
		foreach (int item in list)
		{
			CanonicalLaunchEntry persisted = canonical[item];
			List<int> list2 = (from pair in live.Select((CanonicalLiveDescriptor descriptor, int index) => (descriptor: descriptor, index: index))
				where !boundLive.Contains(pair.index) && ExactMatch(persisted, pair.descriptor)
				select pair.index).ToList();
			if (list2.Count == 1)
			{
				int num = list2[0];
				bindings[item] = num;
				boundLive.Add(num);
				if (HasWorkshopIdentity(persisted.WorkshopItemId) && HasWorkshopIdentity(live[num].WorkshopItemId))
				{
					canonicalReconciliation.WorkshopExact++;
				}
				else
				{
					canonicalReconciliation.PathExact++;
				}
			}
		}
		List<int> source = list.Where((int index) => !bindings.ContainsKey(index)).ToList();
		List<int> source2 = (from index in Enumerable.Range(0, live.Count)
			where !boundLive.Contains(index)
			select index).ToList();
		foreach (string group in source.Select((int index) => GroupKey(canonical[index].Source, canonical[index].Id)).Concat(source2.Select((int index) => GroupKey(live[index].Source, live[index].Id))).ToHashSet<string>(StringComparer.Ordinal))
		{
			List<int> list3 = source.Where((int index) => string.Equals(GroupKey(canonical[index].Source, canonical[index].Id), group, StringComparison.Ordinal)).ToList();
			List<int> list4 = source2.Where((int index) => string.Equals(GroupKey(live[index].Source, live[index].Id), group, StringComparison.Ordinal)).ToList();
			if (list3.Count != 0 && list4.Count != 0)
			{
				if (list3.Count == 1 && list4.Count == 1 && !ConflictingWorkshopIds(canonical[list3[0]], live[list4[0]]))
				{
					bindings[list3[0]] = list4[0];
					boundLive.Add(list4[0]);
					canonicalReconciliation.MovedFallback++;
				}
				else
				{
					canonicalReconciliation.AmbiguousGroups++;
				}
			}
		}
		foreach (int item2 in list)
		{
			if (bindings.TryGetValue(item2, out var value))
			{
				canonicalReconciliation.Seeds.Add(new CanonicalSeedDescriptor(live[value].Index, canonical[item2].Enabled));
			}
			else
			{
				canonicalReconciliation.Dormant.Add(canonical[item2].Clone());
			}
		}
		foreach (CanonicalLiveDescriptor item3 in live.Where((CanonicalLiveDescriptor _, int index) => !boundLive.Contains(index)))
		{
			canonicalReconciliation.Seeds.Add(new CanonicalSeedDescriptor(item3.Index, item3.Enabled));
			canonicalReconciliation.NewCount++;
		}
		if (!keepDisabledInPlace)
		{
			LaunchPlanOrdering.StablePartitionEnabledFirst(canonicalReconciliation.Seeds, (CanonicalSeedDescriptor seed) => seed.Enabled);
		}
		return canonicalReconciliation;
	}

	internal static List<CanonicalLaunchEntry> MergeDormant(IReadOnlyList<CanonicalLaunchEntry> dormant, IReadOnlyList<CanonicalLaunchEntry> departed)
	{
		List<CanonicalLiveDescriptor> live = departed.Select((CanonicalLaunchEntry row, int index) => new CanonicalLiveDescriptor(index, row.Source, row.Id, row.Path, row.WorkshopItemId, row.Enabled)).ToList();
		List<CanonicalLaunchEntry> dormant2 = Reconcile(dormant, live).Dormant;
		dormant2.AddRange(departed.Select((CanonicalLaunchEntry row) => row.Clone()));
		return dormant2;
	}

	internal static bool ExactMatch(CanonicalLaunchEntry persisted, CanonicalLiveDescriptor live)
	{
		if (!string.Equals(persisted.Source, live.Source, StringComparison.OrdinalIgnoreCase) || !string.Equals(persisted.Id, live.Id, StringComparison.Ordinal))
		{
			return false;
		}
		bool num = HasWorkshopIdentity(persisted.WorkshopItemId);
		bool flag = HasWorkshopIdentity(live.WorkshopItemId);
		if (num && flag)
		{
			return string.Equals(persisted.WorkshopItemId, live.WorkshopItemId, StringComparison.Ordinal);
		}
		if (persisted.Path.Length > 0 && live.Path.Length > 0)
		{
			return string.Equals(persisted.Path, live.Path, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		}
		return false;
	}

	private static bool IsUsable(CanonicalLaunchEntry entry)
	{
		if (!string.IsNullOrWhiteSpace(entry.Source) && !string.IsNullOrWhiteSpace(entry.Id))
		{
			if (string.IsNullOrWhiteSpace(entry.Path))
			{
				return HasWorkshopIdentity(entry.WorkshopItemId);
			}
			return true;
		}
		return false;
	}

	private static bool ConflictingWorkshopIds(CanonicalLaunchEntry persisted, CanonicalLiveDescriptor live)
	{
		if (HasWorkshopIdentity(persisted.WorkshopItemId) && HasWorkshopIdentity(live.WorkshopItemId))
		{
			return !string.Equals(persisted.WorkshopItemId, live.WorkshopItemId, StringComparison.Ordinal);
		}
		return false;
	}

	private static bool HasWorkshopIdentity(string? value)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			return value != "0";
		}
		return false;
	}

	private static string GroupKey(string source, string id)
	{
		return source.ToUpperInvariant() + "\n" + id;
	}
}
