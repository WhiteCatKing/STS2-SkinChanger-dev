using System;
using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Profiles;

internal static class MountainGodSharedTargets
{
	internal static IReadOnlyList<EnemyEffectCue> For(EnemyAction action)
	{
		return action switch
		{
			EnemyAction.BasicAttack => new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 50f, 0.55f, 0.55f, -110f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, EnemyPresentationStance.Normal, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.55f, 0.55f, -110f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, EnemyPresentationStance.Normal, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, -50f, 0.55f, 0.55f, -110f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, EnemyPresentationStance.Normal, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 50f, 0.55f, 0.55f, -110f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, EnemyPresentationStance.Transformed, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.55f, 0.55f, -110f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, EnemyPresentationStance.Transformed, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, -50f, 0.55f, 0.55f, -110f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, EnemyPresentationStance.Transformed, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}, 
			EnemyAction.ChargeEnd => new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 50f, 0.55f, 0.55f, 20f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.55f, 0.55f, 20f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, -50f, 0.55f, 0.55f, 20f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 50f, 0.55f, 0.55f, -100f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.55f, 0.55f, -100f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, -50f, 0.55f, 0.55f, -100f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}, 
			_ => Array.Empty<EnemyEffectCue>(), 
		};
	}
}
