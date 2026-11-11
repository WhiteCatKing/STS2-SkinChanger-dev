using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class PromotedPackOverlayPolicy
{
	internal static IReadOnlyList<string> FindBaselinePathsShadowingSelectedRemaps(IEnumerable<string> baselinePaths, IEnumerable<string> selectedProviderOverlayPaths)
	{
		ArgumentNullException.ThrowIfNull(baselinePaths, "baselinePaths");
		ArgumentNullException.ThrowIfNull(selectedProviderOverlayPaths, "selectedProviderOverlayPaths");
		HashSet<string> selectedRemaps = selectedProviderOverlayPaths.Where((string path) => path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		return baselinePaths.Where((string path) => selectedRemaps.Contains(path + ".remap")).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}
}
