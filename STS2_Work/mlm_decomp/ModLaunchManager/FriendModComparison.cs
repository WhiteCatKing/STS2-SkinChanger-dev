using System.Collections.Generic;
using System.Linq;

namespace ModLaunchManager;

internal sealed class FriendModComparison
{
	internal required IReadOnlyList<FriendModComparisonRow> Rows { get; init; }

	internal required IReadOnlyList<LaunchEntry> ExtraEnabled { get; init; }

	internal required bool ExactOrder { get; init; }

	internal int MissingCount => Rows.Count((FriendModComparisonRow row) => row.State == FriendModMatchState.Missing);

	internal int DisabledCount => Rows.Count((FriendModComparisonRow row) => row.State == FriendModMatchState.Disabled);

	internal int VersionMismatchCount => Rows.Count((FriendModComparisonRow row) => row.State == FriendModMatchState.VersionMismatch);

	internal int ContentMismatchCount => Rows.Count((FriendModComparisonRow row) => row.State == FriendModMatchState.ContentMismatch);

	internal int OrderMismatchCount => Rows.Count((FriendModComparisonRow row) => row.State == FriendModMatchState.OrderMismatch);

	internal int CheckingCount => Rows.Count((FriendModComparisonRow row) => row.State == FriendModMatchState.Checking);

	internal bool ExactMatch
	{
		get
		{
			if (MissingCount == 0 && DisabledCount == 0 && VersionMismatchCount == 0 && ContentMismatchCount == 0 && CheckingCount == 0 && ExactOrder)
			{
				return ExtraEnabled.Count == 0;
			}
			return false;
		}
	}
}
