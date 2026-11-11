using System;

namespace STS2SkinChanger.Core;

internal static class FrameworkEntryAnimationPolicy
{
	public static FrameworkEntryAnimationPlan? Resolve(bool hasSelectedFrameworkSkin, bool hasEntryAnimation, string? currentAnimationId, bool currentAnimationLoops)
	{
		if (!hasSelectedFrameworkSkin || !hasEntryAnimation || string.IsNullOrWhiteSpace(currentAnimationId) || currentAnimationId.Equals("entry", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		return new FrameworkEntryAnimationPlan("entry", currentAnimationId, currentAnimationLoops);
	}
}
