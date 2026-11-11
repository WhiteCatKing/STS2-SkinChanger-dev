using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal static class SkinOptionCycle
{
	public static string? NextOption(IReadOnlyList<string> options, string? current, int direction)
	{
		if (options.Count == 0)
		{
			return null;
		}
		for (int i = 0; i < options.Count; i++)
		{
			if (options[i].Equals(current, StringComparison.OrdinalIgnoreCase))
			{
				return options[(i + options.Count + Math.Sign(direction)) % options.Count];
			}
		}
		if (direction >= 0)
		{
			return options[0];
		}
		return options[options.Count - 1];
	}
}
