namespace ModLaunchManager;

internal sealed record FriendModComparisonRow(FriendModEntry Remote, LaunchEntry? Local, FriendModMatchState State, int LocalPosition);
