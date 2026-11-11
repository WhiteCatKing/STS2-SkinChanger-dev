using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLaunchManager;

internal sealed class LocalModFileLedger
{
	internal readonly record struct Fingerprint(long Length, long LastWriteUtcTicks);

	private readonly HashSet<string> _known = new HashSet<string>(LocalModChanges.PathComparer);

	private readonly Dictionary<string, Fingerprint> _unreadable = new Dictionary<string, Fingerprint>(LocalModChanges.PathComparer);

	internal void Reset(IEnumerable<string> present)
	{
		_known.Clear();
		_unreadable.Clear();
		_known.UnionWith(present);
	}

	internal void MarkKnown(string file)
	{
		_known.Add(file);
		_unreadable.Remove(file);
	}

	internal LocalModScanResult Scan(IReadOnlyCollection<string> present, bool holdRemovals, Func<string, Fingerprint?> fingerprintOf, Func<string, bool> tryRead, Func<IReadOnlyList<string>, bool> apply)
	{
		HashSet<string> presentSet = new HashSet<string>(present, LocalModChanges.PathComparer);
		List<string> list = (holdRemovals ? new List<string>() : _known.Where((string file) => !presentSet.Contains(file)).ToList());
		foreach (string item in _unreadable.Keys.Where((string file) => !presentSet.Contains(file)).ToList())
		{
			_unreadable.Remove(item);
		}
		List<string> list2 = new List<string>();
		foreach (string item2 in present)
		{
			if (_known.Contains(item2))
			{
				continue;
			}
			Fingerprint? fingerprint = fingerprintOf(item2);
			if (!fingerprint.HasValue)
			{
				continue;
			}
			Fingerprint valueOrDefault = fingerprint.GetValueOrDefault();
			if (!_unreadable.TryGetValue(item2, out var value) || !(value == valueOrDefault))
			{
				if (!tryRead(item2))
				{
					_unreadable[item2] = valueOrDefault;
					continue;
				}
				_unreadable.Remove(item2);
				list2.Add(item2);
			}
		}
		if (list2.Count == 0 && list.Count == 0)
		{
			return LocalModScanResult.Unchanged;
		}
		if (!apply(list))
		{
			return LocalModScanResult.NotApplied;
		}
		_known.ExceptWith(list);
		_known.UnionWith(list2);
		return LocalModScanResult.Applied;
	}
}
