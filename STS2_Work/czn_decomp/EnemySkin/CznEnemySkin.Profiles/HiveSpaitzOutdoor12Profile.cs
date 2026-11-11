using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Profiles;

internal static class HiveSpaitzOutdoor12Profile
{
	internal const string ProfileId = "spaitz_outdoor1_2_for_hive_standard_combat";

	internal const string TargetActId = "HIVE";

	internal static ActPresentationProfile Profile { get; } = new ActPresentationProfile("spaitz_outdoor1_2_for_hive_standard_combat", new Sts2ActTarget("HIVE", ActRoomScope.StandardCombat), BattlefieldPresentationProfiles.SpaitzOutdoor22, enabledByDefault: true);

	internal static IReadOnlyList<ActPresentationProfile> Profiles { get; } = new _003C_003Ez__ReadOnlyArray<ActPresentationProfile>(new ActPresentationProfile[3]
	{
		Profile,
		new ActPresentationProfile("spaitz_outdoor1_2_for_hive_weak_monster", new Sts2ActTarget("HIVE", ActRoomScope.WeakMonster), BattlefieldPresentationProfiles.SpaitzOutdoor12, enabledByDefault: true, "spaitz_outdoor1_2_for_hive_standard_combat"),
		new ActPresentationProfile("spaitz_outdoor2_2_for_hive_regular_monster", new Sts2ActTarget("HIVE", ActRoomScope.RegularMonster), BattlefieldPresentationProfiles.SpaitzOutdoor22, enabledByDefault: true, "spaitz_outdoor1_2_for_hive_standard_combat")
	});
}
