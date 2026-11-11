using System;
using MegaCrit.Sts2.Core.Debug;

namespace STS2SkinChanger.Core;

internal static class ProviderMinimumVersionPolicy
{
	public static ProviderMinimumVersionOutcome Decide(string? declaredMinimum, string? gameVersion, bool hasDll, Func<bool> assemblyRunsOnCurrentGame)
	{
		if (string.IsNullOrWhiteSpace(declaredMinimum))
		{
			return ProviderMinimumVersionOutcome.GameDecides;
		}
		SemanticVersion val = default(SemanticVersion);
		if (string.IsNullOrWhiteSpace(gameVersion) || !SemanticVersion.TryFromString(gameVersion, ref val))
		{
			return ProviderMinimumVersionOutcome.GameDecides;
		}
		SemanticVersion val2 = default(SemanticVersion);
		if (!SemanticVersion.TryFromString(declaredMinimum, ref val2))
		{
			return ProviderMinimumVersionOutcome.UnreadableRule;
		}
		if (val.CompareTo(val2) >= 0)
		{
			return ProviderMinimumVersionOutcome.GameDecides;
		}
		if (!hasDll)
		{
			return ProviderMinimumVersionOutcome.VerifiedTakeOver;
		}
		if (!assemblyRunsOnCurrentGame())
		{
			return ProviderMinimumVersionOutcome.Unsupported;
		}
		return ProviderMinimumVersionOutcome.VerifiedTakeOver;
	}
}
