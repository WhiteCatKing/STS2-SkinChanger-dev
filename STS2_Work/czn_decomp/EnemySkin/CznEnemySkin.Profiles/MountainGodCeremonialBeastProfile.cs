using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Profiles;

internal static class MountainGodCeremonialBeastProfile
{
	private const string AudioRoot = "CznEnemySkin/audio/enemies/1005006/";

	private const string EffectRoot = "res://CznEnemySkin/scenes/enemy_effects/1005006/";

	private const EnemyEffectMaterialPolicy NeutralMultiply = EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply;

	internal static readonly float Sts2VisualScale = CznEnemyVisualScalePolicy.Resolve(1f);

	internal const string ProfileId = "mountain_god_for_ceremonial_beast";

	internal const string TargetMonsterId = "CEREMONIAL_BEAST";

	internal const string EncounterId = "CEREMONIAL_BEAST_BOSS";

	internal const string BattlefieldId = "b_laino_boss_1";

	internal const float BattlefieldVisualOffsetX = -80f;

	internal const float BattlefieldVisualOffsetY = -40f;

	internal static EnemyReplacementProfile Profile { get; } = new EnemyReplacementProfile("mountain_god_for_ceremonial_beast", new Sts2EnemyTarget("CEREMONIAL_BEAST"), new CznEnemySource("1005006_01", "1005006", "monster@name@1005006_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/ceremonial_beast.tscn", "res://CznEnemySkin/animations/monsters/1005006_mountain_god/1005006_mountain_god_skel_data.tres", Sts2VisualScale, new EnemySelectionBounds(-244.6276f, -582.3442f, 573.5219f, 595.7549f), BuildAnimationBindings(), "normal_enter", BuildActionPresentations(), new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "NCeremonialBeastVfx", "PlowStartTarget", "PlowEndTarget" }), null, new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "EnergyParticlesBack", "DeathParticles", "EnergyParticles" }), BuildNativePresentationPolicy()), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);

	private static IReadOnlyList<EnemyActionPresentation> BuildActionPresentations()
	{
		return new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[10]
		{
			Action(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(Audio("normal_enter"))),
			Action(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(Audio("normal_attack_play1")), MountainGodOfficialAttackEffects.For(EnemyAction.BasicAttack)),
			Action(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(Audio("normal_buff_1")), MountainGodOfficialAttackEffects.For(EnemyAction.Buff)),
			Action(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(Audio("normal_debuff_1")), MountainGodOfficialAttackEffects.For(EnemyAction.Cast)),
			Action(EnemyAction.Charge, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(Audio("stance1_strong_attack_ready"))),
			Action(EnemyAction.ChargeEnd, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(Audio("stance1_strong_attack_play")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				Spine("elemental_spirit_strong_attack_smoke_f", "animation", 8, 0f, 0f, 1f, 1f, 0f, 0f, 0.8333f),
				Particles("strong_attack_particles", 0.8333f),
				Spine("elemental_spirit_strong_attack", "animation", 3, 0f, 0f, 1f, 1f, 0f, 0f, 0.4f, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply),
				Spine("elemental_spirit_strong_attack_smoke_fb", "animation", 2, 0f, 0f, 1f, 1f, 0f, 0f, 0.8333f),
				Spine("elemental_spirit_strong_attack_smoke_b", "animation", -2, 0f, 0f, 1f, 1f, 0f, 0f, 0.7f)
			})),
			Action(EnemyAction.StunTransition, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(Audio("normal_to_stance1")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				Particles("to_stance_particles", 2.4667f),
				Spine("elemental_spirit_to_stance", "animation", 2, 0f, 0f, 1f, 1f, 0f, 0f, 2.0333f, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply)
			})),
			Action(EnemyAction.WakeUp, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(Audio("stance1_attack_ready"))),
			Action(EnemyAction.Hurt, new _003C_003Ez__ReadOnlyArray<EnemyAudioCue>(new EnemyAudioCue[3]
			{
				Audio("groan_1"),
				Audio("groan_2"),
				Audio("groan_3")
			})),
			Action(EnemyAction.Death, new _003C_003Ez__ReadOnlyArray<EnemyAudioCue>(new EnemyAudioCue[2]
			{
				Audio("normal_death"),
				Audio("stance1_death")
			}))
		});
	}

	private static EnemyActionPresentation Action(EnemyAction action, IReadOnlyList<EnemyAudioCue>? audio = null, IReadOnlyList<EnemyEffectCue>? effects = null)
	{
		return new EnemyActionPresentation(action, audio ?? Array.Empty<EnemyAudioCue>(), (effects ?? Array.Empty<EnemyEffectCue>()).Concat(MountainGodSharedTargets.For(action)).ToArray());
	}

	private static EnemyAudioCue Audio(string stem)
	{
		return new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005006/" + stem + ".ogg");
	}

	private static EnemyEffectCue Spine(string source, string animation = "animation", int z = 0, float x = 0f, float y = 0f, float scaleX = 1f, float scaleY = 1f, float rotation = 0f, float delay = 0f, float lifetime = 1f, EnemyEffectMaterialPolicy materialPolicy = EnemyEffectMaterialPolicy.Native)
	{
		return new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005006/spine/" + source + ".tscn", animation, x, y, scaleX, scaleY, rotation, z, delay, lifetime, StartParticles: false, materialPolicy, 1f, null);
	}

	private static EnemyEffectCue Particles(string sceneStem, float lifetime)
	{
		return new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005006/particles/" + sceneStem + ".tscn", null, 0f, 0f, 1f, 1f, 0f, 0, 0f, lifetime, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null);
	}

	private static IReadOnlyList<EnemyAnimationBinding> BuildAnimationBindings()
	{
		return new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[19]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Cast, "shrill", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "buff", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Charge, "plow", "stance1_strong_attack_ready", Loop: false),
			new EnemyAnimationBinding(EnemyAction.ChargeEnd, "plow_end", "stance1_strong_attack_play", Loop: false),
			new EnemyAnimationBinding(EnemyAction.ChargeRecovery, "plow_recovery", "stance1_strong_attack_end", Loop: false),
			new EnemyAnimationBinding(EnemyAction.ChargeDeath, "plow_end_die", "stance1_death", Loop: false),
			new EnemyAnimationBinding(EnemyAction.StunTransition, "stun", "normal_to_stance1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.StunLoop, "stun_loop", "stance1_groggy", Loop: true),
			new EnemyAnimationBinding(EnemyAction.WakeUp, "wake_up", "stance1_attack_ready", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Entrance, "normal_enter", "normal_enter", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "stance1_idle", Loop: true, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "stance1_attack_play1", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Cast, "shrill", "stance1_debuff_1", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "stance1_hit", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "stance1_death", Loop: false, EnemyPresentationStance.Transformed)
		});
	}

	private static EnemyNativePresentationPolicy BuildNativePresentationPolicy()
	{
		return new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[5]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_plow", EnemyAction.Charge),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_plow_end", EnemyAction.ChargeEnd),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_shrill", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/ceremonial_beast/ceremonial_beast_stun", EnemyAction.StunTransition)
		}), new _003C_003Ez__ReadOnlyArray<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression[3]
		{
			new EnemyNativeVfxPathSuppression("vfx/vfx_scream", EnemyAction.Cast),
			new EnemyNativeVfxPathSuppression("vfx/vfx_attack_blunt", EnemyAction.ChargeEnd),
			new EnemyNativeVfxPathSuppression("vfx/vfx_attack_slash", EnemyAction.BasicAttack)
		}), new _003C_003Ez__ReadOnlyArray<EnemyNativeVfxFeatureSuppression>(new EnemyNativeVfxFeatureSuppression[4]
		{
			new EnemyNativeVfxFeatureSuppression(EnemyNativeVfxFeature.RadialBlur, EnemyAction.ChargeEnd),
			new EnemyNativeVfxFeatureSuppression(EnemyNativeVfxFeature.HorizontalLines, EnemyAction.ChargeEnd),
			new EnemyNativeVfxFeatureSuppression(EnemyNativeVfxFeature.LineBurst, EnemyAction.ChargeEnd),
			new EnemyNativeVfxFeatureSuppression(EnemyNativeVfxFeature.SpikeSplash, EnemyAction.BasicAttack)
		}), EnemyAction.Hurt);
	}
}
