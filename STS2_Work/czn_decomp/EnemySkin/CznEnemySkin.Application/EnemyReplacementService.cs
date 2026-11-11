using System;
using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Application;

internal sealed class EnemyReplacementService(EnemyReplacementCatalog catalog, IEnemySkinSettingsRepository settings)
{
	public IReadOnlyCollection<EnemyReplacementProfile> Profiles => catalog.Profiles;

	public event Action<EnemyReplacementProfile, bool>? PreferenceChanged;

	public bool IsEnabled(EnemyReplacementProfile profile)
	{
		return settings.IsEnabled(profile.Id, profile.EnabledByDefault);
	}

	public bool TryResolveMonster(string monsterId, out EnemyReplacementProfile profile)
	{
		if (!catalog.TryGetByMonsterId(monsterId, out profile) || !IsEnabled(profile) || profile.Monster.Mode != PresentationMode.ReplaceReady)
		{
			profile = null;
			return false;
		}
		return true;
	}

	public bool TryGetMonsterProfile(string monsterId, out EnemyReplacementProfile profile)
	{
		return catalog.TryGetByMonsterId(monsterId, out profile);
	}

	public bool TryGetProfile(string profileId, out EnemyReplacementProfile profile)
	{
		return catalog.TryGetById(profileId, out profile);
	}

	public void SetEnabled(EnemyReplacementProfile profile, bool enabled)
	{
		if (IsEnabled(profile) != enabled)
		{
			settings.SetEnabled(profile.Id, enabled);
			this.PreferenceChanged?.Invoke(profile, enabled);
		}
	}

	public bool Toggle(EnemyReplacementProfile profile)
	{
		bool flag = !IsEnabled(profile);
		SetEnabled(profile, flag);
		return flag;
	}
}
