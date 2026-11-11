using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Application;

internal sealed class EncounterBattlefieldService(EncounterBattlefieldCatalog catalog, IEncounterBattlefieldSettingsRepository settings)
{
	public IReadOnlyCollection<EncounterBattlefieldProfile> Profiles => catalog.Profiles;

	public bool TryResolve(string encounterId, out EncounterBattlefieldProfile profile)
	{
		if (!catalog.TryGetByTarget(encounterId, out profile) || !settings.IsEnabled(profile.Id, profile.EnabledByDefault))
		{
			profile = null;
			return false;
		}
		return true;
	}

	public void SetEnabled(EncounterBattlefieldProfile profile, bool enabled)
	{
		settings.SetEnabled(profile.Id, enabled);
	}
}
