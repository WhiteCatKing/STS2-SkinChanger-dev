namespace ModLaunchManager;

internal enum ConfigCloudStatus
{
	Unknown,
	Off,
	SteamUnavailable,
	CloudDisabled,
	WaitingForGameSaves,
	Synced,
	Failed
}
