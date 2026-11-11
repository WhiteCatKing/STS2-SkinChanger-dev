using System;
using System.Linq;

namespace STS2SkinChanger.Catalog;

internal static class DisplayTextExtensions
{
	public static string CapitalizeWords(this string value)
	{
		return string.Join(' ', from part in value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
			select char.ToUpperInvariant(part[0]) + part.Substring(1, part.Length - 1));
	}
}
