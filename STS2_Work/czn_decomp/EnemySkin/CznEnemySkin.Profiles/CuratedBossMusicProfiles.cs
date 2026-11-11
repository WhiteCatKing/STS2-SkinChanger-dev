using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Profiles;

internal static class CuratedBossMusicProfiles
{
	internal static IReadOnlyList<CuratedBossMusicProfile> All { get; } = new _003C_003Ez__ReadOnlyArray<CuratedBossMusicProfile>(new CuratedBossMusicProfile[12]
	{
		Boss("curated.boss.route.soul_fysh", "SOUL_FYSH_BOSS", "curated.boss.3000332.senectus"),
		Boss("curated.boss.route.lagavulin_matriarch", "LAGAVULIN_MATRIARCH_BOSS", "curated.boss.1007021.deforsa"),
		Boss("curated.boss.route.kaiser_crab", "KAISER_CRAB_BOSS", "curated.boss.1001013_3000477.formica_duo"),
		Boss("curated.boss.route.the_insatiable", "THE_INSATIABLE_BOSS", "curated.boss.3000438.punisher_of_resentment"),
		Boss("curated.boss.route.test_subject", "TEST_SUBJECT_BOSS", "curated.boss.3000479.malevolent_one.phase_1", "curated.boss.3000479.malevolent_one.phase_2", "curated.boss.3000479.malevolent_one.phase_3"),
		Boss("curated.boss.route.queen", "QUEEN_BOSS", "curated.boss.3000481.shadow_collector"),
		Boss("curated.boss.route.aeonglass", "AEONGLASS_BOSS", "curated.boss.1003008.viadeus"),
		Boss("curated.boss.route.knowledge_demon", "KNOWLEDGE_DEMON_BOSS", "curated.boss.1004019.wailing_prodigal"),
		Boss("curated.boss.route.waterfall_giant", "WATERFALL_GIANT_BOSS", "curated.boss.3000416.golden_mane"),
		Boss("curated.boss.route.ceremonial_beast", "CEREMONIAL_BEAST_BOSS", "curated.boss.1005006.elemental_spirit"),
		Boss("curated.boss.route.vantom", "VANTOM_BOSS", "curated.boss.1001021.bercula"),
		Boss("curated.boss.route.the_kin", "THE_KIN_BOSS", "curated.boss.1002008.rotten_ripper")
	});

	private static CuratedBossMusicProfile Boss(string id, string encounterId, params string[] phaseTrackIds)
	{
		return new CuratedBossMusicProfile(id, encounterId, phaseTrackIds);
	}
}
