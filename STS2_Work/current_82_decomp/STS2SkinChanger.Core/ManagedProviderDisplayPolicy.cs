using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class ManagedProviderDisplayPolicy
{
	public static bool IsManaged(string? modId, IReadOnlyCollection<string> providerIds, bool isFunctionalHost = false)
	{
		if (!isFunctionalHost && !string.IsNullOrWhiteSpace(modId))
		{
			return providerIds.Contains<string>(modId, StringComparer.OrdinalIgnoreCase);
		}
		return false;
	}
}
