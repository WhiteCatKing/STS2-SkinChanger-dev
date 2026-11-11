using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace CznCore.Presentation;

internal sealed class CombatCardPlayTargetResolver
{
	private sealed class CombatCardPlayTargetState
	{
		public Creature? Target { get; set; }
	}

	private readonly ConditionalWeakTable<CardModel, CombatCardPlayTargetState> _targets = new ConditionalWeakTable<CardModel, CombatCardPlayTargetState>();

	private readonly CombatOpponentTargetResolver _opponentTargets = new CombatOpponentTargetResolver();

	public void Record(CardModel? card, Creature? target)
	{
		if (card != null && target != null)
		{
			_targets.GetOrCreateValue(card).Target = target;
		}
	}

	public IReadOnlyList<Creature>? ResolveTargets(CardModel? card, Creature? explicitTarget, CombatPresentationCardContext context, object? combatState = null, Creature? actor = null, Action<string>? warn = null)
	{
		if (context.TargetScope == CombatPresentationTargetScope.AllEnemies)
		{
			return ResolveAllEnemyTargets(card, actor, combatState, warn);
		}
		Creature val = NormalizeTarget(explicitTarget) ?? ResolveRecordedTarget(card) ?? NormalizeTarget((card != null) ? card.CurrentTarget : null);
		if (val != null)
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Creature>(val);
		}
		if (context.Route == CombatPresentationCardRoute.GeneratedCard)
		{
			object obj;
			if (card == null)
			{
				obj = null;
			}
			else
			{
				Player owner = card.Owner;
				obj = ((owner != null) ? owner.Creature : null);
			}
			if (obj != null)
			{
				return new _003C_003Ez__ReadOnlySingleElementList<Creature>(card.Owner.Creature);
			}
		}
		return null;
	}

	public IReadOnlyList<Creature>? ResolveAllEnemyTargets(CardModel? card, Creature? actor = null, object? combatState = null, Action<string>? warn = null)
	{
		if (actor == null)
		{
			object obj;
			if (card == null)
			{
				obj = null;
			}
			else
			{
				Player owner = card.Owner;
				obj = ((owner != null) ? owner.Creature : null);
			}
			actor = (Creature?)obj;
		}
		if (combatState == null)
		{
			combatState = _opponentTargets.ResolveCombatState(card, actor, warn);
		}
		return _opponentTargets.ResolveAliveOpponents(combatState, actor, warn);
	}

	public void Clear(CardModel? card)
	{
		if (card != null)
		{
			_targets.Remove(card);
		}
	}

	private Creature? ResolveRecordedTarget(CardModel? card)
	{
		if (card == null || !_targets.TryGetValue(card, out CombatCardPlayTargetState value))
		{
			return null;
		}
		return NormalizeTarget(value.Target);
	}

	private static Creature? NormalizeTarget(Creature? target)
	{
		if (target == null || !target.IsAlive)
		{
			return null;
		}
		return target;
	}
}
