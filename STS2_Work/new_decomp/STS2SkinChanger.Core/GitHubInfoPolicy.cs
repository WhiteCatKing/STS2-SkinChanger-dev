namespace STS2SkinChanger.Core;

internal static class GitHubInfoPolicy
{
	public const string FileName = "sc.info";

	public const int MaxBytes = 65536;

	public static GitHubInfoResult Read(string? text, ulong expectedId)
	{
		if (text == null)
		{
			return new GitHubInfoResult(null, "missing");
		}
		if (text.Length > 65536)
		{
			return new GitHubInfoResult(null, "too-large");
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			return new GitHubInfoResult(null, "no-code");
		}
		WorkshopCodeRead workshopCodeRead = WorkshopSubmissionCodec.Read(new _003C_003Ez__ReadOnlySingleElementList<WorkshopCodePost>(new WorkshopCodePost(text, "", 0, 0)));
		WorkshopCodeCandidate[] candidates = workshopCodeRead.Candidates;
		foreach (WorkshopCodeCandidate workshopCodeCandidate in candidates)
		{
			if (workshopCodeCandidate.Payload.Item.Id == expectedId)
			{
				return new GitHubInfoResult(workshopCodeCandidate.Payload.Item, "");
			}
		}
		if (workshopCodeRead.Candidates.Length != 0)
		{
			return new GitHubInfoResult(null, "id-mismatch");
		}
		if (workshopCodeRead.Issues.Length != 0)
		{
			return new GitHubInfoResult(null, workshopCodeRead.Issues[0].Error.ToString().ToLowerInvariant());
		}
		return new GitHubInfoResult(null, "no-code");
	}
}
