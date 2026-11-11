namespace STS2SkinChanger.Core;

internal sealed record GitHubPendingOp(GitHubModsOperationKind Kind, string Folder, string Staging);
