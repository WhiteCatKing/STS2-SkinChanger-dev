using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace STS2SkinChanger.Catalog;

internal static class ProviderInstanceIdentityPolicy
{
	private const string SourceMarker = "::source:";

	public static IReadOnlyList<ProviderInstanceIdentity> Resolve(IReadOnlyList<ProviderInstanceCandidate> candidates)
	{
		ProviderInstanceIdentity[] array = new ProviderInstanceIdentity[candidates.Count];
		foreach (IGrouping<string, (ProviderInstanceCandidate, int)> item in candidates.Select((ProviderInstanceCandidate candidate, int index) => (Candidate: candidate, Index: index)).GroupBy<(ProviderInstanceCandidate, int), string>(((ProviderInstanceCandidate Candidate, int Index) entry) => entry.Candidate.ManifestId, StringComparer.OrdinalIgnoreCase))
		{
			(ProviderInstanceCandidate, int)[] array2 = item.ToArray();
			if (array2.Length == 1)
			{
				(ProviderInstanceCandidate, int) tuple = array2[0];
				array[tuple.Item2] = new ProviderInstanceIdentity(tuple.Item1.ManifestId, tuple.Item1.ManifestId, tuple.Item1.DisplayName);
				continue;
			}
			string manifestId = array2[0].Item1.ManifestId;
			Dictionary<int, string> sourceTokens = array2.ToDictionary(((ProviderInstanceCandidate Candidate, int Index) entry) => entry.Index, ((ProviderInstanceCandidate Candidate, int Index) entry) => BuildSourceToken(entry.Candidate.RootPath, entry.Index), EqualityComparer<int>.Default);
			foreach (IGrouping<string, KeyValuePair<int, string>> item2 in from @group in sourceTokens.GroupBy<KeyValuePair<int, string>, string>((KeyValuePair<int, string> pair) => pair.Value, StringComparer.OrdinalIgnoreCase)
				where @group.Count() > 1
				select @group)
			{
				foreach (KeyValuePair<int, string> item3 in item2)
				{
					Dictionary<int, string> dictionary = sourceTokens;
					int key = item3.Key;
					dictionary[key] = dictionary[key] + "-" + ShortHash(NormalizePath(candidates[item3.Key].RootPath).ToLowerInvariant());
				}
			}
			Dictionary<int, int> dictionary2 = array2.OrderBy<(ProviderInstanceCandidate, int), string>(((ProviderInstanceCandidate Candidate, int Index) entry) => sourceTokens[entry.Index], StringComparer.OrdinalIgnoreCase).Select<(ProviderInstanceCandidate, int), (int, int)>(((ProviderInstanceCandidate Candidate, int Index) entry, int index) => (Index: entry.Index, Rank: index + 1)).ToDictionary<(int, int), int, int>(((int Index, int Rank) entry) => entry.Index, ((int Index, int Rank) entry) => entry.Rank);
			HashSet<string> hashSet = (from @group in array2.GroupBy<(ProviderInstanceCandidate, int), string>(((ProviderInstanceCandidate Candidate, int Index) entry) => entry.Candidate.DisplayName, StringComparer.OrdinalIgnoreCase)
				where @group.Count() > 1
				select @group.Key).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			(ProviderInstanceCandidate, int)[] array3 = array2;
			for (int key = 0; key < array3.Length; key++)
			{
				(ProviderInstanceCandidate, int) tuple2 = array3[key];
				string displayName = (hashSet.Contains(tuple2.Item1.DisplayName) ? $"{tuple2.Item1.DisplayName} · {dictionary2[tuple2.Item2]}" : tuple2.Item1.DisplayName);
				array[tuple2.Item2] = new ProviderInstanceIdentity(tuple2.Item1.ManifestId, manifestId + "::source:" + sourceTokens[tuple2.Item2], displayName);
			}
		}
		return array;
	}

	public static string ScopeOptionId(string manifestId, string instanceId, string optionId)
	{
		if (manifestId.Equals(instanceId, StringComparison.OrdinalIgnoreCase))
		{
			return optionId;
		}
		if (optionId.Equals(manifestId, StringComparison.OrdinalIgnoreCase))
		{
			return instanceId;
		}
		if (optionId.StartsWith(manifestId + "::", StringComparison.OrdinalIgnoreCase))
		{
			int length = manifestId.Length;
			return instanceId + optionId.Substring(length, optionId.Length - length);
		}
		return instanceId + "::option:" + optionId;
	}

	public static bool IsOptionSelectionAlias(string manifestId, string instanceId, string currentOptionId, string savedSelectionId)
	{
		if (currentOptionId.Equals(savedSelectionId, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		string text = ToLegacyOptionId(manifestId, instanceId, currentOptionId);
		string text2 = RemoveSourceScope(savedSelectionId);
		if (text.Equals(text2, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		string text3 = manifestId + "::option:";
		if (text2.StartsWith(text3, StringComparison.OrdinalIgnoreCase))
		{
			string text4 = text2;
			int length = text3.Length;
			return text.Equals(text4.Substring(length, text4.Length - length), StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static string ToLegacyOptionId(string manifestId, string instanceId, string optionId)
	{
		if (optionId.Equals(instanceId, StringComparison.OrdinalIgnoreCase))
		{
			return manifestId;
		}
		string text = instanceId + "::option:";
		string text2;
		int length;
		if (optionId.StartsWith(text, StringComparison.OrdinalIgnoreCase))
		{
			text2 = optionId;
			length = text.Length;
			return text2.Substring(length, text2.Length - length);
		}
		if (!optionId.StartsWith(instanceId, StringComparison.OrdinalIgnoreCase))
		{
			return optionId;
		}
		text2 = optionId;
		length = instanceId.Length;
		return manifestId + text2.Substring(length, text2.Length - length);
	}

	private static string RemoveSourceScope(string optionId)
	{
		int num = optionId.IndexOf("::source:", StringComparison.OrdinalIgnoreCase);
		if (num < 0)
		{
			return optionId;
		}
		int startIndex = num + "::source:".Length;
		int num2 = optionId.IndexOf(':', startIndex);
		if (num2 >= 0)
		{
			string text = optionId.Substring(0, num);
			int num3 = num2;
			return text + optionId.Substring(num3, optionId.Length - num3);
		}
		return optionId.Substring(0, num);
	}

	private static string BuildSourceToken(string? rootPath, int fallbackIndex)
	{
		string text = NormalizePath(rootPath);
		if (text.Length == 0)
		{
			return "unknown-" + (fallbackIndex + 1);
		}
		string text2 = text;
		int num = text.LastIndexOf('/') + 1;
		string text3 = text2.Substring(num, text2.Length - num);
		if (text3.Length > 0 && text3.All(char.IsAsciiDigit))
		{
			return text3;
		}
		string text4 = new string((from character in text3.ToLowerInvariant()
			select (!char.IsAsciiLetterOrDigit(character)) ? '-' : character).ToArray()).Trim('-');
		if (text4.Length > 24)
		{
			text4 = text4.Substring(0, 24).TrimEnd('-');
		}
		return ((text4.Length == 0) ? "local" : text4) + "-" + ShortHash(text.ToLowerInvariant());
	}

	private static string NormalizePath(string? path)
	{
		return (path ?? string.Empty).Replace('\\', '/').TrimEnd('/');
	}

	private static string ShortHash(string value)
	{
		return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).Substring(0, 8).ToLowerInvariant();
	}
}
