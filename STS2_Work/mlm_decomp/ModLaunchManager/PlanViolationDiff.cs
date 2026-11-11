using System.Collections.Generic;
using System.Linq;

namespace ModLaunchManager;

internal static class PlanViolationDiff
{
	internal static PlanViolation? FirstIntroduced(IEnumerable<PlanViolation> after, IReadOnlyCollection<string> before)
	{
		foreach (PlanViolation item in after)
		{
			if (!before.Contains(item.Key))
			{
				return item;
			}
		}
		return null;
	}
}
