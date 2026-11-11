namespace STS2SkinChanger.Core;

internal static class GitHubItemActions
{
	public static GitHubItemAction Primary(bool recognized, GitHubInstallState state, bool pendingRestart)
	{
		if (state == GitHubInstallState.NotInstalled)
		{
			if (!recognized)
			{
				return GitHubItemAction.Scan;
			}
			return GitHubItemAction.Install;
		}
		if (pendingRestart)
		{
			return GitHubItemAction.Restart;
		}
		if (state != GitHubInstallState.Updatable)
		{
			return GitHubItemAction.Uninstall;
		}
		return GitHubItemAction.Update;
	}

	public static bool ShowsUninstall(GitHubItemAction action)
	{
		if ((uint)(action - 2) <= 1u)
		{
			return true;
		}
		return false;
	}
}
