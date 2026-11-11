using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Profiles;

internal static class GloryEllinadOutdoor22Profile
{
	internal const string ProfileId = "ellinad_outdoor2_2_for_glory_standard_combat";

	internal const string TargetActId = "GLORY";

	internal static ActPresentationProfile Profile { get; } = new ActPresentationProfile("ellinad_outdoor2_2_for_glory_standard_combat", new Sts2ActTarget("GLORY", ActRoomScope.StandardCombat), BattlefieldPresentationProfiles.EllinadRegularPool, enabledByDefault: true);

	internal static IReadOnlyList<ActPresentationProfile> Profiles { get; } = new _003C_003Ez__ReadOnlyArray<ActPresentationProfile>(new ActPresentationProfile[3]
	{
		Profile,
		new ActPresentationProfile("ellinad_outdoor1_2_for_glory_weak_monster", new Sts2ActTarget("GLORY", ActRoomScope.WeakMonster), BattlefieldPresentationProfiles.EllinadWeakPool, enabledByDefault: true, "ellinad_outdoor2_2_for_glory_standard_combat"),
		new ActPresentationProfile("ellinad_outdoor2_2_for_glory_regular_monster", new Sts2ActTarget("GLORY", ActRoomScope.RegularMonster), BattlefieldPresentationProfiles.EllinadRegularPool, enabledByDefault: true, "ellinad_outdoor2_2_for_glory_standard_combat")
	});
}
