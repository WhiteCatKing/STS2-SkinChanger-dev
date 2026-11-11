using System;
using System.Threading;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Models;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class EnemyMoveSemanticRuntime
{
	private sealed record ActiveMove(MonsterModel Monster, string Trigger, EnemyAction Action)
	{
		public bool PreparationWaitConsumed { get; set; }
	}

	private sealed class CallerContextRestore(AsyncLocal<ActiveMove?> slot, ActiveMove? previous) : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				slot.Value = previous;
			}
		}
	}

	private readonly AsyncLocal<ActiveMove?> _activeMove = new AsyncLocal<ActiveMove>();

	public IDisposable Capture(MonsterModel monster, string trigger, EnemyAction action)
	{
		ActiveMove value = _activeMove.Value;
		_activeMove.Value = new ActiveMove(monster, trigger, action);
		return new CallerContextRestore(_activeMove, value);
	}

	public bool TryConsumePreparationWait(MonsterModel monster, string trigger)
	{
		if (!TryResolve(monster, trigger, out var _) || _activeMove.Value.PreparationWaitConsumed)
		{
			return false;
		}
		_activeMove.Value.PreparationWaitConsumed = true;
		return true;
	}

	public bool TryResolve(MonsterModel monster, string trigger, out EnemyAction action)
	{
		ActiveMove value = _activeMove.Value;
		if (value != null && value.Monster == monster && string.Equals(value.Trigger, trigger, StringComparison.Ordinal))
		{
			action = value.Action;
			return true;
		}
		action = EnemyAction.Idle;
		return false;
	}
}
