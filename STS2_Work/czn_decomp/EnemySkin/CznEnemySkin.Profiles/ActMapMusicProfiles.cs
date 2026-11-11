using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Profiles;

internal static class ActMapMusicProfiles
{
	internal static IReadOnlyList<ActMapMusicProfile> All { get; } = new _003C_003Ez__ReadOnlyArray<ActMapMusicProfile>(new ActMapMusicProfile[4]
	{
		new ActMapMusicProfile("czn.map.route.overgrowth", "OVERGROWTH", new _003C_003Ez__ReadOnlySingleElementList<string>("czn.map.laino"), EnabledByDefault: true),
		new ActMapMusicProfile("czn.map.route.underdocks", "UNDERDOCKS", new _003C_003Ez__ReadOnlySingleElementList<string>("czn.map.earth"), EnabledByDefault: true),
		new ActMapMusicProfile("czn.map.route.hive", "HIVE", new _003C_003Ez__ReadOnlySingleElementList<string>("czn.map.spaitz"), EnabledByDefault: true),
		new ActMapMusicProfile("czn.map.route.glory", "GLORY", new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "czn.map.ellinad_theater_1", "czn.map.ellinad_theater_2" }), EnabledByDefault: true)
	});
}
