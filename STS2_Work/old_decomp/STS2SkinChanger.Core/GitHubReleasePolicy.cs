using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class GitHubReleasePolicy
{
	public static GitHubSkinEntry[] ReleaseChecks(IEnumerable<GitHubSkinEntry> entries, Func<ulong, bool> installed, int max)
	{
		return (from entry in entries
			where installed(entry.Id)
			orderby entry.PushedAt descending
			select entry).Take(max).ToArray();
	}

	public static GitHubSkinEntry WithRelease(GitHubSkinEntry entry, string? json)
	{
		GitHubReleaseInfo gitHubReleaseInfo = ((json == null) ? null : GitHubSkinPolicy.ParseRelease(json));
		if (gitHubReleaseInfo == null)
		{
			return entry;
		}
		GitHubReleaseAsset gitHubReleaseAsset = GitHubSkinPolicy.SelectAsset(gitHubReleaseInfo, entry.RepoName);
		return entry with
		{
			ReleaseTag = gitHubReleaseInfo.Tag,
			AssetUrl = (gitHubReleaseAsset?.Url ?? ""),
			AssetSize = (gitHubReleaseAsset?.Size ?? 0),
			AssetDigest = (gitHubReleaseAsset?.Digest ?? "")
		};
	}
}
