using System;

namespace STS2SkinChanger.Core;

internal static class WorkshopLoadTags
{
	public static readonly string[] FilterOptions = new string[2] { "restart", "hot" };

	public static string Classify(bool knownRestart, WorkshopTextKey? state, WorkshopLoadReason reason)
	{
		if (state == WorkshopTextKey.Ready)
		{
			return "hot";
		}
		if (!(state == WorkshopTextKey.Restart || knownRestart))
		{
			return "hot";
		}
		return "restart";
	}

	public static bool Matches(string filter, string tag)
	{
		if (filter.Length != 0)
		{
			return filter == tag;
		}
		return true;
	}

	public static string Name(string tag)
	{
		if (!(tag == "restart"))
		{
			if (tag == "hot")
			{
				return WorkshopCommunityText.Get(WorkshopCommunityTextKey.Hot);
			}
			throw new ArgumentOutOfRangeException("tag");
		}
		return WorkshopText.Get(WorkshopTextKey.Restart);
	}
}
