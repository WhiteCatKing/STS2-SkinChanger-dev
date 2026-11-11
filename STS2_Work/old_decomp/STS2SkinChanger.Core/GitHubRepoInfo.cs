using System;

namespace STS2SkinChanger.Core;

internal sealed record GitHubRepoInfo(GitHubRepoRef Repo, string Description, int Stars, DateTimeOffset PushedAt);
