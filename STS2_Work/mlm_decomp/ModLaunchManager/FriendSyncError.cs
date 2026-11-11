using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLaunchManager;

internal sealed record FriendSyncError(string Key, IReadOnlyDictionary<string, object?> Arguments)
{
	internal static FriendSyncError Create(string key, params (string Name, object? Value)[] arguments)
	{
		return new FriendSyncError(key, arguments.ToDictionary<(string, object), string, object>(((string Name, object Value) pair) => pair.Name, ((string Name, object Value) pair) => pair.Value, StringComparer.Ordinal));
	}
}
