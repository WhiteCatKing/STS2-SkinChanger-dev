using System.Collections.Generic;
using System.Linq;

namespace ModLaunchManager;

internal static class WorkshopCollectionChoices
{
	internal static IReadOnlyList<CollectionChoice> Merge(IReadOnlyList<CollectionChoice> own, IReadOnlyList<CollectionChoice> favorited)
	{
		HashSet<ulong> seen = new HashSet<ulong>();
		return (from choice in own.Select((CollectionChoice choice) => choice with
			{
				Favorited = false
			}).Concat(favorited.Select((CollectionChoice choice) => choice with
			{
				Favorited = true
			}))
			where choice.Id != 0 && seen.Add(choice.Id)
			select choice).ToList();
	}
}
