using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Profiles;

internal static class SelectedEncounterBattlefieldProfiles
{
	internal static IReadOnlyList<EncounterBattlefieldProfile> Profiles { get; } = new _003C_003Ez__ReadOnlyArray<EncounterBattlefieldProfile>(new EncounterBattlefieldProfile[13]
	{
		Bind("laino_indoor3_1_for_vantom_boss", "VANTOM_BOSS", BattlefieldPresentationProfiles.LainoIndoor31),
		Bind("laino_boss_1_for_ceremonial_beast_boss", "CEREMONIAL_BEAST_BOSS", BattlefieldPresentationProfiles.LainoBoss1),
		Bind("delta_outdoor4_1_for_the_kin_boss", "THE_KIN_BOSS", BattlefieldPresentationProfiles.DeltaOutdoor41),
		Bind("spaitz_indoor3_1_for_the_insatiable_boss", "THE_INSATIABLE_BOSS", BattlefieldPresentationProfiles.SpaitzIndoor31),
		Bind("spaitz_outdoor0_1_for_knowledge_demon_boss", "KNOWLEDGE_DEMON_BOSS", BattlefieldPresentationProfiles.SpaitzOutdoor01),
		Bind("spaitz_outdoor2_3_for_kaiser_crab_boss", "KAISER_CRAB_BOSS", BattlefieldPresentationProfiles.SpaitzOutdoor23),
		Bind("ellinad_outdoor2_2_for_aeonglass_boss", "AEONGLASS_BOSS", BattlefieldPresentationProfiles.EllinadOutdoor22),
		Bind("ellinad_outdoor3_2_for_test_subject_boss", "TEST_SUBJECT_BOSS", BattlefieldPresentationProfiles.EllinadOutdoor32),
		Bind("ellinad_outdoor3_2_for_the_architect_event", "THE_ARCHITECT_EVENT_ENCOUNTER", BattlefieldPresentationProfiles.EllinadOutdoor32),
		Bind("ellinad_outdoor3_1_for_queen_boss", "QUEEN_BOSS", BattlefieldPresentationProfiles.EllinadOutdoor31),
		Bind("earth_boss_1_for_soul_fysh_boss", "SOUL_FYSH_BOSS", BattlefieldPresentationProfiles.EarthBoss1),
		Bind("earth_boss_1_for_lagavulin_matriarch_boss", "LAGAVULIN_MATRIARCH_BOSS", BattlefieldPresentationProfiles.EarthBoss1),
		Bind("verne_woods2_1_for_waterfall_giant_boss", "WATERFALL_GIANT_BOSS", BattlefieldPresentationProfiles.VerneWoods21)
	});

	private static EncounterBattlefieldProfile Bind(string id, string encounterId, BattlefieldPresentationProfile battlefield)
	{
		return new EncounterBattlefieldProfile(id, new Sts2EncounterBattlefieldTarget(encounterId), battlefield, enabledByDefault: true);
	}
}
