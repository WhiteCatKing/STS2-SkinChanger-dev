namespace CznEnemySkin.Application;

internal interface IRunMusicSettingsRepository
{
	RunMusicSource GetBgmSource();

	void SetBgmSource(RunMusicSource source);

	BossMusicSource GetBossMusicSource();

	void SetBossMusicSource(BossMusicSource source);

	bool IsEnabled(string profileId, bool defaultValue);

	void SetEnabled(string profileId, bool enabled);

	string? GetTrackOverride(string profileId);

	void SetTrackOverride(string profileId, string? trackId);
}
