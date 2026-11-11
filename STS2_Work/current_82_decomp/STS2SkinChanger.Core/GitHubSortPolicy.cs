using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class GitHubSortPolicy
{
	public static ulong[] OrderIds(IEnumerable<ulong> ids, IReadOnlyDictionary<ulong, GitHubSkinEntry> entries, GitHubSort sort, bool reverse = false)
	{
		GitHubSkinEntry value;
		(ulong, string)[] source = ids.Select((ulong id) => (Id: id, Key: entries.TryGetValue(id, out value) ? Key(value, sort) : null)).ToArray();
		return (from item in ((sort != GitHubSort.Name != reverse) ? source.OrderBy(((ulong Id, string Key) item) => item.Key == null).ThenByDescending<(ulong, string), string>(((ulong Id, string Key) item) => item.Key, StringComparer.Ordinal) : source.OrderBy(((ulong Id, string Key) item) => item.Key == null).ThenBy<(ulong, string), string>(((ulong Id, string Key) item) => item.Key, StringComparer.Ordinal)).ThenBy<(ulong, string), ulong>(((ulong Id, string Key) item) => item.Id)
			select item.Id).ToArray();
	}

	private static string? Key(GitHubSkinEntry entry, GitHubSort sort)
	{
		return sort switch
		{
			GitHubSort.Updated => entry.PushedAt.ToString("O"), 
			GitHubSort.Stars => entry.Stars.ToString("D10", CultureInfo.InvariantCulture), 
			_ => entry.Name.ToUpperInvariant(), 
		};
	}

	public static string Metric(GitHubSkinEntry? entry, GitHubSort sort)
	{
		if (entry == null)
		{
			return "—";
		}
		return sort switch
		{
			GitHubSort.Stars => entry.Stars.ToString("N0", CultureInfo.CurrentCulture), 
			GitHubSort.Updated => entry.PushedAt.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), 
			_ => entry.Name, 
		};
	}
}
