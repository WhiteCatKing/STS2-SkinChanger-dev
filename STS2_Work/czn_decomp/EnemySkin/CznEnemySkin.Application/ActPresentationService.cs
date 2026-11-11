using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Application;

internal sealed class ActPresentationService(ActPresentationCatalog catalog, IActPresentationSettingsRepository settings)
{
	public IReadOnlyCollection<ActPresentationProfile> Profiles => catalog.Profiles;

	public bool IsEnabled(ActPresentationProfile profile)
	{
		return settings.IsEnabled(profile.SettingId, profile.EnabledByDefault);
	}

	public bool TryResolve(string actId, ActRoomScope roomScope, out ActPresentationProfile profile)
	{
		if (catalog.TryGetByTarget(actId, roomScope, out profile))
		{
			if (IsEnabled(profile))
			{
				return true;
			}
			profile = null;
			return false;
		}
		if (roomScope != 0 && catalog.TryGetByTarget(actId, ActRoomScope.StandardCombat, out profile) && IsEnabled(profile))
		{
			return true;
		}
		profile = null;
		return false;
	}

	public void SetEnabled(ActPresentationProfile profile, bool enabled)
	{
		settings.SetEnabled(profile.SettingId, enabled);
	}
}
