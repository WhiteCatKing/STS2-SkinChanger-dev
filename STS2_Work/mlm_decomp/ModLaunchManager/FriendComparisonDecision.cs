namespace ModLaunchManager;

internal sealed record FriendComparisonDecision<T>(FriendModEntry Remote, FriendLocalCandidate<T>? Local, FriendModMatchState State, int LocalPosition);
