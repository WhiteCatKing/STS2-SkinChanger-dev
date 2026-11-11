namespace CznEnemySkin.Application;

internal interface IActPresentationSettingsRepository
{
	bool IsEnabled(string profileId, bool defaultValue);

	void SetEnabled(string profileId, bool enabled);
}
