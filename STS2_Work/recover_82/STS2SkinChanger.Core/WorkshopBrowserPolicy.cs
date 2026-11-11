using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class WorkshopBrowserPolicy
{
	public const int TargetPageSize = 10;

	public static bool HasRegions(string kind)
	{
		if (kind == "monster" || kind == "event")
		{
			return true;
		}
		return false;
	}

	public static bool VisibleTarget(WorkshopTarget target)
	{
		if (target.Kind == "companion")
		{
			return !target.Target.Equals("osty", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public static string[] Page(string[] ids, int page)
	{
		return ids.Skip(Math.Clamp(page, 0, Math.Max(0, (ids.Length - 1) / 10)) * 10).Take(10).ToArray();
	}

	public static IEnumerable<WorkshopCatalogItem> Filter(IEnumerable<WorkshopCatalogItem> items, string kind, string target, IReadOnlySet<string>? regionMembers)
	{
		return items.Where((WorkshopCatalogItem item) => item.Targets.Any((WorkshopTarget t) => VisibleTarget(t) && (kind.Length == 0 || (t.Kind == kind && (target.Length == 0 || t.Target.Equals(target, StringComparison.OrdinalIgnoreCase)) && (regionMembers == null || regionMembers.Contains(t.Target))))));
	}

	public static ulong[] FilterIds(string json, string kind, string target, string[] members)
	{
		return (from i in Filter(WorkshopCatalogPolicy.Parse(json), kind, target, members.ToHashSet<string>(StringComparer.OrdinalIgnoreCase))
			select i.Id).ToArray();
	}

	public static float MarqueeOffset(float overflow, double elapsed)
	{
		if (overflow <= 0f || !float.IsFinite(overflow) || !double.IsFinite(elapsed))
		{
			return 0f;
		}
		double num = (double)overflow / 36.0;
		double num2 = Math.Max(0.0, elapsed) % (2.0 * num + 1.6);
		if (num2 <= 0.8)
		{
			return 0f;
		}
		if (num2 <= num + 0.8)
		{
			return (float)((num2 - 0.8) * 36.0);
		}
		if (num2 <= num + 1.6)
		{
			return overflow;
		}
		return (float)Math.Max(0.0, (double)overflow - (num2 - num - 1.6) * 36.0);
	}
}
