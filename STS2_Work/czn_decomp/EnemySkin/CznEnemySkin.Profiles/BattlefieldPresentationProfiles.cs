using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Profiles;

internal static class BattlefieldPresentationProfiles
{
	private static readonly HashSet<string> StandardCombatAcceptedVariants = new HashSet<string>(StringComparer.Ordinal) { "b_laino_outdoor0_2", "b_laino_outdoor2_2", "b_spaitz_outdoor1_2", "b_spaitz_outdoor2_2", "b_ellinad_outdoor1_1", "b_ellinad_outdoor1_3", "b_ellinad_outdoor2_2", "b_earth_outdoor0_2", "b_earth_outdoor3_2" };

	internal static BattlefieldPresentationProfile LainoOutdoor02 { get; } = CreateStandardCombat("b_laino_outdoor0_2", "czn.battle.laino");

	internal static BattlefieldPresentationProfile LainoOutdoor22 { get; } = CreateStandardCombat("b_laino_outdoor2_2", "czn.battle.laino");

	internal static BattlefieldPresentationProfile LainoIndoor31 { get; } = CreateExactEncounter("b_laino_indoor3_1", "czn.battle.laino");

	internal static BattlefieldPresentationProfile LainoBoss1 { get; } = CreateExactEncounter("b_laino_boss_1", "czn.battle.faction_secred");

	internal static BattlefieldPresentationProfile DeltaOutdoor41 { get; } = CreateExactEncounter("b_delta_outdoor4_1", "czn.battle.elite_deepdark_2");

	internal static BattlefieldPresentationProfile SpaitzOutdoor12 { get; } = CreateStandardCombat("b_spaitz_outdoor1_2", "czn.battle.spaitz");

	internal static BattlefieldPresentationProfile SpaitzOutdoor22 { get; } = CreateStandardCombat("b_spaitz_outdoor2_2", "czn.battle.spaitz");

	internal static BattlefieldPresentationProfile SpaitzIndoor31 { get; } = CreateExactEncounter("b_spaitz_indoor3_1", "czn.battle.spaitz");

	internal static BattlefieldPresentationProfile SpaitzOutdoor01 { get; } = CreateExactEncounter("b_spaitz_outdoor0_1", "czn.battle.spaitz");

	internal static BattlefieldPresentationProfile SpaitzOutdoor23 { get; } = CreateExactEncounter("b_spaitz_outdoor2_3", "czn.battle.spaitz");

	internal static BattlefieldPresentationProfile EllinadWeakPool { get; } = CreateStandardCombatPool("ellinad_weak_standard_combat_pool", "b_ellinad_outdoor1_1", "czn.battle.ellinad_theater_1", "b_ellinad_outdoor1_1", "b_ellinad_outdoor1_3");

	internal static BattlefieldPresentationProfile EllinadRegularPool { get; } = CreateStandardCombatPool("ellinad_regular_standard_combat_pool", "b_ellinad_outdoor2_2", "czn.battle.ellinad_theater_1", "b_ellinad_outdoor2_2", "b_ellinad_outdoor1_1", "b_ellinad_outdoor1_3");

	internal static BattlefieldPresentationProfile EllinadOutdoor22 { get; } = CreateExactEncounter("b_ellinad_outdoor2_2", "czn.battle.ellinad_theater_1");

	internal static BattlefieldPresentationProfile EllinadOutdoor32 { get; } = CreateExactEncounter("b_ellinad_outdoor3_2", "czn.battle.ellinad_theater_2");

	internal static BattlefieldPresentationProfile EllinadOutdoor31 { get; } = CreateExactEncounter("b_ellinad_outdoor3_1", "czn.battle.ellinad_theater_2");

	internal static BattlefieldPresentationProfile EarthOutdoor02 { get; } = CreateStandardCombat("b_earth_outdoor0_2", "czn.battle.earth");

	internal static BattlefieldPresentationProfile EarthOutdoor32 { get; } = CreateStandardCombat("b_earth_outdoor3_2", "czn.battle.earth");

	internal static BattlefieldPresentationProfile EarthBoss1 { get; } = CreateExactEncounter("b_earth_boss_1", "czn.boss.earth");

	internal static BattlefieldPresentationProfile VerneWoods21 { get; } = CreateExactEncounter("b_verne_woods2_1", "czn.boss.waterfall_giant");

	private static BattlefieldPresentationProfile CreateStandardCombat(string battlefieldId, string trackId)
	{
		return CreateStandardCombatPool(battlefieldId, battlefieldId, trackId, battlefieldId);
	}

	private static BattlefieldPresentationProfile CreateStandardCombatPool(string profileId, string shellBattlefieldId, string trackId, params string[] backgroundVariantIds)
	{
		if (backgroundVariantIds.Length == 0)
		{
			throw new ArgumentException("A standard-combat battlefield pool cannot be empty.", "backgroundVariantIds");
		}
		string[] array = backgroundVariantIds.Where((string variantId) => !StandardCombatAcceptedVariants.Contains(variantId)).ToArray();
		if (array.Length != 0)
		{
			throw new ArgumentException("Standard-combat battlefield variants require explicit Godot and installed-game visual acceptance: " + string.Join(",", array), "backgroundVariantIds");
		}
		if (!StandardCombatAcceptedVariants.Contains(shellBattlefieldId))
		{
			throw new ArgumentException("A standard-combat shell requires explicit Godot and installed-game visual acceptance: " + shellBattlefieldId, "shellBattlefieldId");
		}
		return Create(profileId, shellBattlefieldId, trackId, backgroundVariantIds);
	}

	private static BattlefieldPresentationProfile CreateExactEncounter(string battlefieldId, string trackId)
	{
		return Create(battlefieldId, battlefieldId, trackId, new _003C_003Ez__ReadOnlySingleElementList<string>(battlefieldId));
	}

	private static BattlefieldPresentationProfile Create(string profileId, string shellBattlefieldId, string trackId, IReadOnlyList<string> backgroundVariantIds)
	{
		string text = "res://CznEnemySkin/battlefields/" + shellBattlefieldId + "/";
		return new BattlefieldPresentationProfile(profileId, new CznBattlefieldSource(shellBattlefieldId), new BattlefieldBackgroundPresentation(PresentationMode.ReplaceReady, text + shellBattlefieldId + "_background.tscn", new _003C_003Ez__ReadOnlySingleElementList<IEnumerable<string>>(backgroundVariantIds.Select((string variantId) => "res://CznEnemySkin/battlefields/" + variantId + "/layers/" + variantId + "_bg_00_a.tscn"))), new BattlefieldBgmPresentation(PresentationMode.ReplaceReady, trackId), BattlefieldCameraPresentation.Inherit);
	}
}
