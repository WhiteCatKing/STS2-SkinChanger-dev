using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal sealed class CanonicalResourceOwnershipTracker
{
	private readonly Dictionary<string, string> _owners = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public bool RequiresActivation(string ownerId, IEnumerable<string> canonicalPaths)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(ownerId, "ownerId");
		ArgumentNullException.ThrowIfNull(canonicalPaths, "canonicalPaths");
		string value;
		return canonicalPaths.Where((string path) => !string.IsNullOrWhiteSpace(path)).Distinct<string>(StringComparer.OrdinalIgnoreCase).Any((string path) => !_owners.TryGetValue(path, out value) || !value.Equals(ownerId, StringComparison.OrdinalIgnoreCase));
	}

	public void MarkActivated(string ownerId, IEnumerable<string> canonicalPaths)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(ownerId, "ownerId");
		ArgumentNullException.ThrowIfNull(canonicalPaths, "canonicalPaths");
		foreach (string item in canonicalPaths.Where((string path) => !string.IsNullOrWhiteSpace(path)).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			_owners[item] = ownerId;
		}
	}

	public void Reset()
	{
		_owners.Clear();
	}
}
