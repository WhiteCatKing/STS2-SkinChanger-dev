namespace CznEnemySkin.Domain;

internal sealed record EnemyEffectCue(string ScenePath, string? SpineAnimationName, float OfficialPositionX, float OfficialPositionY, float ScaleX, float ScaleY, float OfficialRotationDegrees, int ZIndex, float DelaySeconds, float LifetimeSeconds, bool StartParticles = false, EnemyEffectMaterialPolicy MaterialPolicy = EnemyEffectMaterialPolicy.Native, float Opacity = 1f, EnemyPresentationStance? Stance = null, bool TargetHit = false, bool TargetAtEntry = false, bool TargetCenter = false, int TargetHitIndex = -1, int TargetHitPeriod = 0, EnemyTargetOrigin TargetOrigin = EnemyTargetOrigin.Floor)
{
	internal bool MatchesNativeHit(int? index)
	{
		if (TargetHitIndex >= 0)
		{
			if (index.HasValue && index.GetValueOrDefault() >= 0)
			{
				return TargetHitIndex == ((TargetHitPeriod > 0) ? (index.Value % TargetHitPeriod) : index.Value);
			}
			return false;
		}
		return true;
	}

	internal bool MatchesTargetSurface(bool atEntry, bool centerOnly)
	{
		if (TargetHit && TargetAtEntry == atEntry)
		{
			return TargetCenter == centerOnly;
		}
		return false;
	}

	internal bool MatchesStance(EnemyPresentationStance stance)
	{
		if (Stance.HasValue)
		{
			return Stance == stance;
		}
		return true;
	}
}
