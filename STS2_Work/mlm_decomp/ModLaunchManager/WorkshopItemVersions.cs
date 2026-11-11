using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ModLaunchManager;

internal sealed record WorkshopItemVersions(ulong InstalledManifest, IReadOnlyList<WorkshopItemVersion> Versions, bool IsComplete = true)
{
	internal bool HasBranchInformation
	{
		get
		{
			if (IsComplete && Versions.Count > 0)
			{
				return Versions.All((WorkshopItemVersion version) => version.HasKnownBounds);
			}
			return false;
		}
	}

	internal WorkshopItemVersion? SharedPublicVersion
	{
		get
		{
			if (HasBranchInformation)
			{
				WorkshopItemVersion workshopItemVersion = ForBranch("public");
				if ((object)workshopItemVersion != null && ForBranch("public-beta")?.ManifestId == workshopItemVersion.ManifestId)
				{
					return workshopItemVersion;
				}
			}
			return null;
		}
	}

	internal WorkshopItemVersion? ForBranch(string branch)
	{
		return (from version in Versions
			where version.Supports(branch)
			orderby version.Updated descending, version.ManifestId descending
			select version).FirstOrDefault();
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("InstalledManifest = ");
		builder.Append(InstalledManifest.ToString());
		builder.Append(", Versions = ");
		builder.Append(Versions);
		builder.Append(", IsComplete = ");
		builder.Append(IsComplete.ToString());
		return true;
	}
}
