using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Godot;

namespace CznCore.Presentation;

internal sealed class CombatNodeAnchorCache<TOwner> where TOwner : class
{
	private sealed class CachedAnchor
	{
		public int Epoch { get; }

		public Node2D Anchor { get; }

		public CachedAnchor(int epoch, Node2D anchor)
		{
			Epoch = epoch;
			Anchor = anchor;
		}
	}

	private readonly ConditionalWeakTable<TOwner, CachedAnchor> _anchors = new ConditionalWeakTable<TOwner, CachedAnchor>();

	private int _epoch;

	public Node2D? GetOrResolve(TOwner owner, Func<TOwner, Node?> getRoot, Func<TOwner, Node2D?> resolve)
	{
		Node val = getRoot(owner);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			_anchors.Remove(owner);
			return null;
		}
		int num = Volatile.Read(in _epoch);
		if (_anchors.TryGetValue(owner, out CachedAnchor value) && value.Epoch == num && IsAnchorValid(value.Anchor, val))
		{
			return value.Anchor;
		}
		_anchors.Remove(owner);
		Node2D val2 = resolve(owner);
		if (val2 == null || !IsAnchorValid(val2, val))
		{
			return null;
		}
		_anchors.Add(owner, new CachedAnchor(num, val2));
		return val2;
	}

	public void Clear()
	{
		Interlocked.Increment(ref _epoch);
	}

	private static bool IsAnchorValid(Node2D anchor, Node root)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)anchor))
		{
			return false;
		}
		try
		{
			for (Node val = (Node)(object)anchor; val != null; val = val.GetParent())
			{
				if (val == root)
				{
					return true;
				}
			}
		}
		catch
		{
			return false;
		}
		return false;
	}
}
