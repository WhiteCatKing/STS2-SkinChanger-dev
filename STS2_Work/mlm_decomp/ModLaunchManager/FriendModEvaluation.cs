using System;
using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed class FriendModEvaluation
{
	private readonly Dictionary<string, bool?> _contentMatches = new Dictionary<string, bool?>(StringComparer.OrdinalIgnoreCase);

	private readonly bool _current;

	internal bool Checking { get; private set; }

	internal FriendModEvaluation(bool current = false)
	{
		_current = current;
	}

	internal bool? ContentMatches(LaunchEntry local, FriendModEntry remote)
	{
		string key = local.Mod.path + "\n" + remote.ContentHash;
		if (!_contentMatches.TryGetValue(key, out var value))
		{
			value = FriendFileChecks.Matches(local.Mod.path, remote.ContentHash, _current);
			_contentMatches[key] = value;
		}
		Checking |= !value.HasValue;
		return value;
	}
}
