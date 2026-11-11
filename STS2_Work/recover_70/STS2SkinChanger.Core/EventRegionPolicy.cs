using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class EventRegionPolicy
{
	public const string Global = "__global__";

	public const string Shared = "__shared__";

	public const string Other = "__other__";

	public static IReadOnlyDictionary<string, string[]> Group(IEnumerable<string> available, IReadOnlyDictionary<string, IEnumerable<string>> acts, IEnumerable<string> sharedEvents)
	{
		string[] array = available.Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		HashSet<string> valid = array.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> shared = sharedEvents.Where(valid.Contains).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> assigned = new HashSet<string>(shared, StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
		if (array.Length != 0)
		{
			dictionary["__global__"] = array;
		}
		foreach (KeyValuePair<string, IEnumerable<string>> act in acts)
		{
			act.Deconstruct(out var key, out var value);
			string key2 = key;
			string[] array2 = value.Where((string e) => valid.Contains(e) && !shared.Contains(e)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
			if (array2.Length != 0)
			{
				dictionary[key2] = array2;
				assigned.UnionWith(array2);
			}
		}
		if (shared.Count > 0)
		{
			dictionary["__shared__"] = array.Where(shared.Contains).ToArray();
		}
		string[] array3 = array.Where((string e) => !assigned.Contains(e)).ToArray();
		if (array3.Length != 0)
		{
			dictionary["__other__"] = array3;
		}
		return dictionary;
	}
}
