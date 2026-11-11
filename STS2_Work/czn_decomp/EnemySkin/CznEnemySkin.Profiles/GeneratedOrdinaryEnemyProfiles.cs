using System;
using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Profiles;

internal static class GeneratedOrdinaryEnemyProfiles
{
	internal static IReadOnlyList<EnemyReplacementProfile> Profiles { get; } = new _003C_003Ez__ReadOnlyArray<EnemyReplacementProfile>(new EnemyReplacementProfile[102]
	{
		BuildAbyssInsectForDecimillipedeSegmentBack(),
		BuildAbyssInsectForDecimillipedeSegmentFront(),
		BuildAbyssInsectForDecimillipedeSegmentMiddle(),
		BuildAlbithumForPhantasmalGardener(),
		BuildBeamShooterForFabricator(),
		BuildBerculaForVantom(),
		BuildBioAutomatonForDevotedSculptor(),
		BuildBladeIdentifierForEyeWithTeeth(),
		BuildBloodbugForSludgeSpinner(),
		BuildBoltRunnerForBruteRubyRaider(),
		BuildBomberplasmForTheForgotten(),
		BuildBrokenDreamShardForMyte(),
		BuildCavalryAutomataForBygoneEffigy(),
		BuildCrimsonOracleForTurretOperator(),
		BuildCultistArbiterForMagiKnight(),
		BuildDarkimpForLivingFog(),
		BuildDeathchromeForTestSubject(),
		BuildDebolessaForLagavulinMatriarch(),
		BuildDevouredBeingForGasBomb(),
		BuildDibellumForTheObscura(),
		BuildDominionForMechaKnight(),
		BuildDualEdgeLepturusForDampCultist(),
		BuildDustInsectForBowlbugSilk(),
		BuildEvilDullahanForSpinyToad(),
		BuildBladeSoldierForExoskeleton(),
		BuildFairyBoggartForOvicopter(),
		BuildFairyChangelingForToughEgg(),
		BuildFairyGremlinForCorpseSlug(),
		BuildFairyNoxForInklet(),
		BuildFairySprigganForVineShambler(),
		BuildFallaciaForSoulNexus(),
		BuildFirstReplicaForMawler(),
		BuildFlyingVampireForThievingHopper(),
		BuildGiantSoldierForSewerClam(),
		BuildGloomyBeastForScrollOfBiting(),
		BuildGloomyGodfatherForTheLost(),
		BuildGoldenManeForWaterfallGiant(),
		BuildGolemAutomataForPunchConstruct(),
		BuildHaloAutomataForChomper(),
		BuildHeavyArmorInsectForBowlbugRock(),
		BuildHerbalHydracleForFlyconid(),
		BuildHexerForCrossbowRubyRaider(),
		BuildHoneyJarForBowlbugNectar(),
		BuildHowlingProdigalForKnowledgeDemon(),
		BuildHumanShadowForSpectralKnight(),
		BuildHunchbackForSneakyGremlin(),
		BuildHussarAutomataForFrogKnight(),
		BuildHydracleForSnappingJaxfruit(),
		BuildInconvenientDinerForPhrogParasite(),
		BuildKentrisForHauntedShip(),
		BuildLepidolaForEntomancer(),
		BuildLightDevourerForSlitheringStrangler(),
		BuildLostHungerForLouseProgenitor(),
		BuildMaggotForKinFollower(),
		BuildManEaterForGremlinMerc(),
		BuildMegaBiteForFuzzyWurmCrawler(),
		BuildPaleServantForWriggler(),
		BuildPollemicaForCrusher(),
		BuildRackerForCubexConstruct(),
		BuildRechargerForGuardbot(),
		BuildRechargerForNoisebot(),
		BuildRechargerForStabbot(),
		BuildRechargerForZapbot(),
		BuildResentfulConvictForTheInsatiable(),
		BuildRockShellInsectForSkulkingColony(),
		BuildRottenReaperForKinPriest(),
		BuildRustyNailSevenArmsForTorchHeadAmalgam(),
		BuildSaintHoundForLivingShield(),
		BuildSandwormForTunneler(),
		BuildScratcherForParafright(),
		BuildSenectusForSoulFysh(),
		BuildShadowCollectorForQueen(),
		BuildShapeshifterForTwoTailedRat(),
		BuildBareBeetleForShrinkerBeetle(),
		BuildSingleEdgeLepturusForCalcifiedCultist(),
		BuildSlashghoulForFossilStalker(),
		BuildSlaughterForInfestedPrism(),
		BuildSlaveGhoulForLeafSlimeM(),
		BuildSnipperForAssassinRubyRaider(),
		BuildSoldierPrimeForHunterKiller(),
		BuildSpawningInsectForBowlbugEgg(),
		BuildStealerForFatGremlin(),
		BuildStrangeAlauneForSeapunk(),
		BuildStrongAppetiteForNibbit(),
		BuildSuppressedServantForSlimedBerserker(),
		BuildTemplarForAxebot(),
		BuildThornBloodbugForToadpole(),
		BuildStalkerForTrackerRubyRaider(),
		BuildTrapperForFlailKnight(),
		BuildTrapperForMysteriousKnight(),
		BuildBerserkGhoulForTwigSlimeM(),
		BuildGhoulForTwigSlimeS(),
		BuildUnansweredForAxeRubyRaider(),
		BuildUndertakerGhoulForLeafSlimeS(),
		BuildVenomswifterForByrdonis(),
		BuildViadiusForAeonglass(),
		BuildVineLordForFogmog(),
		BuildVoidHungerForTerrorEel(),
		BuildWanderingPollemicaForRocket(),
		BuildWatcherForbiddenTomeForGlobeHead(),
		BuildWingedHumanfaceForOwlMagistrate(),
		BuildWolfSlayerForSlumberingBeetle()
	});

	private static EnemyReplacementProfile BuildAbyssInsectForDecimillipedeSegmentBack()
	{
		return new EnemyReplacementProfile("abyss_insect_for_decimillipede_segment_back", new Sts2EnemyTarget("DECIMILLIPEDE_SEGMENT_BACK"), new CznEnemySource("1001020_01", "1001020", "monster@name@1001020_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/decimillipede_segment_back.tscn", "res://CznEnemySkin/animations/monsters/1001020_abyss_insect/1001020_abyss_insect_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.3f), new EnemySelectionBounds(-170.38164f, -272.26935f, 293.78693f, 297.44818f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[7]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "segment_writhe", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "segment_bulk", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "segment_constrict", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.WakeUp, "regenerate", "normal_enter", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "wither", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[5]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.3f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_skill2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/particles/abyss_insect_par1_1000ms.tscn", null, -30.21f, 248.37f, 1f, 1f, 0f, 40, 0.5f, 2.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_skill3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 0.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill3_smoke_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 20, 0f, 1.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill3_smoke_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -30, 0f, 1.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.WakeUp, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_enter.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[7] { "NDecimillipedeSegmentVfx", "fxnode2back", "SegmentDriver", "fxnode1", "fxnode2", "fxnode3", "fxnode4" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[4]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_triple", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_buff", EnemyAction.Buff),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_weaken", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_heal", EnemyAction.WakeUp)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[3]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Buff,
			EnemyAction.Cast
		}), null, useNativeHostBounds: false, EnemyBodyOriginPolicy.ResetToSkeletonOrigin), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildAbyssInsectForDecimillipedeSegmentFront()
	{
		return new EnemyReplacementProfile("abyss_insect_for_decimillipede_segment_front", new Sts2EnemyTarget("DECIMILLIPEDE_SEGMENT_FRONT"), new CznEnemySource("1001020_01", "1001020", "monster@name@1001020_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/decimillipede_segment_front.tscn", "res://CznEnemySkin/animations/monsters/1001020_abyss_insect/1001020_abyss_insect_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.3f), new EnemySelectionBounds(-170.38164f, -272.26935f, 293.78693f, 297.44818f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[7]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "segment_writhe", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "segment_bulk", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "segment_constrict", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.WakeUp, "regenerate", "normal_enter", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "wither", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[5]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.3f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_skill2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/particles/abyss_insect_par1_1000ms.tscn", null, -30.21f, 248.37f, 1f, 1f, 0f, 40, 0.5f, 2.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_skill3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 0.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill3_smoke_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 20, 0f, 1.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill3_smoke_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -30, 0f, 1.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.WakeUp, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_enter.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[6] { "NDecimillipedeSegmentVfx", "fxnode2back", "SegmentDriver", "fxnode1", "fxnode2", "fxnode4" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[4]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_triple", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_buff", EnemyAction.Buff),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_weaken", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_heal", EnemyAction.WakeUp)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[3]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Buff,
			EnemyAction.Cast
		}), null, useNativeHostBounds: false, EnemyBodyOriginPolicy.ResetToSkeletonOrigin), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildAbyssInsectForDecimillipedeSegmentMiddle()
	{
		return new EnemyReplacementProfile("abyss_insect_for_decimillipede_segment_middle", new Sts2EnemyTarget("DECIMILLIPEDE_SEGMENT_MIDDLE"), new CznEnemySource("1001020_01", "1001020", "monster@name@1001020_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/decimillipede_segment_middle.tscn", "res://CznEnemySkin/animations/monsters/1001020_abyss_insect/1001020_abyss_insect_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.3f), new EnemySelectionBounds(-170.38164f, -272.26935f, 293.78693f, 297.44818f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[7]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "segment_writhe", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "segment_bulk", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "segment_constrict", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.WakeUp, "regenerate", "normal_enter", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "wither", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[5]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.3f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_skill2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/particles/abyss_insect_par1_1000ms.tscn", null, -30.21f, 248.37f, 1f, 1f, 0f, 40, 0.5f, 2.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_skill3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 0.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill3_smoke_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 20, 0f, 1.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001020/spine/abyss_insect_skill3_smoke_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -30, 0f, 1.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.WakeUp, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_enter.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001020/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[8] { "NDecimillipedeSegmentVfx", "fxnode2back", "RightSegmentDriver", "LeftSegmentDriver", "fxnode1", "fxnode2", "fxnode3", "fxnode4" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[4]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_triple", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_buff", EnemyAction.Buff),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_attack_weaken", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/decimillipede/decimillipede_heal", EnemyAction.WakeUp)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[3]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Buff,
			EnemyAction.Cast
		}), null, useNativeHostBounds: false, EnemyBodyOriginPolicy.ResetToSkeletonOrigin), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildAlbithumForPhantasmalGardener()
	{
		return new EnemyReplacementProfile("albithum_for_phantasmal_gardener", new Sts2EnemyTarget("PHANTASMAL_GARDENER"), new CznEnemySource("1001011_01", "1001011", "monster@name@1001011_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/phantasmal_gardener.tscn", "res://CznEnemySkin/animations/monsters/1001011_albithum/1001011_albithum_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.3f), new EnemySelectionBounds(-160.11166f, -390.0221f, 351.46863f, 357.78726f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_multi", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "buff", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001011/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001011/spine/albithum_normal_skill1_f3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.3f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001011/spine/albithum_normal_skill1_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001011/spine/albithum_normal_skill1_b.tscn", "animation", 0f, -20f, 0.7f, 0.7f, 0f, -1, 0f, 1.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001011/spine/albithum_normal_skill1_f2.tscn", "animation", 0f, -20f, 0.7f, 0.7f, 0f, 2, 0f, 1.3f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_pierce_normal_s.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 0, 0f, 1.1f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001011/normal_skill2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001011/spine/albithum_normal_skill2_b.tscn", "animation", 0f, -10f, 0.5f, 0.5f, 0f, -1, 0f, 1.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001011/spine/albithum_normal_skill2_f3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 0.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001011/spine/albithum_normal_skill2_f2.tscn", "animation", 0f, -10f, 0.5f, 0.5f, 0f, 2, 0f, 1.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001011/spine/albithum_normal_skill2_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1.5f, 1.5f, -80f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001011/normal_skill3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001011/spine/albithum_normal_skill3_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 1.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001011/spine/albithum_normal_skill3_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001011/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "SpewSlotNode", "NPhantasmalGardenersVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_attack_bite", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_attack_lick", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_buff", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildBeamShooterForFabricator()
	{
		return new EnemyReplacementProfile("beam_shooter_for_fabricator", new Sts2EnemyTarget("FABRICATOR"), new CznEnemySource("1003013_01", "1003013", "monster@name@1003013_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/fabricator.tscn", "res://CznEnemySkin/animations/monsters/1003013_beam_shooter/1003013_beam_shooter_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.2f), new EnemySelectionBounds(-150.10655f, -369.17892f, 308.61185f, 390.48743f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "fabricate", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003013/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003013/spine/beamshooter_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 0.4667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003013/spine/beamshooter_attack_target.tscn", "animation", 0f, 0f, 0.54f, 0.54f, 0f, 3, 0f, 0.3667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003013/particles/beamshooter_attack_target_pati_01_667ms.tscn", null, 0f, 0f, 1.08f, 1.08f, 0f, 5, 0f, 1.0167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003013/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003013/particles/racker_debuff_01_pati_1067ms.tscn", null, 20f, 150f, 1.8f, 1.8f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003013/particles/racker_debuff_01_pati_467ms.tscn", null, 20f, 200f, 1.8f, 1.8f, 0f, 5, 1.1f, 1.4667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003013/particles/racker_debuff_02_pati_1067ms.tscn", null, 0f, 200f, 1.5f, 1.5f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003013/spine/beamshooter_debuff_ball_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.7f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003013/spine/beamshooter_debuff_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.4667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003013/spine/beamshooter_debuff_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -3, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003013/spine/beamshooter_debuff_ball_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -5, 0f, 1.6667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003013/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/fabricator/fabricator_attack", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildBerculaForVantom()
	{
		return new EnemyReplacementProfile("bercula_for_vantom", new Sts2EnemyTarget("VANTOM"), new CznEnemySource("1001021_01", "1001021", "monster@name@1001021_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/vantom.tscn", "res://CznEnemySkin/animations/monsters/1001021_bercula/1001021_bercula_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-186.56108f, -342.75824f, 467.71185f, 347.1086f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[11]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "stance1_idle", Loop: true, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_double", "normal_m_attack_01", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Cast, "attack_heavy", "stance1_m_attack_01_to_normal", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Buff, "buff", "normal_buff_2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Charge, "charge_up", "normal_buff_1_to_stance1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false, EnemyPresentationStance.Transformed)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[7]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001021/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001021/spine/bercula_normal_skill3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 2.1333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null))),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001021/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001021/spine/bercula_normal_skill2_smoke_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 40, 0f, 1.6f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001021/spine/bercula_normal_skill2_smoke_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -30, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001021/spine/bercula_normal_skill2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.6f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1.5f, 1.5f, -170f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 100f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001021/normal_buff_2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001021/spine/bercula_normal_skill1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.4f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001021/spine/bercula_normal_skill1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -30, 0f, 1.4f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001021/stance1_m_attack_01_to_normal.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001021/spine/bercula_normal_skill4_3_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.8333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001021/spine/bercula_normal_skill4_3_f.tscn", "animation", 0f, -60f, 0.7f, 0.7f, 0f, 1, 0f, 2.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001021/spine/bercula_normal_skill4_3_smoke_b.tscn", "animation", 0f, -70f, 0.8f, 0.8f, 0f, -1, 0f, 2.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1.8f, 1.8f, -20f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Charge, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001021/normal_buff_1_to_stance1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001021/spine/bercula_normal_skill4_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.3667f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001021/spine/bercula_normal_skill4_2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 1.33f, 3.6f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001021/normal_death.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001021/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[11]
		{
			"MegatailSlotNode", "SprayBoneNode", "DeathSprayBoneNode", "NVantomVfx", "ScalingBone", "DeathSpraySlotNode", "DeathSprayBackSlotNode", "TailSlotNode", "TailBladeSlotNode", "MegabladeSlotNode",
			"DeathExplosionSlotNode"
		}), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[5]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/vantom/vantom_inky_lance", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/vantom/vantom_dismember", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/vantom/vantom_buff", EnemyAction.Buff),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/vantom/vantom_extend_1", EnemyAction.Charge),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/vantom/vantom_extend_2", EnemyAction.Charge)
		}), new _003C_003Ez__ReadOnlyArray<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression[2]
		{
			new EnemyNativeVfxPathSuppression("vfx/vfx_attack_blunt", EnemyAction.BasicAttack),
			new EnemyNativeVfxPathSuppression("vfx/vfx_giant_horizontal_slash", EnemyAction.Cast)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxFeatureSuppression>(new EnemyNativeVfxFeatureSuppression(EnemyNativeVfxFeature.RadialBlur, EnemyAction.Cast)), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.Cast,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildBioAutomatonForDevotedSculptor()
	{
		return new EnemyReplacementProfile("bio_automaton_for_devoted_sculptor", new Sts2EnemyTarget("DEVOTED_SCULPTOR"), new CznEnemySource("3000384_01", "3000384", "monster@name@3000384_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/devoted_sculptor.tscn", "res://CznEnemySkin/animations/monsters/3000384_bio_automaton/3000384_bio_automaton_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-224.80688f, -394.72913f, 433.09445f, 412.76886f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_buff", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000384/normal_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[10]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000384/spine/3000384_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0.4667f, 0.8f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000384/particles/3000384_attack_pati_667ms.tscn", null, -95f, 377f, -0.7f, 0.5f, 15f, 5, 0.4667f, 1.1667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000384/spine/3000384_normal_attack_play2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 1.1334f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000384/particles/3000384_attack_pati2_667ms.tscn", null, 0f, 289f, 0.6f, 0.4f, -11f, 5, 1.1334f, 1.1667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_02.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 39f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_03.tscn", "animation2", 0f, -45f, 0.55f, 0.55f, 43f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_03.tscn", "animation2", 0f, 55f, 0.55f, 0.55f, 33f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_02.tscn", "animation", 0f, 0f, 1f, 1f, -110f, 0, 0.6667f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_03.tscn", "animation2", 0f, 65f, 0.55f, 0.55f, -105f, 0, 0.6667f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_03.tscn", "animation2", 0f, -65f, 0.55f, 0.55f, -115f, 0, 0.6667f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlyArray<EnemyAudioLayerCue>(new EnemyAudioLayerCue[3]
			{
				new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000384/normal_attack_play1.ogg", 0.4667f),
				new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000384/normal_attack_play2.ogg", 1.1334f),
				new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000384/normal_attack_end.ogg", 1.8001f)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000384/normal_buff.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000384/spine/3000384_normal_buff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 2.0333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000384/particles/3000384_buff_pati_1200ms.tscn", null, 0f, 155f, 1f, 1f, 0f, 0, 0.411f, 2.55f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000384/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "VoiceBoneNode", "AttackParticles", "NDevotedSculptorVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/devoted_sculptor/devoted_sculptor_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/devoted_sculptor/devoted_sculptor_cast", EnemyAction.Cast)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_scream", EnemyAction.Cast)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildBladeIdentifierForEyeWithTeeth()
	{
		return new EnemyReplacementProfile("blade_identifier_for_eye_with_teeth", new Sts2EnemyTarget("EYE_WITH_TEETH"), new CznEnemySource("1006020_01", "1006020", "monster@name@1006020_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/eye_with_teeth.tscn", "res://CznEnemySkin/animations/monsters/1006020_blade_identifier/1006020_blade_identifier_skel_data.tres", CznEnemyVisualScalePolicy.ResolveSts2HostFit(1f, 1.9341292f), new EnemySelectionBounds(-69.3159f, -198.6154f, 139.5977f, 157.6076f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[4]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006020/normal_skill2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006020/spine/blade_identifier_skill2_self.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006020/particles/blade_identifier_skill2_self_1000ms.tscn", null, -69.158f, 111.36f, 1f, 1f, 0f, 2, 0.5f, 2.4f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006020/spine/blade_identifier_skill2_target.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0.5f, 1.4333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006020/particles/official_normal_skill2_target_01.tscn", null, 0f, 0f, 1f, 1f, 0f, 2, 0.5f, 2.5333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006020/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(Array.Empty<EnemyNativeAudioSuppression>(), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildBloodbugForSludgeSpinner()
	{
		return new EnemyReplacementProfile("bloodbug_for_sludge_spinner", new Sts2EnemyTarget("SLUDGE_SPINNER"), new CznEnemySource("1001008_01", "1001008", "monster@name@1001008_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/sludge_spinner.tscn", "res://CznEnemySkin/animations/monsters/1001008_bloodbug/1001008_bloodbug_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-199.2063f, -362.8421f, 393.24408f, 339.93378f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "slam", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "spray", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001008/normal_skill1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001008/spine/killer_fly_skill1_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.2333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001008/normal_skill3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001008/spine/killer_fly_normal_skill3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 20, 0f, 1.0333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001008/spine/killer_fly_normal_skill3_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -30, 0f, 1.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001008/spine/killer_fly_normal_skill3_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001008/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[6] { "MouthSpraySlot", "HoverHeightAdjust", "TargetingDistanceBone", "MouthDribbleBoneNode", "testBone", "NOilSpillVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/sludge_spinner/sludge_spinner_attack_dash", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/sludge_spinner/sludge_spinner_attack_spin", EnemyAction.Cast)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_blunt", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), Array.Empty<EnemyAction>()), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildBoltRunnerForBruteRubyRaider()
	{
		return new EnemyReplacementProfile("bolt_runner_for_brute_ruby_raider", new Sts2EnemyTarget("BRUTE_RUBY_RAIDER"), new CznEnemySource("1002003_01", "1002003", "monster@name@1002003_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/brute_ruby_raider.tscn", "res://CznEnemySkin/animations/monsters/1002003_bolt_runner/1002003_bolt_runner_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-109.163605f, -314.34283f, 245.75212f, 333.45212f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_defense_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002003/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002003/spine/bolt_runner_attack_01_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002003/spine/bolt_runner_attack_01_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 10f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002003/normal_defense_1.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002003/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/brute_ruby_raider/brute_ruby_raider_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/brute_ruby_raider/brute_ruby_raider_cast", EnemyAction.Cast)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_blunt", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildBomberplasmForTheForgotten()
	{
		return new EnemyReplacementProfile("bomberplasm_for_the_forgotten", new Sts2EnemyTarget("THE_FORGOTTEN"), new CznEnemySource("1004016_01", "1004016", "monster@name@1004016_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/the_forgotten.tscn", "res://CznEnemySkin/animations/monsters/1004016_bomberplasm/1004016_bomberplasm_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.25f), new EnemySelectionBounds(-181.69075f, -348.67938f, 355.97742f, 366.1436f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Cast, "debuff", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004016/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004016/spine/bomberplasm_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 0.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", -40f, 0f, 1f, 1f, -200f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, -200f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 40f, 0f, 1f, 1f, -200f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004016/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004016/spine/bomberplasm_normal_debuff_1_target_b.tscn", "animation", 200f, 0f, 0.65f, 0.65f, 0f, 0, 0f, 1.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004016/spine/bomberplasm_normal_debuff_1_target_f.tscn", "animation", 200f, 0f, 0.65f, 0.65f, 0f, 2, 0f, 2.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004016/normal_enter.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004016/spine/bomberplasm_normal_enter.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[5] { "GranuleEmitterBone", "SpineSlotNode3", "SpineSlotNode2", "SpineSlotNode", "NLostAndForgottenVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(Array.Empty<EnemyNativeAudioSuppression>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_blunt", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Cast
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildBrokenDreamShardForMyte()
	{
		return new EnemyReplacementProfile("broken_dream_shard_for_myte", new Sts2EnemyTarget("MYTE"), new CznEnemySource("3000441_80", "3000441", "monster@name@3000441_80"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/myte.tscn", "res://CznEnemySkin/animations/monsters/3000441_broken_dream_shard/3000441_broken_dream_shard_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-149.52327f, -246.72708f, 324.77936f, 239.43507f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "suck", "normal_buff_2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[5]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000441/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/spine/3000441_fragment_of_broken_dream_glow.tscn", "normal_m_attack_01", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/particles/broken_dream_shard_attack_pat2_1133ms.tscn", null, -250f, 140f, 1f, 1f, 0f, 20, 0.6f, 1.6333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/particles/broken_dream_shard_attack_pat_1133ms.tscn", null, -60f, 140f, 1f, 2.1f, 0f, 20, 0.6f, 1.6333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/spine/3000441_fragment_of_broken_dream_normal_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 15, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/spine/3000441_fragment_of_broken_dream_normal_m_attack_02.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 10, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/spine/3000441_fragment_of_broken_dream_normal_m_attack_03.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_poison_s.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 0.6f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000441/normal_buff_2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/spine/3000441_fragment_of_broken_dream_glow.tscn", "normal_buff_2", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/particles/broken_dream_shard_buff2_pati1_867ms.tscn", null, 0f, 270f, 1f, 1f, 0f, 40, 1f, 1.3667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/spine/3000441_fragment_of_broken_dream_normal_buff_2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 20, 0f, 1.4333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000441/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/spine/3000441_fragment_of_broken_dream_glow.tscn", "normal_buff_1", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/particles/broken_dream_shard_buff1_pati1_1233ms.tscn", null, 0f, 40f, 0.6f, 0.6f, 0f, 15, 0.5f, 1.7333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/spine/3000441_fragment_of_broken_dream_normal_buff_1_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 20, 0f, 1.3f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/spine/3000441_fragment_of_broken_dream_normal_buff_1_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 10, 0f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/spine/3000441_fragment_of_broken_dream_normal_buff_1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -10, 0f, 1.3f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Hurt, Array.Empty<EnemyAudioCue>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/spine/3000441_fragment_of_broken_dream_glow.tscn", "normal_hit", 0f, 0f, 1f, 1f, 0f, -2, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000441/normal_enter.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000441/spine/3000441_fragment_of_broken_dream_glow.tscn", "normal_enter", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "TargetBone", "NMyteVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/mite/mite_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/mite/mite_cast", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/mite/mite_suck", EnemyAction.HeavyAttack)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_bite", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildCavalryAutomataForBygoneEffigy()
	{
		return new EnemyReplacementProfile("cavalry_automata_for_bygone_effigy", new Sts2EnemyTarget("BYGONE_EFFIGY"), new CznEnemySource("1003015_01", "1003015", "monster@name@1003015_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/bygone_effigy.tscn", "res://CznEnemySkin/animations/monsters/1003015_cavalry_automata/1003015_cavalry_automata_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.25f), new EnemySelectionBounds(-172.54187f, -390.86893f, 316.51434f, 395.8399f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003015/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003015/particles/automata_cavalry_attack_pati01_667ms.tscn", null, -150f, 250f, 1f, 0.4f, -40f, 4, 0.03f, 1.4667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003015/particles/automata_cavalry_attack_pati02_667ms.tscn", null, -360f, 0f, 0.8f, 0.8f, 0f, 4, 0f, 1.6667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003015/spine/automata_cavalry_normal_attack_play1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003015/spine/automata_cavalry_normal_attack_play1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 0.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.4f, 0.4f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.4f, 0.4f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003015/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003015/spine/automata_cavalry_normal_buff_1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.2333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003015/spine/automata_cavalry_normal_buff_1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.2333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			}))
		}), new _003C_003Ez__ReadOnlySingleElementList<string>("SpineBoneNode"), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/bygone_effigy/bygone_effigy_attack", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildCrimsonOracleForTurretOperator()
	{
		return new EnemyReplacementProfile("crimson_oracle_for_turret_operator", new Sts2EnemyTarget("TURRET_OPERATOR"), new CznEnemySource("1003009_01", "1003009", "monster@name@1003009_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/turret_operator.tscn", "res://CznEnemySkin/animations/monsters/1003009_crimson_oracle/1003009_crimson_oracle_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-102.83647f, -399.32962f, 210.2093f, 406.51962f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "crank", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003009/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003009/particles/oarcle_attack_fire_pati_967ms.tscn", null, -30f, 200f, 1f, 1f, 0f, 5, 0.533f, 1.467f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003009/spine/oracle_attack2.tscn", "fire", 0f, 0f, 1f, 1f, 0f, 4, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003009/spine/oracle_attack1.tscn", "fire", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.9667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003009/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003009/particles/oracle_debuff_fire_pati01_967ms.tscn", null, 0f, 380f, 1f, 0.3f, -10f, 4, 0.533f, 1.467f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003009/particles/oracle_debuff_fire_pati02_967ms.tscn", null, 0f, 380f, 1f, 0.3f, -10f, 3, 0.533f, 1.867f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003009/spine/oracle_debuff.tscn", "fire", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003009/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/turret_operator/turret_operator_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/turret_operator/turret_operator_buff", EnemyAction.Buff)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildCultistArbiterForMagiKnight()
	{
		return new EnemyReplacementProfile("cultist_arbiter_for_magi_knight", new Sts2EnemyTarget("MAGI_KNIGHT"), new CznEnemySource("1002006_01", "1002006", "monster@name@1002006_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/magi_knight.tscn", "res://CznEnemySkin/animations/monsters/1002006_cultist_arbiter/1002006_cultist_arbiter_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.2f), new EnemySelectionBounds(-185.94733f, -441.2034f, 375.79102f, 484.4099f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "RamAttack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "BombCast", "normal_strong_attack_play", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "ShieldAttack", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002006/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002006/spine/cultist_arbiter_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002006/spine/cultist_arbiter_normal_attack_play1_target_f.tscn", "animation", -20f, -15f, 0.83f, 0.83f, 0f, 2, 0f, 0.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002006/normal_strong_attack_play.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002006/spine/cultist_arbiter_normal_strong_attack_play.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002006/spine/cultist_arbiter_normal_strong_attack_play_blood.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002006/particles/cultist_arbiter_normal_strong_attack_play_pati01_500ms.tscn", null, -90f, 200f, 1f, 1f, -20f, 4, 0f, 1.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002006/particles/cultist_arbiter_normal_strong_attack_play_pati02_500ms.tscn", null, -110f, 200f, 1f, 1f, -20f, 4, 0.25f, 1.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.55f, 0.55f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.55f, 0.55f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002006/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002006/spine/cultist_arbiter_normal_buff_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002006/particles/cultist_arbiter_normal_buff_1_pati01_1933ms.tscn", null, -50f, 50f, 0.2f, 0.4f, 0f, 4, 0.65f, 2.9333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002006/particles/cultist_arbiter_normal_buff_1_pati01_1933ms.tscn", null, -50f, 150f, 0.2f, 0.4f, 0f, 4, 0.75f, 2.9333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, Array.Empty<EnemyAudioCue>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002006/spine/cultist_arbiter_normal_enter.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 2.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "SpineSlotNode", "AttackDistanceControl" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/magi_knight/magi_knight_attack_ram", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/magi_knight/magi_knight_attack_bomb", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/magi_knight/magi_knight_cast_shield", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildDarkimpForLivingFog()
	{
		return new EnemyReplacementProfile("darkimp_for_living_fog", new Sts2EnemyTarget("LIVING_FOG"), new CznEnemySource("1004010_01", "1004010", "monster@name@1004010_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/living_fog.tscn", "res://CznEnemySkin/animations/monsters/1004010_darkimp/1004010_darkimp_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-119.62575f, -471.86377f, 296.72723f, 399.86154f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "debuff", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "spawn_bomb", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004010/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[9]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004010/particles/darkimp_attack_pati_333ms.tscn", null, -150f, 240f, 1f, 1f, -4f, 7, 0.967f, 1.333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004010/particles/sarah_job_attack_play_01_pati_333ms.tscn", null, -130f, 250f, 0.5f, 0.7f, 0f, 6, 0.967f, 1.333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004010/particles/darkimp_attack_pati_867ms.tscn", null, -300f, 300f, 1f, 1f, 0f, 5, 0.433f, 1.867f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004010/particles/sarah_job_attack_play_01_pati_867ms.tscn", null, -156f, 320f, 0.4f, 0.8f, 0f, 4, 0.433f, 1.867f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004010/spine/darkimp_attack.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.3f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0.56f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0.56f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004010/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004010/spine/darkimp_debuff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004010/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004010/spine/darkimp_debuff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004010/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[5] { "SmokeSlot3", "SmokeSlot2", "SmokeSlot1", "AttackFXNode", "NEvilGasVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/living_fog/living_fog_attack_blow", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/living_fog/living_fog_summon", EnemyAction.Buff)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildDeathchromeForTestSubject()
	{
		return new EnemyReplacementProfile("deathchrome_for_test_subject", new Sts2EnemyTarget("TEST_SUBJECT"), new CznEnemySource("3000479_01", "3000479", "monster@name@3000479_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/test_subject.tscn", "res://CznEnemySkin/animations/monsters/3000479_deathchrome/3000479_deathchrome_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.3f), new EnemySelectionBounds(-333.90393f, -531.12354f, 643.8296f, 553.6807f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[13]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "stance1_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "TestSubjectPhase1Attack", "stance1_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "TestSubjectPhase2Attack", "stance3_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "TestSubjectPhase3Multi", "stance5_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Special, "TestSubjectPhase3Pounce", "stance5_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "TestSubjectPhase3Burn", "stance5_defense_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Charge, "TestSubjectInitialTransform", "normal_to_stance1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.StunTransition, "TestSubjectKnockout1", "stance1_to_stance2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.WakeUp, "TestSubjectRespawn2", "stance2_to_stance3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.ChargeDeath, "TestSubjectKnockout2", "stance3_to_stance4", Loop: false),
			new EnemyAnimationBinding(EnemyAction.ChargeRecovery, "TestSubjectRespawn3", "stance4_to_stance5", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "stance1_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "stance5_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[12]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000479/stance1_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance1_attack_play2_b2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -4, 0.4667f, 0.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance1_attack_play2_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0.4667f, 1.2333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance1_attack_play2_b1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0.4667f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance1_attack_play2_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0.4667f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000479/stance1_attack_play2.ogg", 0.4667f))),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000479/stance3_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance1_attack_play1_b1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0.4667f, 2.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance1_attack_play1_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0.4667f, 2.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance1_attack_play1_b2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -4, 0.4667f, 0.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance1_attack_play1_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0.4667f, 1.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000479/stance3_attack_play1.ogg", 0.4667f))),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000479/stance5_defense_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_defense_1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 2.1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_defense_1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000479/stance5_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[10]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play1_f9.tscn", "animation", 2.8664f, 213.45f, 0.95f, 0.95f, 0f, 2, 0.4667f, 0.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play1_f8.tscn", "animation", 2.8664f, 213.45f, 0.95f, 0.95f, 0f, 3, 0.4667f, 0.8f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play1_f7.tscn", "animation", 2.8664f, 213.45f, 0.95f, 0.95f, 0f, 4, 0.4667f, 0.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play1_f6.tscn", "animation", 2.8664f, 213.45f, 1f, 1f, 0f, 5, 0.4667f, 0.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play1_f3.tscn", "animation", 2.8664f, 213.45f, 1.05f, 1.05f, 2.5f, 6, 0.4667f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play1_f2.tscn", "animation", 2.8664f, 213.45f, 0.8f, 0.8f, 0f, 7, 0.4667f, 0.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play1_f1.tscn", "animation", 2.8664f, 213.45f, 1f, 1f, 0f, 9, 0.4667f, 0.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play12_f10.tscn", "animation", 0f, 0f, 0.9f, 0.9f, 0f, 1, 0.4667f, 1.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_target_b.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -202, 0f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_target_f.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 202, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: true)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000479/stance5_attack_play1.ogg", 0.4667f))),
			new EnemyActionPresentation(EnemyAction.Special, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000479/stance5_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[10]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play12_f10.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0.4667f, 1.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play2_f8.tscn", "animation", 46.358f, 201.17f, 1.05f, 1.05f, 0f, 3, 0.4667f, 0.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play2_f7.tscn", "animation", 46.358f, 201.17f, 1.05f, 1.05f, 0f, 4, 0.4667f, 0.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play2_f6.tscn", "animation", 46.358f, 201.17f, 1.05f, 1.05f, 0f, 5, 0.4667f, 0.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play2_f5.tscn", "animation", 46.358f, 201.17f, 1.05f, 1.05f, 0f, 6, 0.4667f, 0.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play2_f4.tscn", "animation", 46.358f, 201.17f, 0.95f, 0.95f, -2.5f, 7, 0.4667f, 0.8f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play2_f2.tscn", "animation", 46.358f, 201.17f, 0.9f, 0.9f, 0f, 8, 0.4667f, 0.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 0.5f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_play2_f1.tscn", "animation", 46.358f, 201.17f, 1.05f, 1.05f, 0f, 9, 0.4667f, 0.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_target_b.tscn", "animation", -20f, -15f, 0.75f, 0.75f, 0f, -202, 0f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_attack_target_f.tscn", "animation", -20f, -15f, 0.75f, 0.75f, 0f, 202, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: true)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000479/stance5_attack_play2.ogg", 0.4667f))),
			new EnemyActionPresentation(EnemyAction.Charge, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000479/normal_to_stance1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stancen1_23_b1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stancen1_23_b2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -4, 0f, 2.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stancen1_23_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.ChargeRecovery, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000479/stance4_to_stance5.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance4_to_stance5_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -4, 0f, 1.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance4_to_stance5_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 8, 0f, 2.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance4_to_stance5_f2.tscn", "animation2", 0f, 0f, 1f, 1f, 0f, 6, 0f, 2.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance45_b1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 3.7f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stancen45_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 2.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stancen45_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 2.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.ChargeDeath, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000479/stance3_to_stance4.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance12_b1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 2.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance12_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance12_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 1.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.StunTransition, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000479/stance1_to_stance2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance12_b1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 2.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance12_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance12_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 1.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.WakeUp, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000479/stance2_to_stance3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stancen1_23_b2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -4, 0f, 2.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stancen1_23_b1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stancen1_23_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000479/stance5_death.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_death_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 3.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_death_f3.tscn", "animation2", 0f, 0f, 1f, 1f, 0f, 2, 0f, 2.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_death_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 2.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_stance5_death_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 3.4333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000479/normal_enter.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_normal_enter_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_normal_enter_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 2.1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000479/spine/deathchrome_normal_enter_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 2.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			}))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[4] { "BackBurnVfxSlot", "NeckParticlesSlot", "FrontBurnVfxSlot", "NTestSubjectVfx" }), new _003C_003Ez__ReadOnlyArray<string>(new string[6] { "BurnFire1", "BurnFire2", "BurnFire3", "FlameParticles", "EmberParticles", "BurnParticleContainer" }), Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[5]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/test_subject/test_subject_bite", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/test_subject/test_subject_slash", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/test_subject/test_subject_knock_out", EnemyAction.StunTransition),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/test_subject/test_subject_revive_two_heads", EnemyAction.WakeUp),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/test_subject/test_subject_revive_three_heads", EnemyAction.ChargeRecovery)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[4]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Cast,
			EnemyAction.HeavyAttack,
			EnemyAction.Special
		}), null, useNativeHostBounds: false, EnemyBodyOriginPolicy.InheritNativeOffset, EnemyHostTransformPolicy.ResetParentTransform), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildDebolessaForLagavulinMatriarch()
	{
		return new EnemyReplacementProfile("debolessa_for_lagavulin_matriarch", new Sts2EnemyTarget("LAGAVULIN_MATRIARCH"), new CznEnemySource("1007021_02", "1007021", "monster@name@1007021_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/lagavulin_matriarch.tscn", "res://CznEnemySkin/animations/monsters/1007021_debolessa/1007021_debolessa_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.2f), new EnemySelectionBounds(-253.4776f, -501.39435f, 571.22034f, 492.51672f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[13]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "sleep_loop", "stance2_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt_sleeping", "stance1_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.WakeUp, "Wake", "stance1_to_normal", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_heavy", "normal_attack_play1", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_double", "normal_attack_play2", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Buff, "PhaseTwo", "normal_to_stance1", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Special, "attack_heavy", "stance1_attack_play1", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Charge, "attack_double", "stance1_attack_play2", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_debuff_1", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Death, "Dead", "normal_death", Loop: false, EnemyPresentationStance.Transformed)
		}), null, new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[8]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, Array.Empty<EnemyAudioCue>(), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[9]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0.6333f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_b.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, -120, 0.6333f, 1.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_f1.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 120, 0.6333f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_f2.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 110, 0.6333f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_f3.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 102, 0.6333f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_f4.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 1, 0.6333f, 0.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/bulk_deforsa_normal_attack_target_play_5.tscn", null, 0f, 0f, 0.8f, 0.8f, 0f, 2, 0.6333f, 3.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", -20f, -20f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", -20f, -20f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1007021/normal_attack_play1.ogg", 0.6333f))),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, Array.Empty<EnemyAudioCue>(), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[18]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0.6333f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_play2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 1.4666f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_b.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, -120, 0.6333f, 1.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_f1.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 120, 0.6333f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_f2.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 110, 0.6333f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_f3.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 102, 0.6333f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_f4.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 1, 0.6333f, 0.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/bulk_deforsa_normal_attack_target_play_5.tscn", null, 0f, 0f, 0.8f, 0.8f, 0f, 2, 0.6333f, 3.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_b.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, -120, 1.4666f, 1.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_f1.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 120, 1.4666f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_f2.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 110, 1.4666f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_f3.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 102, 1.4666f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_attack_target_play_f4.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 1, 1.4666f, 0.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/bulk_deforsa_normal_attack_target_play_5.tscn", null, 0f, 0f, 0.8f, 0.8f, 0f, 2, 1.4666f, 3.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 20f, 20f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 20f, 20f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", -20f, -20f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", -20f, -20f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlyArray<EnemyAudioLayerCue>(new EnemyAudioLayerCue[2]
			{
				new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1007021/normal_attack_play1.ogg", 0.6333f),
				new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1007021/normal_attack_play2.ogg", 1.4666f)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1007021/normal_to_stance1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_to_stance.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/deforsa_boom_par_234ms.tscn", null, 0f, 410f, 0.5f, 0.5f, 0f, 3, 0.699f, 0.9343f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_blackhole.tscn", "loop", 0f, 0f, 0.15f, 0.15f, 0f, 1, 0.7f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_blackhole_b.tscn", "loop", 0f, 0f, 1f, 1f, 0f, -1, 0.7f, 3.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1007021/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_debuff_ready.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_debuff_play.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0.7f, 0.7f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_debuff_target_ready_f.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 110, 0f, 1.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_debuff_target_ready_b.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -110, 0f, 1.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_debuff_target_play_b.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -111, 0f, 1.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_debuff_target_play_m.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 1, 0f, 1.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_debuff_target_play_f.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 105, 0f, 1.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true)
			})),
			new EnemyActionPresentation(EnemyAction.Special, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1007021/stance1_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[9]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_ready.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0.6667f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/deforsa_stance1_attack_par_667ms.tscn", null, 0f, 300f, 1f, -1f, 0f, 1, 0.6667f, 1.9667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_end.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 1.3334f, 0.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_target_play1.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 100, 0.6667f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_target_play2.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 1, 0.6667f, 0.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_target_play_ring.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 150, 0.6667f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/bulk_deforsa_stance1_attack_target_play_3.tscn", null, 0f, 0f, 0.6f, 0.6f, 0f, 80, 0.6667f, 2.8667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/bulk_deforsa_stance1_attack_target_play_4.tscn", null, 0f, 0f, 0.6f, 0.6f, 0f, 120, 0.6667f, 1.4667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1007021/stance1_attack_play1.ogg", 0.6667f))),
			new EnemyActionPresentation(EnemyAction.Charge, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1007021/stance1_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[16]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_ready.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0.6667f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/deforsa_stance1_attack_par_667ms.tscn", null, 0f, 300f, 1f, -1f, 0f, 1, 0.6667f, 1.9667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_play2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 1.3334f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/deforsa_stance1_attack_par_667ms.tscn", null, 0f, 280f, 1f, -1f, 90f, 1, 1.3334f, 2.6334f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_end.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 2.0001f, 0.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_target_play1.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 100, 0.6667f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_target_play2.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 1, 0.6667f, 0.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_target_play_ring.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 150, 0.6667f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/bulk_deforsa_stance1_attack_target_play_3.tscn", null, 0f, 0f, 0.6f, 0.6f, 0f, 80, 0.6667f, 2.8667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/bulk_deforsa_stance1_attack_target_play_4.tscn", null, 0f, 0f, 0.6f, 0.6f, 0f, 120, 0.6667f, 1.4667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_target_play1.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 100, 1.3334f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_target_play2.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 1, 1.3334f, 0.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_stance1_attack_target_play_ring.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 150, 1.3334f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/bulk_deforsa_stance1_attack_target_play_3.tscn", null, 0f, 0f, 0.6f, 0.6f, 0f, 80, 1.3334f, 2.8667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/bulk_deforsa_stance1_attack_target_play_4.tscn", null, 0f, 0f, 0.6f, 0.6f, 0f, 120, 1.3334f, 1.4667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true)
			}), new _003C_003Ez__ReadOnlyArray<EnemyAudioLayerCue>(new EnemyAudioLayerCue[2]
			{
				new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1007021/stance1_attack_play1.ogg", 0.6667f),
				new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1007021/stance1_attack_play2.ogg", 1.3334f)
			})),
			new EnemyActionPresentation(EnemyAction.WakeUp, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1007021/stance1_to_normal.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_blackhole.tscn", "intro", 0f, 0f, 0.15f, 0.15f, 0f, 2, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Death, Array.Empty<EnemyAudioCue>(), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[8]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_death_f1_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 8, 0f, 2.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_death_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 1.7f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_death_b1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_death_f4.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_death_f1_2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 6, 0f, 2.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_death_f1_3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 7, 0f, 2.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/spine/deforsa_normal_death_b2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 2.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007021/particles/deforsa_boom_par_1002ms.tscn", null, 0f, 500f, 0.3f, 0.3f, 0f, 9, 1.665f, 1.7017f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			}))
		}), new _003C_003Ez__ReadOnlySingleElementList<string>("SleepVfxPos"), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[4]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_slam", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_attack_stab", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_cast", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/lagavulin_matriarch/lagavulin_matriarch_awaken", EnemyAction.WakeUp)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[5]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Cast,
			EnemyAction.Charge,
			EnemyAction.HeavyAttack,
			EnemyAction.Special
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildDevouredBeingForGasBomb()
	{
		return new EnemyReplacementProfile("devoured_being_for_gas_bomb", new Sts2EnemyTarget("GAS_BOMB"), new CznEnemySource("3000256_01", "3000256", "monster@name@3000256_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/gas_bomb.tscn", "res://CznEnemySkin/animations/monsters/3000256_devoured_being/3000256_devoured_being_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.2f), new EnemySelectionBounds(-123.174225f, -376.90875f, 242.0786f, 377.73584f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Entrance, "spawn", "normal_enter", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "explode_trigger", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), null, new _003C_003Ez__ReadOnlySingleElementList<EnemyActionPresentation>(new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000256/normal_casting.ogg")), Array.Empty<EnemyEffectCue>())), new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "SmokeTrailSlot", "SmokeBallSlot", "NGasBombVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/living_fog/living_fog_explode", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), Array.Empty<EnemyAction>()), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildDibellumForTheObscura()
	{
		return new EnemyReplacementProfile("dibellum_for_the_obscura", new Sts2EnemyTarget("THE_OBSCURA"), new CznEnemySource("1007008_01", "1007008", "monster@name@1007008_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/the_obscura.tscn", "res://CznEnemySkin/animations/monsters/1007008_dibellum/1007008_dibellum_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-215.50633f, -249.3171f, 517.7762f, 303.63428f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_enter2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "Summon", "normal_enter2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1007008/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007008/spine/develum_normal_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, -70f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.85f, 0.85f, -120f, 0, 0.567f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1007008/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "NTheObscuraVfx", "ParticlesSlot" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/obscura/obscura_attack", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Summon), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildDominionForMechaKnight()
	{
		return new EnemyReplacementProfile("dominion_for_mecha_knight", new Sts2EnemyTarget("MECHA_KNIGHT"), new CznEnemySource("1003006_01", "1003006", "monster@name@1003006_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/mecha_knight.tscn", "res://CznEnemySkin/animations/monsters/1003006_dominion/1003006_dominion_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.2f), new EnemySelectionBounds(-209.65253f, -571.2755f, 455.47302f, 612.29584f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[7]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "charge", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack", "normal_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "wind_up", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "attack_flame", "stance1_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[5]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003006/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/spine/dominion_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 0.4333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/particles/dominion_normal_attack_play_pati1_433ms.tscn", null, -190f, -30f, 1f, 1f, 0f, 2, 0f, 1.4333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/particles/dominion_normal_attack_play1_pati1_433ms.tscn", null, 80f, 320f, 1f, 0.7f, -20f, 2, 0f, 1.4333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003006/normal_attack_play2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/spine/dominion_normal_attack_play2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 0.4333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/particles/dominion_normal_attack_play_pati1_433ms.tscn", null, 200f, -20f, 1f, 1f, 0f, 2, 0f, 1.4333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/particles/dominion_normal_attack_play2_pati1_433ms.tscn", null, -70f, 220f, 1f, 0.6f, 0f, 2, 0f, 1.4333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003006/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/spine/dominion_normal_debuff_ready.tscn", "animation", 0f, 0f, 1.2f, 1.2f, 0f, 1, 0f, 0.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/spine/dominion_normal_debuff_play.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0.48f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/particles/dominion_normal_debuff_play_pati1_900ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 2, 0.48f, 1.9f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/particles/dominion_normal_debuff_play_pati2_900ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 3, 0.813f, 1.9f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003006/stance1_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/spine/dominion_stance_buff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0.58f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/particles/dominion_stance1_buff_pati1_833ms.tscn", null, -16f, 0f, 1f, 1f, 0f, 2, 0.58f, 1.8333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/particles/dominion_stance1_buff_pati2_833ms.tscn", null, 0f, 160f, 1f, 1f, 0f, 2, 0.58f, 1.8333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/particles/dominion_stance1_buff_pati2_833ms.tscn", null, 0f, 160f, -1f, 1f, 0f, 2, 0.58f, 1.8333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003006/normal_enter.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/spine/dominion_enter.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/particles/dominion_enter_pati1_400ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.4f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003006/particles/dominion_enter_pati2_1633ms.tscn", null, 0f, 200f, 1f, 1f, 0f, 2, 1.066f, 2.6333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			}))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "EngineSlot", "FlameParticlesBone", "NMechaKnightVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[4]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_dash", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_heavy_attack", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_buff", EnemyAction.Buff),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/mechaknight/mechaknight_flamethrower", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildDualEdgeLepturusForDampCultist()
	{
		return new EnemyReplacementProfile("dual_edge_lepturus_for_damp_cultist", new Sts2EnemyTarget("DAMP_CULTIST"), new CznEnemySource("1001024_01", "1001024", "monster@name@1001024_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/damp_cultist.tscn", "res://CznEnemySkin/animations/monsters/1001024_dual_edge_lepturus/1001024_dual_edge_lepturus_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-121.082535f, -211.24097f, 200.72949f, 220.35541f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "buff", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001024/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001024/spine/dual_edge_lepturus_normal_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 0.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.6f, 0.6f, -80f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0.31f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001024/normal_skill3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001024/spine/single_edge_lepturus_normal_skill3_f.tscn", "animation", 0f, 5f, 1f, 1f, 0f, 20, 0f, 1.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001024/spine/single_edge_lepturus_normal_skill3_b.tscn", "animation", 0f, 5f, 1f, 1f, 0f, -20, 0f, 1.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001024/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/cultists/cultists_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/cultists/cultists_buff_damp", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildDustInsectForBowlbugSilk()
	{
		return new EnemyReplacementProfile("dust_insect_for_bowlbug_silk", new Sts2EnemyTarget("BOWLBUG_SILK"), new CznEnemySource("1006018_01", "1006018", "monster@name@1006018_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/bowlbug_silk.tscn", "res://CznEnemySkin/animations/monsters/1006018_dust_insect/1006018_dust_insect_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-101.54727f, -226.27148f, 179.19159f, 238.96753f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "spit", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006018/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006018/spine/dust_insect_normal_skill1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006018/spine/dust_insect_normal_skill1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 1.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.3f, 0.3f, -15f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006018/normal_skill3.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006018/spine/dust_insect_normal_skill3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006018/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlySingleElementList<string>("SpineBoneNode"), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/bowlbug_silk/bowlbug_silk_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/workbug_silk/workbug_silk_spit", EnemyAction.Cast)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_blunt", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildEvilDullahanForSpinyToad()
	{
		return new EnemyReplacementProfile("evil_dullahan_for_spiny_toad", new Sts2EnemyTarget("SPINY_TOAD"), new CznEnemySource("1005003_01", "1005003", "monster@name@1005003_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/spiny_toad.tscn", "res://CznEnemySkin/animations/monsters/1005003_evil_dullahan/1005003_evil_dullahan_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.25f), new EnemySelectionBounds(-225.0236f, -276.5478f, 413.0601f, 301.2293f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[10]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_naked_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "stance1_idle", Loop: true, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "Attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Special, "AttackSpiked", "stance1_attack_play1", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Buff, "Spiked", "normal_buff_1_to_stance1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "Unspiked", "stance1_to_normal", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt_naked", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "stance1_hit", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die_naked", "die_naked", Loop: false, EnemyPresentationStance.Transformed)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[5]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005003/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/spine/1005003_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 10f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, -50f, 0.7f, 0.7f, 10f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 50f, 0.7f, 0.7f, 10f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005003/stance1_to_normal.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/spine/1005003_stance1_to_normal_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 10, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/spine/1005003_stance1_to_normal_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/particles/1005003_stance1_to_normal_pati01_1000ms.tscn", null, 0f, 250f, 1f, 1f, 0f, -2, 0.6f, 2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/spine/1005003_eff_b.tscn", "stance1_to_normal", 0f, 0f, 1f, 1f, 0f, -10, 0f, 1.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/spine/1005003_eff_f.tscn", "stance1_to_normal", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005003/normal_buff_1_to_stance1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/spine/1005003_normal_buff_1_to_stance1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 10, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/spine/1005003_normal_buff_1_to_stance1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/particles/1005003_normal_buff_1_to_stance1_pati01_1000ms.tscn", null, 0f, 250f, 1f, 1f, 0f, -2, 0.95f, 2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/spine/1005003_eff_b.tscn", "normal_buff_1_to_stance1", 0f, 0f, 1f, 1f, 0f, -10, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/spine/1005003_eff_f.tscn", "normal_buff_1_to_stance1", 0f, 0f, 1f, 1f, 0f, 5, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Special, Array.Empty<EnemyAudioCue>(), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/spine/1005003_stance1_attack_play1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 10, 0.4667f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/spine/1005003_stance1_attack_play1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0.4667f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/spine/1005003_eff_b.tscn", "stance1_attack_play1", 0f, 0f, 1f, 1f, 0f, -10, 0.4667f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005003/spine/1005003_eff_f.tscn", "stance1_attack_play1", 0f, 0f, 1f, 1f, 0f, 5, 0.4667f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 10f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 50f, 0.7f, 0.7f, 10f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, -50f, 0.7f, 0.7f, 10f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1005003/stance1_attack_play1.ogg", 0.4667f))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005003/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "BurstParticles", "BurstParticlesSub", "NSpinyToadVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/spiny_toad/spiny_toad_lick", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/spiny_toad/spiny_toad_protrude", EnemyAction.Buff),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/spiny_toad/spiny_toad_explode", EnemyAction.HeavyAttack)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Special
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildBladeSoldierForExoskeleton()
	{
		return new EnemyReplacementProfile("blade_soldier_for_exoskeleton", new Sts2EnemyTarget("EXOSKELETON"), new CznEnemySource("1001001_01", "1001001", "monster@name@1001001_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/exoskeleton.tscn", "res://CznEnemySkin/animations/monsters/1001001_blade_soldier/1001001_blade_soldier_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-165.7455f, -259.2077f, 274.0787f, 275.5537f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[7]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_02", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_heavy", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "buff", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001001/normal_m_attack_02.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001001/spine/soldier_blade_normal_m_attack_02_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 30f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 2, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 60f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 2, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 30f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 2, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 60f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 2, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001001/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001001/spine/soldier_blade_normal_m_attack_01_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.8f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001001/normal_casting.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001001/invader_death_01.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[4]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/roaches/roaches_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/roaches/roaches_attack_heavy", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/roaches/roaches_buff", EnemyAction.Buff),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/roaches/roaches_die", EnemyAction.Death)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildFairyBoggartForOvicopter()
	{
		return new EnemyReplacementProfile("fairy_boggart_for_ovicopter", new Sts2EnemyTarget("OVICOPTER"), new CznEnemySource("1005004_01", "1005004", "monster@name@1005004_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/ovicopter.tscn", "res://CznEnemySkin/animations/monsters/1005004_fairy_boggart/1005004_fairy_boggart_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-174.5477f, -446.4152f, 401.702f, 476.4117f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "layTrigger", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "buffTrigger", "normal_attack_ready", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005004/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005004/spine/1005004_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005004/particles/1005004_normal_attack_play1_pati01_667ms.tscn", null, -150f, 500f, 1f, 1f, 0f, 4, 0f, 1.6667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005004/particles/1005004_normal_attack_play1_pati02_607ms.tscn", null, -150f, 480f, 0.75f, 0.18f, 17f, 4, 0.06f, 1.6067f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.45f, 0.45f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.45f, 0.45f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005004/normal_attack_ready.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005004/spine/1005004_normal_attack_ready.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005004/normal_casting.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005004/spine/1005004_normal_casting.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005004/particles/1005004_normal_casting_pati01_1300ms.tscn", null, 80f, 600f, 1f, 1f, 0f, 4, 0.5f, 2.3f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005004/particles/1005004_normal_casting_pati01_1300ms.tscn", null, -60f, 600f, -1f, 1f, 0f, 4, 0.5f, 2.3f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			}))
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/egg_layer/egg_layer_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/egg_layer/egg_layer_lay", EnemyAction.Cast)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_slash", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Cast
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildFairyChangelingForToughEgg()
	{
		return new EnemyReplacementProfile("fairy_changeling_for_tough_egg", new Sts2EnemyTarget("TOUGH_EGG"), new CznEnemySource("1005015_01", "1005015", "monster@name@1005015_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/tough_egg.tscn", "res://CznEnemySkin/animations/monsters/1005015_fairy_changeling/1005015_fairy_changeling_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-122.876816f, -285.6785f, 260.417f, 359.16632f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[7]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Entrance, "egg_spawn", "normal_enter", Loop: false),
			new EnemyAnimationBinding(EnemyAction.StunLoop, "egg_idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Buff, "Hatch", "normal_enter", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), null, new _003C_003Ez__ReadOnlySingleElementList<EnemyActionPresentation>(new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005015/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
		{
			new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 30f, 0, 0.465f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
			new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 265f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
		}))), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(Array.Empty<EnemyNativeAudioSuppression>(), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, new _003C_003Ez__ReadOnlySingleElementList<EnemySecondaryBodyProfile>(new EnemySecondaryBodyProfile("egg", "res://CznEnemySkin/animations/monsters/1005014_fairy_changeling_seed/1005014_fairy_changeling_seed_skel_data.tres", 1f, InitiallyVisible: true, HidePrimaryInitially: true, "normal_enter", "normal_idle")), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Hatch), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildFairyGremlinForCorpseSlug()
	{
		return new EnemyReplacementProfile("fairy_gremlin_for_corpse_slug", new Sts2EnemyTarget("CORPSE_SLUG"), new CznEnemySource("1005017_01", "1005017", "monster@name@1005017_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/corpse_slug.tscn", "res://CznEnemySkin/animations/monsters/1005017_fairy_gremlin/1005017_fairy_gremlin_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-99.868324f, -285.33954f, 218.03378f, 242.10753f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_heavy", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005017/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005017/spine/gremlin_normal_debuff_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 2.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005017/particles/gremlin_debuff_par_1398ms.tscn", null, 80f, 240f, 1f, 1f, 0f, 4, 0.402f, 3.198f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005017/particles/gremlin_debuff_par_1398ms.tscn", null, -65f, 250f, -1f, 1f, 0f, 4, 0.402f, 3.198f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005017/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005017/spine/gremlin_normal_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005017/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/corpse_slugs/corpse_slugs_attack", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/corpse_slugs/corpse_slugs_attack_light", EnemyAction.HeavyAttack)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_slash", EnemyAction.HeavyAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), Array.Empty<EnemyAction>()), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildFairyNoxForInklet()
	{
		return new EnemyReplacementProfile("fairy_nox_for_inklet", new Sts2EnemyTarget("INKLET"), new CznEnemySource("1005001_01", "1005001", "monster@name@1005001_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/inklet.tscn", "res://CznEnemySkin/animations/monsters/1005001_fairy_nox/1005001_fairy_nox_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-90.63527f, -285.18512f, 157.23932f, 295.62778f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_fast", "normal_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), null, new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005001/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005001/spine/1005001_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 0.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005001/spine/1005001_normal_attack_play1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005001/spine/1005001_normal_attack_play1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 0.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005001/particles/1005001_par1_667ms.tscn", null, 60f, 150f, 0.5f, 1f, -70f, 0, 0f, 2.1667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005001/particles/1005001_par2_667ms.tscn", null, -20f, 60f, 1f, 1f, 30f, 3, 0f, 2.3667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 50f, -20f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 50f, -20f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005001/normal_attack_play2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005001/spine/1005001_normal_attack_play2_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 0.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005001/spine/1005001_normal_attack_play2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005001/spine/1005001_normal_attack_play2_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 0.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005001/particles/1005001_par1_667ms.tscn", null, -30f, 140f, -0.5f, 1f, 90f, 1, 0f, 2.1667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005001/particles/1005001_par2_667ms.tscn", null, 0f, 40f, -1f, -1f, -7f, 1, 0f, 2.3667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", -100f, 30f, 0.4f, 0.4f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", -100f, 30f, 0.4f, 0.4f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}))
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/inklet/inklet_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/inklet/inklet_attack_triple", EnemyAction.HeavyAttack)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildFairySprigganForVineShambler()
	{
		return new EnemyReplacementProfile("fairy_spriggan_for_vine_shambler", new Sts2EnemyTarget("VINE_SHAMBLER"), new CznEnemySource("1005002_01", "1005002", "monster@name@1005002_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/vine_shambler.tscn", "res://CznEnemySkin/animations/monsters/1005002_fairy_spriggan/1005002_fairy_spriggan_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-105.55693f, -318.2468f, 211.95421f, 319.92783f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_chomp", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_swipe", "normal_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "attack_vines", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), null, new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005002/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005002/spine/1005002_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 6, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005002/particles/1005002_attack1_pat1_667ms.tscn", null, -30f, 560f, 0.65f, 0.65f, 7f, 5, 0f, 1.2167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 30f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005002/normal_attack_play2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005002/spine/1005002_normal_attack_play2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 6, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005002/particles/1005002_attack2_pat1_667ms.tscn", null, 0f, 450f, 0.65f, 0.65f, 0f, 5, 0f, 1.2167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.8f, 0.8f, -100f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005002/normal_casting.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/vine_shambler/vine_shambler_chomp", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/vine_shambler/vine_shambler_defensive_swipe", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/vine_shambler/vine_shambler_cast", EnemyAction.Cast)
		}), new _003C_003Ez__ReadOnlyArray<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression[2]
		{
			new EnemyNativeVfxPathSuppression("vfx/vfx_bite", EnemyAction.BasicAttack),
			new EnemyNativeVfxPathSuppression("vfx/vfx_scratch", EnemyAction.HeavyAttack)
		}), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildFallaciaForSoulNexus()
	{
		return new EnemyReplacementProfile("fallacia_for_soul_nexus", new Sts2EnemyTarget("SOUL_NEXUS"), new CznEnemySource("1004007_01", "1004007", "monster@name@1004007_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/soul_nexus.tscn", "res://CznEnemySkin/animations/monsters/1004007_fallacia/1004007_fallacia_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-201.4081f, -492.5789f, 436.72003f, 509.4414f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_job_attack_play", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "soul_burn", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_unique_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004007/normal_job_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[9]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/spine/fallacia_normal_job_attack_play.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0.4667f, 0.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/particles/brokenadam_par3_1200ms.tscn", null, -5.6456f, 639.49f, 0.45f, 0.45f, 0f, 2, 0.4667f, 2.2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/spine/fallacia_normal_job_attack_target_play_flash.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 110, 0.4667f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/spine/fallacia_normal_job_attack_target_play_b.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -110, 0.4667f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/spine/fallacia_normal_job_attack_target_play1.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 105, 0.4667f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/spine/fallacia_normal_job_attack_target_play2.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 101, 0.4667f, 0.7f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/spine/fallacia_normal_job_attack_target_play3.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 1, 0.4667f, 0.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/particles/official_normal_job_attack_play_target_05.tscn", null, 0f, 175f, 0.5f, 0.5f, 180f, 1, 0.6667f, 1.9667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/particles/official_normal_job_attack_play_target_06.tscn", null, 0f, 25f, 0.5f, 0.5f, 0f, 102, 0.4667f, 0.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1004007/normal_job_attack_play.ogg", 0.4667f))),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, Array.Empty<EnemyAudioCue>(), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/spine/fallacia_normal_attack_play1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0.4667f, 0.8f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/spine/fallacia_normal_attack_play1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0.4667f, 0.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1004007/normal_attack_play1.ogg", 0.4667f))),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004007/normal_unique_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/spine/fallacia_normal_unique_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/particles/brokenadam_par3_1200ms.tscn", null, -114.92f, 578.39f, 0.5f, 0.5f, 0f, 2, 0.5f, 2.2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/spine/fallacia_normal_unique_1_target_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.4333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/spine/fallacia_normal_unique_1_target_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 1.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/particles/official_normal_unique_1_target_02.tscn", null, 0f, 200f, 1f, 1f, 0f, 2, 0.533f, 2.167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004007/spine/fallacia_normal_unique_1_target_potal.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004007/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[5] { "NSoulNexusVfx", "PathSlot1", "PathSlot2", "PathSlot3", "HeadFireSlot" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/soul_nexus/soul_nexus_attack", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[3]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Cast,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildFirstReplicaForMawler()
	{
		return new EnemyReplacementProfile("first_replica_for_mawler", new Sts2EnemyTarget("MAWLER"), new CznEnemySource("3000350_01", "3000350", "monster@name@3000350_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/mawler.tscn", "res://CznEnemySkin/animations/monsters/3000350_first_replica/3000350_first_replica_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-174.83533f, -373.0806f, 416.604f, 378.91156f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "roar", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000350/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000350/spine/3000350_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000350/particles/3000350_normal_attack_01_pat_567ms.tscn", null, -20f, 190f, 0.7f, 0.45f, -30f, 2, 0f, 1.4667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000350/composite/3000350_normal_attack_target_01.tscn", "animation", 0f, 20f, 0.56f, 0.56f, 20f, 0, 0f, 1.5367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000350/normal_casting.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000350/spine/3000350_normal_casting_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.8f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000350/spine/3000350_normal_casting_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 1.8f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000350/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/mawler/mawler_attack", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildFlyingVampireForThievingHopper()
	{
		return new EnemyReplacementProfile("flying_vampire_for_thieving_hopper", new Sts2EnemyTarget("THIEVING_HOPPER"), new CznEnemySource("1001002_01", "1001002", "monster@name@1001002_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/thieving_hopper.tscn", "res://CznEnemySkin/animations/monsters/1001002_flying_vampire/1001002_flying_vampire_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-133.0849f, -368.7952f, 286.7114f, 369.9387f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[7]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "steal", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "Hover", "normal_attack_ready", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "flee", "normal_attack_end", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001002/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001002/spine/flying_vamp_normal_attack_play1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 0.7f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001002/spine/flying_vamp_normal_attack_play1_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.0333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001002/spine/flying_vamp_normal_attack_play1_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_weak_01_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 0.3f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_weak_01_back.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 0.2667f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001002/normal_attack_end.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001002/normal_attack_ready.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001002/spine/flying_vamp_normal_attack_ready.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 0.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001002/normal_casting.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "CardBoneNode", "SpineBoneNode" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[6]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_attack_hover", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_steal", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_take_off", EnemyAction.Buff),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_flee", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/thieving_hopper/thieving_hopper_flee_hover", EnemyAction.HeavyAttack)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildGiantSoldierForSewerClam()
	{
		return new EnemyReplacementProfile("giant_soldier_for_sewer_clam", new Sts2EnemyTarget("SEWER_CLAM"), new CznEnemySource("1001006_01", "1001006", "monster@name@1001006_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/sewer_clam.tscn", "res://CznEnemySkin/animations/monsters/1001006_giant_soldier/1001006_giant_soldier_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-206.5466f, -289.25f, 404.0197f, 320.4765f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "buff", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001006/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001006/spine/soldier_giant_normal_m_attack_01_f3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 2.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001006/spine/soldier_giant_normal_m_attack_01_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 2.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001006/spine/soldier_giant_normal_m_attack_01_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 6, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.5f, 0.5f, -60f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", -20f, -5f, 0.5f, 0.5f, -60f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 20f, 5f, 0.5f, 0.5f, -60f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001006/normal_casting.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "NSewerClamVfx", "CoralScaleBone", "MouthSlot" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/sewer_clam/sewer_clam_buff", EnemyAction.Cast)), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_blunt", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildGloomyBeastForScrollOfBiting()
	{
		return new EnemyReplacementProfile("gloomy_beast_for_scroll_of_biting", new Sts2EnemyTarget("SCROLL_OF_BITING"), new CznEnemySource("1006017_01", "1006017", "monster@name@1006017_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/scroll_of_biting.tscn", "res://CznEnemySkin/animations/monsters/1006017_gloomy_beast/1006017_gloomy_beast_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-118.24196f, -208.35922f, 283.98358f, 221.76088f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_double", "normal_m_attack_02", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "Cast", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006017/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006017/spine/gloomy_beast_normal_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006017/particles/gloomy_prime_ligjt_ring_par1_650ms.tscn", null, -30f, 120f, 0.8f, 0.22f, 0f, 2, 0.55f, 1.65f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.6f, 0.6f, -100f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006017/normal_m_attack_02.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006017/spine/gloomy_beast_normal_m_attack_02.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006017/particles/gloomy_beast_par1_917ms.tscn", null, 0f, 50f, 1f, 1f, 0f, 2, 0.55f, 1.9167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006017/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006017/spine/gloomy_beast_normal_buff_1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006017/spine/gloomy_beast_normal_buff_1_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006017/spine/gloomy_beast_normal_buff_1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006017/particles/gloomy_beast_par2_983ms.tscn", null, 0f, 50f, 1f, 1f, 0f, 3, 0.55f, 1.9833f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006017/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/scroll_of_biting/scroll_of_biting_bite", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/scroll_of_biting/scroll_of_biting_bite_double", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/scroll_of_biting/scroll_of_biting_buff", EnemyAction.Buff)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildGloomyGodfatherForTheLost()
	{
		return new EnemyReplacementProfile("gloomy_godfather_for_the_lost", new Sts2EnemyTarget("THE_LOST"), new CznEnemySource("1004017_01", "1004017", "monster@name@1004017_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/the_lost.tscn", "res://CznEnemySkin/animations/monsters/1004017_gloomy_godfather/1004017_gloomy_godfather_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-298.4797f, -430.46008f, 479.33688f, 463.70633f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Cast, "debuff", "normal_debuff_unique_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_job_attack_play", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004017/normal_job_attack_play.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004017/spine/grizzly_job_attack_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004017/spine/grizzly_job_attack_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, -100f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004017/normal_debuff_unique_1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004017/spine/grizzly_debuff_unique.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 2.1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004017/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[5] { "GranuleEmitterBone", "SpineSlotNode3", "SpineSlotNode2", "SpineSlotNode", "NLostAndForgottenVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(Array.Empty<EnemyNativeAudioSuppression>(), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildGoldenManeForWaterfallGiant()
	{
		return new EnemyReplacementProfile("golden_mane_for_waterfall_giant", new Sts2EnemyTarget("WATERFALL_GIANT"), new CznEnemySource("3000416_01", "3000416", "monster@name@3000183_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/waterfall_giant.tscn", "res://CznEnemySkin/animations/monsters/3000416_golden_mane/3000416_golden_mane_skel_data.tres", CznEnemyVisualScalePolicy.ResolveSts2ScaleCap(1.32f, 1f), new EnemySelectionBounds(-488.53754f, -762.41473f, 1029.2456f, 830.1706f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[9]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "Attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "AttackDebuff", "normal_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "AttackBuff", "normal_unique_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "Heal", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Special, "Erupt", "normal_s_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.StunTransition, "AboutToBlow", "normal_groggy", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[7]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000416/normal_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/particles/3000183_normal_attack_pati1_667ms.tscn", null, -50f, 0f, 2f, 2f, 0f, 0, 0.4667f, 1.2167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_normal_attack_play_floor.tscn", "animation", -80f, -55f, 1f, 1f, 0f, -1, 0.4667f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_normal_attack_play1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0.4667f, 0.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/particles/3000183_normal_flame_burst_444ms.tscn", null, 400f, 125f, 1.4f, 1.5f, 15f, 0, 0.4667f, 1.694f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/particles/3000183_normal_flame_burst_444ms.tscn", null, -125f, 25f, 1.4f, 1.6f, -41f, 0, 0.4667f, 1.694f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000416/normal_attack_play1.ogg", 0.4667f))),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000416/normal_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/particles/3000183_normal_attack_pati1_667ms.tscn", null, -180f, 0f, -2f, 2f, 0f, 0, 0.4667f, 1.2167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_normal_attack_play_floor.tscn", "animation", -120f, -60f, 1f, 1f, 0f, -1, 0.4667f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_normal_attack_play2_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0.4667f, 0.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/particles/3000183_normal_flame_burst_667ms.tscn", null, -100f, 55f, 1.6f, 2.5f, 75f, 0, 0.4667f, 1.9167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000416/normal_attack_play2.ogg", 0.4667f))),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000416/normal_unique_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_normal_unique_1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0.75f, 1.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_normal_unique_1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0.75f, 1.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000416/normal_casting.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_normal_casting_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0.98f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Special, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000416/normal_s_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[13]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_stance1_unique_02.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1000, 0f, 4.1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/particles/3000183_stance1_unique_01_pati_3_1333ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 999, 2.333f, 2.033f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/particles/3000183_stance1_unique_01_pati_2_2166ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 998, 0f, 2.766f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/particles/3000183_stance1_unique_01_pati_1_1300ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 997, 2.333f, 2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_stance1_unique_smoke_f_5.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 996, 0f, 3.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_stance1_unique_smoke_f_4.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 995, 0f, 3.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_stance1_unique_smoke_f_3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 994, 0f, 3.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_stance1_unique_smoke_f_2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 993, 0f, 4.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_stance1_unique_smoke_f_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 992, 0f, 4.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_stance1_unique_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 991, 0f, 4.3667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_stance1_unique_smoke_b_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -500, 0f, 4.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_stance1_unique_01_target_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 10, 0f, 5.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_stance1_unique_01_target_02.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -999, 0f, 4.8f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true)
			})),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000416/normal_death.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/secred_creature_death_01.tscn", "animation", 0f, 0f, 1.36f, 1.36f, 0f, 1, 2.4f, 1.1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000416/normal_enter.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_normal_enter_01_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 10, 0f, 1.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_normal_enter_02_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 9, 0f, 3.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_normal_enter_02_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -998, 0f, 3.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000416/spine/3000183_normal_enter_01_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -999, 0f, 1.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			}))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[14]
		{
			"SteamSlot4", "SteamSlot3", "SteamLeakSlot3", "BodyWaterSlot", "MistSlot", "SteamLeakSlot1", "SteamLeakSlot2", "SteamSlot5", "SteamSlot6", "MouthWaterSlot",
			"MouthDropletsSlot", "SteamSlot1", "SteamSlot2", "NWaterfallGiantVfx"
		}), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[5]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_attack_kick", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_attack_stomp", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_eruption", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_ambient", EnemyAction.Entrance),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/waterfall_giant/waterfall_giant_die", EnemyAction.Special)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[3]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack,
			EnemyAction.Special
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildGolemAutomataForPunchConstruct()
	{
		return new EnemyReplacementProfile("golem_automata_for_punch_construct", new Sts2EnemyTarget("PUNCH_CONSTRUCT"), new CznEnemySource("1003012_01", "1003012", "monster@name@1003012_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/punch_construct.tscn", "res://CznEnemySkin/animations/monsters/1003012_golem_automata/1003012_golem_automata_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-180.1171f, -402.29196f, 343.0782f, 409.06183f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_02", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_double", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "block", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003012/normal_m_attack_02.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003012/spine/automata_golem_normal_m_attack_02.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003012/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003012/spine/automata_golem_normal_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 2.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 2, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 2, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 2, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 2, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003012/normal_casting.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/punch_construct/punch_construct_attack_single", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/punch_construct/punch_construct_attack_double", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/punch_construct/punch_construct_buff", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildHaloAutomataForChomper()
	{
		return new EnemyReplacementProfile("halo_automata_for_chomper", new Sts2EnemyTarget("CHOMPER"), new CznEnemySource("1003003_01", "1003003", "monster@name@1003003_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/chomper.tscn", "res://CznEnemySkin/animations/monsters/1003003_halo_automata/1003003_halo_automata_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.2f), new EnemySelectionBounds(-99.168976f, -217.8596f, 202.48856f, 222.14189f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003003/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[9]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003003/spine/automata_halo_normal_m_attack_01_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 3.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003003/spine/automata_halo_normal_m_attack_01_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 1.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003003/spine/automata_halo_normal_m_attack_01_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 3.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.5f, 0.5f, -105f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 30f, 0.5f, 0.5f, -105f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, -30f, 0.5f, 0.5f, -105f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 30f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 30f, 0.5f, 0.5f, 30f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, -30f, 0.5f, 0.5f, 30f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003003/normal_casting.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/chomper/chomper_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/chomper/chomper_cast", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildHeavyArmorInsectForBowlbugRock()
	{
		return new EnemyReplacementProfile("heavy_armor_insect_for_bowlbug_rock", new Sts2EnemyTarget("BOWLBUG_ROCK"), new CznEnemySource("1001015_01", "1001015", "monster@name@1001015_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/bowlbug_rock.tscn", "res://CznEnemySkin/animations/monsters/1001015_heavy_armor_insect/1001015_heavy_armor_insect_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-148.6289f, -343.00983f, 315.7591f, 364.71628f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[8]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "headbutt", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "buff", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.StunTransition, "stun", "normal_groggy", Loop: true),
			new EnemyAnimationBinding(EnemyAction.StunLoop, "stunned_loop", "normal_groggy", Loop: true),
			new EnemyAnimationBinding(EnemyAction.WakeUp, "wake_up", "wake_up", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001015/normal_skill2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001015/spine/iron_heavyarmor_insect_normal_skill2_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001015/spine/iron_heavyarmor_insect_normal_skill2_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -30, 0f, 1.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001015/normal_skill3.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001015/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/workbug_rock/workbug_rock_attack", EnemyAction.BasicAttack)), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_blunt", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildHerbalHydracleForFlyconid()
	{
		return new EnemyReplacementProfile("herbal_hydracle_for_flyconid", new Sts2EnemyTarget("FLYCONID"), new CznEnemySource("3000421_01", "3000421", "monster@name@3000421_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/flyconid.tscn", "res://CznEnemySkin/animations/monsters/3000421_herbal_hydracle/3000421_herbal_hydracle_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.3f), new EnemySelectionBounds(-110.458405f, -157.80818f, 221.24756f, 186.12761f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000421/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000421/spine/hydracle_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 0.5376f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 265f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000421/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000421/spine/hydracle_normal_buff_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.2097f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000421/particles/hydracle_normal_buff_pat1_410ms.tscn", null, 0f, 130f, 0.7f, 0.7f, 180f, 4, 0.8f, 1.5597f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000421/particles/hydracle_normal_buff_pat1_610ms.tscn", null, 0f, 130f, 0.6f, 0.6f, 90f, 3, 0.6f, 1.5597f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000421/particles/hydracle_normal_buff_pat1_810ms.tscn", null, 0f, 130f, 0.7f, 0.7f, 0f, 2, 0.4f, 1.5597f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000421/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "SporesAttach", "VfxController" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(Array.Empty<EnemyNativeAudioSuppression>(), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildHexerForCrossbowRubyRaider()
	{
		return new EnemyReplacementProfile("hexer_for_crossbow_ruby_raider", new Sts2EnemyTarget("CROSSBOW_RUBY_RAIDER"), new CznEnemySource("1002010_01", "1002010", "monster@name@1002010_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/crossbow_ruby_raider.tscn", "res://CznEnemySkin/animations/monsters/1002010_hexer/1002010_hexer_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-220.79137f, -411.48303f, 396.5912f, 427.25882f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "reload", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002010/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002010/spine/hexer_normal_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002010/particles/hexer_normal_m_attack_01_pati1_1134ms.tscn", null, 30f, 220f, 0.4f, 1.4f, 50f, 1, 0.933f, 1.8337f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blade_normal_s.tscn", "animation", 0f, 0f, 1f, 1f, -150f, 0, 0.533f, 1.9f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002010/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002010/spine/hexer_normal_debuff_1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002010/spine/hexer_normal_debuff_1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002010/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/crossbow_ruby_raider/crossbow_ruby_raider_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/crossbow_ruby_raider/crossbow_ruby_raider_reload", EnemyAction.Buff)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_slash", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildHoneyJarForBowlbugNectar()
	{
		return new EnemyReplacementProfile("honey_jar_for_bowlbug_nectar", new Sts2EnemyTarget("BOWLBUG_NECTAR"), new CznEnemySource("1001016_01", "1001016", "monster@name@1001016_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/bowlbug_nectar.tscn", "res://CznEnemySkin/animations/monsters/1001016_honey_jar_porte/1001016_honey_jar_porte_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-83.91199f, -153.93088f, 190.11246f, 175.15326f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[9]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "buff", "normal_to_stance1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_empowered", "stance1_idle", Loop: true, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Special, "attack_empowered", "stance1_skill1", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt_empowered", "stance1_hit", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Death, "die_empowered", "stance1_death", Loop: false, EnemyPresentationStance.Transformed)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001016/normal_skill1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001016/spine/honey_jar_porte_normal_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001016/normal_to_stance1.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Special, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001016/stance1_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001016/spine/honey_jar_porte_active_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 5f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.6f, 0.6f, -60f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001016/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlySingleElementList<string>("SpineBoneNode"), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/workbug_goop/workbug_goop_spit", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.Special)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildHowlingProdigalForKnowledgeDemon()
	{
		return new EnemyReplacementProfile("howling_prodigal_for_knowledge_demon", new Sts2EnemyTarget("KNOWLEDGE_DEMON"), new CznEnemySource("1004019_01", "1004019", "monster@name@1004019_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/knowledge_demon.tscn", "res://CznEnemySkin/animations/monsters/1004019_howling_prodigal/1004019_howling_prodigal_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-408.97687f, -740.2505f, 775.3993f, 763.1815f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[10]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "MediumAttackTrigger", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "HeavyAttackTrigger", "normal_job_attack_play", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "MindRotTrigger", "normal_unique_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Idle, "burnt_loop", "normal_idle", Loop: true, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Buff, "HealTrigger", "normal_buff_1", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt_burnt", "normal_hit", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Death, "die_burnt", "normal_death", Loop: false, EnemyPresentationStance.Transformed)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[6]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004019/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_attack_play1_f.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 2, 0f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_attack_play1_b.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, -1, 0f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/particles/ectoqueen_attack_paly1_par_833ms.tscn", null, 150f, 450f, 1.35f, 1.5f, 0f, 1, 0f, 1.9333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, -20f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, Array.Empty<EnemyAudioCue>(), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_job_attack_play_1.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 4, 0.4667f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_job_attack_play_2.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 2, 0.4667f, 0.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_job_attack_play_3.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 1, 0.4667f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_job_attack_play_4.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, -1, 0.4667f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1004019/normal_job_attack_play.ogg", 0.4667f))),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004019/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_buff_f.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 1, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_buff_b.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, -1, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/particles/ectoqueen_normal_buff_600ms.tscn", null, 0f, 300f, 1.5f, 1.5f, 0f, 2, 0.5f, 2.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004019/normal_unique_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_unique_1_f.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 1, 0f, 2.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_unique_1_b.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, -1, 0f, 1.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004019/normal_death.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[10]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_death1.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 2, 1.73f, 1.1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_death2.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 1, 1.73f, 0.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_death3.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 1, 1.73f, 0.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_death4.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 1, 1.73f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_death5.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 1, 1.73f, 1.4333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_death6.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 1, 1.73f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_death7.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, -1, 1.73f, 1.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_death8.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, -2, 1.73f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_death_attach_f.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 1, 0f, 1.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_death_attach_b.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, -1, 0f, 1.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004019/normal_enter.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/spine/ectoqueen_normal_enter.tscn", "animation", 0f, 0f, 1.5f, 1.5f, 0f, 2, 0f, 2.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004019/particles/ectoqueen_normal_enter_1700ms.tscn", null, 0f, 0f, 1.5f, 1.5f, 0f, 1, 0f, 3.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			}))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[9] { "FireSlot1", "FireSlot2", "FireSlot3", "FireSlot4", "EmberParticles", "ThinEmberParticles", "ExplosionParticles", "DamageParticles", "NKnowledgeDemonVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/knowledge_demon/knowledge_demon_slap", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/knowledge_demon/knowledge_demon_clap", EnemyAction.HeavyAttack)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildHumanShadowForSpectralKnight()
	{
		return new EnemyReplacementProfile("human_shadow_for_spectral_knight", new Sts2EnemyTarget("SPECTRAL_KNIGHT"), new CznEnemySource("1006011_01", "1006011", "monster@name@1006011_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/spectral_knight.tscn", "res://CznEnemySkin/animations/monsters/1006011_human_shadow/1006011_human_shadow_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-119.185616f, -385.84106f, 234.49066f, 376.35977f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Cast, "debuff", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_sword", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_flame", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006011/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006011/spine/human_shadow_normal_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 50, 0f, 2.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 15f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006011/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006011/spine/human_shadow_normal_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 50, 0f, 2.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 15f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006011/normal_skill2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006011/spine/human_shadow_normal_skill2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 50, 0f, 1.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006011/spine/human_shadow_normal_skill2_smoke.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 40, 0f, 1.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006011/particles/human_shadow_normal_smoke2_1900ms.tscn", null, 0f, 20f, 1f, 1f, 0f, 30, 0f, 3.4f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006011/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[5] { "FlameParticlesFlat", "FlameParticlesAdd", "CinderParticles", "NSpectralKnightVfx", "HeadFireNode" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/spectral_knight/spectral_knight_hex", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/spectral_knight/spectral_knight_soul_slash", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/spectral_knight/spectral_knight_soul_flame", EnemyAction.HeavyAttack)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildHunchbackForSneakyGremlin()
	{
		return new EnemyReplacementProfile("hunchback_for_sneaky_gremlin", new Sts2EnemyTarget("SNEAKY_GREMLIN"), new CznEnemySource("1002012_01", "1002012", "monster@name@1002012_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/sneaky_gremlin.tscn", "res://CznEnemySkin/animations/monsters/1002012_hunchback/1002012_hunchback_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-182.76498f, -224.12233f, 368.74658f, 260.17755f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[7]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "awake_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Entrance, "spawn", "normal_enter", Loop: false),
			new EnemyAnimationBinding(EnemyAction.StunLoop, "stunned_loop", "normal_groggy", Loop: true),
			new EnemyAnimationBinding(EnemyAction.WakeUp, "WakeUpTrigger", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt_awake", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), null, new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002012/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002012/spine/hunchback_normal_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, -110f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 30f, 0, 0.267f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.WakeUp, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002012/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002012/spine/hunchback_normal_buff_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 2.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002012/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/gremlin_merc/sneaky_gremlin_attack", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildHussarAutomataForFrogKnight()
	{
		return new EnemyReplacementProfile("hussar_automata_for_frog_knight", new Sts2EnemyTarget("FROG_KNIGHT"), new CznEnemySource("3000482_01", "3000482", "monster@name@3000482_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/frog_knight.tscn", "res://CznEnemySkin/animations/monsters/3000482_hussar_automata/3000482_hussar_automata_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.25f), new EnemySelectionBounds(-172.54187f, -390.86893f, 316.51434f, 395.8399f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[7]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Buff, "buff", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_tongue", "normal_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Charge, "charge", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000482/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/particles/automata_cavalry_attack_pati01_667ms.tscn", null, -150f, 250f, 1f, 0.4f, -40f, 4, 0.03f, 1.4667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/particles/automata_cavalry_attack_pati02_667ms.tscn", null, -360f, 0f, 0.8f, 0.8f, 0f, 4, 0f, 1.6667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/spine/automata_cavalry_normal_attack_play1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/spine/automata_cavalry_normal_attack_play1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 0.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.4f, 0.4f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.4f, 0.4f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000482/normal_attack_play2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/particles/automata_cavalry_attack_pati01_667ms.tscn", null, -30f, 300f, -1.4f, -0.35f, 40f, 4, 0f, 1.4667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/particles/automata_cavalry_attack_pati01_667ms.tscn", null, 15f, 300f, -1.1f, -0.3f, 40f, 4, 0f, 1.4667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/particles/automata_cavalry_attack_pati01_667ms.tscn", null, 0f, 300f, -0.9f, -0.1f, 40f, 4, 0f, 1.4667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/spine/automata_cavalry_normal_attack_play2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, -35f, 0.4f, 0.4f, -90f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 35f, 0.4f, 0.4f, -90f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.5f, 0.5f, -90f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000482/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/spine/automata_cavalry_normal_buff_1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.2333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/spine/automata_cavalry_normal_buff_1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.2333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Charge, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000482/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/particles/automata_cavalry_attack_pati01_667ms.tscn", null, -150f, 250f, 1f, 0.4f, -40f, 4, 0.03f, 1.4667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/particles/automata_cavalry_attack_pati02_667ms.tscn", null, -360f, 0f, 0.8f, 0.8f, 0f, 4, 0f, 1.6667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/spine/automata_cavalry_normal_attack_play1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000482/spine/automata_cavalry_normal_attack_play1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 0.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.4f, 0.4f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.4f, 0.4f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}))
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[4]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/frog_knight/frog_knight_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/frog_knight/frog_knight_tongue_lash", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/frog_knight/frog_knight_charge", EnemyAction.Charge),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/frog_knight/frog_knight_buff", EnemyAction.Buff)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[3]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Charge,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildHydracleForSnappingJaxfruit()
	{
		return new EnemyReplacementProfile("hydracle_for_snapping_jaxfruit", new Sts2EnemyTarget("SNAPPING_JAXFRUIT"), new CznEnemySource("1005016_01", "1005016", "monster@name@1005016_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/snapping_jaxfruit.tscn", "res://CznEnemySkin/animations/monsters/1005016_hydracle/1005016_hydracle_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.3f), new EnemySelectionBounds(-110.458405f, -157.80818f, 221.24756f, 186.12761f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005016/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005016/spine/hydracle_normal_buff_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.2097f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005016/particles/hydracle_normal_buff_pat1_410ms.tscn", null, 0f, 130f, 0.7f, 0.7f, 180f, 4, 0.8f, 1.5597f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005016/particles/hydracle_normal_buff_pat1_610ms.tscn", null, 0f, 130f, 0.6f, 0.6f, 90f, 3, 0.6f, 1.5597f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005016/particles/hydracle_normal_buff_pat1_810ms.tscn", null, 0f, 130f, 0.7f, 0.7f, 0f, 2, 0.4f, 1.5597f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005016/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005016/spine/hydracle_normal_buff_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.2097f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005016/particles/hydracle_normal_buff_pat1_410ms.tscn", null, 0f, 130f, 0.7f, 0.7f, 180f, 4, 0.8f, 1.5597f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005016/particles/hydracle_normal_buff_pat1_610ms.tscn", null, 0f, 130f, 0.6f, 0.6f, 90f, 3, 0.6f, 1.5597f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005016/particles/hydracle_normal_buff_pat1_810ms.tscn", null, 0f, 130f, 0.7f, 0.7f, 0f, 2, 0.4f, 1.5597f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005016/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "TargetBone", "ProjectileAttachBone", "NSnappingJaxfruitVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(Array.Empty<EnemyNativeAudioSuppression>(), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), Array.Empty<EnemyAction>()), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildInconvenientDinerForPhrogParasite()
	{
		return new EnemyReplacementProfile("inconvenient_diner_for_phrog_parasite", new Sts2EnemyTarget("PHROG_PARASITE"), new CznEnemySource("1004003_01", "1004003", "monster@name@1004003_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/phrog_parasite.tscn", "res://CznEnemySkin/animations/monsters/1004003_inconvenient_diner/1004003_inconvenient_diner_skel_data.tres", CznEnemyVisualScalePolicy.ResolveSts2HostFit(1f, 1.4079617f), new EnemySelectionBounds(-177.8075f, -282.9178f, 357.96426f, 317.4539f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_debuff_unique_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004003/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004003/spine/meltymud_attack.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.3f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 2, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 2, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 2, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 2, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004003/normal_debuff_unique_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004003/spine/meltymud_debuff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004003/particles/meltymud_debuff_pati1_1001ms.tscn", null, 230f, 250f, 1f, 1f, 0f, 2, 0.666f, 1.7007f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004003/particles/meltymud_debuff_pati1_1001ms.tscn", null, -230f, 250f, 1f, 1f, 180f, 2, 0.666f, 1.7007f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004003/particles/meltymud_debuff_pati1_1001ms.tscn", null, 11f, 170f, 1f, 1f, 90f, 2, 0.666f, 1.7007f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004003/particles/meltymud_debuff_pati1_1001ms.tscn", null, 11f, 350f, 1f, 1f, -90f, 2, 0.666f, 1.7007f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004003/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[6] { "BubbleBSlotNode", "BubbleCBoneNode", "BubbleABoneNode", "DeathParticles", "DeathWormParticles", "NPhrogParasiteVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/phrog_parasite/phrog_parasite_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/phrog_parasite/phrog_parasite_cast", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildKentrisForHauntedShip()
	{
		return new EnemyReplacementProfile("kentris_for_haunted_ship", new Sts2EnemyTarget("HAUNTED_SHIP"), new CznEnemySource("1006007_01", "1006007", "monster@name@1006007_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/haunted_ship.tscn", "res://CznEnemySkin/animations/monsters/1006007_kentris/1006007_kentris_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.3f), new EnemySelectionBounds(-212.39381f, -374.28336f, 456.74957f, 449.01886f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_triple", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "debuff", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006007/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006007/spine/b_kentris_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 20, 0f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, -90f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 50f, 0, 0.21f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006007/normal_skill3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006007/spine/b_kentris_skill3_3_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 40, 1.84f, 2.9667f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006007/spine/b_kentris_skill3_3_m.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 1.84f, 2.1f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006007/spine/b_kentris_skill3_3_ch.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 20, 1.84f, 1.8667f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006007/spine/b_kentris_skill3_3_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -30, 1.84f, 2.4333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006007/normal_skill2.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006007/spine/b_kentris_skill2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 3f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006007/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[5] { "HeadSlot", "NHauntedShipVfx", "EyeBone1", "EyeBone2", "EyeBone3" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/haunted_ship/haunted_ship_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/haunted_ship/haunted_ship_cast", EnemyAction.Cast)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_spooky_scream", EnemyAction.Cast)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildLepidolaForEntomancer()
	{
		return new EnemyReplacementProfile("lepidola_for_entomancer", new Sts2EnemyTarget("ENTOMANCER"), new CznEnemySource("2302001_01", "2302001", "monster@name@2302001_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/entomancer.tscn", "res://CznEnemySkin/animations/monsters/2302001_lepidola/2302001_lepidola_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-278.29608f, -489.14407f, 568.599f, 488.99695f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_ranged", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack", "normal_skill4", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/2302001/normal_skill3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/spine/dreamy_lepidola_normal_skill3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.303f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 60f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/2302001/normal_skill4.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/spine/dreamy_lepidola_normal_skill4.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.4242f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, -70f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/2302001/normal_skill2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[25]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/spine/dreamy_lepidola_normal_skill2_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -30, 0f, 1.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/spine/dreamy_lepidola_normal_skill2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 2.4242f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par1_324ms.tscn", null, 170f, 470f, 1f, 1f, 45f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par1_324ms.tscn", null, -170f, 470f, -1f, 1f, -45f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par2_324ms.tscn", null, 120f, 230f, 1f, 1f, 50f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par2_324ms.tscn", null, -120f, 230f, -1f, 1f, -50f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par3_324ms.tscn", null, 160f, 380f, 1f, 1f, -10f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par3_324ms.tscn", null, -160f, 380f, -1f, 1f, 10f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par3_324ms.tscn", null, -100f, 330f, -1f, 1f, 0f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par3_324ms.tscn", null, -180f, 330f, -1f, 1f, 5f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par3_324ms.tscn", null, 180f, 330f, 1f, 1f, -5f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par3_324ms.tscn", null, 100f, 330f, 1f, 1f, 0f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par3_324ms.tscn", null, 200f, 120f, 1f, 1f, 5f, 10, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par3_324ms.tscn", null, -200f, 120f, -1f, 1f, -5f, 10, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par4_324ms.tscn", null, 260f, 480f, 1f, 1f, 30f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par4_324ms.tscn", null, -260f, 480f, -1f, 1f, -30f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par4_324ms.tscn", null, 70f, 200f, 1f, 1f, -30f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par4_324ms.tscn", null, -70f, 200f, -1f, 1f, 30f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par3_324ms.tscn", null, 180f, 540f, 1f, 1f, -20f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par3_324ms.tscn", null, -180f, 540f, -1f, 1f, 20f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par4_324ms.tscn", null, 70f, 430f, -1f, 1f, 35f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par4_324ms.tscn", null, -70f, 430f, 1f, 1f, -35f, 40, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par4_324ms.tscn", null, 220f, 200f, 1f, 1f, -30f, 10, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill2_par4_324ms.tscn", null, -220f, 200f, -1f, 1f, 30f, 10, 2.1f, 0.8242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2302001/particles/dreamy_lepidola_normal_skill1_par1_1224ms.tscn", null, 0f, 350f, 1f, 1f, 0f, 60, 1.2f, 1.7242f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/2302001/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "SwarmTargetNode", "SwarmParticles", "NEntomancerVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/entomancer/entomancer_cast", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/entomancer/entomancer_attack_ranged", EnemyAction.BasicAttack)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildLightDevourerForSlitheringStrangler()
	{
		return new EnemyReplacementProfile("light_devourer_for_slithering_strangler", new Sts2EnemyTarget("SLITHERING_STRANGLER"), new CznEnemySource("3000394_01", "3000394", "monster@name@3000394_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/slithering_strangler.tscn", "res://CznEnemySkin/animations/monsters/3000394_light_devourer/3000394_light_devourer_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-103.29968f, -288.05112f, 203.57498f, 226.75726f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Cast, "constrict", "normal_attack_ready", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_defend", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000394/normal_attack_play2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000394/spine/3000394_eff_normal_attack_play2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 10f, -5f, 0.7f, 0.7f, -10f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_weak_01_front.tscn", "animation", 10f, -5f, 1f, 1f, 0f, 0, 0f, 0.3f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_weak_01_back.tscn", "animation", 10f, -5f, 1f, 1f, 0f, -1, 0f, 0.2667f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000394/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000394/spine/3000394_eff_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_weak_01_front.tscn", "animation", -10f, 5f, 1f, 1f, 0f, 0, 0f, 0.3f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_weak_01_back.tscn", "animation", -10f, 5f, 1f, 1f, 0f, -1, 0f, 0.2667f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", -10f, 5f, 0.7f, 0.7f, -90f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000394/normal_attack_ready.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000394/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/slithering_strangler/slithering_strangler_cast", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/slithering_strangler/slithering_strangler_attack_headbutt", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/slithering_strangler/slithering_strangler_tail", EnemyAction.BasicAttack)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildLostHungerForLouseProgenitor()
	{
		return new EnemyReplacementProfile("lost_hunger_for_louse_progenitor", new Sts2EnemyTarget("LOUSE_PROGENITOR"), new CznEnemySource("3000354_01", "3000354", "monster@name@3000354_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/louse_progenitor.tscn", "res://CznEnemySkin/animations/monsters/3000354_lost_hunger/3000354_lost_hunger_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(0.8f), new EnemySelectionBounds(-171.093f, -369.3891f, 466.0321f, 392.885f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[7]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "attack_web", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "curl", "normal_debuff_unique_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.WakeUp, "uncurl", "stance1_to_normal", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000354/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000354/particles/voideater_attack_pati_01_667ms.tscn", null, -220f, 135f, 0.4f, 0.9f, -35f, 5, 0f, 1.6667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000354/spine/voideater_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 0.3f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.8f, 0.8f, -80f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000354/normal_debuff_unique_1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000354/spine/3000354_unique1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.6f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000354/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000354/spine/3000354_debuff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000354/normal_enter.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000354/particles/voidater_enter_pati_01_1067ms.tscn", null, -30f, 220f, 1f, 1f, 0f, 4, 0.733f, 2.067f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000354/spine/voideater_enter.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			}))
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/giant_louse/giant_louse_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/giant_louse/giant_louse_attack_web", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/giant_louse/giant_louse_curl", EnemyAction.Buff)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_blunt", EnemyAction.Cast)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildMaggotForKinFollower()
	{
		return new EnemyReplacementProfile("maggot_for_kin_follower", new Sts2EnemyTarget("KIN_FOLLOWER"), new CznEnemySource("1002013_01", "1002013", "monster@name@1002013_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/kin_follower.tscn", "res://CznEnemySkin/animations/monsters/1002013_maggot/1002013_maggot_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.2f), new EnemySelectionBounds(-165.24738f, -153.04288f, 310.2853f, 181.9163f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_slash", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_boomerang", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "buff", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002013/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002013/spine/maggot_normal_m_attack_01.tscn", "animation", 25f, 45f, 1f, 1f, 0f, 2, 0f, 1.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002013/particles/maggot_att_circle_pat1_1067ms.tscn", null, 47.249f, -158f, 0.4f, 1f, 0f, 3, 0.533f, 2.067f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002013/particles/maggot_att_imp_pat1_1133ms.tscn", null, -136.36f, -64f, 0.7f, 0.5f, 0f, 3, 0.467f, 2.133f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_s.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002013/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002013/spine/maggot_normal_m_attack_01.tscn", "animation", 25f, 45f, 1f, 1f, 0f, 2, 0f, 1.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002013/particles/maggot_att_circle_pat1_1067ms.tscn", null, 47.249f, -158f, 0.4f, 1f, 0f, 3, 0.533f, 2.067f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002013/particles/maggot_att_imp_pat1_1133ms.tscn", null, -136.36f, -64f, 0.7f, 0.5f, 0f, 3, 0.467f, 2.133f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_s.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002013/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002013/spine/maggot_normal_buff_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002013/particles/maggot_buff_upat1_900ms.tscn", null, -30f, -30f, 0.6f, 1f, 0f, 3, 0.667f, 1.8f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002013/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[5] { "Boomerang1Slot", "Boomerang2Slot", "HaySlot", "AttackDistanceControl", "NKinFollowerVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/the_kin_minion/the_kin_minion_quick_slash", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/the_kin_minion/the_kin_minion_boomerang_slash", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/the_kin_minion/the_kin_minion_buff", EnemyAction.Buff)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildManEaterForGremlinMerc()
	{
		return new EnemyReplacementProfile("man_eater_for_gremlin_merc", new Sts2EnemyTarget("GREMLIN_MERC"), new CznEnemySource("1002011_01", "1002011", "monster@name@1002011_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/gremlin_merc.tscn", "res://CznEnemySkin/animations/monsters/1002011_man_eater/1002011_man_eater_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-227.77466f, -305.24393f, 371.10144f, 320.44113f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_single", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_double", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "gimme", "normal_buff_2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002011/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002011/spine/man_eater_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, -100f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002011/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002011/spine/man_eater_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, -100f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002011/normal_buff_2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002011/particles/man_eater_buff_02_pati_867ms.tscn", null, 5f, 210f, 1f, 1f, 0f, 5, 0.533f, 1.367f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002011/spine/man_eater_buff_02_smoke.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002011/spine/man_eater_buff_02_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 0.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002011/spine/man_eater_buff_02_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002011/spine/man_eater_buff_02_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 0.6f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002011/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/gremlin_merc/gremlin_merc_attack", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/gremlin_merc/gremlin_merc_attack_buff", EnemyAction.BasicAttack)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_coin_explosion_regular", EnemyAction.HeavyAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildMegaBiteForFuzzyWurmCrawler()
	{
		return new EnemyReplacementProfile("mega_bite_for_fuzzy_wurm_crawler", new Sts2EnemyTarget("FUZZY_WURM_CRAWLER"), new CznEnemySource("1004021_01", "1004021", "monster@name@1004021_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/fuzzy_wurm_crawler.tscn", "res://CznEnemySkin/animations/monsters/1004021_mega_bite/1004021_mega_bite_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-100.4151f, -198.93459f, 221.48647f, 257.74274f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "inhale", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004021/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004021/spine/mega_bite_normal_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.0333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004021/normal_skill2.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004021/spine/mega_bite_normal_skill2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004021/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "SpitParticlesBone", "SpineBoneNode", "NFuzzyWurmCrawlerSpitTrailVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/fuzzy_wurm_crawler/fuzzy_wurm_crawler_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/fuzzy_wurm_crawler/fuzzy_wurm_crawler_cast", EnemyAction.Buff)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_slash", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildPaleServantForWriggler()
	{
		return new EnemyReplacementProfile("pale_servant_for_wriggler", new Sts2EnemyTarget("WRIGGLER"), new CznEnemySource("1004012_01", "1004012", "monster@name@1004012_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/wriggler.tscn", "res://CznEnemySkin/animations/monsters/1004012_pale_servant/1004012_pale_servant_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-166.31946f, -316.04193f, 342.64035f, 275.49805f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[4]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004012/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004012/spine/ectowing_attack_play1.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 2, 0f, 1.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, -90f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004012/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(Array.Empty<EnemyNativeAudioSuppression>(), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack), null, useNativeHostBounds: false, EnemyBodyOriginPolicy.ResetToSkeletonOrigin, EnemyHostTransformPolicy.Inherit, mirrorBodyHorizontally: false, centerAnchorOnSelectionBounds: true), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildPollemicaForCrusher()
	{
		return new EnemyReplacementProfile("pollemica_for_crusher", new Sts2EnemyTarget("CRUSHER"), new CznEnemySource("1001013_01", "1001013", "monster@name@1001013_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/crusher.tscn", "res://CznEnemySkin/animations/monsters/1001013_pollemica/1001013_pollemica_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.3f), new EnemySelectionBounds(-188.13843f, -405.48608f, 487.13034f, 427.41992f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[7]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "CrusherThrash", "normal_skill4", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "CrusherStrike", "normal_skill1_3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "CrusherAdapt", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "CrusherBugSting", "normal_skill1_3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[6]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001013/normal_skill4.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1.3f, 1.3f, -80f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body))),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001013/normal_skill1_3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/elite_insect_skill1_3_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.6667f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/elite_insect_skill1_3_back.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -5, 0f, 1.4333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 30f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 2f, 2f, -80f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001013/normal_skill3.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/elite_insect_skill3_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 3.5667f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001013/normal_skill1_3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/elite_insect_skill1_3_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.6667f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/elite_insect_skill1_3_back.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -5, 0f, 1.4333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 30f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 2f, 2f, -80f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001013/normal_death.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[12]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/nihilum_albino_death_01_main_b.tscn", "center", 0f, 0f, 1f, 1f, 0f, -1, 0.8f, 1.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/nihilum_albino_death_01_main_f.tscn", "center", 0f, 0f, 1f, 1f, 0f, 2, 0.8f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/nihilum_albino_death_01_meat1.tscn", "center", 0f, 0f, 1f, 1f, 0f, 1, 0.8f, 0.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/nihilum_albino_death_01_meat2.tscn", "center", 0f, 0f, 1f, 1f, 0f, 1, 0.8f, 0.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/nihilum_albino_death_01_meat3.tscn", "center", 0f, 0f, 1f, 1f, 0f, 1, 0.8f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/nihilum_albino_death_01_meat4.tscn", "center", 0f, 0f, 1f, 1f, 0f, 1, 0.8f, 0.2333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/nihilum_albino_death_01_meat5.tscn", "center", 0f, 0f, 1f, 1f, 0f, 1, 0.8f, 0.3f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/nihilum_albino_death_01_meat_stack.tscn", "center", 0f, 0f, 1f, 1f, 0f, 1, 0.8f, 1.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/invader_death_01_f4.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/invader_death_01_f3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/invader_death_01_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 6, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001013/spine/invader_death_01_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 8, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001013/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[7]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_attack_scoop", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_attack_slam", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_attack_slam", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("heavy_attack.mp3", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_attack_scissor", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_buff", EnemyAction.Buff),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_left_die", EnemyAction.Death)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[3]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Cast,
			EnemyAction.HeavyAttack
		}), null, useNativeHostBounds: false, EnemyBodyOriginPolicy.ResetVerticalOffset, EnemyHostTransformPolicy.Inherit, mirrorBodyHorizontally: true), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildRackerForCubexConstruct()
	{
		return new EnemyReplacementProfile("racker_for_cubex_construct", new Sts2EnemyTarget("CUBEX_CONSTRUCT"), new CznEnemySource("1003002_01", "1003002", "monster@name@1003002_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/cubex_construct.tscn", "res://CznEnemySkin/animations/monsters/1003002_racker/1003002_racker_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-111.47494f, -275.2292f, 249.24335f, 283.73608f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "burrowed_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_loop", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "Charge", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), null, new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003002/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003002/particles/racker_attack_pati_533ms.tscn", null, 0f, 154f, 0.42f, 0.42f, 0f, 4, 0.5f, 1.2333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003002/spine/racker_attack.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 2, 0f, 1.0333f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003002/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003002/particles/racker_debuff_02_pati_800ms.tscn", null, 0f, 84f, 0.7f, 0.7f, 0f, 5, 0.5f, 1.7f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003002/particles/racker_debuff_01_pati_900ms.tscn", null, -70f, 91f, 0.63f, 0.63f, 0f, 4, 0.5f, 2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003002/particles/racker_debuff_01_pati_900ms.tscn", null, 70f, 91f, 0.63f, 0.63f, 0f, 4, 0.5f, 2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003002/spine/racker_debuff_r_f.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 2, 0f, 1.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003002/spine/racker_debuff_l_f.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 2, 0f, 1.4f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003002/spine/racker_debuff_l_b.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -2, 0f, 1.4f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003002/spine/racker_debuff_r_b.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -2, 0f, 1.4f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			}))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "LaserBone", "NCubexConstructVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/cubex_construct/cubex_construct_charge_attack", EnemyAction.Buff)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildRechargerForGuardbot()
	{
		return new EnemyReplacementProfile("recharger_for_guardbot", new Sts2EnemyTarget("GUARDBOT"), new CznEnemySource("1003014_01", "1003014", "monster@name@1003014_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/guardbot.tscn", "res://CznEnemySkin/animations/monsters/1003014_recharger_bot/1003014_recharger_bot_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-92.386055f, -141.63075f, 189.96678f, 152.21149f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003014/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_attack_pati_1067ms.tscn", null, -15f, 110f, 0.5f, 0.5f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/spine/recharger_attack.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003014/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_debuff_01_pati_1067ms.tscn", null, -90f, 70f, 0.7f, 0.7f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_debuff_01_pati_1067ms.tscn", null, 90f, 70f, 0.7f, 0.7f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_debuff_02_pati_1067ms.tscn", null, 0f, 50f, 1f, 1f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/spine/recharger_buff_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/spine/recharger_buff_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -3, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003014/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "HeightControl", "FallControl" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/guardbot/guardbot_cast", EnemyAction.Cast)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), Array.Empty<EnemyAction>(), null, useNativeHostBounds: false, EnemyBodyOriginPolicy.ResetToSkeletonOrigin), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildRechargerForNoisebot()
	{
		return new EnemyReplacementProfile("recharger_for_noisebot", new Sts2EnemyTarget("NOISEBOT"), new CznEnemySource("1003014_01", "1003014", "monster@name@1003014_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/noisebot.tscn", "res://CznEnemySkin/animations/monsters/1003014_recharger_bot/1003014_recharger_bot_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-92.386055f, -141.63075f, 189.96678f, 152.21149f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003014/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_attack_pati_1067ms.tscn", null, -15f, 110f, 0.5f, 0.5f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/spine/recharger_attack.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003014/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_debuff_01_pati_1067ms.tscn", null, -90f, 70f, 0.7f, 0.7f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_debuff_01_pati_1067ms.tscn", null, 90f, 70f, 0.7f, 0.7f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_debuff_02_pati_1067ms.tscn", null, 0f, 50f, 1f, 1f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/spine/recharger_buff_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/spine/recharger_buff_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -3, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003014/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "HeightControl", "FallControl" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/noisebot/noisebot_cast", EnemyAction.Cast)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), Array.Empty<EnemyAction>(), null, useNativeHostBounds: false, EnemyBodyOriginPolicy.ResetToSkeletonOrigin), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildRechargerForStabbot()
	{
		return new EnemyReplacementProfile("recharger_for_stabbot", new Sts2EnemyTarget("STABBOT"), new CznEnemySource("1003014_01", "1003014", "monster@name@1003014_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/stabbot.tscn", "res://CznEnemySkin/animations/monsters/1003014_recharger_bot/1003014_recharger_bot_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-92.386055f, -141.63075f, 189.96678f, 152.21149f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003014/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_attack_pati_1067ms.tscn", null, -15f, 110f, 0.5f, 0.5f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/spine/recharger_attack.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003014/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_debuff_01_pati_1067ms.tscn", null, -90f, 70f, 0.7f, 0.7f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_debuff_01_pati_1067ms.tscn", null, 90f, 70f, 0.7f, 0.7f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_debuff_02_pati_1067ms.tscn", null, 0f, 50f, 1f, 1f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/spine/recharger_buff_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/spine/recharger_buff_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -3, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003014/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "HeightControl", "FallControl" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(Array.Empty<EnemyNativeAudioSuppression>(), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack), null, useNativeHostBounds: false, EnemyBodyOriginPolicy.ResetToSkeletonOrigin), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildRechargerForZapbot()
	{
		return new EnemyReplacementProfile("recharger_for_zapbot", new Sts2EnemyTarget("ZAPBOT"), new CznEnemySource("1003014_01", "1003014", "monster@name@1003014_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/zapbot.tscn", "res://CznEnemySkin/animations/monsters/1003014_recharger_bot/1003014_recharger_bot_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-92.386055f, -141.63075f, 189.96678f, 152.21149f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003014/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_attack_pati_1067ms.tscn", null, -15f, 110f, 0.5f, 0.5f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/spine/recharger_attack.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003014/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_debuff_01_pati_1067ms.tscn", null, -90f, 70f, 0.7f, 0.7f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_debuff_01_pati_1067ms.tscn", null, 90f, 70f, 0.7f, 0.7f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/particles/racker_debuff_02_pati_1067ms.tscn", null, 0f, 50f, 1f, 1f, 0f, 5, 0.5f, 2.0667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/spine/recharger_buff_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003014/spine/recharger_buff_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -3, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003014/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "HeightControl", "FallControl", "LaserAttach" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(Array.Empty<EnemyNativeAudioSuppression>(), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack), null, useNativeHostBounds: false, EnemyBodyOriginPolicy.ResetToSkeletonOrigin), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildResentfulConvictForTheInsatiable()
	{
		return new EnemyReplacementProfile("resentful_convict_for_the_insatiable", new Sts2EnemyTarget("THE_INSATIABLE"), new CznEnemySource("3000438_01", "3000438", "monster@name@3000438_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/the_insatiable.tscn", "res://CznEnemySkin/animations/monsters/3000438_resentful_convict/3000438_resentful_convict_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-576.351f, -665.51044f, 1048.5208f, 674.1715f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[8]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "Thrash", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "Bite", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "salivate", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "liquify_sand", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Special, "eat_player", "normal_s_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[7]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000438/normal_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_attack1_play_slash_obake.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0.4667f, 0.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_attack1_play_slash_smoke.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0.4667f, 0.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/particles/3000438_attack1_play_slash_pat_667ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 0, 0.4667f, 1.3167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_attack_play_target.tscn", "animation", 0f, 0f, 0.6f, 0.6f, -35f, 0, 0f, 0.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/particles/bulk_3000438_attack_play_target_1.tscn", null, 0f, 0f, 0.6f, 0.6f, -35f, -5, 0f, 1.1667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_attack1_smoke_f.tscn", "animation", 70f, 50f, 0.7f, 0.7f, 0f, 0, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_attack1_smoke_b.tscn", "animation", 70f, 50f, 0.7f, 0.7f, 0f, -1, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000438/normal_attack_play1.ogg", 0.4667f))),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000438/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[9]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_m_attack_play_star.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_m_attack_play_slash2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 2.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_m_attack_play_slash4.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 1.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_m_attack_play_smoke.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 2.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/particles/3000438_m_attack_play_target_pat_3267ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 0, 0f, 4.5167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_m_attack_ready_mouse.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 1.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_m_attack_play_target.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 0.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_attack1_smoke_f.tscn", "animation", 70f, 50f, 0.9f, 0.9f, 0f, 0, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_attack1_smoke_b.tscn", "animation", 70f, 50f, 0.9f, 0.9f, 0f, -1, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000438/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_buff_play_backsmoke.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 3.2333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_buff_play_scream1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 2.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_buff_play_scream2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 2.7f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_buff_play_smoke.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 2.8f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000438/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_debuff_play_light.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_debuff_play_smoke.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 1.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_debuff_ready.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Special, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000438/normal_s_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_s_attack_play_mousedust.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 2.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_s_attack_play_mouse.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_s_attack_ready.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 0.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_s_attack_play_obake.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_s_attack_play_bite.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 2.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000438/spine/3000438_s_attack_play_line.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 2.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true)
			})),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000438/normal_death.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000438/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[4] { "SandSlotNode", "SalivaSlotNode", "BaseBlastSlot", "NTheInsatiableVfx" }), new _003C_003Ez__ReadOnlySingleElementList<string>("PhobiaModeVisuals"), Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[5]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_liquify_ground", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_thrash", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_lunging_bite", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_salivate", EnemyAction.Buff),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/the_insatiable/the_insatiable_finisher", EnemyAction.Special)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_scream", EnemyAction.Cast)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[3]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack,
			EnemyAction.Special
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildRockShellInsectForSkulkingColony()
	{
		return new EnemyReplacementProfile("rock_shell_insect_for_skulking_colony", new Sts2EnemyTarget("SKULKING_COLONY"), new CznEnemySource("1001019_01", "1001019", "monster@name@1001019_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/skulking_colony.tscn", "res://CznEnemySkin/animations/monsters/1001019_rock_shell_insect/1001019_rock_shell_insect_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.1f), new EnemySelectionBounds(-178.73558f, -295.02155f, 267.08252f, 316.26147f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_heavy", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_double", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "attack_buff", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001019/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001019/spine/rock_shell_insect_normal_skill1_3_shoot.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001019/spine/rock_shell_insect_normal_skill1_3_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001019/spine/rock_shell_insect_normal_skill1_3_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 0.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0.467f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -1, 0.467f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001019/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001019/spine/rock_shell_insect_normal_skill1_3_shoot.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001019/spine/rock_shell_insect_normal_skill1_3_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001019/spine/rock_shell_insect_normal_skill1_3_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 0.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0.467f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -1, 0.467f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001019/normal_skill2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001019/spine/rock_shell_insect_normal_skill2_smoke_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001019/spine/rock_shell_insect_normal_skill2_smoke_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -30, 0f, 1.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001019/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[4] { "ParticleSlot1", "ParticleSlot2", "ParticleSlot3", "NSkulkingColonyVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/skulking_colony/skulking_colony_kick", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/skulking_colony/skulking_colony_spin", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/skulking_colony/skulking_colony_thrust", EnemyAction.Buff)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_blunt", EnemyAction.Buff)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildRottenReaperForKinPriest()
	{
		return new EnemyReplacementProfile("rotten_reaper_for_kin_priest", new Sts2EnemyTarget("KIN_PRIEST"), new CznEnemySource("1002008_01", "1002008", "monster@name@1002008_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/kin_priest.tscn", "res://CznEnemySkin/animations/monsters/1002008_rotten_reaper/1002008_rotten_reaper_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.1f), new EnemySelectionBounds(-182.75378f, -478.76285f, 384.69604f, 492.47775f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_grenade", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_laser", "normal_s_attack_01_to_stance1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "rally", "stance1_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002008/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/spine/rotten_reaper_normal_m_attack_01_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0.5f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/spine/rotten_reaper_normal_m_attack_01_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 402, 0.5f, 0.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/particles/rotten_reaper_obk_pati1_933ms.tscn", null, -80f, 210f, 0.88f, 2.8f, 4f, 1, 0.567f, 1.833f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/particles/rotten_reaper_exp_pati1_967ms.tscn", null, -210f, -65f, 1f, 1f, 0f, 3, 0.533f, 1.967f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, -30f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002008/normal_s_attack_01_to_stance1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[8]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/spine/rotten_reaper_normal_special_1_to_stance1_b2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -402, 0f, 1.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/spine/rotten_reaper_normal_special_1_to_stance1_b1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.0333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/spine/rotten_reaper_normal_special_1_to_stance1_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.7f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/spine/rotten_reaper_normal_special_1_to_stance1_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 402, 0f, 0.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/particles/rotten_reaper_exp_pati2_1200ms.tscn", null, 175.33f, 177.5f, 4f, 4f, 0f, 1, 0.5f, 2.2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/particles/rotten_reaper_buff_pati1_1200ms.tscn", null, 75.33f, 77.5f, 1f, 1f, 0f, 1, 0.5f, 2.7f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/particles/rotten_reaper_buff_pati1_1200ms.tscn", null, 275.33f, 77.5f, 1f, 1f, 0f, 1, 0.5f, 2.7f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_s.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 1f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002008/stance1_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/spine/rotten_reaper_stance1_buff_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0.5f, 1.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/particles/rotten_reaper_buff_pati1_934ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 0, 1.233f, 2.434f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002008/normal_enter.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/spine/rotten_reaper_normal_enter_b2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -402, 0f, 1.9667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/spine/rotten_reaper_normal_enter_b1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002008/spine/rotten_reaper_normal_enter_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			}))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[4] { "TorchFireBone", "Beam", "SpineBoneNode", "NKinPriestVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_soul_grenade", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_soul_beam", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/the_kin_priest/the_kin_priest_rally", EnemyAction.Buff)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildRustyNailSevenArmsForTorchHeadAmalgam()
	{
		return new EnemyReplacementProfile("rusty_nail_seven_arms_for_torch_head_amalgam", new Sts2EnemyTarget("TORCH_HEAD_AMALGAM"), new CznEnemySource("3000491_01", "3000491", "monster@name@3000491_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/torch_head_amalgam.tscn", "res://CznEnemySkin/animations/monsters/3000491_rusty_nail_seven_arms/3000491_rusty_nail_seven_arms_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.1f), new EnemySelectionBounds(-185.77638f, -423.14746f, 412.20392f, 488.06607f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "Attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "DebuffTrigger", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, Array.Empty<EnemyAudioCue>(), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000491/spine/seven_arms_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0.4667f, 0.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000491/spine/seven_arms_smoke.tscn", "normal_attack_play1", 0f, 0f, 1f, 1f, 0f, 1, 0.4667f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000491/particles/seven_arms_normal_attack_play1_pati1_667ms.tscn", null, 200f, 50f, 1f, 1f, 30f, 3, 0.4667f, 1.8f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_pierce_normal_s.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 0, 0f, 1.1f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000491/normal_attack_play1.ogg", 0.4667f))),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000491/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000491/spine/seven_arms_normal_debuff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000491/particles/seven_arms_normal_debuff_pati1_1067ms.tscn", null, 0f, -150f, 1f, 1f, 0f, 2, 0.533f, 2.7f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000491/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[11]
		{
			"torch1Slot", "torch2Slot", "torch3Slot", "CPUDeathParticles", "laserBaseBone", "laserHitBone", "torch1UnscaledBone", "torch2UnscaledBone", "torch3UnscaledBone", "LaserControlBone",
			"NAmalgamVfx"
		}), new _003C_003Ez__ReadOnlySingleElementList<string>("laserHitParticles"), Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/torch_head_amalgam/torch_head_amalgam_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/torch_head_amalgam/torch_head_amalgam_beam", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack), new EnemyInteractionBoundsAdapter(350f)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildSaintHoundForLivingShield()
	{
		return new EnemyReplacementProfile("saint_hound_for_living_shield", new Sts2EnemyTarget("LIVING_SHIELD"), new CznEnemySource("1003011_01", "1003011", "monster@name@1003011_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/living_shield.tscn", "res://CznEnemySkin/animations/monsters/1003011_saint_hound/1003011_saint_hound_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-139.40135f, -248.25758f, 383.86053f, 283.53204f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_smash", "normal_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003011/normal_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003011/spine/saint_hound_normal_attack_play1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0.5667f, 0.6333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003011/spine/saint_hound_normal_attack_play1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0.5667f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 30f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1003011/normal_attack_play1.ogg", 0.5667f))),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003011/normal_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003011/spine/saint_hound_normal_attack_play2_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0.5667f, 0.6333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003011/spine/saint_hound_normal_attack_play2_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0.5667f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, -110f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1003011/normal_attack_play2.ogg", 0.5667f)))
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/living_shield/living_shield_attack", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildSandwormForTunneler()
	{
		return new EnemyReplacementProfile("sandworm_for_tunneler", new Sts2EnemyTarget("TUNNELER"), new CznEnemySource("2104121_80", "2104121", "monster@name@2104121_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/tunneler.tscn", "res://CznEnemySkin/animations/monsters/2104121_sandworm/2104121_sandworm_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-140.312f, -221.46931f, 288f, 241.87782f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[9]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "BurrowAttack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "burrow", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.StunTransition, "stun", "normal_groggy", Loop: false),
			new EnemyAnimationBinding(EnemyAction.StunLoop, "stunned_loop", "normal_groggy", Loop: true),
			new EnemyAnimationBinding(EnemyAction.WakeUp, "wake_up", "normal_enter", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[5]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/2104121/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/particles/sandworm_attack_pati_01_02_1000ms.tscn", null, -200f, 350f, 0.8f, 0.8f, 0f, 6, 0.5f, 2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/particles/sandworm_attack_pati_01_01_1000ms.tscn", null, 140f, 50f, 0.8f, 0.8f, 0f, 6, 0.5f, 2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/spine/sandworm_attack.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/2104121/nest_normal_special_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/particles/sandworm_nest_special_01_pati_1133ms.tscn", null, 0f, 250f, 1f, 1f, 0f, 6, 0.5f, 2.1333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/spine/sandworm_nest_special.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/2104121/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[12]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/particles/elemental_spirit_normal_debuff_01_pati_01_234ms.tscn", null, -50f, 130f, 1f, 1f, 0f, 5, 1.266f, 1.234f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/particles/elemental_spirit_normal_debuff_01_pati_01_300ms.tscn", null, -50f, 130f, 1f, 1f, 0f, 5, 1.2f, 1.3f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/particles/elemental_spirit_normal_debuff_01_pati_01_400ms.tscn", null, -50f, 130f, 1f, 1f, 0f, 5, 1.1f, 1.4f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/particles/elemental_spirit_normal_debuff_01_pati_01_500ms.tscn", null, -50f, 130f, 1f, 1f, 0f, 5, 1f, 1.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/particles/elemental_spirit_normal_debuff_01_pati_01_600ms.tscn", null, -50f, 130f, 1f, 1f, 0f, 5, 0.9f, 1.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/particles/elemental_spirit_normal_debuff_01_pati_01_700ms.tscn", null, -50f, 130f, 1f, 1f, 0f, 5, 0.8f, 1.7f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/particles/elemental_spirit_normal_debuff_01_pati_01_800ms.tscn", null, -50f, 130f, 1f, 1f, 0f, 5, 0.7f, 1.8f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/particles/elemental_spirit_normal_debuff_01_pati_01_900ms.tscn", null, -50f, 130f, 1f, 1f, 0f, 5, 0.6f, 1.9f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/particles/elemental_spirit_normal_debuff_01_pati_01_1000ms.tscn", null, -50f, 130f, 1f, 1f, 0f, 5, 0.5f, 2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/spine/sandworm_debuff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 1.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/particles/sandworm_nest_debuff_01_pati_1066ms.tscn", null, 0f, 200f, 1f, 1f, 0f, 6, 0.567f, 2.0663f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/2104121/spine/sandworm_nest_debuff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.1f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.WakeUp, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/2104121/normal_enter.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/2104121/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlySingleElementList<string>("SpineBoneNode"), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_burrow", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/burrowing_bug/burrowing_bug_hidden_attack", EnemyAction.HeavyAttack)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, new _003C_003Ez__ReadOnlySingleElementList<EnemySecondaryBodyProfile>(new EnemySecondaryBodyProfile("nest", "res://CznEnemySkin/animations/monsters/2104123_sandworm_nest/2104123_sandworm_nest_skel_data.tres", 1f, InitiallyVisible: false, HidePrimaryInitially: false, null, "normal_idle", -1)), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildScratcherForParafright()
	{
		return new EnemyReplacementProfile("scratcher_for_parafright", new Sts2EnemyTarget("PARAFRIGHT"), new CznEnemySource("1007004_01", "1007004", "monster@name@1007004_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/parafright.tscn", "res://CznEnemySkin/animations/monsters/1007004_scratcher/1007004_scratcher_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-160.36363f, -307.75247f, 280.86383f, 304.83008f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[8]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Entrance, "spawn", "normal_enter", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.StunTransition, "StunTrigger", "normal_groggy", Loop: false),
			new EnemyAnimationBinding(EnemyAction.StunLoop, "stunned_loop", "normal_groggy", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Buff, "WakeUpTrigger", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_groggy", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1007004/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007004/spine/scratcher_normal_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, -110f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1007004/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007004/spine/scratcher_normal_buff_1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1007004/spine/scratcher_normal_buff_1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1007004/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "NParafrightVfx", "ParticlesSlot" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/obscura/obscura_hologram_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/obscura/obscura_hologram_heal", EnemyAction.Buff)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildSenectusForSoulFysh()
	{
		return new EnemyReplacementProfile("senectus_for_soul_fysh", new Sts2EnemyTarget("SOUL_FYSH"), new CznEnemySource("3000332_01", "3000332", "monster@name@3000332_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/soul_fysh.tscn", "res://CznEnemySkin/animations/monsters/3000332_senectus/3000332_senectus_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-202.00238f, -407.77985f, 313.55743f, 432.5667f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[11]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_heavy", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_beckon", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Special, "Beckon", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "IntangibleStart", "normal_to_stance1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Idle, "intangible_loop", "stance1_idle", Loop: true, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Cast, "AttackDebuffTrigger", "stance1_debuff_1", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt_intangible", "stance1_hit", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Death, "die_intangible", "die_intangible", Loop: false, EnemyPresentationStance.Transformed)
		}), null, new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[5]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000332/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_attack_play_1_f.tscn", "normal", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_attack_play_1_b.tscn", "normal", 0f, 0f, 1f, 1f, 0f, -5, 0f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_attack_target_f.tscn", "animation", 0f, 0f, 0.65f, 0.65f, 0f, 5, 0.667f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/particles/bulk_senectus_3000332_attack_target_1.tscn", null, 0f, 0f, 0.65f, 0.65f, 0f, 3, 1.234f, 2.5663f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/particles/bulk_senectus_3000332_attack_target_2.tscn", null, 0f, 0f, 0.65f, 0.65f, 0f, 2, 1.234f, 0.066f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_attack_target_b.tscn", "animation", 0f, 0f, 0.65f, 0.65f, 0f, -5, 0.667f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null, TargetHit: true, TargetAtEntry: true)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000332/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_attack_play_1_f.tscn", "normal", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_attack_play_1_b.tscn", "normal", 0f, 0f, 1f, 1f, 0f, -5, 0f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_attack_target_f.tscn", "animation", 0f, 0f, 0.65f, 0.65f, 0f, 5, 0.667f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/particles/bulk_senectus_3000332_attack_target_1.tscn", null, 0f, 0f, 0.65f, 0.65f, 0f, 3, 1.234f, 2.5663f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/particles/bulk_senectus_3000332_attack_target_2.tscn", null, 0f, 0f, 0.65f, 0.65f, 0f, 2, 1.234f, 0.066f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_attack_target_b.tscn", "animation", 0f, 0f, 0.65f, 0.65f, 0f, -5, 0.667f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null, TargetHit: true, TargetAtEntry: true)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000332/normal_to_stance1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/particles/senectus_3000332_stance_1_idle_pati_04_667ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 5, 2f, 2.5667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_stance_1_idle_f.tscn", "intro", 0f, 0f, 1f, 1f, 0f, 3, 0f, 5.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_stance_1_idle_b_1.tscn", "intro", 0f, 0f, 1f, 1f, 0f, -3, 0f, 5.8667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/particles/senectus_3000332_stance_1_idle_pati_01_367ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 3, 2.3f, 1.8667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_stance_1_idle_b_2.tscn", "intro", 0f, 0f, 1f, 1f, 0f, -5, 0f, 5.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000332/stance1_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[9]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_stance_debuff_play_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/particles/senectus_3000332_stance_debuff_target_pati_03_1900ms.tscn", null, 0f, 320f, 1f, 1f, 0f, 2, 0.1f, 3.8f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/particles/senectus_3000332_stance_debuff_target_pati_01_1000ms.tscn", null, 0f, 320f, 1f, 1f, 0f, 1, 1f, 1.9f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_stance_debuff_play_b_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -3, 0f, 1.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_stance_debuff_play_b_2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -4, 0f, 1.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_stance_debuff_play_b_3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -5, 0f, 1.4667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_stance_debuff_target1.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 5, 0.467f, 0.5667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null, TargetHit: true, TargetAtEntry: true, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/particles/bulk_senectus_3000332_stance_debuff_target_2_0.tscn", null, 0f, 50f, 0.65f, 0.65f, 0f, 4, 1.034f, 1.4997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true, TargetCenter: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_stance_debuff_target2.tscn", "animation", 0f, 50f, 0.65f, 0.65f, 0f, 3, 0.467f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null, TargetHit: true, TargetAtEntry: true, TargetCenter: true)
			})),
			new EnemyActionPresentation(EnemyAction.Special, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000332/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_attack_play_1_f.tscn", "normal", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_attack_play_1_b.tscn", "normal", 0f, 0f, 1f, 1f, 0f, -5, 0f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_attack_target_f.tscn", "animation", 0f, 0f, 0.65f, 0.65f, 0f, 5, 0.667f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/particles/bulk_senectus_3000332_attack_target_1.tscn", null, 0f, 0f, 0.65f, 0.65f, 0f, 3, 1.234f, 2.5663f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/particles/bulk_senectus_3000332_attack_target_2.tscn", null, 0f, 0f, 0.65f, 0.65f, 0f, 2, 1.234f, 0.066f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000332/spine/senectus_3000332_attack_target_b.tscn", "animation", 0f, 0f, 0.65f, 0.65f, 0f, -5, 0.667f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null, TargetHit: true, TargetAtEntry: true)
			}))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "Soundwave", "Beckonwave", "NSoulFyshVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[5]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_beckon", EnemyAction.Special),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_intangible", EnemyAction.Buff),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_wave", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/soul_fysh/soul_fysh_reappear", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[4]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Cast,
			EnemyAction.HeavyAttack,
			EnemyAction.Special
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildShadowCollectorForQueen()
	{
		return new EnemyReplacementProfile("shadow_collector_for_queen", new Sts2EnemyTarget("QUEEN"), new CznEnemySource("3000481_01", "3000481", "monster@name@3000481_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/queen.tscn", "res://CznEnemySkin/animations/monsters/3000481_shadow_collector/3000481_shadow_collector_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.2f), new EnemySelectionBounds(-268.53903f, -483.72327f, 517.588f, 555.6882f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[11]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "Attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "Cast", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Charge, "PhaseTwo", "normal_to_stance1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "stance1_idle", Loop: true, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "stance1_multi", "stance1_attack_play1", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Special, "stance1_execution", "stance1_attack_play2", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "stance1_hit", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "stance1_death", Loop: false, EnemyPresentationStance.Transformed)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[7]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000481/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_normal_attack_play1_b.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, -2, 0f, 1.1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_normal_attack_play1_f1.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 2, 0f, 1.1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_normal_attack_play1_f2.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 500, 0f, 0.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_w_obk_pati1_1167ms.tscn", null, 10f, 255f, 0.44f, 1.782f, -21f, 0, 0.033f, 2.3f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000481/stance1_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[13]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_stance1_attack_play1_b.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, -2, 0.2667f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_stance1_attack_play1_f1.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 2, 0.2667f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_stance1_attack_play1_f2.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 500, 0.2667f, 0.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati1_1267ms.tscn", null, 95f, 352f, 3.19f, 1.375f, 3f, 1, 0.3067f, 2.4f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati1_1267ms.tscn", null, 95f, 310f, 3.08f, 1.43f, 1f, 1, 0.3067f, 2.4f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati1_1267ms.tscn", null, 100f, 250f, 3.08f, 1.408f, 4.5f, 1, 0.3067f, 2.4f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati1_1267ms.tscn", null, 90f, 205f, 2.86f, 1.408f, 6.5f, 1, 0.3067f, 2.4f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati1_1267ms.tscn", null, 90f, 205f, 2.86f, 1.408f, 6.5f, 1, 0.3067f, 2.4f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati1_1267ms.tscn", null, 90f, 205f, 2.86f, 1.408f, 6.5f, 1, 0.3067f, 2.4f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati1_1267ms.tscn", null, 90f, 205f, 2.86f, 1.408f, 6.5f, 1, 0.3067f, 2.4f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", -70f, -70f, 0.9f, 0.9f, -107f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 100f, 0.9f, 0.9f, -93f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1.2f, 1.2f, -100f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000481/stance1_attack_play1.ogg", 0.2667f))),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000481/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_normal_debuff_f1.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 2, 0f, 1.1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_normal_debuff_f2.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 500, 0f, 0.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_normal_debuff_target_b.tscn", "animation", 0f, -20f, 0.7f, 0.7f, 0f, -300, 0.5f, 1.3f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_normal_debuff_target_f.tscn", "animation", 0f, -20f, 0.7f, 0.7f, 0f, 302, 0.5f, 0.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true)
			})),
			new EnemyActionPresentation(EnemyAction.Special, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000481/stance1_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[13]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_stance1_attack_play2_b.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, -2, 0.2667f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_stance1_attack_play2_f1.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 2, 0.2667f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_stance1_attack_play2_f2.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 500, 0.2667f, 0.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati2_1067ms.tscn", null, -60f, 345f, -3.08f, 1.155f, -12f, 1, 0.3067f, 2.2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati2_1067ms.tscn", null, -128f, 290f, -3.41f, 1.32f, -10f, 1, 0.3067f, 2.2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati2_1067ms.tscn", null, -125f, 260f, -3.41f, 1.485f, -13f, 1, 0.3067f, 2.2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati2_1067ms.tscn", null, -125f, 235f, -2.97f, 1.54f, -15f, 1, 0.3067f, 2.2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati2_1067ms.tscn", null, -125f, 235f, -2.97f, 1.54f, -15f, 1, 0.3067f, 2.2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati2_1067ms.tscn", null, -125f, 235f, -2.97f, 1.54f, -15f, 1, 0.3067f, 2.2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_h_obk_pati2_1067ms.tscn", null, -125f, 235f, -2.97f, 1.54f, -15f, 1, 0.3067f, 2.2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 100f, 0.9f, 0.9f, 38f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, -100f, 0.9f, 0.9f, 52f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1.2f, 1.2f, 45f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000481/stance1_attack_play2.ogg", 0.2667f))),
			new EnemyActionPresentation(EnemyAction.Charge, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000481/normal_to_stance1.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000481/normal_death.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[14]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_normal_death_b.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, -500, 0f, 2.0333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_normal_death_f2.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 1, 0f, 1.0333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_normal_death_f1.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 2, 0f, 3.1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_aura_pati1_1400ms.tscn", null, 150f, 0f, 1.1f, 1.1f, 0f, 3, 1.067f, 3.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_aura_pati1_1400ms.tscn", null, -150f, 0f, 1.1f, 1.1f, 0f, 3, 1.067f, 3.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_aura_pati1_1400ms.tscn", null, 0f, 0f, 1.54f, 1.54f, 0f, 3, 1.067f, 3.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_aura_pati1_1400ms.tscn", null, 0f, 0f, 1.1f, 1.1f, 0f, 3, 1.1f, 3.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_aura_pati1_1400ms.tscn", null, 0f, 100f, 0.77f, 0.77f, 0f, 3, 1.133f, 3.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_death_pat1_1400ms.tscn", null, 0f, 70f, 2.2f, 2.2f, 0f, 3, 1.033f, 3.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_death_pat2_1400ms.tscn", null, 0f, -50f, 2.2f, 2.2f, 0f, 3, 1.1f, 3.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_death_pat2_1400ms.tscn", null, 0f, -50f, 2.2f, 2.2f, 0f, 3, 1.1f, 3.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_death_pat2_1400ms.tscn", null, 0f, -50f, 2.2f, 2.2f, 0f, 3, 1.1f, 3.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_aura_pati1_1400ms.tscn", null, 100f, 0f, 1.1f, 1.1f, 0f, 3, 1.1f, 3.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_death_pat2_1400ms.tscn", null, 0f, 150f, 1.1f, 1.1f, 0f, 3, 1.1f, 3.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000481/normal_enter.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_enter_b.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, -500, 0f, 1.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/spine/soulcollector_enter_f.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 2, 0f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_aura_pati2_1067ms.tscn", null, 0f, 70f, 1.32f, 1.32f, 0f, 3, 0.6f, 2.7f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000481/particles/soulcollector_aura_pati3_1067ms.tscn", null, 0f, 120f, 0.88f, 0.88f, 0f, 4, 0.6f, 2.7f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			}))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[6] { "eyeSlot", "eyeSlot2", "SpineParticleSlot", "SpineBoneNode", "EyeBoneTop", "EyeBoneBot" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/queen/queen_cast", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/queen/queen_arms_attack", EnemyAction.BasicAttack)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[3]
		{
			EnemyAction.Cast,
			EnemyAction.HeavyAttack,
			EnemyAction.Special
		}), new EnemyInteractionBoundsAdapter(316f)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildShapeshifterForTwoTailedRat()
	{
		return new EnemyReplacementProfile("shapeshifter_for_two_tailed_rat", new Sts2EnemyTarget("TWO_TAILED_RAT"), new CznEnemySource("1004004_01", "1004004", "monster@name@1004004_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/two_tailed_rat.tscn", "res://CznEnemySkin/animations/monsters/1004004_shapeshifter/1004004_shapeshifter_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.4f), new EnemySelectionBounds(-142.22118f, -285.98453f, 367.56595f, 311.98035f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "debuff", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "summon", "normal_enter", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004004/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004004/spine/shapeshifter_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 160f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", -60f, 0f, 0.7f, 0.7f, 160f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 60f, 0f, 0.7f, 0.7f, 160f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004004/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004004/spine/shapeshifter_skill3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004004/particles/shapeshifter_skill3_pat_1500ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 0, 0.467f, 1.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004004/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004004/spine/shapeshifter_skill3.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004004/particles/shapeshifter_skill3_pat_1500ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 0, 0.467f, 1.6f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004004/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/two_tail_rats/two_tail_rats_attack_hands", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/two_tail_rats/two_tail_rats_attack_bite", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/two_tail_rats/two_tail_rats_summon", EnemyAction.Buff)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildBareBeetleForShrinkerBeetle()
	{
		return new EnemyReplacementProfile("bare_beetle_for_shrinker_beetle", new Sts2EnemyTarget("SHRINKER_BEETLE"), new CznEnemySource("1001023_01", "1001023", "monster@name@1001023_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/shrinker_beetle.tscn", "res://CznEnemySkin/animations/monsters/1001023_bare_beetle/1001023_bare_beetle_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-88.9003f, -114.5015f, 198.6978f, 134.2705f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001023/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[8]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001023/spine/bare_beetle_normal_skill1_smoke_s_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001023/spine/bare_beetle_normal_skill1_smoke_s_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -30, 0f, 1.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001023/spine/bare_beetle_normal_skill1_smoke_b_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -50, 0f, 1.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001023/spine/bare_beetle_normal_skill1_smoke_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 50, 0f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001023/spine/bare_beetle_normal_skill1_obake.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 20, 0f, 0.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001023/spine/bare_beetle_normal_skill1_shoot.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 40, 0f, 0.7f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001023/normal_skill2.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001023/spine/bare_beetle_normal_skill2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.1f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001023/invader_death_01.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001023/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/shrinker_beetle/shrinker_beetle_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/shrinker_beetle/shrinker_beetle_cast", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/shrinker_beetle/shrinker_beetle_die", EnemyAction.Death)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_slash", EnemyAction.BasicAttack)), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxFeatureSuppression>(new EnemyNativeVfxFeatureSuppression(EnemyNativeVfxFeature.RadialBlur, EnemyAction.Cast)), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildSingleEdgeLepturusForCalcifiedCultist()
	{
		return new EnemyReplacementProfile("single_edge_lepturus_for_calcified_cultist", new Sts2EnemyTarget("CALCIFIED_CULTIST"), new CznEnemySource("1001025_01", "1001025", "monster@name@1001025_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/calcified_cultist.tscn", "res://CznEnemySkin/animations/monsters/1001025_single_edge_lepturus/1001025_single_edge_lepturus_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-152.90407f, -256.42636f, 297.72852f, 279.3168f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "buff", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001025/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001025/spine/single_edge_lepturus_normal_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 20, 0f, 0.7f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.5f, 0.5f, -80f, 0, 0.03f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001025/normal_skill3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001025/spine/single_edge_lepturus_normal_skill3_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 20, 0f, 1.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001025/spine/single_edge_lepturus_normal_skill3_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -20, 0f, 1.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001025/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/cultists/cultists_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/cultists/cultists_buff_calcified", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildSlashghoulForFossilStalker()
	{
		return new EnemyReplacementProfile("slashghoul_for_fossil_stalker", new Sts2EnemyTarget("FOSSIL_STALKER"), new CznEnemySource("1002021_01", "1002021", "monster@name@1002021_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/fossil_stalker.tscn", "res://CznEnemySkin/animations/monsters/1002021_slashghoul/1002021_slashghoul_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-145.32361f, -239.33264f, 324.3684f, 253.8044f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_double", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "debuff", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), null, new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002021/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002021/spine/slashghoul_normal_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 25f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.6f, 0.6f, -80f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002021/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002021/spine/slashghoul_normal_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 25f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.6f, 0.6f, -80f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002021/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002021/spine/slashghoul_normal_debuff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "DebuffBone", "NFossilStalkerVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/fossil_stalker/fossil_stalker_attack_single", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/fossil_stalker/fossil_stalker_attack_double", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/fossil_stalker/fossil_stalker_attack_buff", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildSlaughterForInfestedPrism()
	{
		return new EnemyReplacementProfile("slaughter_for_infested_prism", new Sts2EnemyTarget("INFESTED_PRISM"), new CznEnemySource("1003007_01", "1003007", "monster@name@1003007_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/infested_prism.tscn", "res://CznEnemySkin/animations/monsters/1003007_slaughter/1003007_slaughter_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.25f), new EnemySelectionBounds(-207.39561f, -472.01096f, 439.5714f, 450.44473f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[7]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Cast, "buff", "normal_buff", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_block", "normal_job_attack_play", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Charge, "attack_double", "stance1_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[5]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003007/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_normal_attack_play1_f1.tscn", "animation", 0f, 0f, 1.4f, 1.4f, 0f, 3, 0f, 0.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_normal_attack_play1_f3.tscn", "animation", 0f, 0f, 1.4f, 1.4f, 0f, 1, 0f, 0.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_normal_attack_play1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_normal_attack_play1_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 60f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1003007/hit_slash_normal_01.ogg", 0.1f))),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003007/normal_job_attack_play.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_normal_job_attack_play.tscn", "animation", 0f, 0f, 1.4f, 1.4f, 0f, 2, 0f, 0.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_normal_job_attack_target_f2_action.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 202, 0f, 1.0333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_normal_job_attack_target_f3_action.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 201, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_normal_job_attack_target_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1003007/hit_slash_normal_01.ogg", 0.25f))),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003007/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_normal_buff.tscn", "animation", 0f, 0f, 1.4f, 1.4f, 0f, 2, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Charge, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003007/stance1_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_stance1_m_attack_01_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_stance1_m_attack_01_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 1.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_stance1_m_attack_01_target_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0.533f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_stance1_m_attack_01_target_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0.533f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/1003007/hit_slash_normal_01.ogg", 0.7f))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003007/normal_enter.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003007/spine/slaughter_normal_enter.tscn", "animation", 0f, 0f, 1.4f, 1.4f, 0f, 2, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)))
		}), new _003C_003Ez__ReadOnlySingleElementList<string>("HeightController"), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[4]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/infested_prisms/infested_prisms_buff", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/infested_prisms/infested_prisms_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/infested_prisms/infested_prisms_attack_defend", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/infested_prisms/infested_prisms_attack_spin", EnemyAction.Charge)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[3]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Charge,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildSlaveGhoulForLeafSlimeM()
	{
		return new EnemyReplacementProfile("slave_ghoul_for_leaf_slime_m", new Sts2EnemyTarget("LEAF_SLIME_M"), new CznEnemySource("1002005_01", "1002005", "monster@name@1002005_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/leaf_slime_m.tscn", "res://CznEnemySkin/animations/monsters/1002005_slave_ghoul/1002005_slave_ghoul_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.4f), new EnemySelectionBounds(-87.80659f, -222.79195f, 192.60428f, 244.71661f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002005/normal_attack_play2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002005/spine/ghoul_slave_normal_attack_play2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002005/spine/ghoul_slave_normal_attack_play2_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 1.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, -120f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002005/normal_casting.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002005/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlySingleElementList<string>("SpitTarget"), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/leaf_slime_m/leaf_slime_m_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/leaf_slime_m/leaf_slime_m_cast", EnemyAction.Cast)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_slime_impact", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildSnipperForAssassinRubyRaider()
	{
		return new EnemyReplacementProfile("snipper_for_assassin_ruby_raider", new Sts2EnemyTarget("ASSASSIN_RUBY_RAIDER"), new CznEnemySource("1002002_01", "1002002", "monster@name@1002002_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/assassin_ruby_raider.tscn", "res://CznEnemySkin/animations/monsters/1002002_snipper/1002002_snipper_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-98.20561f, -321.23035f, 207.68489f, 330.2522f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[4]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002002/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002002/spine/snipper_normal_m_attack_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 15f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, -95f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002002/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/assassin_ruby_raider/assassin_ruby_raider_attack", EnemyAction.BasicAttack)), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_slash", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildSoldierPrimeForHunterKiller()
	{
		return new EnemyReplacementProfile("soldier_prime_for_hunter_killer", new Sts2EnemyTarget("HUNTER_KILLER"), new CznEnemySource("1001012_01", "1001012", "monster@name@1001012_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/hunter_killer.tscn", "res://CznEnemySkin/animations/monsters/1001012_soldier_prime/1001012_soldier_prime_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.2f), new EnemySelectionBounds(-273.27963f, -369.09372f, 501.68484f, 385.38437f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_triple", "normal_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001012/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001012/particles/soldier_prime_attack1_pat1_667ms.tscn", null, 0f, 620f, 1f, 1f, 0f, 15, 0f, 1.5667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001012/spine/soldier_prime_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 10, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blade_normal_s.tscn", "animation", 0f, 0f, 1f, 1f, 80f, 0, 0f, 1.9f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001012/normal_attack_play2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001012/particles/soldier_prime_attack2_pat1_667ms.tscn", null, 15f, 840f, 1f, 1f, 0f, 15, 0f, 1.5667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001012/spine/soldier_prime_normal_attack_play2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 10, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blade_normal_s.tscn", "animation", 0f, 0f, 1f, 1f, -170f, 0, 0f, 1.9f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001012/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001012/spine/soldier_prime_normal_buff_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 20, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001012/particles/soldier_prime_buff_pat1_1276ms.tscn", null, 30f, 80f, 1f, 1f, 0f, 15, 0.924f, 2.276f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001012/particles/arwen_buff_ready_pat1_1276ms.tscn", null, -110f, 0f, 0.6f, 0.6f, 0f, 10, 0.924f, 1.626f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001012/particles/arwen_buff_ready_pat2_1276ms.tscn", null, -80f, 0f, 0.4f, 0.4f, 0f, 5, 0.924f, 1.726f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001012/spine/soldier_prime_normal_buff_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -10, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001012/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "MouthBone", "NHunterKillerVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/hunter_killer/hunter_killer_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/hunter_killer/hunter_killer_cast", EnemyAction.Cast)
		}), new _003C_003Ez__ReadOnlyArray<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression[2]
		{
			new EnemyNativeVfxPathSuppression("vfx/vfx_bite", EnemyAction.BasicAttack),
			new EnemyNativeVfxPathSuppression("vfx/vfx_attack_slash", EnemyAction.HeavyAttack)
		}), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildSpawningInsectForBowlbugEgg()
	{
		return new EnemyReplacementProfile("spawning_insect_for_bowlbug_egg", new Sts2EnemyTarget("BOWLBUG_EGG"), new CznEnemySource("1006002_01", "1006002", "monster@name@1006002_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/bowlbug_egg.tscn", "res://CznEnemySkin/animations/monsters/1006002_spawning_insect/1006002_spawning_insect_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-101.85335f, -225.90256f, 191.1451f, 237.21909f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[4]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006002/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006002/spine/spawn_insect_normal_skill1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006002/spine/spawn_insect_normal_skill1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 1.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.6f, 0.6f, -20f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Hurt, Array.Empty<EnemyAudioCue>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006002/spine/spawn_insect_normal_hit.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 1, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006002/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "CocoonSlotNode", "SpineBoneNode" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/workbug_egg/workbug_egg_attack", EnemyAction.BasicAttack)), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_blunt", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildStealerForFatGremlin()
	{
		return new EnemyReplacementProfile("stealer_for_fat_gremlin", new Sts2EnemyTarget("FAT_GREMLIN"), new CznEnemySource("1002001_01", "1002001", "monster@name@1002001_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/fat_gremlin.tscn", "res://CznEnemySkin/animations/monsters/1002001_stealer/1002001_stealer_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-124.84015f, -238.10236f, 209.15964f, 239.97011f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[8]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "awake_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Entrance, "spawn", "normal_enter", Loop: false),
			new EnemyAnimationBinding(EnemyAction.StunLoop, "stunned_loop", "normal_groggy", Loop: true),
			new EnemyAnimationBinding(EnemyAction.WakeUp, "WakeUpTrigger", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "FleeTrigger", "normal_unique_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt_awake", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), null, new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002001/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002001/spine/stealer_normal_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 2.3f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002001/normal_unique_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002001/spine/mistery_smoke_granade.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 2, 0.2f, 0.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002001/spine/mistery_smoke_granade.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 2, 1.367f, 0.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.WakeUp, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002001/normal_casting.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/gremlin_merc/fat_gremlin_escape", EnemyAction.Cast)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), Array.Empty<EnemyAction>()), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildStrangeAlauneForSeapunk()
	{
		return new EnemyReplacementProfile("strange_alaune_for_seapunk", new Sts2EnemyTarget("SEAPUNK"), new CznEnemySource("1005022_50", "1005022", "monster@name@1005022_50"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/seapunk.tscn", "res://CznEnemySkin/animations/monsters/1005022_strange_alaune/1005022_strange_alaune_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.4f), new EnemySelectionBounds(-127.25861f, -251.87517f, 251.87692f, 266.65332f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_multi", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005022/normal_skill1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.8f, 0.8f, -110f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body))),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005022/normal_skill2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005022/spine/gorgoph_normal_skill2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/system_eff_debuff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0.74f, 0.8333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/system_eff_debuff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0.54f, 0.8333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/system_eff_debuff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 1.04f, 0.8333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/system_eff_debuff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 1.25f, 0.8333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005022/normal_skill3.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005022/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "BubbleParticles", "WeedParticles", "NSeapunkVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/seapunk/seapunk_kick", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/seapunk/seapunk_kick_multi", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/seapunk/seapunk_buff", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildStrongAppetiteForNibbit()
	{
		return new EnemyReplacementProfile("strong_appetite_for_nibbit", new Sts2EnemyTarget("NIBBIT"), new CznEnemySource("1004002_01", "1004002", "monster@name@1004002_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/nibbit.tscn", "res://CznEnemySkin/animations/monsters/1004002_strong_appetite/1004002_strong_appetite_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.3f), new EnemySelectionBounds(-125.47237f, -268.19904f, 260.95215f, 288.8329f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "hiss", "normal_buff_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004002/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004002/spine/power_taker_attack.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.3f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004002/particles/power_taker_attack_pati_633ms.tscn", null, -50f, -10f, 0.8f, 0.8f, 0f, 5, 0.5f, 0.8833f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, -110f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 20f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004002/normal_buff_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[11]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004002/particles/power_taker_buff_pati_467ms.tscn", null, -20f, 70f, 1f, 0.5f, -50f, 5, 1f, 0.8167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004002/particles/power_taker_buff_pati_500ms.tscn", null, 80f, 100f, 0.5f, 1f, 20f, 5, 0.967f, 0.8497f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004002/particles/power_taker_buff_pati_534ms.tscn", null, 0f, 0f, 1f, 1f, -15f, 5, 0.933f, 0.8837f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004002/particles/power_taker_buff_pati_600ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 5, 0.867f, 0.9497f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004002/particles/power_taker_buff_pati_700ms.tscn", null, -20f, 70f, 1f, 0.5f, -50f, 5, 0.767f, 1.0497f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004002/particles/power_taker_buff_pati_734ms.tscn", null, 50f, 50f, 0.5f, 1f, 120f, 5, 0.733f, 1.0837f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004002/particles/power_taker_buff_pati_800ms.tscn", null, 0f, 0f, 1f, 1f, -15f, 5, 0.667f, 1.1497f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004002/particles/power_taker_buff_pati_900ms.tscn", null, 80f, 100f, 0.5f, 1f, 20f, 5, 0.567f, 1.2497f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004002/particles/power_taker_buff_pati_967ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 5, 0.5f, 1.3167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004002/spine/power_taker_buff_debuff_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.3f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004002/spine/power_taker_buff_debuff_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -1, 0f, 1.1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004002/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/nibbit/nibbit_attack", EnemyAction.BasicAttack)), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_slash", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildSuppressedServantForSlimedBerserker()
	{
		return new EnemyReplacementProfile("suppressed_servant_for_slimed_berserker", new Sts2EnemyTarget("SLIMED_BERSERKER"), new CznEnemySource("3000417_01", "3000417", "monster@name@3000417_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/slimed_berserker.tscn", "res://CznEnemySkin/animations/monsters/3000417_suppressed_servant/3000417_suppressed_servant_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(0.6f), new EnemySelectionBounds(-482.96573f, -686.6641f, 933.10095f, 730.71985f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[12]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Cast, "hug", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "vomit", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Charge, "phase_two", "normal_to_stance1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "stance1_idle", Loop: true, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "stance1_attack", "stance1_attack_play1", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Special, "stance1_skill", "stance1_buff", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "stance1_hit", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "stance1_death", Loop: false, EnemyPresentationStance.Transformed)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[8]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, Array.Empty<EnemyAudioCue>(), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_play1_eff_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3010, 1f, 0.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_play1_eff_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3500, 1f, 0.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_att_arm.tscn", "normal_attack_play1", 0f, 0f, 1f, 1f, 0f, 9, 1f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_play1_eff_cloud_self_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 10, 1f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_play1_eff_cloud_self_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -10, 1f, 1.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/composite/3000339_attack_play1_target_c.tscn", "animation", 0f, 0f, 0.67f, 0.67f, 0f, 0, 0f, 1.2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/composite/3000339_normal_attack_target.tscn", "animation", 0f, 5f, 0.7f, 0.7f, -10f, 0, 0f, 2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000417/normal_attack_play1.ogg", 1f))),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000417/stance1_attack_ready.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_stance1_play1_eff_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3010, 1f, 0.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_stance1_play1_eff_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3500, 1f, 0.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_att_arm.tscn", "stance1_attack_play1", 0f, 0f, 1f, 1f, 0f, 9, 1f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_play1_eff_cloud_self_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 10, 1f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_play1_eff_cloud_self_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -10, 1f, 1.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/composite/3000339_stance1_attack_target.tscn", "animation", 0f, 15f, 0.5f, 0.5f, -10f, 0, 0f, 2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/composite/3000339_attack_play1_target_c.tscn", "animation", 0f, 0f, 0.67f, 0.67f, 0f, 0, 0f, 1.2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: true)
			}), new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioLayerCue>(new EnemyAudioLayerCue("res://CznEnemySkin/audio/enemies/3000417/stance1_attack_play1.ogg", 1f))),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000417/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_buff_eff_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -10, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_buff_eff_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3500, 0f, 2.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_buff_eff_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3502, 0f, 1.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000417/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_dbuff_eff_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -10, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_dbuff_eff_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3500, 0f, 2.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_dbuff_eff_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3502, 0f, 1.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Special, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000417/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_buff_eff_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -10, 0f, 2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_buff_eff_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3500, 0f, 2.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000417/spine/3000339_normal_buff_eff_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3502, 0f, 1.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Charge, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000417/normal_to_stance1.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000417/normal_death.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000417/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[4] { "ParticleSlotNodeL", "ParticleSlotNodeR", "ParticleSlotNodeVomit", "NSlimedBerserkerVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/slimed_berserker/slimed_berserker_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/slimed_berserker/slimed_berserker_buff", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/slimed_berserker/slimed_berserker_slime", EnemyAction.Buff)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildTemplarForAxebot()
	{
		return new EnemyReplacementProfile("templar_for_axebot", new Sts2EnemyTarget("AXEBOT"), new CznEnemySource("1003004_01", "1003004", "monster@name@1003004_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/axebot.tscn", "res://CznEnemySkin/animations/monsters/1003004_templar/1003004_templar_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-195.5452f, -358.73105f, 358.54053f, 399.92923f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[7]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Buff, "sharpen", "normal_attack_ready", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "uppercut", "normal_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Entrance, "respawn", "normal_attack_ready", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), null, new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003004/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003004/spine/templar_attack_play1_smoke_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 0.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003004/spine/templar_attack_play1_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.6f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003004/spine/templar_attack_play1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 0.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003004/spine/templar_attack_play1_smoke_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -4, 0f, 0.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003004/normal_attack_play2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003004/particles/lucy_attack_target_01_pati_367ms.tscn", null, -40f, 90f, 1f, 1f, 0f, 5, 0f, 1.3667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003004/spine/templar_attack_play2_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 4, 0f, 0.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003004/spine/templar_attack_play2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.2667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			}))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[4] { "SmokeNodeRight", "SmokeNodeLeft", "SparksBoneNode", "NAxebotVfx" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/axebot/axebot_attack_spin", EnemyAction.HeavyAttack)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildThornBloodbugForToadpole()
	{
		return new EnemyReplacementProfile("thorn_bloodbug_for_toadpole", new Sts2EnemyTarget("TOADPOLE"), new CznEnemySource("1001005_01", "1001005", "monster@name@1001005_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/toadpole.tscn", "res://CznEnemySkin/animations/monsters/1001005_thorn_bloodbug/1001005_thorn_bloodbug_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-153.24257f, -346.81613f, 333.52408f, 285.06042f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_single", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_triple", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "buff", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001005/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001005/spine/killer_fly_skill1_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.2333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001005/normal_skill2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001005/spine/killer_fly_skill2_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.6f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 30f, -30f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 30f, -30f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 2, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 2, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", -30f, 30f, 0.4f, 0.4f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", -30f, 30f, 0.4f, 0.4f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001005/normal_skill3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001005/spine/killer_fly_skill3_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0.2f, 1.0333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1001005/spine/killer_fly_skill3_back.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -5, 0.2f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1001005/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/toadpole/toadpole_attack_spin", EnemyAction.HeavyAttack)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildStalkerForTrackerRubyRaider()
	{
		return new EnemyReplacementProfile("stalker_for_tracker_ruby_raider", new Sts2EnemyTarget("TRACKER_RUBY_RAIDER"), new CznEnemySource("1002009_01", "1002009", "monster@name@1002009_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/tracker_ruby_raider.tscn", "res://CznEnemySkin/animations/monsters/1002009_stalker/1002009_stalker_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-199.88943f, -417.47827f, 378.86255f, 463.63083f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[4]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002009/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002009/spine/stalker_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002009/chaos_religion_death_01.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002009/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/tracker_ruby_raider/tracker_ruby_raider_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/tracker_ruby_raider/tracker_ruby_raider_die", EnemyAction.Death)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildTrapperForFlailKnight()
	{
		return new EnemyReplacementProfile("trapper_for_flail_knight", new Sts2EnemyTarget("FLAIL_KNIGHT"), new CznEnemySource("1002007_01", "1002007", "monster@name@1002007_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/flail_knight.tscn", "res://CznEnemySkin/animations/monsters/1002007_trapper/1002007_trapper_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.25f), new EnemySelectionBounds(-122.238495f, -450.55734f, 313.90427f, 458.07086f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_flail", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_ram", "normal_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "buff", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002007/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_attack_play1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_attack_play1_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_attack_play1_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 502, 0f, 0.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/particles/traper_obk_pat1_600ms.tscn", null, 3f, 242f, 1.6f, 0.73f, -35f, 1, 0.067f, 1.5997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/particles/traper_obk_pat1_600ms.tscn", null, 10f, 229f, 1.6f, 0.73f, -37f, 1, 0.067f, 1.5997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 20f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002007/normal_attack_play2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_attack_play2_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_attack_play2_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 502, 0f, 0.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/particles/traper_obk_pat2_600ms.tscn", null, -30f, 215f, 1.31f, 0.89f, 35f, 1, 0.067f, 1.5997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, -95f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002007/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_buff_1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_buff_1_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_buff_1_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002007/normal_enter.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_enter.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 502, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)))
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/flail_knight/flail_knight_flail", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/flail_knight/flail_knight_ram", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/flail_knight/flail_knight_war_chant", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildTrapperForMysteriousKnight()
	{
		return new EnemyReplacementProfile("trapper_for_mysterious_knight", new Sts2EnemyTarget("MYSTERIOUS_KNIGHT"), new CznEnemySource("1002007_01", "1002007", "monster@name@1002007_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/mysterious_knight.tscn", "res://CznEnemySkin/animations/monsters/1002007_trapper/1002007_trapper_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.25f), new EnemySelectionBounds(-122.238495f, -450.55734f, 313.90427f, 458.07086f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_flail", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_ram", "normal_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "buff", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002007/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_attack_play1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_attack_play1_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_attack_play1_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 502, 0f, 0.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/particles/traper_obk_pat1_600ms.tscn", null, 3f, 242f, 1.6f, 0.73f, -35f, 1, 0.067f, 1.5997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/particles/traper_obk_pat1_600ms.tscn", null, 10f, 229f, 1.6f, 0.73f, -37f, 1, 0.067f, 1.5997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 20f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002007/normal_attack_play2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_attack_play2_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_attack_play2_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 502, 0f, 0.3667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/particles/traper_obk_pat2_600ms.tscn", null, -30f, 215f, 1.31f, 0.89f, 35f, 1, 0.067f, 1.5997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, -95f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002007/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_buff_1_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -2, 0f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_buff_1_f1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_buff_1_f2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002007/normal_enter.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002007/spine/traper_normal_enter.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 502, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)))
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/flail_knight/flail_knight_flail", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/flail_knight/flail_knight_ram", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/flail_knight/flail_knight_war_chant", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildBerserkGhoulForTwigSlimeM()
	{
		return new EnemyReplacementProfile("berserk_ghoul_for_twig_slime_m", new Sts2EnemyTarget("TWIG_SLIME_M"), new CznEnemySource("1002020_01", "1002020", "monster@name@1002020_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/twig_slime_m.tscn", "res://CznEnemySkin/animations/monsters/1002020_berserk_ghoul/1002020_berserk_ghoul_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-132.73187f, -185.13261f, 320.51703f, 200.1138f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), null, new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002020/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002020/spine/snapghoul_normal_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_s.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 0, 0f, 1f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002020/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002020/spine/snapghoul_normal_debuff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002020/chaos_religion_death_01.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/twig_slime_m/twig_slime_m_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/twig_slime_m/twig_slime_m_cast", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/twig_slime_m/twig_slime_m_die", EnemyAction.Death)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_slime_impact", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildGhoulForTwigSlimeS()
	{
		return new EnemyReplacementProfile("ghoul_for_twig_slime_s", new Sts2EnemyTarget("TWIG_SLIME_S"), new CznEnemySource("1002019_01", "1002019", "monster@name@1002019_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/twig_slime_s.tscn", "res://CznEnemySkin/animations/monsters/1002019_ghoul/1002019_ghoul_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-134.1895f, -150.6544f, 226.7884f, 166.5688f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[4]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002019/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002019/spine/ghoul_normal_m_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.2f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_s.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 0f, 0, 0f, 1f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002019/chaos_religion_death_01.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002019/normal_enter.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002019/spine/ghoul_normal_enter.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)))
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/twig_slime_s/twig_slime_s_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/twig_slime_s/twig_slime_s_die", EnemyAction.Death)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_slime_impact", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildUnansweredForAxeRubyRaider()
	{
		return new EnemyReplacementProfile("unanswered_for_axe_ruby_raider", new Sts2EnemyTarget("AXE_RUBY_RAIDER"), new CznEnemySource("3000443_01", "3000443", "monster@name@3000443_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/axe_ruby_raider.tscn", "res://CznEnemySkin/animations/monsters/3000443_unanswered/3000443_unanswered_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-168.54135f, -233.47928f, 279.6495f, 238.50414f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[4]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000443/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000443/spine/unanswered_3000443_normal_m_attack_01_obk.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000443/spine/unanswered_3000443_normal_m_attack_01_smoke_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.6333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.6f, 0.6f, -95f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000443/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/axe_ruby_raider/axe_ruby_raider_attack", EnemyAction.BasicAttack)), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_attack_slash", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildUndertakerGhoulForLeafSlimeS()
	{
		return new EnemyReplacementProfile("undertaker_ghoul_for_leaf_slime_s", new Sts2EnemyTarget("LEAF_SLIME_S"), new CznEnemySource("1002004_01", "1002004", "monster@name@1002004_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/leaf_slime_s.tscn", "res://CznEnemySkin/animations/monsters/1002004_undertaker_ghoul/1002004_undertaker_ghoul_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-114.3056f, -285.1505f, 229.34889f, 299.78455f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_casting", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002004/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1002004/spine/ghoul_undertaker_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 0.6f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, -95f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1002004/normal_casting.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/leaf_slime_s/leaf_slime_s_attack", EnemyAction.BasicAttack)), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_slime_impact", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildVenomswifterForByrdonis()
	{
		return new EnemyReplacementProfile("venomswifter_for_byrdonis", new Sts2EnemyTarget("BYRDONIS"), new CznEnemySource("1005021_01", "1005021", "monster@name@1005021_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/byrdonis.tscn", "res://CznEnemySkin/animations/monsters/1005021_venomswifter/1005021_venomswifter_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.2f), new EnemySelectionBounds(-162.57803f, -365.1247f, 347.56262f, 384.21057f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "get_angry", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005021/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005021/spine/venomswifter_attack_01_smoke.tscn", "animation", 100f, 0f, 1f, 1f, 0f, 8, 0f, 0.8333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005021/particles/venomswifter_attack_pati_01_833ms.tscn", null, -90f, 300f, 1.2f, 1.7f, -80f, 5, 0f, 1.8333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005021/spine/venomswifter_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 0.5f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005021/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[6]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005021/spine/venomswifter_debuff_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 2.9f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005021/particles/venomswifter_unique_pati_01_500ms.tscn", null, 0f, 420f, 1f, 1f, 0f, 4, 0.467f, 1.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005021/particles/venomswifter_unique_pati_01_500ms.tscn", null, 0f, 420f, 1f, 1f, -30f, 5, 0.833f, 1.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005021/particles/venomswifter_unique_pati_01_500ms.tscn", null, 0f, 420f, 1f, 1f, -15f, 6, 1.333f, 1.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005021/particles/venomswifter_unique_pati_01_500ms.tscn", null, 0f, 420f, 1f, 1f, 45f, 7, 1.633f, 1.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1005021/particles/venomswifter_unique_pati_01_500ms.tscn", null, -120f, 370f, 1f, 1f, -40f, 8, 2f, 1.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1005021/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(Array.Empty<EnemyNativeAudioSuppression>(), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildViadiusForAeonglass()
	{
		return new EnemyReplacementProfile("viadius_for_aeonglass", new Sts2EnemyTarget("AEONGLASS"), new CznEnemySource("1003008_01", "1003008", "monster@name@1003008_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/aeonglass.tscn", "res://CznEnemySkin/animations/monsters/1003008_viadius/1003008_viadius_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.3f), new EnemySelectionBounds(-556.6577f, -648.668f, 1156.9849f, 610.3784f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "AttackHeavy", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "AttackDouble", "normal_attack_play2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "Cast", "normal_defense_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[5]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003008/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[10]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_attack_play1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 0.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_ring_pati1_566ms.tscn", null, -234f, 240f, 0.88f, 0.88f, 0f, 3, 0.167f, 1.6663f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_ex1_633ms.tscn", null, -80f, 276f, 2.8f, 2.8f, 0f, 3, 0.1f, 1.6333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_ex1_633ms.tscn", null, -80f, 276f, 2f, 2f, 180f, 3, 0.1f, 1.6333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_attack_play1_target_f2_action.tscn", "animation", -25f, 0f, 0.9f, 0.9f, -3f, 202, 0f, 1.0333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_attack_play1_target_f3_action.tscn", "animation", -25f, 0f, 0.9f, 0.9f, -3f, 201, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/bulk_viadeus_normal_attack_play1_target_action_2.tscn", null, -25f, 0f, 0.9f, 0.9f, -3f, 3, 0.033f, 2.0337f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/bulk_viadeus_normal_attack_play1_target_action_3.tscn", null, -25f, 0f, 0.9f, 0.9f, -3f, 3, 0.1f, 1.9667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/bulk_viadeus_normal_attack_play1_target_action_4.tscn", null, -25f, 0f, 0.9f, 0.9f, -3f, 3, 0.033f, 2.0337f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_attack_play1_target_f1.tscn", "animation", -25f, 0f, 0.9f, 0.9f, -3f, 2, 0.5347f, 0.4333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003008/normal_attack_play2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[10]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_attack_play2.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 0.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_ring_pati1_566ms.tscn", null, -40f, 230f, 0.88f, 0.88f, 0f, 3, 0.167f, 1.6663f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_ex1_633ms.tscn", null, 70f, 280f, 2.8f, 2.8f, 0f, 3, 0.1f, 1.6333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_ex1_633ms.tscn", null, 70f, 280f, 2f, 2f, 180f, 3, 0.1f, 1.6333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_attack_play1_target_f2_action.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 202, 0f, 1.0333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_attack_play1_target_f3_action.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 201, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/bulk_viadeus_normal_attack_play1_target_action_2.tscn", null, 0f, 0f, 1.1f, 1.1f, 0f, 3, 0.033f, 2.0337f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/bulk_viadeus_normal_attack_play1_target_action_3.tscn", null, 0f, 0f, 1.1f, 1.1f, 0f, 3, 0.1f, 1.9667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/bulk_viadeus_normal_attack_play1_target_action_4.tscn", null, 0f, 0f, 1.1f, 1.1f, 0f, 3, 0.033f, 2.0337f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_attack_play1_target_f1.tscn", "animation", 0f, 0f, 1.1f, 1.1f, 0f, 2, 0.5347f, 0.4333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: true)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003008/normal_defense_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[7]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_defense_1.tscn", "animation", 25f, 70f, 0.8f, 0.8f, 0f, 402, 0f, 1.8f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_defense_1_g.tscn", "animation", 25f, 70f, 0.8f, 0.8f, 0f, 6002, 0f, 1.8f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_def1_1734ms.tscn", null, 25f, 198f, 1.48f, 1.48f, 0f, 403, 0.066f, 2.834f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_def1_1734ms.tscn", null, 25f, 198f, -1.48f, 1.48f, 0f, 403, 0.066f, 2.834f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_def2_1200ms.tscn", null, 13f, 190f, 2.04f, 2.04f, 0f, 403, 0.6f, 2.5f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_def3_567ms.tscn", null, 25f, 226f, 0.92f, 0.92f, 0f, 403, 1.233f, 1.867f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_def3_567ms.tscn", null, 25f, 226f, -0.92f, 0.92f, 0f, 403, 1.233f, 1.867f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003008/normal_death.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[23]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_death_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -402, 0f, 4.1667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_death_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 402, 0f, 4.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_death_g.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 6002, 0f, 3.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/slaughter_unq_pati_1800ms.tscn", null, 12f, 215.6f, 2f, 2f, 0f, 401, 2.667f, 4.1997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_ex2_2467ms.tscn", null, 12f, 215.6f, 5f, 5f, 0f, 403, 2f, 3.4667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_ash_2067ms.tscn", null, 0f, 50f, 2f, 2f, 0f, 401, 2.4f, 4.1667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_ash_2067ms.tscn", null, -200f, 50f, 2f, 2f, 0f, 401, 2.4f, 4.1667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_ash_2067ms.tscn", null, -400f, 50f, 2f, 2f, 0f, 401, 2.4f, 4.1667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_ash_2067ms.tscn", null, 200f, 50f, 2f, 2f, 0f, 401, 2.4f, 4.1667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_ash_2067ms.tscn", null, 400f, 50f, 2f, 2f, 0f, 401, 2.4f, 4.1667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_fire2_1834ms.tscn", null, -400f, 50f, 2f, 2f, 0f, 401, 2.633f, 3.5337f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_fire2_1834ms.tscn", null, -200f, 50f, 2f, 2f, 0f, 401, 2.633f, 3.5337f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_fire2_1834ms.tscn", null, 0f, 50f, 2f, 2f, 0f, 401, 2.633f, 3.5337f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_fire2_1834ms.tscn", null, 200f, 50f, 2f, 2f, 0f, 401, 2.633f, 3.5337f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_fire2_1834ms.tscn", null, 400f, 50f, 2f, 2f, 0f, 401, 2.633f, 3.5337f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_fire2_2400ms.tscn", null, -400f, 100f, -3f, 3f, 0f, 403, 2.067f, 4.0997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_fire2_2400ms.tscn", null, -400f, 300f, -3f, 3f, 0f, 403, 2.067f, 4.0997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_fire2_2400ms.tscn", null, -400f, 500f, -3f, 3f, 0f, 403, 2.067f, 4.0997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_fire2_2400ms.tscn", null, 400f, 100f, 3f, 3f, 0f, 403, 2.067f, 4.0997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_fire2_2400ms.tscn", null, 400f, 300f, 3f, 3f, 0f, 403, 2.067f, 4.0997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/viadeus_fire2_2400ms.tscn", null, 400f, 500f, 3f, 3f, 0f, 403, 2.067f, 4.0997f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/diallos_meca_death_01.tscn", "center", 0f, 0f, 1.2f, 1.2f, 0f, 1, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/particles/diallos_meca_death_01_pati1_600ms.tscn", null, 0f, 0f, 1.2f, 1.2f, 0f, 2, 0f, 1.45f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1003008/normal_enter.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1003008/spine/viadeus_normal_enter.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 3.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)))
		}), new _003C_003Ez__ReadOnlyArray<string>(new string[8] { "NAeonGlassVfx", "RaySlot", "GlassCenterSlot", "LiquidSlot", "WitherSlot", "TopSparksSlot", "BottomSparksSlot", "GroundPlowSlot" }), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(Array.Empty<EnemyNativeAudioSuppression>(), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		}), null, useNativeHostBounds: true), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildVineLordForFogmog()
	{
		return new EnemyReplacementProfile("vine_lord_for_fogmog", new Sts2EnemyTarget("FOGMOG"), new CznEnemySource("3000385_01", "3000385", "monster@name@3000385_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/fogmog.tscn", "res://CznEnemySkin/animations/monsters/3000385_vine_lord/3000385_vine_lord_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-157.18045f, -381.14075f, 318.5412f, 391.38324f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "summon", "normal_buff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), null, new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[2]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000385/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000385/spine/vineload_3000385_attack_play_1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 20f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000385/normal_buff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000385/spine/vineload_3000385_buff_eff.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 1.4667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000385/particles/3000385_normal_buff_par_1867ms.tscn", null, 0f, 0f, 1f, 1f, 0f, 0, 0f, 2.8667f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			}))
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/fogmog/fogmog_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/fogmog/fogmog_summon", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildVoidHungerForTerrorEel()
	{
		return new EnemyReplacementProfile("void_hunger_for_terror_eel", new Sts2EnemyTarget("TERROR_EEL"), new CznEnemySource("1004006_01", "1004006", "monster@name@1004006_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/terror_eel.tscn", "res://CznEnemySkin/animations/monsters/1004006_void_hunger/1004006_void_hunger_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.3f), new EnemySelectionBounds(-171.09305f, -369.38907f, 466.03207f, 392.88498f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_debuff_unique_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_job_attack_play", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_triple", "normal_attack_play1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[4]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004006/normal_job_attack_play.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[5]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004006/spine/voideater_job_attack_smoke.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 6, 0f, 0.8f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004006/particles/voideater_job_attack_pati_667ms.tscn", null, -120f, 0f, 1f, 1f, 0f, 4, 0f, 1.0167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004006/spine/voideater_job_attack.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 0.3f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.8f, 0.8f, 15f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.8f, 0.8f, -90f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004006/normal_attack_play1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004006/particles/voideater_attack_pati_667ms.tscn", null, -220f, 135f, 0.4f, 0.9f, -35f, 5, 0f, 1.1167f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004006/spine/voideater_attack_01.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 0.3f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.8f, 0.8f, -80f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004006/normal_debuff_unique_1.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004006/spine/voideater_unique1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3, 0f, 1.6f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1004006/normal_enter.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004006/particles/voidater_enter_pati_1800ms.tscn", null, -30f, 220f, 1f, 1f, 0f, 4, 0.733f, 3f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1004006/spine/voideater_enter.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 2, 0f, 1.5667f, StartParticles: false, EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply, 1f, null)
			}))
		}), new _003C_003Ez__ReadOnlySingleElementList<string>("HeightControl"), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[3]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/terror_eel/terror_eel_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/terror_eel/terror_eel_attack_multi", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/terror_eel/terror_eel_debuff", EnemyAction.Cast)
		}), new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeVfxPathSuppression>(new EnemyNativeVfxPathSuppression("vfx/vfx_scream", EnemyAction.Cast)), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		})), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildWanderingPollemicaForRocket()
	{
		return new EnemyReplacementProfile("wandering_pollemica_for_rocket", new Sts2EnemyTarget("ROCKET"), new CznEnemySource("3000477_01", "3000477", "monster@name@3000477_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/rocket.tscn", "res://CznEnemySkin/animations/monsters/3000477_wandering_pollemica/3000477_wandering_pollemica_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.3f), new EnemySelectionBounds(-188.13843f, -405.48608f, 487.13034f, 427.41992f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[8]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "RocketTarget", "normal_skill4", Loop: false),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "RocketPrecision", "normal_skill1_3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "RocketCharge", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "RocketLaser", "normal_skill1_3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Special, "RocketRecharge", "normal_skill2_3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "normal_death", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[7]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000477/normal_skill4.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1.3f, 1.3f, -80f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body))),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000477/normal_skill1_3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/elite_insect_skill1_3_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.6667f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/elite_insect_skill1_3_back.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -5, 0f, 1.4333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 2f, 2f, -80f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 30f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000477/normal_skill3.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/elite_insect_skill3_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 3.5667f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000477/normal_skill1_3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[4]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/elite_insect_skill1_3_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.6667f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/elite_insect_skill1_3_back.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -5, 0f, 1.4333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 2f, 2f, -80f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 30f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Special, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000477/normal_skill2_3.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/elite_insect_skill2_3_front.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 5, 0f, 1.8f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/elite_insect_skill2_3_back.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -100, 0f, 1.5333f, StartParticles: false, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Death, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000477/normal_death.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[8]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/nihilum_albino_death_01_main_b.tscn", "center", 0f, 0f, 1f, 1f, 0f, -1, 0.8f, 1.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/nihilum_albino_death_01_main_f.tscn", "center", 0f, 0f, 1f, 1f, 0f, 2, 0.8f, 1.3333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/nihilum_albino_death_01_meat1.tscn", "center", 0f, 0f, 1f, 1f, 0f, 1, 0.8f, 0.9333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/nihilum_albino_death_01_meat2.tscn", "center", 0f, 0f, 1f, 1f, 0f, 1, 0.8f, 0.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/nihilum_albino_death_01_meat3.tscn", "center", 0f, 0f, 1f, 1f, 0f, 1, 0.8f, 0.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/nihilum_albino_death_01_meat4.tscn", "center", 0f, 0f, 1f, 1f, 0f, 1, 0.8f, 0.2333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/nihilum_albino_death_01_meat5.tscn", "center", 0f, 0f, 1f, 1f, 0f, 1, 0.8f, 0.3f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/3000477/spine/nihilum_albino_death_01_meat_stack.tscn", "center", 0f, 0f, 1f, 1f, 0f, 1, 0.8f, 1.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/3000477/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[8]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_attack_snap", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_attack_slam", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/rocket/rocket_attack", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("heavy_attack.mp3", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_buff", EnemyAction.Buff),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_rocket", EnemyAction.Cast),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_regrow", EnemyAction.Special),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/kaiser_crab/kaiser_crab_right_die", EnemyAction.Death)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.PlayAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[3]
		{
			EnemyAction.BasicAttack,
			EnemyAction.Cast,
			EnemyAction.HeavyAttack
		}), null, useNativeHostBounds: false, EnemyBodyOriginPolicy.ResetVerticalOffset), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildWatcherForbiddenTomeForGlobeHead()
	{
		return new EnemyReplacementProfile("watcher_forbidden_tome_for_globe_head", new Sts2EnemyTarget("GLOBE_HEAD"), new CznEnemySource("1006014_50", "1006014", "monster@name@1006014_50"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/globe_head.tscn", "res://CznEnemySkin/animations/monsters/1006014_watcher_forbidden_tome/1006014_watcher_forbidden_tome_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-239.70901f, -522.1084f, 415.81094f, 537.1722f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[5]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack", "normal_m_attack_01", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Cast, "cast", "normal_debuff_1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[3]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006014/normal_m_attack_01.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006014/spine/elder_vain_normal_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 0.6667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, 0f, 55, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Cast, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006014/normal_debuff_1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006014/spine/elder_vain_normal_skill3_3_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 2.4f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006014/spine/elder_vain_normal_skill3_3_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, -30, 0f, 0.7333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006014/particles/watcher_forbidden_tome_debuff_par3_1700ms.tscn", null, 0f, 417f, 1f, 1f, 0f, 40, 0.7f, 2.2f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null)
			})),
			new EnemyActionPresentation(EnemyAction.Entrance, Array.Empty<EnemyAudioCue>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006014/spine/elder_vain_normal_enter.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 0, 0f, 1.8667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null)))
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[2]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/globe_head/globe_head_slap", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/globe_head/globe_head_charge", EnemyAction.Cast)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildWingedHumanfaceForOwlMagistrate()
	{
		return new EnemyReplacementProfile("winged_humanface_for_owl_magistrate", new Sts2EnemyTarget("OWL_MAGISTRATE"), new CznEnemySource("1006013_01", "1006013", "monster@name@1006013_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/owl_magistrate.tscn", "res://CznEnemySkin/animations/monsters/1006013_winged_humanface/1006013_winged_humanface_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1f), new EnemySelectionBounds(-306.9227f, -311.05447f, 466.09586f, 346.13626f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[10]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Special, "magistrate_scrutiny", "normal_skill3", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "attack_peck", "normal_skill2", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Buff, "take_off", "fly_loop", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Idle, "flight_hold", "fly_loop", Loop: true, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.HeavyAttack, "attack_dive", "normal_skill1", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt_flying", "normal_hit", Loop: false, EnemyPresentationStance.Transformed),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die_flying", "die_flying", Loop: false, EnemyPresentationStance.Transformed)
		}), "normal_enter", new _003C_003Ez__ReadOnlyArray<EnemyActionPresentation>(new EnemyActionPresentation[5]
		{
			new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006013/normal_skill2.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[14]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006013/spine/wing_humanface_nomal_skill2_f.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 3500, 0f, 1.1333f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006013/spine/wing_humanface_nomal_skill2_b.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 1.0667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 6, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 0, 6, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 6, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 1, 6, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 2, 6, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 2, 6, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 3, 6, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 3, 6, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 4, 6, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 4, 6, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 5, 6, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.5f, 0.5f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, 5, 6, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.HeavyAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006013/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[2]
			{
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_front.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, 0, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
				new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_blunt_normal_01_back.tscn", "animation", 0f, 0f, 0.6f, 0.6f, 0f, -1, 0f, 1.3333f, StartParticles: true, EnemyEffectMaterialPolicy.Native, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
			})),
			new EnemyActionPresentation(EnemyAction.Buff, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006013/normal_to_stance1.ogg")), Array.Empty<EnemyEffectCue>()),
			new EnemyActionPresentation(EnemyAction.Special, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006013/normal_skill3.ogg")), new _003C_003Ez__ReadOnlySingleElementList<EnemyEffectCue>(new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006013/spine/wing_humanface_nomal_skill4.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 2.7667f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null))),
			new EnemyActionPresentation(EnemyAction.Entrance, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006013/normal_enter.ogg")), Array.Empty<EnemyEffectCue>())
		}), Array.Empty<string>(), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlyArray<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression[4]
		{
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_attack_peck", EnemyAction.BasicAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_attack_dive", EnemyAction.HeavyAttack),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_take_off", EnemyAction.Buff),
			new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/owl_magistrate/owl_magistrate_fly_loop", EnemyAction.Buff)
		}), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlyArray<EnemyAction>(new EnemyAction[2]
		{
			EnemyAction.BasicAttack,
			EnemyAction.HeavyAttack
		}), null, useNativeHostBounds: false, EnemyBodyOriginPolicy.InheritNativeOffset, EnemyHostTransformPolicy.Inherit, mirrorBodyHorizontally: false, centerAnchorOnSelectionBounds: false, applyNeutralMultiplyToBody: true), new BestiaryPresentation(EnemyBestiaryPreviewAction.Entrance), enabledByDefault: true);
	}

	private static EnemyReplacementProfile BuildWolfSlayerForSlumberingBeetle()
	{
		return new EnemyReplacementProfile("wolf_slayer_for_slumbering_beetle", new Sts2EnemyTarget("SLUMBERING_BEETLE"), new CznEnemySource("1006005_01", "1006005", "monster@name@1006005_01"), new MonsterPresentation(PresentationMode.ReplaceReady, "res://scenes/creature_visuals/slumbering_beetle.tscn", "res://CznEnemySkin/animations/monsters/1006005_wolf_slayer/1006005_wolf_slayer_skel_data.tres", CznEnemyVisualScalePolicy.Resolve(1.4f), new EnemySelectionBounds(-159.7726f, -301.0161f, 398.5605f, 325.0208f), new _003C_003Ez__ReadOnlyArray<EnemyAnimationBinding>(new EnemyAnimationBinding[6]
		{
			new EnemyAnimationBinding(EnemyAction.Idle, "idle_loop", "normal_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.StunLoop, "sleep_loop", "stance2_idle", Loop: true),
			new EnemyAnimationBinding(EnemyAction.Buff, "WakeUp", "stance2_to_normal", Loop: false),
			new EnemyAnimationBinding(EnemyAction.BasicAttack, "Rollout", "normal_skill1", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Hurt, "hurt", "normal_hit", Loop: false),
			new EnemyAnimationBinding(EnemyAction.Death, "die", "die", Loop: false)
		}), null, new _003C_003Ez__ReadOnlySingleElementList<EnemyActionPresentation>(new EnemyActionPresentation(EnemyAction.BasicAttack, new _003C_003Ez__ReadOnlySingleElementList<EnemyAudioCue>(new EnemyAudioCue("res://CznEnemySkin/audio/enemies/1006005/normal_skill1.ogg")), new _003C_003Ez__ReadOnlyArray<EnemyEffectCue>(new EnemyEffectCue[3]
		{
			new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/1006005/spine/wolves_vain_normal_skill1.tscn", "animation", 0f, 0f, 1f, 1f, 0f, 30, 0f, 0.9f, StartParticles: false, EnemyEffectMaterialPolicy.Native, 1f, null),
			new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 0.7f, 0.7f, -100f, 0, 0f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body),
			new EnemyEffectCue("res://CznEnemySkin/scenes/enemy_effects/shared/composite/hit_slash_normal_01.tscn", "animation", 0f, 0f, 1f, 1f, 180f, 0, 0.233f, 1.3367f, StartParticles: true, EnemyEffectMaterialPolicy.OpaqueBlackAdditive, 1f, null, TargetHit: true, TargetAtEntry: false, TargetCenter: false, -1, 0, EnemyTargetOrigin.Body)
		}))), new _003C_003Ez__ReadOnlySingleElementList<string>("SpineBoneNode"), null, Array.Empty<string>(), new EnemyNativePresentationPolicy(new _003C_003Ez__ReadOnlySingleElementList<EnemyNativeAudioSuppression>(new EnemyNativeAudioSuppression("event:/sfx/enemy/enemy_attacks/slumbering_beetle/slumbering_beetle_roll", EnemyAction.BasicAttack)), Array.Empty<EnemyNativeVfxPathSuppression>(), Array.Empty<EnemyNativeVfxFeatureSuppression>(), null), EnemyDeathBodyPolicy.NativeCleanupWithoutBodyAnimation, Array.Empty<EnemySecondaryBodyProfile>(), new _003C_003Ez__ReadOnlySingleElementList<EnemyAction>(EnemyAction.BasicAttack)), new BestiaryPresentation(EnemyBestiaryPreviewAction.Awaken), enabledByDefault: true);
	}
}
