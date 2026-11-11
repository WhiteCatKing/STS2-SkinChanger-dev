using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class AliasedDependencyCachePolicy
{
	public static bool CanReuseExternalDependencies(IEnumerable<AliasedDependencyReference> references, IEnumerable<string> aliasedSourcePaths)
	{
		HashSet<string> aliases = aliasedSourcePaths.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (AliasedDependencyReference reference in references)
		{
			if (reference.ResourcePaths.Count != 0 && (!reference.IsRewritable || reference.ResourcePaths.Any((string path) => !aliases.Contains(path))))
			{
				return false;
			}
		}
		return true;
	}
}
