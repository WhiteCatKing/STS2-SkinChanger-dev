using System;
using System.Collections.Generic;
using System.Linq;

namespace ModLaunchManager;

internal static class FriendImportSelection
{
	internal static FriendModList GameplayOnly(FriendModList source, Func<string, IEnumerable<string>>? localDependencies = null)
	{
		Dictionary<string, FriendModEntry> dictionary = source.Mods.ToDictionary<FriendModEntry, string>((FriendModEntry m) => m.Id, StringComparer.OrdinalIgnoreCase);
		HashSet<string> selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		Queue<string> queue = new Queue<string>(from m in source.Mods
			where m.AffectsGameplay
			select m.Id);
		string result;
		while (queue.TryDequeue(out result))
		{
			if (!dictionary.TryGetValue(result, out var value) || !selected.Add(result))
			{
				continue;
			}
			IEnumerable<string> dependencies = value.Dependencies;
			foreach (string item in dependencies ?? localDependencies?.Invoke(result) ?? Array.Empty<string>())
			{
				queue.Enqueue(item);
			}
		}
		return new FriendModList
		{
			ProtocolVersion = source.ProtocolVersion,
			GameVersion = source.GameVersion,
			ManagerVersion = source.ManagerVersion,
			Mods = source.Mods.Where((FriendModEntry m) => selected.Contains(m.Id)).ToList()
		};
	}
}
