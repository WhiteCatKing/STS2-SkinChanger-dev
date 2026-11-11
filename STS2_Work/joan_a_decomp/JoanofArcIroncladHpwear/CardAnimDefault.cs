using System;

namespace JoanofArcIroncladHpwear;

public static class CardAnimDefault
{
	public static string? Resolve(string? requested)
	{
		if (string.IsNullOrEmpty(requested))
		{
			return "Cast";
		}
		if (requested.Equals("none", StringComparison.OrdinalIgnoreCase))
		{
			return "Cast";
		}
		return requested;
	}

	public static string ResolveMissingForCard(string cardTypeName)
	{
		if (!string.Equals(cardTypeName, "Omnislice", StringComparison.Ordinal))
		{
			return "Cast";
		}
		return "Attack";
	}
}
