using System;
using System.Collections.Concurrent;

namespace JoanofArcIroncladHpwear;

public static class CardPlayAnimTracker
{
	private const int MaxPending = 64;

	private static readonly ConcurrentDictionary<object, bool> SawByPlay = new ConcurrentDictionary<object, bool>();

	private static object? _current;

	internal static int PendingCount => SawByPlay.Count;

	public static void Begin(object play)
	{
		ArgumentNullException.ThrowIfNull(play, "play");
		_current = play;
		SawByPlay[play] = false;
		if (SawByPlay.Count > 64)
		{
			Clear();
		}
	}

	public static void NotePlayerAnim()
	{
		object current = _current;
		if (current != null)
		{
			SawByPlay[current] = true;
		}
	}

	public static bool NeedsCast(object? play)
	{
		if (play != null && SawByPlay.TryGetValue(play, out var value))
		{
			return !value;
		}
		return false;
	}

	public static bool ShouldInjectCast(object? play)
	{
		if (play == null || !SawByPlay.TryRemove(play, out var value))
		{
			return false;
		}
		if (_current == play)
		{
			_current = null;
		}
		return !value;
	}

	public static void Clear()
	{
		SawByPlay.Clear();
		_current = null;
	}
}
