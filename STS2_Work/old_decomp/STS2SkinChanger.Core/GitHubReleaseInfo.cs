using System;

namespace STS2SkinChanger.Core;

internal sealed record GitHubReleaseInfo(string Tag, string Title, DateTimeOffset PublishedAt, GitHubReleaseAsset[] Assets);
