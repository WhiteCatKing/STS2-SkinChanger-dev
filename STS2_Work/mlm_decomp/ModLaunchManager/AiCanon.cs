using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace ModLaunchManager;

internal sealed record AiCanon(string Work, IReadOnlyDictionary<string, string> Names)
{
	internal const int MaxNames = 400;

	internal static readonly AiCanon None = new AiCanon("", new Dictionary<string, string>());

	private static readonly char[] Plain = new char[9] { '[', ']', '{', '}', '<', '>', '#', '\r', '\n' };

	internal static AiCanon Parse(string reply, IReadOnlyCollection<string> listed)
	{
		int num = reply.IndexOf('{');
		int num2 = reply.LastIndexOf('}');
		if (num < 0 || num2 <= num)
		{
			throw new TranslationFileException("response")
			{
				ReplyIssue = "json"
			};
		}
		int num3 = num;
		using JsonDocument jsonDocument = JsonDocument.Parse(reply.Substring(num3, num2 + 1 - num3));
		if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object || !jsonDocument.RootElement.TryGetProperty("work", out var value) || value.ValueKind != JsonValueKind.String)
		{
			throw new TranslationFileException("response")
			{
				ReplyIssue = "shape"
			};
		}
		string text = value.GetString().Trim();
		num3 = text.Length;
		bool flag = ((num3 > 200 || num3 == 0) ? true : false);
		if (flag || text.IndexOfAny(Plain) >= 0)
		{
			return None;
		}
		HashSet<string> hashSet = listed.ToHashSet<string>(StringComparer.Ordinal);
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		if (jsonDocument.RootElement.TryGetProperty("names", out var value2) && value2.ValueKind == JsonValueKind.Object)
		{
			foreach (JsonProperty item in value2.EnumerateObject())
			{
				if (!hashSet.Contains(item.Name) || item.Value.ValueKind != JsonValueKind.String)
				{
					continue;
				}
				string text2 = item.Value.GetString().Trim();
				if (text2 != null)
				{
					num3 = text2.Length;
					if (num3 > 0 && num3 <= 60 && text2 != item.Name && text2.IndexOfAny(Plain) < 0 && !text2.Contains("__MLM_KEEP_", StringComparison.Ordinal) && dictionary.Count < 400)
					{
						dictionary[item.Name] = text2;
					}
				}
			}
		}
		return new AiCanon(text, dictionary);
	}

	internal static AiCanon Merge(IEnumerable<AiCanon> parts)
	{
		string text = "";
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		foreach (AiCanon part in parts)
		{
			if (text.Length == 0)
			{
				text = part.Work;
			}
			foreach (KeyValuePair<string, string> name in part.Names)
			{
				dictionary.TryAdd(name.Key, name.Value);
			}
		}
		if (text.Length != 0)
		{
			return new AiCanon(text, dictionary);
		}
		return None;
	}

	[CompilerGenerated]
	private AiCanon(AiCanon original)
	{
		Work = original.Work;
		Names = original.Names;
	}
}
