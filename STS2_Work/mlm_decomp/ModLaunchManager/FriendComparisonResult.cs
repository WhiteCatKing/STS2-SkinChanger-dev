using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed class FriendComparisonResult<T>
{
	internal required IReadOnlyList<FriendComparisonDecision<T>> Rows { get; init; }

	internal required IReadOnlyList<FriendLocalCandidate<T>> ExtraEnabled { get; init; }

	internal required bool ExactOrder { get; init; }
}
