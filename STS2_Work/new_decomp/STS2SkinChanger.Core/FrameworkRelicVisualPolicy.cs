using System;
using System.Collections.Generic;
using System.Linq;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class FrameworkRelicVisualPolicy
{
	public static FrameworkRelicVisualPlan? Resolve(IEnumerable<FrameworkModelSkinContract> relics, string targetModelName, bool largeIcon)
	{
		FrameworkModelSkinContract frameworkModelSkinContract = relics.LastOrDefault((FrameworkModelSkinContract candidate) => candidate.TargetModelName.Equals(targetModelName, StringComparison.Ordinal));
		if (frameworkModelSkinContract == null)
		{
			return null;
		}
		string key = (largeIcon ? "BigIconPath" : "PackedIconPath");
		string key2 = (largeIcon ? "PackedIconPath" : "BigIconPath");
		if (!frameworkModelSkinContract.Resources.TryGetValue(key, out string value) && !frameworkModelSkinContract.Resources.TryGetValue(key2, out value))
		{
			return null;
		}
		string value2;
		string outlinePath = ((!largeIcon && frameworkModelSkinContract.Resources.TryGetValue("PackedIconOutlinePath", out value2)) ? value2 : null);
		return new FrameworkRelicVisualPlan(value, outlinePath);
	}
}
