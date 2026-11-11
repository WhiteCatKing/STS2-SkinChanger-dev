using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLaunchManager;

internal static class FriendModComparisonLogic
{
	private readonly record struct Selection<T>(FriendLocalCandidate<T>? Copy, bool? Enabled);

	internal static FriendComparisonResult<T> Compare<T>(IReadOnlyList<FriendModEntry> remote, IReadOnlyList<FriendLocalCandidate<T>> local)
	{
		List<FriendLocalCandidate<T>> list = local.Where((FriendLocalCandidate<T> entry) => entry.Enabled).ToList();
		Dictionary<string, List<FriendLocalCandidate<T>>> copiesById = local.Where((FriendLocalCandidate<T> entry) => !string.IsNullOrWhiteSpace(entry.Id)).GroupBy<FriendLocalCandidate<T>, string>((FriendLocalCandidate<T> entry) => entry.Id, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, FriendLocalCandidate<T>>, string, List<FriendLocalCandidate<T>>>((IGrouping<string, FriendLocalCandidate<T>> group) => group.Key, (IGrouping<string, FriendLocalCandidate<T>> group) => group.ToList(), StringComparer.OrdinalIgnoreCase);
		Dictionary<string, Selection<T>> selectedById = remote.Where((FriendModEntry mod) => copiesById.ContainsKey(mod.Id)).ToDictionary<FriendModEntry, string, Selection<T>>((FriendModEntry mod) => mod.Id, (FriendModEntry mod) => SelectLocalCopy(copiesById[mod.Id], mod), StringComparer.OrdinalIgnoreCase);
		List<string> list2 = remote.Select((FriendModEntry mod) => mod.Id).ToList();
		List<string> list3 = list.Select((FriendLocalCandidate<T> entry) => entry.Id).ToList();
		bool exactOrder = list2.SequenceEqual<string>(list3, StringComparer.OrdinalIgnoreCase);
		Selection<T> value;
		HashSet<string> hashSet = (from mod in remote
			where selectedById.TryGetValue(mod.Id, out value) && value.Enabled == true
			select mod.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		bool flag = selectedById.Values.All((Selection<T> selection) => selection.Enabled.HasValue);
		List<string> list4 = list2.Where(hashSet.Contains).ToList();
		List<string> list5 = list3.Where(hashSet.Contains).ToList();
		List<FriendComparisonDecision<T>> list6 = new List<FriendComparisonDecision<T>>(remote.Count);
		foreach (FriendModEntry mod2 in remote)
		{
			Selection<T> value2;
			bool num = selectedById.TryGetValue(mod2.Id, out value2);
			FriendLocalCandidate<T> entry2 = value2.Copy;
			FriendLocalCandidate<T> friendLocalCandidate = entry2;
			int localPosition = (((object)friendLocalCandidate != null && friendLocalCandidate.Enabled) ? (list.FindIndex((FriendLocalCandidate<T> candidate) => (object)candidate == entry2) + 1) : 0);
			FriendModMatchState state;
			if (!num)
			{
				state = FriendModMatchState.Missing;
			}
			else if (entry2 == null)
			{
				state = FriendModMatchState.Checking;
			}
			else if (!string.IsNullOrWhiteSpace(mod2.Version) && !string.Equals(entry2.Version, mod2.Version, StringComparison.OrdinalIgnoreCase))
			{
				state = FriendModMatchState.VersionMismatch;
			}
			else
			{
				bool? flag2 = entry2.ContentMatches(mod2);
				if (!flag2.HasValue)
				{
					state = FriendModMatchState.Checking;
				}
				else if (flag2 != true)
				{
					state = FriendModMatchState.ContentMismatch;
				}
				else if (!entry2.Enabled)
				{
					state = FriendModMatchState.Disabled;
				}
				else if (!flag)
				{
					state = FriendModMatchState.Checking;
				}
				else
				{
					int num2 = list4.FindIndex((string id) => string.Equals(id, mod2.Id, StringComparison.OrdinalIgnoreCase));
					int num3 = list5.FindIndex((string id) => string.Equals(id, mod2.Id, StringComparison.OrdinalIgnoreCase));
					state = ((num2 != num3) ? FriendModMatchState.OrderMismatch : FriendModMatchState.Match);
				}
			}
			list6.Add(new FriendComparisonDecision<T>(mod2, entry2, state, localPosition));
		}
		HashSet<string> remoteIds = remote.Select((FriendModEntry mod) => mod.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		return new FriendComparisonResult<T>
		{
			Rows = list6,
			ExtraEnabled = list.Where((FriendLocalCandidate<T> entry) => !remoteIds.Contains(entry.Id)).ToList(),
			ExactOrder = exactOrder
		};
	}

	private static Selection<T> SelectLocalCopy<T>(IReadOnlyList<FriendLocalCandidate<T>> copies, FriendModEntry remote)
	{
		if (remote.WorkshopId != 0)
		{
			FriendLocalCandidate<T> friendLocalCandidate = copies.FirstOrDefault((FriendLocalCandidate<T> entry) => entry.WorkshopId == remote.WorkshopId);
			if ((object)friendLocalCandidate != null)
			{
				return new Selection<T>(friendLocalCandidate, friendLocalCandidate.Enabled);
			}
		}
		if (copies.Count == 1)
		{
			return new Selection<T>(copies[0], copies[0].Enabled);
		}
		List<FriendLocalCandidate<T>> list = new List<FriendLocalCandidate<T>>();
		bool flag = false;
		foreach (FriendLocalCandidate<T> item in copies.Where((FriendLocalCandidate<T> entry) => entry.Enabled).Concat(copies.Where((FriendLocalCandidate<T> entry) => !entry.Enabled)))
		{
			bool? flag2 = item.ContentMatches(remote);
			if (flag2 != false)
			{
				list.Add(item);
				if (flag2 == true)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			list.Add(copies.FirstOrDefault((FriendLocalCandidate<T> entry) => entry.Enabled) ?? copies[0]);
		}
		List<FriendLocalCandidate<T>> list2 = ((IEnumerable<FriendLocalCandidate<T>>)list).Distinct((IEqualityComparer<FriendLocalCandidate<T>>?)ReferenceEqualityComparer.Instance).ToList();
		if (list2.Count == 1)
		{
			return new Selection<T>(list2[0], list2[0].Enabled);
		}
		bool? enabled = (list2.All((FriendLocalCandidate<T> entry) => entry.Enabled) ? new bool?(true) : (list2.All((FriendLocalCandidate<T> entry) => !entry.Enabled) ? new bool?(false) : ((bool?)null)));
		return new Selection<T>(null, enabled);
	}
}
