using System;
using Steamworks;

namespace STS2SkinChanger.Core;

internal static class WorkshopMetadataReader
{
	public static WorkshopDetails Read(SteamUGCDetails_t item, string preview, Func<EItemStatistic, ulong?> statistic)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		return new WorkshopDetails(((SteamUGCDetails_t)(ref item)).m_rgchTitle, preview)
		{
			Subscriptions = statistic((EItemStatistic)0),
			LifetimeSubscriptions = statistic((EItemStatistic)3),
			Favorites = statistic((EItemStatistic)1),
			Comments = statistic((EItemStatistic)10),
			Score = (float.IsFinite(item.m_flScore) ? new float?(Math.Clamp(item.m_flScore, 0f, 1f)) : ((float?)null)),
			VotesUp = item.m_unVotesUp,
			VotesDown = item.m_unVotesDown,
			Created = item.m_rtimeCreated,
			Updated = item.m_rtimeUpdated
		};
	}
}
