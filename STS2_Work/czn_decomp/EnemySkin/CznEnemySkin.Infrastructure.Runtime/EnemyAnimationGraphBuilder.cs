using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class EnemyAnimationGraphBuilder
{
	private readonly IReadOnlyDictionary<(EnemyPresentationStance Stance, EnemyAction Action), EnemyAnimationBinding> _bindings;

	public EnemyAnimationGraphBuilder(EnemyReplacementProfile profile, MegaSprite controller)
	{
		_bindings = profile.Monster.AnimationBindings.ToDictionary((EnemyAnimationBinding binding) => (Stance: binding.Stance, Action: binding.Action));
		foreach (EnemyAnimationBinding value in _bindings.Values)
		{
			if (!controller.HasAnimation(value.CznAnimationName))
			{
				throw new InvalidOperationException("Replacement skeleton is missing phase animation '" + value.CznAnimationName + "' " + $"[{value.Stance}/{value.Action}].");
			}
		}
	}

	public AnimState State(EnemyPresentationStance stance, EnemyAction action)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		if (!_bindings.TryGetValue((stance, action), out EnemyAnimationBinding value))
		{
			throw new InvalidOperationException("Replacement profile is missing phase animation binding " + $"[{stance}/{action}].");
		}
		return new AnimState(value.CznAnimationName, value.Loop);
	}
}
