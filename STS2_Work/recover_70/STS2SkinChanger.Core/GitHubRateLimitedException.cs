using System;

namespace STS2SkinChanger.Core;

internal sealed class GitHubRateLimitedException : Exception
{
	public GitHubRateLimitedException(string message)
		: base(message)
	{
	}
}
