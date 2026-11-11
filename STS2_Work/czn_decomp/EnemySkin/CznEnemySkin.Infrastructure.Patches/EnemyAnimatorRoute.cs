using System;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;

namespace CznEnemySkin.Infrastructure.Patches;

internal sealed record EnemyAnimatorRoute(Type PatchTarget, Type MonsterType, string FailureDescription, Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?> Create)
{
	public string? MonsterIdOverride { get; init; }

	public string? RequiredProfileId { get; init; }
}
