using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class WorkshopSortPolicy
{
	public static ulong[] OrderIds(IEnumerable<ulong> ids, IReadOnlyDictionary<ulong, WorkshopDetails> details, WorkshopSort sort, bool reverse = false, IReadOnlyDictionary<ulong, int>? replies = null)
	{
		return (from id in ids
			select (Id: id, Value: Key(id)) into item
			orderby !item.Value.HasValue, (!reverse) ? (-item.Value) : item.Value, item.Id
			select item.Id).ToArray();
		decimal? Key(ulong id)
		{
			if (sort != WorkshopSort.LatestReply)
			{
				return Value(details.GetValueOrDefault(id), sort);
			}
			return Math.Max(-1, replies?.GetValueOrDefault(id, -1) ?? (-1));
		}
	}

	private static decimal? Value(WorkshopDetails? detail, WorkshopSort sort)
	{
		decimal? result;
		switch (sort)
		{
		case WorkshopSort.Subscriptions:
			return detail?.Subscriptions;
		case WorkshopSort.LifetimeSubscriptions:
			return detail?.LifetimeSubscriptions;
		case WorkshopSort.Rating:
		{
			float? num = detail?.Score;
			if (num.HasValue)
			{
				float valueOrDefault = num.GetValueOrDefault();
				if (float.IsFinite(valueOrDefault))
				{
					result = (decimal)Math.Clamp(valueOrDefault, 0f, 1f);
					goto IL_00ff;
				}
			}
			result = null;
			goto IL_00ff;
		}
		case WorkshopSort.Published:
			return detail?.Created;
		case WorkshopSort.Updated:
			return detail?.Updated;
		case WorkshopSort.Comments:
			return detail?.Comments;
		case WorkshopSort.Favorites:
			return detail?.Favorites;
		default:
			{
				return null;
			}
			IL_00ff:
			return result;
		}
	}

	public static string Metric(WorkshopDetails? detail, WorkshopSort sort, string language, int? reply = null)
	{
		if (sort == WorkshopSort.LatestReply)
		{
			if (!reply.HasValue || reply.GetValueOrDefault() < 0)
			{
				return "—";
			}
			return string.Format(CultureInfo.CurrentCulture, WorkshopDetailsText.ForLanguage(language, WorkshopDetailsTextKey.ReplyNumber), reply.Value);
		}
		decimal? num = Value(detail, sort);
		if (num.HasValue)
		{
			decimal valueOrDefault = num.GetValueOrDefault();
			WorkshopDetailsTextKey key = (WorkshopDetailsTextKey)(7 + sort);
			CultureInfo currentCulture;
			string format;
			object arg;
			object arg2;
			bool flag;
			switch (sort)
			{
			case WorkshopSort.Rating:
			{
				currentCulture = CultureInfo.CurrentCulture;
				format = WorkshopDetailsText.ForLanguage(language, key);
				arg = valueOrDefault * 100m;
				uint? num2 = detail?.VotesUp;
				if (num2.HasValue)
				{
					uint valueOrDefault2 = num2.GetValueOrDefault();
					num2 = detail.VotesDown;
					if (num2.HasValue)
					{
						uint valueOrDefault3 = num2.GetValueOrDefault();
						arg2 = ((ulong)valueOrDefault2 + (ulong)valueOrDefault3).ToString("N0", CultureInfo.CurrentCulture);
						goto IL_00ec;
					}
				}
				arg2 = "—";
				goto IL_00ec;
			}
			case WorkshopSort.Published:
			case WorkshopSort.Updated:
				flag = true;
				break;
			default:
				{
					flag = false;
					break;
				}
				IL_00ec:
				return string.Format(currentCulture, format, arg, arg2);
			}
			string arg3 = (flag ? DateTimeOffset.FromUnixTimeSeconds((long)valueOrDefault).ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : valueOrDefault.ToString("N0", CultureInfo.CurrentCulture));
			return string.Format(CultureInfo.CurrentCulture, WorkshopDetailsText.ForLanguage(language, key), arg3);
		}
		return "—";
	}
}
