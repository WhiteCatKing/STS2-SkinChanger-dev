namespace STS2SkinChanger.Core;

internal sealed record GitHubInstall(string Folder, string PackageId, string Version, string Digest = "");
