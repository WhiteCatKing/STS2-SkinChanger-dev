using System;
using System.Collections.Generic;

namespace JoanofArcIroncladHpwear;

internal sealed class EnemyTurnGate<T> where T : class
{
	private T? last;

	private bool finished;

	private readonly HashSet<T> lowAtStart = new HashSet<T>();

	private readonly HashSet<T> released = new HashSet<T>();

	internal void Begin(T? lastAttacker, IEnumerable<T> alreadyLow)
	{
		last = lastAttacker;
		finished = lastAttacker == null;
		lowAtStart.Clear();
		released.Clear();
		foreach (T item in alreadyLow)
		{
			lowAtStart.Add(item);
		}
	}

	internal void MoveFinished(T attacker)
	{
		if (attacker == last)
		{
			finished = true;
		}
	}

	internal void EndTurn()
	{
		finished = true;
	}

	internal void Reset()
	{
		last = null;
		finished = true;
		lowAtStart.Clear();
		released.Clear();
	}

	internal bool ShouldHold(T player, bool isLow, bool dead, string? animation)
	{
		if (dead || !isLow || last == null || lowAtStart.Contains(player) || released.Contains(player))
		{
			return false;
		}
		if (!finished)
		{
			return true;
		}
		if (string.IsNullOrEmpty(animation) || animation.Equals("hurt", StringComparison.OrdinalIgnoreCase) || animation.Equals("hit", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		released.Add(player);
		return false;
	}
}
