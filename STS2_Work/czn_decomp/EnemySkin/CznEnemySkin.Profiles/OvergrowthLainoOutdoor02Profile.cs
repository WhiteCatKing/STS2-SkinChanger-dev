using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Profiles;

internal static class OvergrowthLainoOutdoor02Profile
{
	internal const string ProfileId = "laino_outdoor0_2_for_overgrowth_normal_combat";

	internal const string TargetActId = "OVERGROWTH";

	internal static ActPresentationProfile Profile { get; } = new ActPresentationProfile("laino_outdoor0_2_for_overgrowth_normal_combat", new Sts2ActTarget("OVERGROWTH", ActRoomScope.StandardCombat), BattlefieldPresentationProfiles.LainoOutdoor22, enabledByDefault: true);

	internal static IReadOnlyList<ActPresentationProfile> Profiles { get; } = new _003C_003Ez__ReadOnlyArray<ActPresentationProfile>(new ActPresentationProfile[3]
	{
		Profile,
		new ActPresentationProfile("laino_outdoor0_2_for_overgrowth_weak_monster", new Sts2ActTarget("OVERGROWTH", ActRoomScope.WeakMonster), BattlefieldPresentationProfiles.LainoOutdoor02, enabledByDefault: true, "laino_outdoor0_2_for_overgrowth_normal_combat"),
		new ActPresentationProfile("laino_outdoor2_2_for_overgrowth_regular_monster", new Sts2ActTarget("OVERGROWTH", ActRoomScope.RegularMonster), BattlefieldPresentationProfiles.LainoOutdoor22, enabledByDefault: true, "laino_outdoor0_2_for_overgrowth_normal_combat")
	});
}
