using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class SkinSourceRoutingPolicy
{
	public static bool IsSteamItem(ulong id)
	{
		return SkinSourceIdentity.IsSteam(id);
	}

	public static WorkshopCatalogItem[] CatalogUnion(IReadOnlyList<WorkshopCatalogItem> steam, IReadOnlyList<WorkshopCatalogItem> github)
	{
		return (from item in steam.Concat(github)
			group item by item.Id into @group
			select @group.First()).ToArray();
	}
}
