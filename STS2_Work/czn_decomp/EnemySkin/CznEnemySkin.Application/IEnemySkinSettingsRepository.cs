namespace CznEnemySkin.Application;

internal interface IEnemySkinSettingsRepository
{
	bool IsEnabled(string profileId, bool defaultValue);

	void SetEnabled(string profileId, bool enabled);
}
