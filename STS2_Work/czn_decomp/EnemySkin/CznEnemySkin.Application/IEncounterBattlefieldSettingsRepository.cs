namespace CznEnemySkin.Application;

internal interface IEncounterBattlefieldSettingsRepository
{
	bool IsEnabled(string profileId, bool defaultValue);

	void SetEnabled(string profileId, bool enabled);
}
