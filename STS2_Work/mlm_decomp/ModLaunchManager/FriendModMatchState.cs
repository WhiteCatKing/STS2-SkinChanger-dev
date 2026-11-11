namespace ModLaunchManager;

internal enum FriendModMatchState
{
	Match,
	Missing,
	Disabled,
	VersionMismatch,
	ContentMismatch,
	OrderMismatch,
	Checking
}
