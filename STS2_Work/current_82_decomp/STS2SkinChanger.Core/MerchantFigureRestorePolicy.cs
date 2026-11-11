using System;

namespace STS2SkinChanger.Core;

internal static class MerchantFigureRestorePolicy
{
	internal static bool KeepsTransform(string? nodeOwnerProviderId, string providerBeingDeactivated, bool providerStillSelected)
	{
		if (string.IsNullOrEmpty(nodeOwnerProviderId) || string.IsNullOrEmpty(providerBeingDeactivated))
		{
			return false;
		}
		if (providerStillSelected)
		{
			return nodeOwnerProviderId.Equals(providerBeingDeactivated, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}
}
