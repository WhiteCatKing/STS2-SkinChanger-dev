using System;
using System.Collections.Generic;
using CznCore.Presentation;

namespace MeirinWatcherSkin.Characters.Meirin;

internal sealed class MeirinCombatPresentationProfile : ICombatPresentationProfile, ICombatPresentationRoutePlanResolver<CombatPresentationCardContext>
{
	internal const string AttackPlay1ComboCue = "attack_play1_combo";

	internal const string AttackPlay2ComboCue = "attack_play2_combo";

	private const string CardReadyStateId = "meirin.combat.card_ready";

	private const string RestoreIdleStateId = "meirin.combat.restore_idle";

	private const string HitStateId = "meirin.combat.hit";

	private static readonly Dictionary<string, CombatPresentationState> States = new Dictionary<string, CombatPresentationState>(StringComparer.Ordinal)
	{
		["meirin.combat.card_ready"] = new CombatPresentationState("meirin.combat.card_ready", (IReadOnlyList<CombatPresentationBodyStep>)new _003C_003Ez__ReadOnlySingleElementList<CombatPresentationBodyStep>(new CombatPresentationBodyStep("idle_to_b_idle", (string)null, (float?)null)), "b_idle", false),
		["meirin.combat.restore_idle"] = new CombatPresentationState("meirin.combat.restore_idle", (IReadOnlyList<CombatPresentationBodyStep>)new _003C_003Ez__ReadOnlySingleElementList<CombatPresentationBodyStep>(new CombatPresentationBodyStep("b_idle_to_idle", (string)null, (float?)null)), "idle", true),
		["meirin.combat.hit"] = new CombatPresentationState("meirin.combat.hit", (IReadOnlyList<CombatPresentationBodyStep>)new _003C_003Ez__ReadOnlySingleElementList<CombatPresentationBodyStep>(new CombatPresentationBodyStep("hit", (string)null, (float?)null)), "idle", true)
	};

	private static readonly Dictionary<CombatPresentationCardRoute, CombatPresentationRoutePlan> RoutePlans = new Dictionary<CombatPresentationCardRoute, CombatPresentationRoutePlan> { [(CombatPresentationCardRoute)0] = new CombatPresentationRoutePlan((CombatPresentationCardRoute)0, (CombatPresentationRouteBodyMode)3, (string)null, (IReadOnlyList<CombatPresentationBodyStep>)null, (string)null, (IReadOnlyList<CombatPresentationSfxCue>)null, (CombatPresentationVoiceCue)null, "unknown_restore_idle", (CombatPresentationHitResponsePolicy)0, (IReadOnlyList<string>)null, (string)null, 0f) };

	private static readonly Dictionary<CombatPresentationCardRoute, CombatPresentationAttackPlan> AttackPlans = new Dictionary<CombatPresentationCardRoute, CombatPresentationAttackPlan>();

	private static readonly Dictionary<string, CombatPresentationAnimationTiming> BodyAnimationTimings = new Dictionary<string, CombatPresentationAnimationTiming>(StringComparer.Ordinal)
	{
		["enter_ready"] = new CombatPresentationAnimationTiming(0.3333f),
		["enter_play"] = new CombatPresentationAnimationTiming(0.6667f),
		["enter_end"] = new CombatPresentationAnimationTiming(0.4667f),
		["victory_ready"] = new CombatPresentationAnimationTiming(0.9333f),
		["victory"] = new CombatPresentationAnimationTiming(2.6667f),
		["idle_to_b_idle"] = new CombatPresentationAnimationTiming(0.4667f),
		["b_idle_to_idle"] = new CombatPresentationAnimationTiming(0.4667f),
		["attack_ready"] = new CombatPresentationAnimationTiming(0.2f),
		["attack_play1"] = new CombatPresentationAnimationTiming(0.6667f),
		["attack_play2"] = new CombatPresentationAnimationTiming(0.6667f),
		["attack_end"] = new CombatPresentationAnimationTiming(0.4f),
		["f_strong_attack_ready"] = new CombatPresentationAnimationTiming(0.2f),
		["f_strong_attack_play"] = new CombatPresentationAnimationTiming(0.6667f),
		["f_strong_attack_end"] = new CombatPresentationAnimationTiming(0.4f),
		["u1_buff_ready"] = new CombatPresentationAnimationTiming(0.4667f),
		["u1_buff_play"] = new CombatPresentationAnimationTiming(1.1f),
		["u2_attack_ready"] = new CombatPresentationAnimationTiming(0.4f),
		["u2_attack_play"] = new CombatPresentationAnimationTiming(0.6667f),
		["u2_attack_end"] = new CombatPresentationAnimationTiming(0.4f),
		["u3_buff_ready"] = new CombatPresentationAnimationTiming(0.4667f),
		["u3_buff_play"] = new CombatPresentationAnimationTiming(1f),
		["u4_buff_ready"] = new CombatPresentationAnimationTiming(0.4667f),
		["u4_buff_play"] = new CombatPresentationAnimationTiming(1f),
		["buff_ready"] = new CombatPresentationAnimationTiming(0.4667f),
		["buff_play"] = new CombatPresentationAnimationTiming(1.1f),
		["debuff_ready"] = new CombatPresentationAnimationTiming(0.4667f),
		["debuff_play"] = new CombatPresentationAnimationTiming(1.0667f),
		["defense_ready"] = new CombatPresentationAnimationTiming(0.4667f),
		["defense_play"] = new CombatPresentationAnimationTiming(1.1f),
		["cure_ready"] = new CombatPresentationAnimationTiming(0.4667f),
		["cure_play"] = new CombatPresentationAnimationTiming(1.1f),
		["hit"] = new CombatPresentationAnimationTiming(0.6f),
		["death_ready"] = new CombatPresentationAnimationTiming(0.9333f),
		["death"] = new CombatPresentationAnimationTiming(0.1667f)
	};

	private static readonly Dictionary<string, IReadOnlyList<CombatPresentationPersistentEffectBinding>> PersistentEffectBindings = new Dictionary<string, IReadOnlyList<CombatPresentationPersistentEffectBinding>>(StringComparer.Ordinal);

	private const string NormalAttackSceneRoot = "res://MeirinWatcherSkin/scenes/combat_effects/";

	private static readonly Dictionary<string, IReadOnlyList<CombatPresentationEffectCue>> TransientEffects = new Dictionary<string, IReadOnlyList<CombatPresentationEffectCue>>(StringComparer.Ordinal)
	{
		["attack_play1"] = NormalAttackEffects("meirin_attack_play1.tscn", 0.9667f, "meirin_attack_play1"),
		["attack_play2"] = NormalAttackEffects("meirin_attack_play2.tscn", 1f, "meirin_attack_play2"),
		["attack_play1_combo"] = NormalAttackContinuationEffects("meirin_attack_play1.tscn", 0.9667f, "meirin_attack_play1"),
		["attack_play2_combo"] = NormalAttackContinuationEffects("meirin_attack_play2.tscn", 1f, "meirin_attack_play2"),
		["f_strong_attack_play"] = StrongAttackEffects(),
		["buff_sequence"] = StandardBuffSequenceEffects(),
		["u1_buff_sequence"] = U1BuffSequenceEffects(),
		["debuff_sequence"] = DebuffSequenceEffects(),
		["block_gain_feedback"] = CombatPresentationCommonDefense.BuildSystemBuffShieldEffectCues("res://MeirinWatcherSkin/scenes/combat_effects/", 0.62f, (CombatPresentationEffectAnchor)0, 1f, 1f, "system_buff_shield"),
		["u2_attack_sequence"] = U2AttackSequenceEffects(),
		["u3_buff_sequence"] = U3BuffSequenceEffects(),
		["u4_buff_sequence"] = U4BuffSequenceEffects(),
		["skill_x_effect_library"] = SkillXEffectLibrary()
	};

	private static readonly Dictionary<string, IReadOnlyList<CombatPresentationSfxCue>> SfxCues = new Dictionary<string, IReadOnlyList<CombatPresentationSfxCue>>(StringComparer.Ordinal)
	{
		["attack_play1"] = new _003C_003Ez__ReadOnlyArray<CombatPresentationSfxCue>((CombatPresentationSfxCue[])(object)new CombatPresentationSfxCue[2]
		{
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_attack_ready.ogg", 0f, 1f),
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_attack_play1.ogg", 0.2f, 1f)
		}),
		["attack_play2"] = new _003C_003Ez__ReadOnlyArray<CombatPresentationSfxCue>((CombatPresentationSfxCue[])(object)new CombatPresentationSfxCue[2]
		{
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_attack_ready.ogg", 0f, 1f),
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_attack_play2.ogg", 0.2f, 1f)
		}),
		["attack_play1_combo"] = new _003C_003Ez__ReadOnlySingleElementList<CombatPresentationSfxCue>(new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_attack_play1.ogg", 0f, 1f)),
		["attack_play2_combo"] = new _003C_003Ez__ReadOnlySingleElementList<CombatPresentationSfxCue>(new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_attack_play2.ogg", 0f, 1f)),
		["f_strong_attack_play"] = new _003C_003Ez__ReadOnlyArray<CombatPresentationSfxCue>((CombatPresentationSfxCue[])(object)new CombatPresentationSfxCue[2]
		{
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_strong_attack_ready.ogg", 0f, 1f),
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_strong_attack_play.ogg", 0.2f, 1f)
		}),
		["defense_ready"] = new _003C_003Ez__ReadOnlySingleElementList<CombatPresentationSfxCue>(new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_defense_ready.ogg", 0f, 1f)),
		["u2_attack_play"] = new _003C_003Ez__ReadOnlyArray<CombatPresentationSfxCue>((CombatPresentationSfxCue[])(object)new CombatPresentationSfxCue[2]
		{
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u2_attack_ready.ogg", 0f, 1f),
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u2_attack_play.ogg", 0.4f, 1f)
		}),
		["u1_buff_sequence"] = new _003C_003Ez__ReadOnlyArray<CombatPresentationSfxCue>((CombatPresentationSfxCue[])(object)new CombatPresentationSfxCue[2]
		{
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u1_buff_ready.ogg", 0f, 1f),
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u1_buff_play.ogg", 0.4667f, 1f)
		}),
		["u3_buff_sequence"] = new _003C_003Ez__ReadOnlyArray<CombatPresentationSfxCue>((CombatPresentationSfxCue[])(object)new CombatPresentationSfxCue[2]
		{
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u3_buff_ready.ogg", 0f, 1f),
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u3_buff_play.ogg", 0.4667f, 1f)
		}),
		["u4_buff_sequence"] = new _003C_003Ez__ReadOnlySingleElementList<CombatPresentationSfxCue>(new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_u4_buff_ready.ogg", 0f, 1f)),
		["debuff_sequence"] = new _003C_003Ez__ReadOnlyArray<CombatPresentationSfxCue>((CombatPresentationSfxCue[])(object)new CombatPresentationSfxCue[2]
		{
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_debuff_ready.ogg", 0f, 1f),
			new CombatPresentationSfxCue("res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_debuff_play.ogg", 0.4667f, 1f)
		}),
		["block_gain_feedback"] = CombatPresentationCommonDefense.BuildSystemBuffShieldSfxCues("res://CznCore/audio/sfx/system_buff_shield.ogg")
	};

	private readonly CombatCardPresentationProfile _cardPresentationProfile;

	private readonly CombatPresentationProfileTables _tables;

	public CombatPresentationCommonStateIds CommonStateIds { get; } = new CombatPresentationCommonStateIds("meirin.combat.card_ready", "meirin.combat.restore_idle", "meirin.combat.hit");

	public MeirinCombatPresentationProfile(CombatCardPresentationProfile cardPresentationProfile)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		_cardPresentationProfile = cardPresentationProfile ?? throw new ArgumentNullException("cardPresentationProfile");
		_tables = new CombatPresentationProfileTables((IReadOnlyDictionary<string, CombatPresentationState>)States, (IReadOnlyDictionary<CombatPresentationCardRoute, CombatPresentationRoutePlan>)RoutePlans, (IReadOnlyDictionary<CombatPresentationCardRoute, CombatPresentationAttackPlan>)AttackPlans, (IReadOnlyDictionary<string, CombatPresentationAnimationTiming>)BodyAnimationTimings, (IReadOnlyDictionary<string, IReadOnlyList<CombatPresentationPersistentEffectBinding>>)PersistentEffectBindings, (IReadOnlyDictionary<string, IReadOnlyList<CombatPresentationEffectCue>>)TransientEffects, (IReadOnlyDictionary<string, IReadOnlyList<CombatPresentationSfxCue>>)SfxCues, (Func<string, bool>)null, (Func<string, bool>)null);
	}

	public bool TryGetState(string stateId, out CombatPresentationState state)
	{
		return _tables.TryGetState(stateId, ref state);
	}

	public bool TryGetRoutePlan(CombatPresentationCardRoute route, out CombatPresentationRoutePlan plan)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _tables.TryGetRoutePlan(route, ref plan);
	}

	public bool TryGetRoutePlan(CombatPresentationCardContext context, out CombatPresentationRoutePlan plan)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!_cardPresentationProfile.TryGetRoutePlanOverride(context, ref plan))
		{
			return TryGetRoutePlan(context.Route, out plan);
		}
		return true;
	}

	public bool TryGetAttackPlan(CombatPresentationCardContext context, out CombatPresentationAttackPlan plan)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		if (!_cardPresentationProfile.TryGetAttackPlanOverride(context, ref plan))
		{
			return _tables.TryGetAttackPlan(context.Route, ref plan);
		}
		return true;
	}

	public bool TryGetBodyAnimationTiming(string animationName, out CombatPresentationAnimationTiming timing)
	{
		return _tables.TryGetBodyAnimationTiming(animationName, ref timing);
	}

	public bool TryGetPersistentEffectBindings(string animationName, out IReadOnlyList<CombatPresentationPersistentEffectBinding> effects)
	{
		return _tables.TryGetPersistentEffectBindings(animationName, ref effects);
	}

	public bool TryGetTransientEffects(string animationName, out IReadOnlyList<CombatPresentationEffectCue> effects)
	{
		return _tables.TryGetTransientEffects(animationName, ref effects);
	}

	public IEnumerable<CombatPresentationEffectCue> EnumerateTransientEffectCues()
	{
		return _tables.EnumerateTransientEffectCues();
	}

	public IEnumerable<CombatPresentationSfxCue> EnumerateSfxCues()
	{
		return _tables.EnumerateSfxCues(true);
	}

	public bool TryGetSfxCues(string animationName, out IReadOnlyList<CombatPresentationSfxCue> sfxCues)
	{
		return _tables.TryGetSfxCues(animationName, ref sfxCues);
	}

	private static IReadOnlyList<CombatPresentationEffectCue> NormalAttackEffects(string ownerScene, float ownerLifetimeSeconds, string ownerGroup)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Expected O, but got Unknown
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Expected O, but got Unknown
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Expected O, but got Unknown
		return new _003C_003Ez__ReadOnlyArray<CombatPresentationEffectCue>((CombatPresentationEffectCue[])(object)new CombatPresentationEffectCue[6]
		{
			new CombatPresentationEffectCue("res://MeirinWatcherSkin/scenes/combat_effects/common_hit_eff.tscn", 0.2f, 0.8333f, false, (CombatPresentationEffectPalette)0, (CombatPresentationEffectAnchor)1, false, false, "common_hit_eff", 0.8f, 1f, 0f, 0f, 0f, 1f, 0, 0f, 0f, true, 0, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, true, (CombatPresentationEffectWarmupTier)0, "loop", 0f, false),
			new CombatPresentationEffectCue("res://MeirinWatcherSkin/scenes/combat_effects/common_hit_eff_particles.tscn", 0.2f, 0.8333f, false, (CombatPresentationEffectPalette)0, (CombatPresentationEffectAnchor)1, false, false, "common_hit_eff_particles", 0.8f, 1f, 0f, 0f, 0f, 1f, 0, 0f, 0f, true, 0, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, true, (CombatPresentationEffectWarmupTier)0, (string)null, 0.8f, false),
			new CombatPresentationEffectCue("res://MeirinWatcherSkin/scenes/combat_effects/" + ownerScene, 0.2f, ownerLifetimeSeconds, false, (CombatPresentationEffectPalette)0, (CombatPresentationEffectAnchor)0, false, false, ownerGroup, 0.6f, 1f, 0f, 0f, 0f, 1f, 50, 0f, 0f, true, 0, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, false, (CombatPresentationEffectWarmupTier)0, "animation", 0f, false),
			new CombatPresentationEffectCue("res://MeirinWatcherSkin/scenes/combat_effects/meirin_attack_target_botglow_only.tscn", 0.266f, 0.4667f, false, (CombatPresentationEffectPalette)0, (CombatPresentationEffectAnchor)1, false, false, "meirin_attack_play_target_botglow", 0.6f, 1f, 0f, 0f, 0f, 1f, 0, 0f, 0f, true, 0, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, true, (CombatPresentationEffectWarmupTier)0, "animation", 0f, false),
			new CombatPresentationEffectCue("res://MeirinWatcherSkin/scenes/combat_effects/meirin_attack_target_only.tscn", 0.266f, 0.5333f, false, (CombatPresentationEffectPalette)0, (CombatPresentationEffectAnchor)4, false, false, "meirin_attack_play_target", 0.6f, 1f, 0f, 0f, 0f, 1f, 40, 0f, 0f, true, 0, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, true, (CombatPresentationEffectWarmupTier)0, "animation", 0f, false),
			new CombatPresentationEffectCue("res://MeirinWatcherSkin/scenes/combat_effects/meirin_attack_target_particles.tscn", 0.266f, 0.5333f, false, (CombatPresentationEffectPalette)0, (CombatPresentationEffectAnchor)4, false, false, "meirin_attack_play_target_particles", 0.6f, 1f, 0f, 0f, 0f, 1f, 40, 0f, 0f, true, 0, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, true, (CombatPresentationEffectWarmupTier)0, (string)null, 1.3f, false)
		});
	}

	private static IReadOnlyList<CombatPresentationEffectCue> NormalAttackContinuationEffects(string ownerScene, float ownerLifetimeSeconds, string ownerGroup)
	{
		IReadOnlyList<CombatPresentationEffectCue> readOnlyList = NormalAttackEffects(ownerScene, ownerLifetimeSeconds, ownerGroup);
		CombatPresentationEffectCue[] array = (CombatPresentationEffectCue[])(object)new CombatPresentationEffectCue[readOnlyList.Count];
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			CombatPresentationEffectCue val = readOnlyList[i];
			int num = i;
			CombatPresentationEffectCue obj = val._003CClone_003E_0024();
			obj.set_StartDelaySeconds(Math.Max(0f, val.StartDelaySeconds - 0.2f));
			array[num] = obj;
		}
		return array;
	}

	private static IReadOnlyList<CombatPresentationEffectCue> StrongAttackEffects()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Expected O, but got Unknown
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Expected O, but got Unknown
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Expected O, but got Unknown
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Expected O, but got Unknown
		return new _003C_003Ez__ReadOnlyArray<CombatPresentationEffectCue>((CombatPresentationEffectCue[])(object)new CombatPresentationEffectCue[6]
		{
			new CombatPresentationEffectCue("res://MeirinWatcherSkin/scenes/combat_effects/meirin_strong_attack_owner.tscn", 0.2f, 1.2333f, false, (CombatPresentationEffectPalette)0, (CombatPresentationEffectAnchor)0, false, false, "meirin_strong_attack_owner", 0.6f, 1f, 0f, 0f, 0f, 1f, 0, 0f, 0f, true, 0, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, true, (CombatPresentationEffectWarmupTier)0, "animation", 0f, false),
			new CombatPresentationEffectCue("res://MeirinWatcherSkin/scenes/combat_effects/meirin_strong_attack_owner_particles.tscn", 0.2f, 1.2333f, false, (CombatPresentationEffectPalette)0, (CombatPresentationEffectAnchor)0, false, false, "meirin_strong_attack_owner_particles", 0.6f, 1f, 0f, 0f, 0f, 1f, 0, 0f, 0f, true, 0, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, true, (CombatPresentationEffectWarmupTier)0, (string)null, 1.1f, false),
			new CombatPresentationEffectCue("res://MeirinWatcherSkin/scenes/combat_effects/meirin_strong_attack_botflash.tscn", 0.2f, 0.5333f, false, (CombatPresentationEffectPalette)0, (CombatPresentationEffectAnchor)1, false, false, "meirin_strong_attack_botflash", 0.6f, 1f, 0f, 0f, 0f, 1f, 0, 0f, 0f, true, 0, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, true, (CombatPresentationEffectWarmupTier)0, "animation", 0f, false),
			new CombatPresentationEffectCue("res://MeirinWatcherSkin/scenes/combat_effects/meirin_attack_target_botglow_only.tscn", 0.2f, 0.4667f, false, (CombatPresentationEffectPalette)0, (CombatPresentationEffectAnchor)1, false, false, "meirin_strong_attack_target_botglow", 1f, 1f, 0f, 0f, 0f, 1f, 0, 0f, 0f, true, 0, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, true, (CombatPresentationEffectWarmupTier)0, "animation", 0f, false),
			new CombatPresentationEffectCue("res://MeirinWatcherSkin/scenes/combat_effects/meirin_attack_target_only.tscn", 0.2f, 0.5333f, false, (CombatPresentationEffectPalette)0, (CombatPresentationEffectAnchor)4, false, false, "meirin_strong_attack_target", 1f, 1f, 0f, 0f, 0f, 1f, 40, 0f, 0f, true, 0, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, true, (CombatPresentationEffectWarmupTier)0, "animation", 0f, false),
			new CombatPresentationEffectCue("res://MeirinWatcherSkin/scenes/combat_effects/meirin_attack_target_particles.tscn", 0.2f, 0.5333f, false, (CombatPresentationEffectPalette)0, (CombatPresentationEffectAnchor)4, false, false, "meirin_strong_attack_target_particles", 1f, 1f, 0f, 0f, 0f, 1f, 40, 0f, 0f, true, 0, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, true, (CombatPresentationEffectWarmupTier)0, (string)null, 1.3f, false)
		});
	}

	private static IReadOnlyList<CombatPresentationEffectCue> StandardBuffSequenceEffects()
	{
		return new _003C_003Ez__ReadOnlyArray<CombatPresentationEffectCue>((CombatPresentationEffectCue[])(object)new CombatPresentationEffectCue[10]
		{
			SupportCue("meirin_buff_ready", 0f, 1.5667f, (CombatPresentationEffectAnchor)0, 0.6f, 0, "meirin_buff_ready", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false),
			SupportCue("meirin_buff_play_botglow", 0.4667f, 1.0667f, (CombatPresentationEffectAnchor)0, 0.45f, 0, "meirin_buff_play_botglow"),
			SupportCue("meirin_buff_play_self", 0.4667f, 1.1f, (CombatPresentationEffectAnchor)0, 0.6f, 0, "meirin_buff_play_self", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false),
			SupportCue("meirin_buff_play_target2_m", 0.4667f, 0.1f, (CombatPresentationEffectAnchor)0, 0.45f, 0, "meirin_buff_play_target2_m"),
			SupportCue("meirin_buff_play_target2_f", 0.5327f, 1.8333f, (CombatPresentationEffectAnchor)0, 0.45f, 0, "meirin_buff_play_target2_f"),
			SupportCue("meirin_buff_play_firework_yellow", 0.6327f, 0.9f, (CombatPresentationEffectAnchor)0, 0.45f, 0, "meirin_buff_play_firework_yellow"),
			SupportCue("meirin_buff_play_firework_red", 1.1667f, 0.9f, (CombatPresentationEffectAnchor)0, 0.45f, 0, "meirin_buff_play_firework_red"),
			SupportCue("meirin_buff_play_particles_dom", 0.4667f, 1.8993f, (CombatPresentationEffectAnchor)0, 0.45f, 0, "meirin_buff_play_particles_dom", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: true, 1.5f),
			SupportCue("meirin_buff_play_particles_yellow", 0.4667f, 1.8993f, (CombatPresentationEffectAnchor)0, 0.45f, 0, "meirin_buff_play_particles_yellow", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: true, 1.1f),
			SupportCue("meirin_buff_play_particles_red", 0.4667f, 1.8993f, (CombatPresentationEffectAnchor)0, 0.45f, 0, "meirin_buff_play_particles_red", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: true, 1.1f)
		});
	}

	private static IReadOnlyList<CombatPresentationEffectCue> U1BuffSequenceEffects()
	{
		return new _003C_003Ez__ReadOnlyArray<CombatPresentationEffectCue>((CombatPresentationEffectCue[])(object)new CombatPresentationEffectCue[12]
		{
			SupportCue("meirin_buff_ready", 0f, 1.5667f, (CombatPresentationEffectAnchor)0, 0.6f, 50, "meirin_buff_ready", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false),
			SupportCue("meirin_buff_play_botglow", 0.4667f, 1.0667f, (CombatPresentationEffectAnchor)0, 0.6f, 0, "meirin_buff_play_botglow"),
			SupportCue("meirin_buff_play_self", 0.4667f, 1.1f, (CombatPresentationEffectAnchor)0, 0.6f, 0, "meirin_buff_play_self", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false),
			SupportCue("meirin_buff_play_target2_m", 0.4667f, 0.1f, (CombatPresentationEffectAnchor)0, 0.6f, 1000, "meirin_buff_play_target2_m"),
			SupportCue("meirin_buff_play_target2_f", 0.5327f, 1.8333f, (CombatPresentationEffectAnchor)0, 0.6f, 1000, "meirin_buff_play_target2_f"),
			SupportCue("meirin_buff_play_firework_yellow", 0.6327f, 0.9f, (CombatPresentationEffectAnchor)0, 0.6f, 1000, "meirin_buff_play_firework_yellow"),
			SupportCue("meirin_buff_play_firework_blue", 0.9667f, 0.9f, (CombatPresentationEffectAnchor)0, 0.6f, -1000, "meirin_buff_play_firework_blue"),
			SupportCue("meirin_buff_play_firework_red", 1.1667f, 0.9f, (CombatPresentationEffectAnchor)0, 0.6f, 1000, "meirin_buff_play_firework_red"),
			SupportCue("meirin_buff_play_particles_dom", 0.4667f, 1.8993f, (CombatPresentationEffectAnchor)0, 0.6f, 1000, "meirin_buff_play_particles_dom", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: true, 1.5f),
			SupportCue("meirin_buff_play_particles_yellow", 0.4667f, 1.8993f, (CombatPresentationEffectAnchor)0, 0.6f, 1000, "meirin_buff_play_particles_yellow", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: true, 1.1f),
			SupportCue("meirin_buff_play_particles_blue", 0.4667f, 1.4f, (CombatPresentationEffectAnchor)0, 0.6f, -1000, "meirin_buff_play_particles_blue", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: true, 1.1f),
			SupportCue("meirin_buff_play_particles_red", 0.4667f, 1.8993f, (CombatPresentationEffectAnchor)0, 0.6f, 1000, "meirin_buff_play_particles_red", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: true, 1.1f)
		});
	}

	private static IReadOnlyList<CombatPresentationEffectCue> DebuffSequenceEffects()
	{
		return new _003C_003Ez__ReadOnlyArray<CombatPresentationEffectCue>((CombatPresentationEffectCue[])(object)new CombatPresentationEffectCue[6]
		{
			SupportCue("meirin_debuff_ready_owner", 0f, 1.5667f, (CombatPresentationEffectAnchor)0, 0.6f, 0, "meirin_debuff_ready_owner", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false),
			TargetVisualCue("meirin_debuff_ready_target_ground", 0f, 1.2333f, 0.6f, 0, "meirin_debuff_ready_target_ground", behindTarget: true, useTargetSurface: true),
			TargetVisualCue("meirin_debuff_ready_target_f", 0f, 1.2333f, 0.6f, 0, "meirin_debuff_ready_target_f"),
			TargetVisualCue("meirin_debuff_play_target_ground", 0.4667f, 0.9667f, 0.5f, 0, "meirin_debuff_play_target_ground", behindTarget: true, useTargetSurface: true),
			TargetVisualCue("meirin_debuff_play_target_b", 0.4667f, 1.3667f, 0.5f, 0, "meirin_debuff_play_target_b", behindTarget: true),
			TargetVisualCue("meirin_debuff_play_target_f", 0.4667f, 1.3667f, 0.5f, 0, "meirin_debuff_play_target_f")
		});
	}

	private static IReadOnlyList<CombatPresentationEffectCue> U2AttackSequenceEffects()
	{
		return new _003C_003Ez__ReadOnlyArray<CombatPresentationEffectCue>((CombatPresentationEffectCue[])(object)new CombatPresentationEffectCue[8]
		{
			SupportCue("meirin_u2_owner", 0.4f, 1.1667f, (CombatPresentationEffectAnchor)0, 0.6f, 0, "meirin_u2_owner", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false),
			TargetVisualCue("meirin_attack_target_botglow_only", 0.466f, 0.4667f, 0.6f, -1, "meirin_u2_hit_botglow", behindTarget: true, useTargetSurface: true),
			TargetVisualCue("meirin_u2_target_b", 0.4f, 1.9667f, 0.6f, 0, "meirin_u2_target_b", behindTarget: true, useTargetSurface: true, 0f, CznOffsetYToGodot(-30f)),
			TargetVisualCue("meirin_u2_target_f", 0.4f, 1.4667f, 0.6f, 0, "meirin_u2_target_f", behindTarget: false, useTargetSurface: true, 0f, CznOffsetYToGodot(-30f)),
			TargetVisualCue("meirin_u2_attack_target_particles", 0.4f, 1.5f, 0.6f, 1, "meirin_u2_target_particles", behindTarget: false, useTargetSurface: true, 0f, CznOffsetYToGodot(-30f), string.Empty, 1.9f),
			TargetVisualCue("meirin_attack_target_only", 0.466f, 0.5333f, 0.8f, 2, "meirin_u2_hit"),
			TargetVisualCue("meirin_attack_target_particles", 0.466f, 0.5333f, 0.8f, 2, "meirin_u2_hit_particles", behindTarget: false, useTargetSurface: false, 0f, 0f, string.Empty, 1.3f),
			TargetVisualCue("meirin_u2_flash", 0.466f, 1.3333f, 0.6f, 3, "meirin_u2_flash")
		});
	}

	private static IReadOnlyList<CombatPresentationEffectCue> U3BuffSequenceEffects()
	{
		return new _003C_003Ez__ReadOnlyArray<CombatPresentationEffectCue>((CombatPresentationEffectCue[])(object)new CombatPresentationEffectCue[3]
		{
			SupportCue("meirin_u3_buff_play", 0.4667f, 1.4f, (CombatPresentationEffectAnchor)0, 1f, 0, "meirin_u3_buff_play", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false),
			SupportCue("meirin_u3_buff_inner_effect", 0.4667f, 1.2f, (CombatPresentationEffectAnchor)0, 1f, 0, "meirin_u3_buff_inner_effect", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false),
			SupportCue("meirin_u3_buff_play_particles", 0.4667f, 1.1f, (CombatPresentationEffectAnchor)0, 1f, 0, "meirin_u3_buff_play_particles", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false, 2f)
		});
	}

	private static IReadOnlyList<CombatPresentationEffectCue> U4BuffSequenceEffects()
	{
		return new _003C_003Ez__ReadOnlyArray<CombatPresentationEffectCue>((CombatPresentationEffectCue[])(object)new CombatPresentationEffectCue[4]
		{
			SupportCue("meirin_u4_buff_ready", 0f, 0.7333f, (CombatPresentationEffectAnchor)0, 1f, 0, "meirin_u4_buff_ready", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false),
			SupportCue("meirin_u4_buff_ready_particles", 0f, 0.7f, (CombatPresentationEffectAnchor)0, 1f, 0, "meirin_u4_buff_ready_particles", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false, 0.3f),
			SupportCue("meirin_u4_buff_play", 0.4667f, 1.2333f, (CombatPresentationEffectAnchor)0, 1f, 0, "meirin_u4_buff_play", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false),
			SupportCue("meirin_u4_buff_play_particles", 0.4667f, 0.6333f, (CombatPresentationEffectAnchor)0, 1f, 0, "meirin_u4_buff_play_particles", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false, 1.4f)
		});
	}

	private static IReadOnlyList<CombatPresentationEffectCue> SkillXEffectLibrary()
	{
		return new _003C_003Ez__ReadOnlyArray<CombatPresentationEffectCue>((CombatPresentationEffectCue[])(object)new CombatPresentationEffectCue[6]
		{
			SupportCue("meirin_skill_x_self_effects", 0f, 3.8f, (CombatPresentationEffectAnchor)0, 1f, 0, "meirin_skill_x_self_effects", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false),
			SupportCue("meirin_skill_x_self_limited_break", 0f, 3.8f, (CombatPresentationEffectAnchor)0, 1f, 0, "meirin_skill_x_self_limited_break", 0f, 0f, usePma: false, preserveSceneRoot: true, "limited_break", neutralizeAnchorScale: false),
			SupportCue("meirin_skill_x_self_particles", 0f, 3.1333f, (CombatPresentationEffectAnchor)0, 1f, 0, "meirin_skill_x_self_particles", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false, 0.55f),
			SupportCue("meirin_skill_x_screen_limited_break", 0f, 3.8f, (CombatPresentationEffectAnchor)2, 1f, 1, "meirin_skill_x_screen_limited_break", 0f, 0f, usePma: false, preserveSceneRoot: true, "limited_break", neutralizeAnchorScale: false),
			SupportCue("meirin_skill_x_screen_effects", 0f, 3.8f, (CombatPresentationEffectAnchor)2, 1f, 2, "meirin_skill_x_screen_effects", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false),
			SupportCue("meirin_skill_x_screen_particles", 0f, 1.3f, (CombatPresentationEffectAnchor)2, 1f, 3, "meirin_skill_x_screen_particles", 0f, 0f, usePma: false, preserveSceneRoot: true, "animation", neutralizeAnchorScale: false, 0.7f)
		});
	}

	private static float CznOffsetYToGodot(float cznOffsetY)
	{
		return 0f - cznOffsetY;
	}

	private static CombatPresentationEffectCue TargetVisualCue(string sceneName, float startDelaySeconds, float lifetimeSeconds, float scale, int bandZIndex, string groupName, bool behindTarget = false, bool useTargetSurface = false, float offsetX = 0f, float offsetY = 0f, string spineAnimation = "animation", float particleTailSeconds = 0f)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		string text = "res://MeirinWatcherSkin/scenes/combat_effects/" + sceneName + ".tscn";
		CombatPresentationEffectAnchor val = (useTargetSurface ? ((!behindTarget) ? ((CombatPresentationEffectAnchor)6) : ((CombatPresentationEffectAnchor)7)) : ((!behindTarget) ? ((CombatPresentationEffectAnchor)4) : ((CombatPresentationEffectAnchor)5)));
		return new CombatPresentationEffectCue(text, startDelaySeconds, lifetimeSeconds, false, (CombatPresentationEffectPalette)0, val, false, false, groupName, scale, 1f, offsetX, offsetY, 0f, 1f, bandZIndex, 0f, 0f, true, 0, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, true, (CombatPresentationEffectWarmupTier)0, spineAnimation, particleTailSeconds, false);
	}

	private static CombatPresentationEffectCue SupportCue(string sceneName, float startDelaySeconds, float lifetimeSeconds, CombatPresentationEffectAnchor anchor, float scale, int zIndex, string groupName, float offsetX = 0f, float offsetY = 0f, bool usePma = false, bool preserveSceneRoot = true, string spineAnimation = "animation", bool neutralizeAnchorScale = true, float particleTailSeconds = 0f)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		string text = "res://MeirinWatcherSkin/scenes/combat_effects/" + sceneName + ".tscn";
		bool flag = neutralizeAnchorScale;
		return new CombatPresentationEffectCue(text, startDelaySeconds, lifetimeSeconds, false, (CombatPresentationEffectPalette)0, anchor, usePma, false, groupName, scale, 1f, offsetX, offsetY, 0f, 1f, zIndex, 0f, 0f, true, 0, preserveSceneRoot, (CombatPresentationEffectImportance)0, 1f, (int?)null, flag, (CombatPresentationEffectWarmupTier)0, spineAnimation, particleTailSeconds, false);
	}
}
