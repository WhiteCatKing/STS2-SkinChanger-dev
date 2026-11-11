namespace STS2SkinChanger.Core;

internal static class WorkshopItemActions
{
	public static WorkshopTextKey? Primary(bool subscribed, bool active, bool installed, bool knownRestart, WorkshopTextKey? state, bool busy)
	{
		if (!subscribed)
		{
			return WorkshopTextKey.Subscribe;
		}
		if (busy)
		{
			return WorkshopTextKey.Download;
		}
		if (active || state == WorkshopTextKey.Ready)
		{
			return null;
		}
		if (installed && (state == WorkshopTextKey.Restart || (!state.HasValue && knownRestart)))
		{
			return WorkshopTextKey.Restart;
		}
		return WorkshopTextKey.Download;
	}

	public static WorkshopTextKey? Status(WorkshopTextKey? state)
	{
		bool flag;
		if (state.HasValue)
		{
			WorkshopTextKey valueOrDefault = state.GetValueOrDefault();
			if ((uint)(valueOrDefault - 6) <= 1u)
			{
				flag = true;
				goto IL_001d;
			}
		}
		flag = false;
		goto IL_001d;
		IL_001d:
		if (!flag)
		{
			return state;
		}
		return null;
	}

	public static string ItemUrl(ulong id, bool client)
	{
		if (client)
		{
			return $"steam://url/CommunityFilePage/{id}";
		}
		return $"https://steamcommunity.com/sharedfiles/filedetails/?id={id}";
	}
}
