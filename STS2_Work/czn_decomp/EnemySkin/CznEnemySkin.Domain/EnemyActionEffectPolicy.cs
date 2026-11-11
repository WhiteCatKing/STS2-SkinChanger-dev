namespace CznEnemySkin.Domain;

internal static class EnemyActionEffectPolicy
{
	public static bool IsAttackOrSkill(EnemyAction action)
	{
		if ((uint)(action - 1) <= 7u)
		{
			return true;
		}
		return false;
	}

	public static bool ShouldPlay(EnemyAction action, bool enabled)
	{
		if (!enabled)
		{
			return !IsAttackOrSkill(action);
		}
		return true;
	}
}
