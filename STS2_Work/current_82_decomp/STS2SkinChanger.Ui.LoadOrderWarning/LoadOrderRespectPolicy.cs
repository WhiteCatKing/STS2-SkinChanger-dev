using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Ui.LoadOrderWarning;

internal static class LoadOrderRespectPolicy
{
	internal static (IReadOnlyList<string> Deliberate, IReadOnlyList<string> Actionable) Resolve(IEnumerable<string> providersAboveSelf, IEnumerable<string> rememberedProvidersAboveSelf)
	{
		HashSet<string> hashSet = new HashSet<string>(rememberedProvidersAboveSelf, StringComparer.OrdinalIgnoreCase);
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		foreach (string item in providersAboveSelf)
		{
			if (hashSet.Remove(item))
			{
				list.Add(item);
			}
			else
			{
				list2.Add(item);
			}
		}
		return (Deliberate: list, Actionable: list2);
	}

	internal static IReadOnlyList<string> Remember(IEnumerable<string> rememberedProvidersAboveSelf, IEnumerable<string> movedProviders)
	{
		return (from id in rememberedProvidersAboveSelf.Concat(movedProviders)
			where !string.IsNullOrWhiteSpace(id)
			select id).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}
}
