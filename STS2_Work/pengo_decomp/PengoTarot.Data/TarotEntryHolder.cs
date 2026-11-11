using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Merchant;

namespace PengoTarot.Data;

public static class TarotEntryHolder
{
	private static readonly Dictionary<MerchantInventory, MerchantTarotEntry> _map = new Dictionary<MerchantInventory, MerchantTarotEntry>();

	public static void SetEntry(MerchantInventory inventory, MerchantTarotEntry entry)
	{
		_map[inventory] = entry;
	}

	public static MerchantTarotEntry? GetEntry(MerchantInventory inventory)
	{
		if (!_map.TryGetValue(inventory, out MerchantTarotEntry value))
		{
			return null;
		}
		return value;
	}

	public static void ClearEntry(MerchantInventory inventory)
	{
		_map.Remove(inventory);
	}
}
