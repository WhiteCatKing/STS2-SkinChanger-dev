using System;
using System.IO;

namespace STS2SkinChanger.Core;

internal static class ProviderRuntimeTypeLookupPolicy
{
	public static Type? TryResolve(string? providerTypeName, Func<string, Type?> lookup)
	{
		ArgumentNullException.ThrowIfNull(lookup, "lookup");
		if (string.IsNullOrWhiteSpace(providerTypeName))
		{
			return null;
		}
		try
		{
			return lookup(providerTypeName.Replace('/', '+'));
		}
		catch (Exception ex) when (((ex is ArgumentException || ex is TypeLoadException || ex is FileLoadException || ex is FileNotFoundException || ex is BadImageFormatException) ? 1 : 0) != 0)
		{
			return null;
		}
	}
}
