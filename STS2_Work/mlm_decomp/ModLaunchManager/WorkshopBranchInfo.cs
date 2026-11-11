using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLaunchManager;

internal sealed record WorkshopBranchInfo(WorkshopBranchKind Kind, IReadOnlyList<string> Branches)
{
	internal static WorkshopBranchInfo Resolve(WorkshopItemVersions? catalog, IReadOnlyList<string>? reportedBranches, bool reportedUnrestricted)
	{
		if ((object)catalog != null && catalog.HasBranchInformation)
		{
			string[] array = new string[2] { "public", "public-beta" }.Where((string branch) => catalog.ForBranch(branch) != null).ToArray();
			WorkshopItemVersion sharedPublicVersion = catalog.SharedPublicVersion;
			if ((object)sharedPublicVersion != null)
			{
				return new WorkshopBranchInfo(sharedPublicVersion.IsUnrestricted ? WorkshopBranchKind.AllVersions : WorkshopBranchKind.SharedPublicVersion, array);
			}
			return new WorkshopBranchInfo((array.Length == 0) ? WorkshopBranchKind.NoPublicVersion : WorkshopBranchKind.Versioned, array);
		}
		if (reportedBranches == null)
		{
			return new WorkshopBranchInfo(WorkshopBranchKind.Unknown, Array.Empty<string>());
		}
		return new WorkshopBranchInfo(reportedUnrestricted ? WorkshopBranchKind.AllVersions : ((reportedBranches.Count == 0) ? WorkshopBranchKind.NoPublicVersion : WorkshopBranchKind.Versioned), reportedBranches);
	}
}
