using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLaunchManager;

internal sealed class WorkshopCollectionReader
{
	internal const int MaxDepth = 5;

	internal const int MaxItems = 500;

	private readonly WorkshopItemFetch _fetch;

	private readonly Dictionary<ulong, WorkshopItemFacts> _facts = new Dictionary<ulong, WorkshopItemFacts>();

	private Action<CollectionReadResult>? _done;

	private ulong _rootId;

	internal bool Running => _done != null;

	internal WorkshopCollectionReader(WorkshopItemFetch fetch)
	{
		_fetch = fetch;
	}

	internal void Start(ulong collectionId, Action<CollectionReadResult> done)
	{
		_facts.Clear();
		_rootId = collectionId;
		_done = done;
		Fetch(new _003C_003Ez__ReadOnlySingleElementList<ulong>(collectionId), delegate
		{
			CollectionReadError collectionReadError = Facts(collectionId).Kind switch
			{
				WorkshopItemKind.Collection => CollectionReadError.None, 
				WorkshopItemKind.Item => CollectionReadError.SingleItem, 
				WorkshopItemKind.OtherGame => CollectionReadError.OtherGame, 
				WorkshopItemKind.NotAMod => CollectionReadError.NotACollection, 
				_ => CollectionReadError.NotFound, 
			};
			if (collectionReadError != 0)
			{
				Finish(new CollectionReadResult(null, collectionReadError));
			}
			else
			{
				Expand(new Dictionary<ulong, int> { [collectionId] = 0 });
			}
		});
	}

	internal void Cancel()
	{
		_done = null;
	}

	private void Expand(Dictionary<ulong, int> opened)
	{
		Dictionary<ulong, int> depth = new Dictionary<ulong, int>();
		foreach (var (id2, num3) in opened)
		{
			foreach (ulong child in Facts(id2).Children)
			{
				if (!_facts.ContainsKey(child) && !depth.ContainsKey(child))
				{
					depth[child] = num3 + 1;
				}
			}
		}
		foreach (WorkshopItemFacts item in _facts.Values.Where((WorkshopItemFacts item) => item.Kind == WorkshopItemKind.Item))
		{
			foreach (ulong child2 in item.Children)
			{
				if (!_facts.ContainsKey(child2))
				{
					depth.TryAdd(child2, int.MaxValue);
				}
			}
		}
		if (depth.Count == 0)
		{
			Finish(Snapshot());
			return;
		}
		if (_facts.Count - 1 + depth.Count > 500 || depth.Any((KeyValuePair<ulong, int> pair) => pair.Value != int.MaxValue && pair.Value > 5))
		{
			Finish(new CollectionReadResult(null, CollectionReadError.TooLarge));
			return;
		}
		List<ulong> ids = depth.Keys.ToList();
		Fetch(ids, delegate
		{
			Expand(ids.Where((ulong id) => depth[id] != int.MaxValue && Facts(id).Kind == WorkshopItemKind.Collection).ToDictionary((ulong id) => id, (ulong id) => depth[id]));
		});
	}

	private void Fetch(IReadOnlyList<ulong> ids, Action next)
	{
		Action<CollectionReadResult> owner = _done;
		_fetch(ids, delegate(IReadOnlyList<WorkshopItemFacts>? results)
		{
			if (_done != null && (object)_done == owner)
			{
				if (results == null)
				{
					Finish(new CollectionReadResult(null, CollectionReadError.Failed));
				}
				else
				{
					foreach (WorkshopItemFacts item in results.Where((WorkshopItemFacts facts) => ids.Contains(facts.Id)))
					{
						_facts[item.Id] = item;
					}
					foreach (ulong item2 in ids.Where((ulong id) => !_facts.ContainsKey(id)))
					{
						_facts[item2] = new WorkshopItemFacts(item2, WorkshopItemKind.Unavailable, string.Empty, Array.Empty<ulong>());
					}
					next();
				}
			}
		});
	}

	private WorkshopItemFacts Facts(ulong id)
	{
		if (!_facts.TryGetValue(id, out WorkshopItemFacts value))
		{
			return new WorkshopItemFacts(id, WorkshopItemKind.Unavailable, string.Empty, Array.Empty<ulong>());
		}
		return value;
	}

	private CollectionReadResult Snapshot()
	{
		List<CollectionMember> listed = new List<CollectionMember>();
		HashSet<ulong> seen = new HashSet<ulong>();
		HashSet<ulong> path = new HashSet<ulong> { _rootId };
		Walk(Facts(_rootId), null, 0uL);
		List<CollectionMember> ordered = new List<CollectionMember>();
		HashSet<ulong> placed = new HashSet<ulong>();
		foreach (CollectionMember item in listed)
		{
			placed.Add(item.ItemId);
		}
		foreach (CollectionMember item2 in listed)
		{
			Place(item2);
		}
		if (ordered.Count == 0)
		{
			return new CollectionReadResult(null, CollectionReadError.Empty);
		}
		if (ordered.Count <= 500)
		{
			return new CollectionReadResult(new CollectionSnapshot(_rootId, Facts(_rootId).Title, ordered), CollectionReadError.None);
		}
		return new CollectionReadResult(null, CollectionReadError.TooLarge);
		void Place(CollectionMember member)
		{
			if (member.Kind == WorkshopItemKind.Item)
			{
				foreach (ulong child in Facts(member.ItemId).Children)
				{
					if (placed.Add(child))
					{
						WorkshopItemFacts workshopItemFacts2 = Facts(child);
						WorkshopItemKind kind = ((workshopItemFacts2.Kind == WorkshopItemKind.Collection) ? WorkshopItemKind.NotAMod : workshopItemFacts2.Kind);
						Place(new CollectionMember(child, workshopItemFacts2.Title, kind, CollectionMemberRole.Required, member.Title, member.ItemId));
					}
				}
			}
			ordered.Add(member);
		}
		void Walk(WorkshopItemFacts collection, string? via, ulong viaId)
		{
			foreach (ulong child2 in collection.Children)
			{
				WorkshopItemFacts workshopItemFacts = Facts(child2);
				if (workshopItemFacts.Kind == WorkshopItemKind.Collection)
				{
					if (path.Add(child2))
					{
						Walk(workshopItemFacts, workshopItemFacts.Title, workshopItemFacts.Id);
						path.Remove(child2);
					}
				}
				else if (seen.Add(child2))
				{
					listed.Add(new CollectionMember(child2, workshopItemFacts.Title, workshopItemFacts.Kind, CollectionMemberRole.Listed, via, viaId));
				}
			}
		}
	}

	private void Finish(CollectionReadResult result)
	{
		Action<CollectionReadResult>? done = _done;
		_done = null;
		done?.Invoke(result);
	}
}
