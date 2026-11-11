using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Profiles;

internal static class GeneratedSharedHitRoutes
{
	internal static readonly IReadOnlyList<SharedHitRoute> All = new SharedHitRoute[121]
	{
		new SharedHitRoute(typeof(DampCultist), "dual_edge_lepturus_for_damp_cultist", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(Fogmog), "vine_lord_for_fogmog", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: true, 0f),
		new SharedHitRoute(typeof(FuzzyWurmCrawler), "mega_bite_for_fuzzy_wurm_crawler", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(LeafSlimeM), "slave_ghoul_for_leaf_slime_m", "Attack", EnemyAction.BasicAttack, "vfx/vfx_slime_impact", FollowTarget: false, 0f),
		new SharedHitRoute(typeof(LeafSlimeS), "undertaker_ghoul_for_leaf_slime_s", "Attack", EnemyAction.BasicAttack, "vfx/vfx_slime_impact", FollowTarget: true, 0f),
		new SharedHitRoute(typeof(Nibbit), "strong_appetite_for_nibbit", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(Ovicopter), "fairy_boggart_for_ovicopter", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.06f),
		new SharedHitRoute(typeof(Parafright), "scratcher_for_parafright", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(SewerClam), "giant_soldier_for_sewer_clam", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0.6f),
		new SharedHitRoute(typeof(ShrinkerBeetle), "bare_beetle_for_shrinker_beetle", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(SneakyGremlin), "hunchback_for_sneaky_gremlin", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: true, 0.533f),
		new SharedHitRoute(typeof(Stabbot), "recharger_for_stabbot", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.53f),
		new SharedHitRoute(typeof(TheForgotten), "bomberplasm_for_the_forgotten", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0f),
		new SharedHitRoute(typeof(TheObscura), "dibellum_for_the_obscura", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(ToughEgg), "fairy_changeling_for_tough_egg", "Attack", EnemyAction.BasicAttack, "vfx/vfx_bite", FollowTarget: true, 0.403f),
		new SharedHitRoute(typeof(TwoTailedRat), "shapeshifter_for_two_tailed_rat", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.4f),
		new SharedHitRoute(typeof(Zapbot), "recharger_for_zapbot", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.53f),
		new SharedHitRoute(typeof(MagiKnight), "cultist_arbiter_for_magi_knight", "BombCast", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0.12f),
		new SharedHitRoute(typeof(FrogKnight), "hussar_automata_for_frog_knight", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0.03f),
		new SharedHitRoute(typeof(FrogKnight), "hussar_automata_for_frog_knight", "Lash", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.03f),
		new SharedHitRoute(typeof(FrogKnight), "hussar_automata_for_frog_knight", "charge", EnemyAction.Charge, "vfx/vfx_attack_slash", FollowTarget: true, 0.03f),
		new SharedHitRoute(typeof(SlitheringStrangler), "light_devourer_for_slithering_strangler", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: true, 0f),
		new SharedHitRoute(typeof(SlitheringStrangler), "light_devourer_for_slithering_strangler", "AttackDefendTrigger", EnemyAction.HeavyAttack, "vfx/vfx_attack_slash", FollowTarget: true, 0f),
		new SharedHitRoute(typeof(ThievingHopper), "flying_vampire_for_thieving_hopper", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: true, 0f),
		new SharedHitRoute(typeof(Tunneler), "sandworm_for_tunneler", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(Axebot), "templar_for_axebot", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0f, NativeMultiHit: true),
		new SharedHitRoute(typeof(Axebot), "templar_for_axebot", "uppercut", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0f),
		new SharedHitRoute(typeof(CubexConstruct), "racker_for_cubex_construct", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.53f, NativeMultiHit: true),
		new SharedHitRoute(typeof(Byrdonis), "venomswifter_for_byrdonis", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0f, NativeMultiHit: true),
		new SharedHitRoute(typeof(Inklet), "fairy_nox_for_inklet", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0f),
		new SharedHitRoute(typeof(Inklet), "fairy_nox_for_inklet", "TRIPLE_ATTACK", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0f, NativeMultiHit: true),
		new SharedHitRoute(typeof(FossilStalker), "slashghoul_for_fossil_stalker", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0.4f),
		new SharedHitRoute(typeof(FossilStalker), "slashghoul_for_fossil_stalker", "AttackDouble", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0.4f, NativeMultiHit: true),
		new SharedHitRoute(typeof(Entomancer), "lepidola_for_entomancer", "attack_ranged", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.76f, NativeMultiHit: true),
		new SharedHitRoute(typeof(Entomancer), "lepidola_for_entomancer", "Attack", EnemyAction.HeavyAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.79f),
		new SharedHitRoute(typeof(FlailKnight), "trapper_for_flail_knight", "FlailAttack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0f, NativeMultiHit: true),
		new SharedHitRoute(typeof(FlailKnight), "trapper_for_flail_knight", "RamAttack", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0f),
		new SharedHitRoute(typeof(GremlinMerc), "man_eater_for_gremlin_merc", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.533f),
		new SharedHitRoute(typeof(GremlinMerc), "man_eater_for_gremlin_merc", "AttackDouble", EnemyAction.HeavyAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.533f, NativeMultiHit: true),
		new SharedHitRoute(typeof(Chomper), "halo_automata_for_chomper", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: true, 0.733f, NativeMultiHit: true),
		new SharedHitRoute(typeof(PunchConstruct), "golem_automata_for_punch_construct", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0.9f),
		new SharedHitRoute(typeof(PunchConstruct), "golem_automata_for_punch_construct", "DoubleAttack", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0.77f, NativeMultiHit: true),
		new SharedHitRoute(typeof(Exoskeleton), "blade_soldier_for_exoskeleton", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.5f, NativeMultiHit: true),
		new SharedHitRoute(typeof(Exoskeleton), "blade_soldier_for_exoskeleton", "HeavyAttack", EnemyAction.HeavyAttack, "vfx/vfx_bite", FollowTarget: false, 0.633f),
		new SharedHitRoute(typeof(SkulkingColony), "rock_shell_insect_for_skulking_colony", "AttackHeavy", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0f),
		new SharedHitRoute(typeof(SkulkingColony), "rock_shell_insect_for_skulking_colony", "AttackDouble", EnemyAction.HeavyAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0f, NativeMultiHit: true),
		new SharedHitRoute(typeof(SpectralKnight), "human_shadow_for_spectral_knight", "AttackSword", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(SpectralKnight), "human_shadow_for_spectral_knight", "AttackFlame", EnemyAction.HeavyAttack, null, FollowTarget: false, 0.5f, NativeMultiHit: true),
		new SharedHitRoute(typeof(Toadpole), "thorn_bloodbug_for_toadpole", "AttackSingle", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(Toadpole), "thorn_bloodbug_for_toadpole", "AttackTriple", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.55f, NativeMultiHit: true),
		new SharedHitRoute(typeof(TurretOperator), "crimson_oracle_for_turret_operator", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.57f, NativeMultiHit: true),
		new SharedHitRoute(typeof(ScrollOfBiting), "gloomy_beast_for_scroll_of_biting", "Attack", EnemyAction.BasicAttack, "vfx/vfx_bite", FollowTarget: false, 0.53f),
		new SharedHitRoute(typeof(ScrollOfBiting), "gloomy_beast_for_scroll_of_biting", "ATTACK_DOUBLE", EnemyAction.HeavyAttack, "vfx/vfx_bite", FollowTarget: false, 0.53f, NativeMultiHit: true),
		new SharedHitRoute(typeof(TheLost), "gloomy_godfather_for_the_lost", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0f, NativeMultiHit: true),
		new SharedHitRoute(typeof(TerrorEel), "void_hunger_for_terror_eel", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0f),
		new SharedHitRoute(typeof(TerrorEel), "void_hunger_for_terror_eel", "AttackTripleTrigger", EnemyAction.HeavyAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0f, NativeMultiHit: true),
		new SharedHitRoute(typeof(Flyconid), "herbal_hydracle_for_flyconid", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0f),
		new SharedHitRoute(typeof(SlumberingBeetle), "wolf_slayer_for_slumbering_beetle", "Rollout", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(LouseProgenitor), "lost_hunger_for_louse_progenitor", "Attack", EnemyAction.BasicAttack, null, FollowTarget: false, 0f),
		new SharedHitRoute(typeof(MysteriousKnight), "trapper_for_mysterious_knight", "FlailAttack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0f, NativeMultiHit: true),
		new SharedHitRoute(typeof(MysteriousKnight), "trapper_for_mysterious_knight", "RamAttack", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0f),
		new SharedHitRoute(typeof(DecimillipedeSegmentFront), "abyss_insect_for_decimillipede_segment_front", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0f, NativeMultiHit: true, "WRITHE_MOVE"),
		new SharedHitRoute(typeof(DecimillipedeSegmentFront), "abyss_insect_for_decimillipede_segment_front", "Attack", EnemyAction.Cast, "vfx/vfx_attack_blunt", FollowTarget: false, 0f, NativeMultiHit: false, "CONSTRICT_MOVE"),
		new SharedHitRoute(typeof(DecimillipedeSegmentMiddle), "abyss_insect_for_decimillipede_segment_middle", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0f, NativeMultiHit: true, "WRITHE_MOVE"),
		new SharedHitRoute(typeof(DecimillipedeSegmentMiddle), "abyss_insect_for_decimillipede_segment_middle", "Attack", EnemyAction.Cast, "vfx/vfx_attack_blunt", FollowTarget: false, 0f, NativeMultiHit: false, "CONSTRICT_MOVE"),
		new SharedHitRoute(typeof(DecimillipedeSegmentBack), "abyss_insect_for_decimillipede_segment_back", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0f, NativeMultiHit: true, "WRITHE_MOVE"),
		new SharedHitRoute(typeof(DecimillipedeSegmentBack), "abyss_insect_for_decimillipede_segment_back", "Attack", EnemyAction.Cast, "vfx/vfx_attack_blunt", FollowTarget: false, 0f, NativeMultiHit: false, "CONSTRICT_MOVE"),
		new SharedHitRoute(typeof(LivingFog), "darkimp_for_living_fog", "Attack", EnemyAction.BasicAttack, null, FollowTarget: false, 0.42f),
		new SharedHitRoute(typeof(MechaKnight), "dominion_for_mecha_knight", "charge", EnemyAction.BasicAttack, null, FollowTarget: false, 0f),
		new SharedHitRoute(typeof(MechaKnight), "dominion_for_mecha_knight", "Attack", EnemyAction.HeavyAttack, null, FollowTarget: false, 0f),
		new SharedHitRoute(typeof(Wriggler), "pale_servant_for_wriggler", "Attack", EnemyAction.BasicAttack, "vfx/vfx_bite", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(LivingShield), "saint_hound_for_living_shield", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.5667f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(LivingShield), "saint_hound_for_living_shield", "Attack", EnemyAction.HeavyAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.5667f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(SpinyToad), "evil_dullahan_for_spiny_toad", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: true, 0f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(SpinyToad), "evil_dullahan_for_spiny_toad", "Attack", EnemyAction.Special, "vfx/vfx_attack_slash", FollowTarget: true, 0.4667f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(SpinyToad), "evil_dullahan_for_spiny_toad", "Unspiked", EnemyAction.Special, "vfx/vfx_attack_slash", FollowTarget: true, 0.4667f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(BowlbugNectar), "honey_jar_for_bowlbug_nectar", "Attack", EnemyAction.Special, "vfx/vfx_attack_blunt", FollowTarget: false, 0.5f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(SoulNexus), "fallacia_for_soul_nexus", "Attack", EnemyAction.HeavyAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.4667f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(InfestedPrism), "slaughter_for_infested_prism", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0f),
		new SharedHitRoute(typeof(InfestedPrism), "slaughter_for_infested_prism", "AttackDouble", EnemyAction.Charge, "vfx/vfx_attack_slash", FollowTarget: false, 0.7f, NativeMultiHit: true),
		new SharedHitRoute(typeof(GlobeHead), "watcher_forbidden_tome_for_globe_head", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.533f, NativeMultiHit: false, null, StateSelected: false, "vfx/vfx_attack_lightning"),
		new SharedHitRoute(typeof(TrackerRubyRaider), "stalker_for_tracker_ruby_raider", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.51f, NativeMultiHit: true, "HOUNDS_MOVE"),
		new SharedHitRoute(typeof(Seapunk), "strange_alaune_for_seapunk", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(Seapunk), "strange_alaune_for_seapunk", "MultiAttack", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0f, NativeMultiHit: false, null, StateSelected: false, null, EntryTarget: true),
		new SharedHitRoute(typeof(HauntedShip), "kentris_for_haunted_ship", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(HauntedShip), "kentris_for_haunted_ship", "AttackTriple", EnemyAction.HeavyAttack, "vfx/vfx_attack_slash", FollowTarget: false, 3.64f, NativeMultiHit: true),
		new SharedHitRoute(typeof(PhantasmalGardener), "albithum_for_phantasmal_gardener", "AttackMulti", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.5f, NativeMultiHit: true),
		new SharedHitRoute(typeof(PhrogParasite), "inconvenient_diner_for_phrog_parasite", "Attack", EnemyAction.BasicAttack, null, FollowTarget: false, 0.5f, NativeMultiHit: true),
		new SharedHitRoute(typeof(OwlMagistrate), "winged_humanface_for_owl_magistrate", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: true, 0.5f, NativeMultiHit: true, null, StateSelected: true),
		new SharedHitRoute(typeof(WaterfallGiant), "golden_mane_for_waterfall_giant", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0.4667f),
		new SharedHitRoute(typeof(WaterfallGiant), "golden_mane_for_waterfall_giant", "AttackDebuff", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0.4667f),
		new SharedHitRoute(typeof(KnowledgeDemon), "howling_prodigal_for_knowledge_demon", "MediumAttackTrigger", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.4667f),
		new SharedHitRoute(typeof(KnowledgeDemon), "howling_prodigal_for_knowledge_demon", "HeavyAttackTrigger", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.4667f, NativeMultiHit: true),
		new SharedHitRoute(typeof(Crusher), "pollemica_for_crusher", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(Rocket), "wandering_pollemica_for_rocket", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.05f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(Rocket), "wandering_pollemica_for_rocket", "Attack", EnemyAction.Cast, "vfx/vfx_attack_blunt", FollowTarget: false, 0.3f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(Crusher), "pollemica_for_crusher", "Attack", EnemyAction.HeavyAttack, "vfx/vfx_heavy_blunt", FollowTarget: false, 0.15f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(Crusher), "pollemica_for_crusher", "Attack", EnemyAction.Cast, "vfx/vfx_attack_slash", FollowTarget: false, 0.3f, NativeMultiHit: true, null, StateSelected: true),
		new SharedHitRoute(typeof(Rocket), "wandering_pollemica_for_rocket", "Attack", EnemyAction.HeavyAttack, "vfx/vfx_heavy_blunt", FollowTarget: false, 0.3f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(Queen), "shadow_collector_for_queen", "Attack", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0.2667f, NativeMultiHit: true, null, StateSelected: true),
		new SharedHitRoute(typeof(Queen), "shadow_collector_for_queen", "Attack", EnemyAction.Special, "vfx/vfx_attack_blunt", FollowTarget: true, 0.2667f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(TestSubject), "deathchrome_for_test_subject", "BiteTrigger", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0.6167f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(TestSubject), "deathchrome_for_test_subject", "MultiAttackTrigger", EnemyAction.HeavyAttack, null, FollowTarget: true, 0.6167f, NativeMultiHit: true, null, StateSelected: true),
		new SharedHitRoute(typeof(Vantom), "bercula_for_vantom", "ATTACK_DOUBLE", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.766f, NativeMultiHit: true),
		new SharedHitRoute(typeof(LagavulinMatriarch), "debolessa_for_lagavulin_matriarch", "AttackHeavy", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.7993f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(KinPriest), "rotten_reaper_for_kin_priest", "AttackGrenade", EnemyAction.BasicAttack, null, FollowTarget: true, 0f),
		new SharedHitRoute(typeof(LagavulinMatriarch), "debolessa_for_lagavulin_matriarch", "AttackDouble", EnemyAction.HeavyAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.7993f, NativeMultiHit: true, null, StateSelected: true),
		new SharedHitRoute(typeof(Vantom), "bercula_for_vantom", "Attack", EnemyAction.Cast, "vfx/vfx_giant_horizontal_slash", FollowTarget: false, 0f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(PhantasmalGardener), "albithum_for_phantasmal_gardener", "Attack", EnemyAction.BasicAttack, "vfx/vfx_bite", FollowTarget: false, 0.5f),
		new SharedHitRoute(typeof(Myte), "broken_dream_shard_for_myte", "Attack", EnemyAction.BasicAttack, "vfx/vfx_bite", FollowTarget: true, 0.633f),
		new SharedHitRoute(typeof(CrossbowRubyRaider), "hexer_for_crossbow_ruby_raider", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.4f),
		new SharedHitRoute(typeof(KinFollower), "maggot_for_kin_follower", "SlashTrigger", EnemyAction.BasicAttack, "vfx/vfx_attack_slash", FollowTarget: true, 0.5f),
		new SharedHitRoute(typeof(KinFollower), "maggot_for_kin_follower", "BoomerangTrigger", EnemyAction.HeavyAttack, "vfx/vfx_attack_slash", FollowTarget: true, 0.5f, NativeMultiHit: true),
		new SharedHitRoute(typeof(KinPriest), "rotten_reaper_for_kin_priest", "AttackLaser", EnemyAction.HeavyAttack, "vfx/vfx_attack_blunt", FollowTarget: true, 0.633f, NativeMultiHit: true),
		new SharedHitRoute(typeof(TorchHeadAmalgam), "rusty_nail_seven_arms_for_torch_head_amalgam", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.4667f),
		new SharedHitRoute(typeof(HunterKiller), "soldier_prime_for_hunter_killer", "Attack", EnemyAction.BasicAttack, "vfx/vfx_bite", FollowTarget: false, 0f),
		new SharedHitRoute(typeof(HunterKiller), "soldier_prime_for_hunter_killer", "TripleAttack", EnemyAction.HeavyAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0f, NativeMultiHit: true),
		new SharedHitRoute(typeof(TwigSlimeM), "berserk_ghoul_for_twig_slime_m", "Attack", EnemyAction.BasicAttack, "vfx/vfx_slime_impact", FollowTarget: true, 0.4f),
		new SharedHitRoute(typeof(TwigSlimeS), "ghoul_for_twig_slime_s", "Attack", EnemyAction.BasicAttack, "vfx/vfx_slime_impact", FollowTarget: true, 0.4f),
		new SharedHitRoute(typeof(OwlMagistrate), "winged_humanface_for_owl_magistrate", "Attack", EnemyAction.HeavyAttack, "vfx/vfx_attack_slash", FollowTarget: false, 0.6f, NativeMultiHit: false, null, StateSelected: true),
		new SharedHitRoute(typeof(DevotedSculptor), "bio_automaton_for_devoted_sculptor", "Attack", EnemyAction.BasicAttack, "vfx/vfx_attack_blunt", FollowTarget: false, 0.4667f)
	};

	private static readonly Dictionary<(Type, string), SharedHitRoute[]> ByMonsterTrigger = (from r in All
		group r by (MonsterType: r.MonsterType, Trigger: r.Trigger)).ToDictionary((IGrouping<(Type MonsterType, string Trigger), SharedHitRoute> g) => g.Key, (IGrouping<(Type MonsterType, string Trigger), SharedHitRoute> g) => g.ToArray());

	private static readonly Dictionary<(string, string), SharedHitRoute[]> ByProfileTrigger = (from r in All
		group r by (ProfileId: r.ProfileId, Trigger: r.Trigger)).ToDictionary((IGrouping<(string ProfileId, string Trigger), SharedHitRoute> g) => g.Key, (IGrouping<(string ProfileId, string Trigger), SharedHitRoute> g) => g.ToArray());

	private static readonly HashSet<(string, EnemyAction)> FollowingActions = (from r in All
		where r.FollowTarget
		select (ProfileId: r.ProfileId, Action: r.Action)).ToHashSet();

	internal static SharedHitRoute? Find(Type? type, string? trigger)
	{
		if (!(type != null) || trigger == null || !ByMonsterTrigger.TryGetValue((type, trigger), out SharedHitRoute[] value))
		{
			return null;
		}
		return Array.Find(value, (SharedHitRoute r) => r.MoveId == null && !r.StateSelected);
	}

	internal static SharedHitRoute? FindForMonster(MonsterModel? monster, string? trigger)
	{
		if (monster == null || trigger == null || !ByMonsterTrigger.TryGetValue((((object)monster).GetType(), trigger), out SharedHitRoute[] value))
		{
			return null;
		}
		return Array.Find(value, (SharedHitRoute r) => (r.MoveId == null || (monster.IsPerformingMove && monster.NextMove.StateId == r.MoveId)) && (!r.StateSelected || SharedHitPresentationSelector.Matches(monster, trigger, r.Action)));
	}

	internal static float MinimumWait(string profile, string trigger, MonsterModel? monster = null)
	{
		if (!ByProfileTrigger.TryGetValue((profile, trigger), out SharedHitRoute[] value))
		{
			return 0f;
		}
		return Array.Find(value, (SharedHitRoute r) => (r.MoveId == null || (monster != null && monster.IsPerformingMove && monster.NextMove.StateId == r.MoveId)) && (!r.StateSelected || (monster != null && SharedHitPresentationSelector.Matches(monster, trigger, r.Action))))?.MinimumWait ?? 0f;
	}

	internal static bool Follows(string profile, EnemyAction action)
	{
		return FollowingActions.Contains((profile, action));
	}
}
