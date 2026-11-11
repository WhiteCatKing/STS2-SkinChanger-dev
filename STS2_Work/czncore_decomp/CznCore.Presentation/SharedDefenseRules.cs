using MegaCrit.Sts2.Core.Entities.Creatures;

namespace CznCore.Presentation;

internal static class SharedDefenseRules
{
	public static bool PlayHit(int before, int after)
	{
		if (before > after)
		{
			return after > 0;
		}
		return false;
	}

	public static bool NeedsFullyBlockedBreakSound(DamageResult result)
	{
		if (result.WasBlockBroken && result.BlockedDamage > 0 && result.WasFullyBlocked)
		{
			return result.UnblockedDamage == 0;
		}
		return false;
	}
}
