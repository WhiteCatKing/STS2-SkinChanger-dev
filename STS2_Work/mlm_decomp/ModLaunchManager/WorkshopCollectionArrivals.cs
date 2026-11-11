using System.Collections.Generic;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class WorkshopCollectionArrivals
{
	private static readonly HashSet<ulong> Items = new HashSet<ulong>();

	internal static void Protect(ulong itemId)
	{
		if (itemId != 0)
		{
			Items.Add(itemId);
		}
	}

	internal static bool KeepsOff(Mod mod)
	{
		if (Items.Count > 0)
		{
			return Items.Contains(PhysicalModIdentity.WorkshopItemIdFor(mod));
		}
		return false;
	}

	internal static void Clear()
	{
		Items.Clear();
	}
}
