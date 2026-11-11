using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace STS2SkinChanger.Core;

internal static class WorkshopDiscussionSource
{
	public const string Topic = "592940620292752301";

	public const string Url = "https://steamcommunity.com/workshop/filedetails/discussion/3787302680/592940620292752301/";

	private static readonly HttpClient Client = new HttpClient(new HttpClientHandler
	{
		AllowAutoRedirect = false
	})
	{
		Timeout = TimeSpan.FromSeconds(25L)
	};

	private static readonly Regex Initialization = Pattern("InitializeCommentThread\\s*\\(\\s*\"ForumTopic\"[^\\{]*\\{");

	private static readonly Regex Divs = Pattern("</?div\\b[^>]*>");

	private static readonly Regex Classes = Pattern("\\bclass\\s*=\\s*[\"']([^\"']*)[\"']");

	private static readonly Regex Tags = Pattern("<[^>]*>");

	private static readonly Regex Scripts = Pattern("<(script|style)\\b[^>]*>.*?</\\1\\s*>");

	private static readonly Regex CommentId = Pattern("\\bid\\s*=\\s*[\"']comment_content_([0-9]+)[\"']");

	private static readonly Regex Breaks = Pattern("<br\\b[^>]*>|</(?:div|p|pre)\\s*>");

	private static Regex Pattern(string pattern)
	{
		return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2L));
	}

	public static WorkshopDiscussionPage Parse(string html)
	{
		if (html.Length > 2097152)
		{
			throw new InvalidDataException("Discussion page too large.");
		}
		string text = Scripts.Matches(html).Cast<Match>().FirstOrDefault((Match m) => m.Groups[1].Value.Equals("script", StringComparison.OrdinalIgnoreCase) && Initialization.IsMatch(m.Value))?.Value;
		if (text == null)
		{
			throw new InvalidDataException("Steam discussion is unavailable or changed format.");
		}
		Match match = Initialization.Match(text);
		Encoding uTF = Encoding.UTF8;
		string text2 = text;
		int num = match.Index + match.Length - 1;
		Utf8JsonReader reader = new Utf8JsonReader(uTF.GetBytes(text2.Substring(num, text2.Length - num)));
		using JsonDocument jsonDocument = JsonDocument.ParseValue(ref reader);
		JsonElement rootElement = jsonDocument.RootElement;
		if (rootElement.GetProperty("feature2").GetString() != "592940620292752301")
		{
			throw new InvalidDataException("Unexpected discussion topic.");
		}
		int @int = rootElement.GetProperty("total_count").GetInt32();
		int int2 = rootElement.GetProperty("start").GetInt32();
		int int3 = rootElement.GetProperty("pagesize").GetInt32();
		bool flag = ((@int < 0 || @int > 30000) ? true : false);
		bool flag2 = flag || int2 < 0;
		if (!flag2)
		{
			bool flag3 = ((int3 < 1 || int3 > 100) ? true : false);
			flag2 = flag3;
		}
		if (flag2 || int2 > @int)
		{
			throw new InvalidDataException("Invalid discussion pagination.");
		}
		List<WorkshopCodePost> list = new List<WorkshopCodePost>();
		html = Scripts.Replace(html, "");
		int num2 = 0;
		int num3 = -1;
		int num4 = 0;
		string url = "https://steamcommunity.com/workshop/filedetails/discussion/3787302680/592940620292752301/";
		int reply = 0;
		int num5 = int2 / int3 + 1;
		foreach (Match item in Divs.Matches(html))
		{
			bool flag4 = item.Value.StartsWith("</", StringComparison.Ordinal);
			if (num3 >= 0)
			{
				num2 += ((!flag4) ? 1 : (-1));
				if (num2 == 0)
				{
					Regex tags = Tags;
					Regex breaks = Breaks;
					string text3 = html;
					num = num3;
					list.Add(new WorkshopCodePost(WebUtility.HtmlDecode(tags.Replace(breaks.Replace(text3.Substring(num, item.Index - num), "\n"), "")), url, num5, reply));
					num3 = -1;
				}
			}
			else
			{
				if (flag4)
				{
					continue;
				}
				string[] source = Classes.Match(item.Value).Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
				if (source.Contains("forum_op") || source.Contains("commentthread_comment_text"))
				{
					bool num6 = source.Contains("commentthread_comment_text");
					if (num6)
					{
						num4++;
					}
					reply = (num6 ? (int2 + num4) : 0);
					string value = CommentId.Match(item.Value).Groups[1].Value;
					url = "https://steamcommunity.com/workshop/filedetails/discussion/3787302680/592940620292752301/" + ((num5 > 1) ? ("?ctp=" + num5) : "") + ((value.Length > 0) ? ("#c" + value) : "");
					num3 = item.Index + item.Length;
					num2 = 1;
				}
			}
		}
		if (num3 >= 0 || list.Count == 0 || num4 != Math.Min(int3, @int - int2))
		{
			throw new InvalidDataException("Incomplete discussion body.");
		}
		return new WorkshopDiscussionPage(@int, int2, int3, list.ToArray());
	}

	public static async Task<WorkshopCodePost[]> ReadAllPosts(Func<Uri, CancellationToken, Task<string>> fetch, IProgress<(int Current, int Total)>? progress, CancellationToken token)
	{
		WorkshopDiscussionPage first = Parse(await fetch(new Uri("https://steamcommunity.com/workshop/filedetails/discussion/3787302680/592940620292752301/"), token));
		if (first.Start != 0)
		{
			throw new InvalidDataException("Discussion did not start at page one.");
		}
		int pages = Math.Max(1, (first.Total + first.PageSize - 1) / first.PageSize);
		List<WorkshopCodePost> posts = new List<WorkshopCodePost>(first.Sources);
		progress?.Report((1, pages));
		for (int page = 2; page <= pages; page++)
		{
			token.ThrowIfCancellationRequested();
			WorkshopDiscussionPage workshopDiscussionPage = Parse(await fetch(new Uri("https://steamcommunity.com/workshop/filedetails/discussion/3787302680/592940620292752301/?ctp=" + page), token));
			if (workshopDiscussionPage.Total != first.Total || workshopDiscussionPage.PageSize != first.PageSize || workshopDiscussionPage.Start != (page - 1) * first.PageSize)
			{
				throw new InvalidDataException("Discussion changed during pagination; retain the last complete catalog.");
			}
			posts.AddRange(workshopDiscussionPage.Sources.Where((WorkshopCodePost p) => p.Reply > 0));
			if (((IEnumerable<WorkshopCodePost>)posts).Sum((Func<WorkshopCodePost, long>)((WorkshopCodePost p) => p.Text.Length)) > 33554432)
			{
				throw new InvalidDataException("Discussion exceeds refresh budget.");
			}
			progress?.Report((page, pages));
		}
		return posts.ToArray();
	}

	internal static async Task<string> Fetch(Uri uri, CancellationToken token)
	{
		if (uri.Scheme != "https" || uri.Host != "steamcommunity.com" || uri.AbsolutePath != new Uri("https://steamcommunity.com/workshop/filedetails/discussion/3787302680/592940620292752301/").AbsolutePath)
		{
			throw new InvalidDataException("Unexpected discussion URL.");
		}
		using HttpResponseMessage response = await Client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token);
		response.EnsureSuccessStatusCode();
		if (response.Content.Headers.ContentLength > 2097152)
		{
			throw new InvalidDataException("Discussion response too large.");
		}
		using Stream input = await response.Content.ReadAsStreamAsync(token);
		using MemoryStream output = new MemoryStream();
		byte[] buffer = new byte[8192];
		int num;
		while ((num = await input.ReadAsync(buffer, token)) > 0)
		{
			if (output.Length + num > 2097152)
			{
				throw new InvalidDataException("Discussion response too large.");
			}
			output.Write(buffer, 0, num);
		}
		return Encoding.UTF8.GetString(output.ToArray());
	}
}
