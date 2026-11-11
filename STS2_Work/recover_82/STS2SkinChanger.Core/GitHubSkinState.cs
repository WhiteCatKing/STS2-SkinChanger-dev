using System;

namespace STS2SkinChanger.Core;

internal sealed record GitHubSkinState(int Format, GitHubSkinEntry[] Entries)
{
	public static GitHubSkinState Empty => new GitHubSkinState(4, Array.Empty<GitHubSkinEntry>());
}
