namespace STS2SkinChanger.Core;

internal static class RunEnvironmentRefreshPolicy
{
	public static RunEnvironmentMusicMode SelectMusicMode(bool isCombatInProgress, bool hasCombatState)
	{
		if (!(isCombatInProgress && hasCombatState))
		{
			return RunEnvironmentMusicMode.Map;
		}
		return RunEnvironmentMusicMode.Combat;
	}
}
