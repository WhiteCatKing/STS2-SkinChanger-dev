namespace STS2SkinChanger.Core;

internal sealed record GitHubReleaseAsset(string Name, long Size, string Url, string Digest = "");
