using System;

namespace ModLaunchManager;

internal sealed record FriendLocalCandidate<T>(T Value, string Id, bool Enabled, string Version, ulong WorkshopId, Func<FriendModEntry, bool?> ContentMatches)
{
	internal FriendLocalCandidate(T value, string id, bool enabled, string version, ulong workshopId, Func<FriendModEntry, bool> contentMatches)
		: this(value, id, enabled, version, workshopId, (Func<FriendModEntry, bool?>)((FriendModEntry remote) => contentMatches(remote)))
	{
	}
}
