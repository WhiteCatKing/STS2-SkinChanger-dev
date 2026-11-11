using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Profiles;

internal static class MusicTrackProfiles
{
	private const float CznBattleReferenceGainLinear = 0.2f;

	internal const string LainoBattleId = "czn.battle.laino";

	internal const string FactionSecredBattleId = "czn.battle.faction_secred";

	internal const string EliteDeepdark2BattleId = "czn.battle.elite_deepdark_2";

	internal const string SpaitzBattleId = "czn.battle.spaitz";

	internal const string EllinadTheater1BattleId = "czn.battle.ellinad_theater_1";

	internal const string EllinadTheater2BattleId = "czn.battle.ellinad_theater_2";

	internal const string EarthBattleId = "czn.battle.earth";

	internal const string EarthBossId = "czn.boss.earth";

	internal const string WaterfallGiantBossId = "czn.boss.waterfall_giant";

	internal const string CuratedSenectusId = "curated.boss.3000332.senectus";

	internal const string CuratedDeforsaId = "curated.boss.1007021.deforsa";

	internal const string CuratedFormicaDuoId = "curated.boss.1001013_3000477.formica_duo";

	internal const string CuratedPunisherOfResentmentId = "curated.boss.3000438.punisher_of_resentment";

	internal const string CuratedMalevolentOnePhase1Id = "curated.boss.3000479.malevolent_one.phase_1";

	internal const string CuratedMalevolentOnePhase2Id = "curated.boss.3000479.malevolent_one.phase_2";

	internal const string CuratedMalevolentOnePhase3Id = "curated.boss.3000479.malevolent_one.phase_3";

	internal const string CuratedShadowCollectorId = "curated.boss.3000481.shadow_collector";

	internal const string CuratedViadeusId = "curated.boss.1003008.viadeus";

	internal const string CuratedWailingProdigalId = "curated.boss.1004019.wailing_prodigal";

	internal const string CuratedGoldenManeId = "curated.boss.3000416.golden_mane";

	internal const string CuratedElementalSpiritId = "curated.boss.1005006.elemental_spirit";

	internal const string CuratedBerculaId = "curated.boss.1001021.bercula";

	internal const string CuratedRottenRipperId = "curated.boss.1002008.rotten_ripper";

	internal const string LainoMapId = "czn.map.laino";

	internal const string EarthMapId = "czn.map.earth";

	internal const string SpaitzMapId = "czn.map.spaitz";

	internal const string EllinadTheater1MapId = "czn.map.ellinad_theater_1";

	internal const string EllinadTheater2MapId = "czn.map.ellinad_theater_2";

	internal static IReadOnlyList<MusicTrackProfile> All { get; } = new _003C_003Ez__ReadOnlyArray<MusicTrackProfile>(new MusicTrackProfile[28]
	{
		Battle("czn.battle.laino", "bgm_laino_battle", 0.211349f),
		Battle("czn.battle.faction_secred", "bgm_faction_secred_battle", 0.199526f),
		Battle("czn.battle.elite_deepdark_2", "bgm_elite_deepdark_2", 0.2f),
		Battle("czn.battle.spaitz", "bgm_spaitz_battle", 0.2238721f),
		Battle("czn.battle.ellinad_theater_1", "bgm_theater_of_illusions_1_battle", 0.19952624f),
		Battle("czn.battle.ellinad_theater_2", "bgm_theater_of_illusions_2_battle", 0.29853824f),
		Battle("czn.battle.earth", "bgm_earth_battle", 0.149624f),
		Battle("czn.boss.earth", "bgm_earth_boss", 0.141254f),
		Battle("czn.boss.waterfall_giant", "bgm_waterfall_giant_boss", 0.188365f, 5.844354f),
		Curated("curated.boss.3000332.senectus", "3000332_senectus", 0.659933f),
		Curated("curated.boss.1007021.deforsa", "1007021_deforsa", 0.747309f),
		Curated("curated.boss.1001013_3000477.formica_duo", "1001013_3000477_formica_duo", 0.948418f),
		Curated("curated.boss.3000438.punisher_of_resentment", "3000438_punisher_of_resentment", 0.676083f),
		Curated("curated.boss.3000479.malevolent_one.phase_1", "3000479_malevolent_one_phase_1", 0.519996f),
		Curated("curated.boss.3000479.malevolent_one.phase_2", "3000479_malevolent_one_phase_2", 0.49204f),
		Curated("curated.boss.3000479.malevolent_one.phase_3", "3000479_malevolent_one_phase_3", 0.400405f),
		Curated("curated.boss.3000481.shadow_collector", "3000481_shadow_collector", 0.742164f),
		Curated("curated.boss.1003008.viadeus", "1003008_viadeus", 0.726106f),
		Curated("curated.boss.1004019.wailing_prodigal", "1004019_wailing_prodigal", 0.708761f),
		Curated("curated.boss.3000416.golden_mane", "3000416_golden_mane", 0.778933f),
		Curated("curated.boss.1005006.elemental_spirit", "1005006_elemental_spirit", 0.635331f),
		Curated("curated.boss.1001021.bercula", "1001021_bercula", 0.893305f),
		Curated("curated.boss.1002008.rotten_ripper", "1002008_rotten_ripper", 0.628782f),
		Map("czn.map.laino", "bgm_laino_base"),
		Map("czn.map.earth", "bgm_earth_base"),
		Map("czn.map.spaitz", "bgm_spaitz_base"),
		Map("czn.map.ellinad_theater_1", "bgm_theater_of_illusions_1_base"),
		Map("czn.map.ellinad_theater_2", "bgm_theater_of_illusions_2_base")
	});

	private static MusicTrackProfile Battle(string id, string stem, float sourceGainLinear, float loopOffsetSeconds = 0f)
	{
		return new MusicTrackProfile(id, BgmTransport.GodotStream, "res://CznEnemySkin/audio/bgm/" + stem + ".ogg", sourceGainLinear, 0.2f, loopOffsetSeconds);
	}

	private static MusicTrackProfile Curated(string id, string stem, float playbackGainLinear)
	{
		return new MusicTrackProfile(id, BgmTransport.GodotStream, "res://CznEnemySkin/audio/bgm/curated_boss/" + stem + ".ogg", playbackGainLinear);
	}

	private static MusicTrackProfile Map(string id, string stem)
	{
		return new MusicTrackProfile(id, BgmTransport.GodotStream, "res://CznEnemySkin/audio/bgm/" + stem + ".ogg");
	}
}
