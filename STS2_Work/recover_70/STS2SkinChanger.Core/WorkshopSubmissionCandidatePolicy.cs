using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class WorkshopSubmissionCandidatePolicy
{
	internal static HashSet<ulong> EligibleIds(IEnumerable<WorkshopCatalogItem> recognized, IEnumerable<WorkshopCatalogItem> listed)
	{
		HashSet<ulong> hashSet = (from item in recognized
			where item.Id != 0 && item.Id != 3787302680u && item.Targets.Length != 0
			select item.Id).ToHashSet();
		hashSet.ExceptWith(listed.Select((WorkshopCatalogItem item) => item.Id));
		return hashSet;
	}

	internal static WorkshopLocalSubmission[] Filter(IEnumerable<WorkshopLocalSubmission> installed, IEnumerable<WorkshopCatalogItem> recognized, IEnumerable<WorkshopCatalogItem> listed)
	{
		HashSet<ulong> ids = EligibleIds(recognized, listed);
		return installed.Where((WorkshopLocalSubmission item) => ids.Contains(item.Id)).DistinctBy((WorkshopLocalSubmission item) => item.Id).OrderBy<WorkshopLocalSubmission, string>((WorkshopLocalSubmission item) => item.Name, StringComparer.CurrentCulture)
			.ThenBy((WorkshopLocalSubmission item) => item.Id)
			.ToArray();
	}
}
