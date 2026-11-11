using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal static class MerchantAppearanceScopePolicy
{
	private const string MerchantButtonToken = "MerchantButton";

	private const string MerchantHandToken = "MerchantHand";

	private const string ShopInterfaceToken = "MerchantInventory";

	private const string RoomToken = "MerchantRoom";

	private const string FakeMerchantToken = "FakeMerchant";

	private const string BackgroundNodeName = "BgContainer";

	internal static bool IsMerchantAppearance(IEnumerable<(string TypeName, string NodeName)> ancestry)
	{
		ArgumentNullException.ThrowIfNull(ancestry, "ancestry");
		foreach (var (typeName, text) in ancestry)
		{
			if (Contains(typeName, "MerchantHand") || Contains(typeName, "MerchantButton"))
			{
				return true;
			}
			if (Contains(typeName, "MerchantInventory") || Contains(typeName, "MerchantRoom") || Contains(typeName, "FakeMerchant") || text.Equals("BgContainer", StringComparison.Ordinal))
			{
				return false;
			}
		}
		return false;
	}

	private static bool Contains(string? typeName, string token)
	{
		return typeName?.Contains(token, StringComparison.OrdinalIgnoreCase) ?? false;
	}
}
