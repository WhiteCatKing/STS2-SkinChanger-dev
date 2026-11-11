using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace STS2SkinChanger.Core;

internal sealed class ScopedMonsterSelectionSnapshot
{
	private Dictionary<string, HashSet<string>> _selectedMonsterIdsByProvider = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

	public void Replace(IReadOnlyDictionary<string, IReadOnlyCollection<string>> selectedMonsterIdsByProvider)
	{
		Dictionary<string, HashSet<string>> value = selectedMonsterIdsByProvider.ToDictionary<KeyValuePair<string, IReadOnlyCollection<string>>, string, HashSet<string>>((KeyValuePair<string, IReadOnlyCollection<string>> pair) => pair.Key, (KeyValuePair<string, IReadOnlyCollection<string>> pair) => pair.Value.ToHashSet<string>(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
		Volatile.Write(ref _selectedMonsterIdsByProvider, value);
	}

	public bool IsSelected(string providerId, string monsterId)
	{
		if (Volatile.Read(in _selectedMonsterIdsByProvider).TryGetValue(providerId, out HashSet<string> value))
		{
			return value.Contains(monsterId);
		}
		return false;
	}
}
