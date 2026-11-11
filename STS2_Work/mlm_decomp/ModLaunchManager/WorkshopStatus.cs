using System;
using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed class WorkshopStatus
{
	internal required ulong ItemId { get; init; }

	internal WorkshopUpdateState State { get; set; }

	internal uint LocalUpdated { get; set; }

	internal ulong LocalSize { get; set; }

	internal uint ServerUpdated { get; set; }

	internal ulong ServerSize { get; set; }

	internal bool DetectedFromListing { get; set; }

	internal double DownloadWait { get; set; }

	internal uint DismissedListingUpdated { get; set; }

	internal float Progress { get; set; }

	internal string? Title { get; set; }

	internal IReadOnlyList<WorkshopTag>? Tags { get; set; }

	internal string? PreviousVersion { get; set; }

	internal string? NewVersion { get; set; }

	internal string? RefreshError { get; set; }

	internal WorkshopBranchCompatibility LatestBranchCompatibility { get; set; }

	internal string? LatestBranchRanges { get; set; }

	internal string? LatestMaxSupportedBranch { get; set; }

	internal IReadOnlyList<string>? AvailableBranches { get; set; }

	internal bool AllBranchesUnrestricted { get; set; }

	internal bool IsLatestBranchUnsupported => LatestBranchCompatibility == WorkshopBranchCompatibility.Unsupported;

	internal bool HasAvailableUpdate
	{
		get
		{
			if (State == WorkshopUpdateState.UpdateAvailable)
			{
				return !WorkshopBranches.IsPinned(ItemId);
			}
			return false;
		}
	}

	internal bool HasIncompatibleAvailableUpdate
	{
		get
		{
			if (HasAvailableUpdate)
			{
				return IsLatestBranchUnsupported;
			}
			return false;
		}
	}

	internal static string FormatTime(uint unixTime)
	{
		if (unixTime != 0)
		{
			return DisplayTime.Format(DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime());
		}
		return "—";
	}
}
