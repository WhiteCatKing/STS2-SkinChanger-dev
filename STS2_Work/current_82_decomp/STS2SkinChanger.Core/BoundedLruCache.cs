using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal sealed class BoundedLruCache<TKey, TValue> where TKey : notnull
{
	private sealed class CacheEntry(TValue value, LinkedListNode<TKey> node)
	{
		public TValue Value { get; set; } = value;

		public LinkedListNode<TKey> Node { get; } = node;
	}

	private readonly int _capacity;

	private readonly Dictionary<TKey, CacheEntry> _entries;

	private readonly LinkedList<TKey> _recency = new LinkedList<TKey>();

	public int Count => _entries.Count;

	public IReadOnlyCollection<TKey> Keys => (IReadOnlyCollection<TKey>)(object)_entries.Keys.ToArray();

	public BoundedLruCache(int capacity, IEqualityComparer<TKey>? comparer = null)
	{
		if (capacity <= 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		_capacity = capacity;
		_entries = new Dictionary<TKey, CacheEntry>(comparer);
	}

	public bool ContainsKey(TKey key)
	{
		return _entries.ContainsKey(key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		if (!_entries.TryGetValue(key, out CacheEntry value2))
		{
			value = default(TValue);
			return false;
		}
		Touch(value2);
		value = value2.Value;
		return true;
	}

	public bool Set(TKey key, TValue value, out KeyValuePair<TKey, TValue> evicted)
	{
		if (_entries.TryGetValue(key, out CacheEntry value2))
		{
			value2.Value = value;
			Touch(value2);
			evicted = default(KeyValuePair<TKey, TValue>);
			return false;
		}
		LinkedListNode<TKey> node = _recency.AddFirst(key);
		_entries.Add(key, new CacheEntry(value, node));
		if (_entries.Count <= _capacity)
		{
			evicted = default(KeyValuePair<TKey, TValue>);
			return false;
		}
		TKey value3 = _recency.Last.Value;
		CacheEntry cacheEntry = _entries[value3];
		_recency.RemoveLast();
		_entries.Remove(value3);
		evicted = new KeyValuePair<TKey, TValue>(value3, cacheEntry.Value);
		return true;
	}

	public bool Remove(TKey key)
	{
		if (!_entries.Remove(key, out CacheEntry value))
		{
			return false;
		}
		_recency.Remove(value.Node);
		return true;
	}

	public void Clear()
	{
		_entries.Clear();
		_recency.Clear();
	}

	private void Touch(CacheEntry entry)
	{
		if (_recency.First != entry.Node)
		{
			_recency.Remove(entry.Node);
			_recency.AddFirst(entry.Node);
		}
	}
}
