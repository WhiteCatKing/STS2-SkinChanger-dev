using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Profiles;

internal static class UnderdocksEarthOutdoor32Profile
{
	internal const string ProfileId = "earth_outdoor3_2_for_underdocks_standard_combat";

	internal const string TargetActId = "UNDERDOCKS";

	internal static ActPresentationProfile Profile { get; } = new ActPresentationProfile("earth_outdoor3_2_for_underdocks_standard_combat", new Sts2ActTarget("UNDERDOCKS", ActRoomScope.StandardCombat), BattlefieldPresentationProfiles.EarthOutdoor32, enabledByDefault: true);

	internal static IReadOnlyList<ActPresentationProfile> Profiles { get; } = new _003C_003Ez__ReadOnlyArray<ActPresentationProfile>(new ActPresentationProfile[3]
	{
		Profile,
		new ActPresentationProfile("earth_outdoor0_2_for_underdocks_weak_monster", new Sts2ActTarget("UNDERDOCKS", ActRoomScope.WeakMonster), BattlefieldPresentationProfiles.EarthOutdoor02, enabledByDefault: true, "earth_outdoor3_2_for_underdocks_standard_combat"),
		new ActPresentationProfile("earth_outdoor3_2_for_underdocks_regular_monster", new Sts2ActTarget("UNDERDOCKS", ActRoomScope.RegularMonster), BattlefieldPresentationProfiles.EarthOutdoor32, enabledByDefault: true, "earth_outdoor3_2_for_underdocks_standard_combat")
	});
}
