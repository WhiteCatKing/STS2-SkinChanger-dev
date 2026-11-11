using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Patches;

internal static class EnemyAnimatorRoutes
{
	private const string CyborgProfileId = "bio_automaton_for_devoted_sculptor";

	private const string SaintHoundProfileId = "saint_hound_for_living_shield";

	private const string SoulNexusProfileId = "fallacia_for_soul_nexus";

	private static readonly EnemyAnimatorRoute[] All = new EnemyAnimatorRoute[20]
	{
		Route<TorchHeadAmalgam>("Torch Head Amalgam replacement animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel _, MegaSprite controller, EnemyReplacementProfile profile) => TorchHeadAmalgamAnimatorAdapter.Create(controller, profile))),
		Route<WaterfallGiant>("Waterfall Giant replacement animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => WaterfallGiantAnimatorAdapter.Create((WaterfallGiant)monster, controller, profile))),
		Route<TheInsatiable>("The Insatiable replacement animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel _, MegaSprite controller, EnemyReplacementProfile profile) => TheInsatiableAnimatorAdapter.Create(controller, profile))),
		Route<TestSubject>("Test Subject replacement animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => TestSubjectAnimatorAdapter.Create((TestSubject)monster, controller, profile))),
		Route<BowlbugRock>("Bowlbug Rock replacement animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => BowlbugRockAnimatorAdapter.Create((BowlbugRock)monster, controller, profile))),
		Route<SpinyToad>("Spiny Toad stance adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => SpinyToadAnimatorAdapter.Create((SpinyToad)monster, controller, profile))),
		Route<BowlbugNectar>("Bowlbug Nectar replacement animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => BowlbugNectarAnimatorAdapter.Create((BowlbugNectar)monster, controller, profile))),
		Route<GremlinMerc>("Gremlin Merc move-aware animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => GremlinMercAnimatorAdapter.Create((GremlinMerc)monster, controller, profile))),
		Route<Vantom>("Bercula/Vantom stance animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => (controller.HasAnimation("stance1_idle") && controller.HasAnimation("stance1_m_attack_01_to_normal")) ? BerculaVantomAnimatorAdapter.Create((Vantom)monster, controller, profile) : null)),
		Route<CeremonialBeast>("Enemy phase animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => HasNormalIdleAnimation(profile, controller) ? CeremonialBeastAnimatorAdapter.Create((CeremonialBeast)monster, controller, profile) : null))with
		{
			MonsterIdOverride = "CEREMONIAL_BEAST"
		},
		Route<LagavulinMatriarch>("Lagavulin Matriarch replacement animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile _) => (controller.HasAnimation("stance2_idle") && controller.HasAnimation("stance1_attack_ready") && controller.HasAnimation("normal_attack_ready")) ? LagavulinMatriarchAnimatorAdapter.Create((LagavulinMatriarch)monster, controller) : null)),
		Route<KnowledgeDemon>("Knowledge Demon replacement animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => (controller.HasAnimation("normal_job_attack_ready") && controller.HasAnimation("normal_job_attack_end")) ? KnowledgeDemonAnimatorAdapter.Create((KnowledgeDemon)monster, controller, profile) : null)),
		Route<Aeonglass>("Aeonglass replacement animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel _, MegaSprite controller, EnemyReplacementProfile profile) => controller.HasAnimation("normal_attack_ready") ? AeonglassAnimatorAdapter.Create(controller, profile) : null)),
		Route<DecimillipedeSegment>("Decimillipede replacement animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel _, MegaSprite controller, EnemyReplacementProfile profile) => DecimillipedeSegmentAnimatorAdapter.Create(controller, profile))),
		BaseRoute<Queen>("Queen replacement animator adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => QueenAnimatorAdapter.Create((Queen)monster, controller, profile))),
		BaseRoute<DevotedSculptor>("Phased enemy attack adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel _, MegaSprite controller, EnemyReplacementProfile profile) => PhasedAttackAnimatorAdapter.Create(controller, profile, true, "normal_attack_ready", "normal_attack_play1", "normal_attack_play2", "normal_attack_end")))with
		{
			RequiredProfileId = "bio_automaton_for_devoted_sculptor"
		},
		BaseRoute<LivingShield>("Phased enemy attack adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => LivingShieldAnimatorAdapter.Create((LivingShield)monster, controller, profile)))with
		{
			RequiredProfileId = "saint_hound_for_living_shield"
		},
		BaseRoute<SoulNexus>("Phased enemy attack adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => SoulNexusAnimatorAdapter.Create((SoulNexus)monster, controller, profile)))with
		{
			RequiredProfileId = "fallacia_for_soul_nexus"
		},
		Route<SlimedBerserker>("Phased enemy attack adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => SlimedBerserkerAnimatorAdapter.Create((SlimedBerserker)monster, controller, profile)))with
		{
			RequiredProfileId = "suppressed_servant_for_slimed_berserker"
		},
		Route<OwlMagistrate>("Phased enemy attack adapter", (Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?>)((MonsterModel monster, MegaSprite controller, EnemyReplacementProfile profile) => OwlMagistrateAnimatorAdapter.Create((OwlMagistrate)monster, controller, profile)))with
		{
			RequiredProfileId = "winged_humanface_for_owl_magistrate"
		}
	};

	private static readonly IReadOnlyDictionary<Type, EnemyAnimatorRoute[]> ByPatchTarget = (from route in All
		group route by route.PatchTarget).ToDictionary((IGrouping<Type, EnemyAnimatorRoute> group) => group.Key, (IGrouping<Type, EnemyAnimatorRoute> group) => group.ToArray());

	public static IEnumerable<Type> PatchTargets => ByPatchTarget.Keys;

	public static IReadOnlyList<EnemyAnimatorRoute> For(Type patchTarget)
	{
		if (!ByPatchTarget.TryGetValue(patchTarget, out EnemyAnimatorRoute[] value))
		{
			return Array.Empty<EnemyAnimatorRoute>();
		}
		return value;
	}

	private static EnemyAnimatorRoute Route<TMonster>(string failureDescription, Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?> create) where TMonster : MonsterModel
	{
		return new EnemyAnimatorRoute(typeof(TMonster), typeof(TMonster), failureDescription, create);
	}

	private static EnemyAnimatorRoute BaseRoute<TMonster>(string failureDescription, Func<MonsterModel, MegaSprite, EnemyReplacementProfile, CreatureAnimator?> create) where TMonster : MonsterModel
	{
		return new EnemyAnimatorRoute(typeof(MonsterModel), typeof(TMonster), failureDescription, create);
	}

	private static bool HasNormalIdleAnimation(EnemyReplacementProfile profile, MegaSprite controller)
	{
		return profile.Monster.AnimationBindings.Any((EnemyAnimationBinding binding) => binding.Stance == EnemyPresentationStance.Normal && binding.Action == EnemyAction.Idle && controller.HasAnimation(binding.CznAnimationName));
	}
}
