namespace ModLaunchManager;

internal sealed record OnlineFriendMods(ulong SteamId, string PersonaName, FriendPresenceState State, FriendModList? List, string Header, FriendSyncError? Error);
