using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace STS2SkinChanger.Core;

internal static class GitHubSkinPolicy
{
	public const string SelfRepo = "gurio-wine/STS2-SkinChanger";

	public const string Topic = "sts2-sc-mod";

	public const long MaxAssetBytes = 134217728L;

	public const int SearchPageSize = 100;

	public const int MaxSearchPages = 3;

	private const int MaxJsonBytes = 4194304;

	private static readonly string[] Hosts = new string[7] { "api.github.com", "github.com", "objects.githubusercontent.com", "release-assets.githubusercontent.com", "codeload.github.com", "opengraph.githubassets.com", "raw.githubusercontent.com" };

	public static Uri SearchUri(int page)
	{
		return new Uri($"https://api.github.com/search/repositories?q=topic%3A{"sts2-sc-mod"}&sort=updated&order=desc&per_page={100}&page={Math.Clamp(page, 1, 3)}");
	}

	public static Uri LatestReleaseUri(GitHubRepoRef repo)
	{
		return new Uri("https://api.github.com/repos/" + repo.FullName + "/releases/latest");
	}

	public static Uri InfoUri(GitHubRepoRef repo)
	{
		return new Uri("https://raw.githubusercontent.com/" + repo.FullName + "/HEAD/sc.info");
	}

	public static Uri ReadmeUri(GitHubRepoRef repo)
	{
		return new Uri("https://api.github.com/repos/" + repo.FullName + "/readme");
	}

	public static string CoverUrl(GitHubRepoRef repo)
	{
		return "https://opengraph.githubassets.com/1/" + repo.FullName;
	}

	public static bool IsAllowedHost(string? host)
	{
		if (host != null)
		{
			return Hosts.Contains<string>(host, StringComparer.OrdinalIgnoreCase);
		}
		return false;
	}

	public static string NormalizeDigest(string? value)
	{
		string text = (value ?? "").Trim();
		if (text.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
		{
			string text2 = text;
			int length = "sha256:".Length;
			text = text2.Substring(length, text2.Length - length);
		}
		if (text.Length != 64 || !text.All(char.IsAsciiHexDigit))
		{
			return "";
		}
		return text.ToLowerInvariant();
	}

	public static bool TryParseFullName(string? fullName, out GitHubRepoRef repo)
	{
		repo = null;
		string[] array = (fullName ?? "").Split('/');
		if (array.Length != 2 || !SafeName(array[0]) || !SafeName(array[1]))
		{
			return false;
		}
		repo = new GitHubRepoRef(array[0], array[1]);
		return true;
	}

	private static bool SafeName(string value)
	{
		int length = value.Length;
		if (length > 0 && length <= 100 && value.Any((char c) => c != '.'))
		{
			return value.All(delegate(char c)
			{
				bool flag = char.IsAsciiLetterOrDigit(c);
				if (!flag)
				{
					bool flag2 = ((c == '-' || c == '.' || c == '_') ? true : false);
					flag = flag2;
				}
				return flag;
			});
		}
		return false;
	}

	public static bool Eligible(GitHubRepoRef repo, bool fork)
	{
		if (!fork)
		{
			return !repo.FullName.Equals("gurio-wine/STS2-SkinChanger", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public static GitHubRepoInfo[] ParseSearch(string json)
	{
		int length = json.Length;
		if ((length > 4194304 || length == 0) ? true : false)
		{
			return Array.Empty<GitHubRepoInfo>();
		}
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(json, new JsonDocumentOptions
			{
				MaxDepth = 16
			});
			if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object || !jsonDocument.RootElement.TryGetProperty("items", out var value) || value.ValueKind != JsonValueKind.Array)
			{
				return Array.Empty<GitHubRepoInfo>();
			}
			List<GitHubRepoInfo> list = new List<GitHubRepoInfo>();
			foreach (JsonElement item in value.EnumerateArray().Take(1000))
			{
				if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("full_name", out var value2) && value2.ValueKind == JsonValueKind.String && TryParseFullName(value2.GetString(), out GitHubRepoRef repo) && item.TryGetProperty("fork", out var value3) && value3.ValueKind == JsonValueKind.False && Eligible(repo, fork: false))
				{
					JsonElement value4;
					int value5;
					int stars = ((item.TryGetProperty("stargazers_count", out value4) && value4.TryGetInt32(out value5)) ? Math.Max(0, value5) : 0);
					JsonElement value6;
					DateTimeOffset result;
					DateTimeOffset pushedAt = ((item.TryGetProperty("pushed_at", out value6) && value6.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(value6.GetString(), out result)) ? result : default(DateTimeOffset));
					JsonElement value7;
					string text = ((item.TryGetProperty("description", out value7) && value7.ValueKind == JsonValueKind.String) ? (value7.GetString() ?? "") : "");
					list.Add(new GitHubRepoInfo(repo, text.Substring(0, Math.Min(text.Length, 1024)), stars, pushedAt));
				}
			}
			return list.DistinctBy<GitHubRepoInfo, string>((GitHubRepoInfo info) => info.Repo.FullName, StringComparer.OrdinalIgnoreCase).ToArray();
		}
		catch (JsonException)
		{
			return Array.Empty<GitHubRepoInfo>();
		}
	}

	public static GitHubReleaseInfo? ParseRelease(string json)
	{
		int length = json.Length;
		if ((length > 4194304 || length == 0) ? true : false)
		{
			return null;
		}
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(json, new JsonDocumentOptions
			{
				MaxDepth = 16
			});
			JsonElement rootElement = jsonDocument.RootElement;
			if (rootElement.ValueKind != JsonValueKind.Object || !rootElement.TryGetProperty("assets", out var value) || value.ValueKind != JsonValueKind.Array)
			{
				return null;
			}
			JsonElement value2;
			string text = ((rootElement.TryGetProperty("tag_name", out value2) && value2.ValueKind == JsonValueKind.String) ? (value2.GetString() ?? "") : "");
			JsonElement value3;
			string text2 = ((rootElement.TryGetProperty("name", out value3) && value3.ValueKind == JsonValueKind.String) ? (value3.GetString() ?? "") : "");
			JsonElement value4;
			DateTimeOffset result;
			DateTimeOffset publishedAt = ((rootElement.TryGetProperty("published_at", out value4) && value4.ValueKind == JsonValueKind.String && DateTimeOffset.TryParse(value4.GetString(), out result)) ? result : default(DateTimeOffset));
			List<GitHubReleaseAsset> list = new List<GitHubReleaseAsset>();
			foreach (JsonElement item in value.EnumerateArray().Take(64))
			{
				if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("name", out var value5) && value5.ValueKind == JsonValueKind.String && item.TryGetProperty("browser_download_url", out var value6) && value6.ValueKind == JsonValueKind.String)
				{
					string text3 = value5.GetString() ?? "";
					JsonElement value7;
					long value8;
					long size = ((item.TryGetProperty("size", out value7) && value7.TryGetInt64(out value8)) ? value8 : 0);
					JsonElement value9;
					string digest = ((item.TryGetProperty("digest", out value9) && value9.ValueKind == JsonValueKind.String) ? NormalizeDigest(value9.GetString()) : "");
					length = text3.Length;
					bool flag = ((length > 128 || length == 0) ? true : false);
					if (!flag && Uri.TryCreate(value6.GetString(), UriKind.Absolute, out Uri result2) && !(result2.Scheme != Uri.UriSchemeHttps) && IsAllowedHost(result2.Host))
					{
						list.Add(new GitHubReleaseAsset(text3, size, result2.AbsoluteUri, digest));
					}
				}
			}
			return new GitHubReleaseInfo(text.Substring(0, Math.Min(text.Length, 128)), text2.Substring(0, Math.Min(text2.Length, 256)), publishedAt, list.ToArray());
		}
		catch (JsonException)
		{
			return null;
		}
	}

	public static GitHubReleaseAsset? SelectAsset(GitHubReleaseInfo release, string repoName)
	{
		GitHubReleaseAsset[] source = release.Assets.Where(delegate(GitHubReleaseAsset asset)
		{
			if (asset.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
			{
				long size = asset.Size;
				if (size > 0 && size <= 134217728)
				{
					return !asset.Name.Contains("..", StringComparison.Ordinal);
				}
			}
			return false;
		}).ToArray();
		return source.FirstOrDefault((GitHubReleaseAsset asset) => Path.GetFileNameWithoutExtension(asset.Name).Equals(repoName, StringComparison.OrdinalIgnoreCase)) ?? source.OrderByDescending((GitHubReleaseAsset asset) => asset.Size).FirstOrDefault();
	}

	public static bool KeepWithoutTargets(ulong id, int targetCount, string kind)
	{
		if (targetCount == 0 && kind.Length == 0)
		{
			return SkinSourceIdentity.IsGitHub(id);
		}
		return false;
	}
}
