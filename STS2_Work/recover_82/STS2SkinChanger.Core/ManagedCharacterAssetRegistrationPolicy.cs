using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal static class ManagedCharacterAssetRegistrationPolicy
{
	public static bool ShouldSuppress(string? registryModId, IReadOnlySet<string> managedProviderIds)
	{
		if (!string.IsNullOrWhiteSpace(registryModId))
		{
			return managedProviderIds.Contains(registryModId);
		}
		return false;
	}
}
