using System;
using System.Collections.Concurrent;

namespace JoanofArcIroncladHpwear;

public static class PortraitNetwork
{
	private static readonly ConcurrentDictionary<ulong, int> Stages = new ConcurrentDictionary<ulong, int>();

	private static object? lobby;

	public static Func<object, int, bool>? Send { get; set; }

	public static void Receive(ulong sender, int stage)
	{
		if (stage >= 0 && stage <= 1)
		{
			Stages[sender] = stage;
		}
	}

	public static int StageFor(ulong sender)
	{
		if (!Stages.TryGetValue(sender, out var value))
		{
			return 0;
		}
		return value;
	}

	public static void Reset()
	{
		Stages.Clear();
		lobby = null;
	}

	internal static void Begin(object current)
	{
		if (lobby != current)
		{
			Stages.Clear();
			lobby = current;
		}
	}
}
