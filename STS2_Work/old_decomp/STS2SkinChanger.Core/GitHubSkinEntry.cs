using System;
using System.Text.Json.Serialization;

namespace STS2SkinChanger.Core;

internal sealed record GitHubSkinEntry(string FullName, string Name, string Description, int Stars, DateTimeOffset PushedAt, string Readme, GitHubRecognition Recognition, string Error, WorkshopTarget[] Targets, bool RestartRequired, string ReleaseTag, string AssetUrl, long AssetSize, string AssetDigest = "")
{
	[JsonIgnore]
	public ulong Id => SkinSourceIdentity.GitHubId(Owner, RepoName);

	[JsonIgnore]
	public string Owner => FullName.Split('/')[0];

	[JsonIgnore]
	public string RepoName => FullName.Split('/')[1];

	[JsonIgnore]
	public GitHubRepoRef Repo => new GitHubRepoRef(Owner, RepoName);

	[JsonIgnore]
	public string CoverUrl => GitHubSkinPolicy.CoverUrl(Repo);
}
