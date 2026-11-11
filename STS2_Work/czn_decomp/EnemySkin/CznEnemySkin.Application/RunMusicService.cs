using System;
using System.Collections.Generic;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Application;

internal sealed class RunMusicService(RunMusicCatalog catalog, IRunMusicSettingsRepository settings, Action<string> warn)
{
	private readonly HashSet<string> _reportedInvalidOverrides = new HashSet<string>(StringComparer.Ordinal);

	public IReadOnlyCollection<MusicTrackProfile> Tracks => catalog.Tracks;

	public IReadOnlyCollection<ActMapMusicProfile> ActMapProfiles => catalog.ActMapProfiles;

	public IReadOnlyCollection<CuratedBossMusicProfile> CuratedBossProfiles => catalog.CuratedBossProfiles;

	public RunMusicSource BgmSource => settings.GetBgmSource();

	public BossMusicSource BossBgmSource => settings.GetBossMusicSource();

	public event Action? SettingsChanged;

	public void SetBgmSource(RunMusicSource source)
	{
		if (Enum.IsDefined(source) && BgmSource != source)
		{
			settings.SetBgmSource(source);
			this.SettingsChanged?.Invoke();
		}
	}

	public void SetBossMusicSource(BossMusicSource source)
	{
		if (Enum.IsDefined(source) && BossBgmSource != source)
		{
			settings.SetBossMusicSource(source);
			this.SettingsChanged?.Invoke();
		}
	}

	public void Reset()
	{
		bool num = BgmSource != 0 || BossBgmSource != BossMusicSource.Curated;
		settings.SetBgmSource(RunMusicSource.Czn);
		settings.SetBossMusicSource(BossMusicSource.Curated);
		if (num)
		{
			this.SettingsChanged?.Invoke();
		}
	}

	public bool TryResolveTrack(string trackId, out MusicTrackProfile track)
	{
		if (BgmSource == RunMusicSource.Czn)
		{
			return catalog.TryGetTrack(trackId, out track);
		}
		track = null;
		return false;
	}

	public bool TryResolveActMap(string actId, ulong runSeed, out ActMapMusicSelection selection)
	{
		if (BgmSource != 0 || !catalog.TryGetActMap(actId, out ActMapMusicProfile profile) || !settings.IsEnabled(profile.Id, profile.EnabledByDefault))
		{
			selection = null;
			return false;
		}
		string trackOverride = settings.GetTrackOverride(profile.Id);
		if (!string.IsNullOrWhiteSpace(trackOverride))
		{
			if (catalog.TryGetTrack(trackOverride, out MusicTrackProfile track))
			{
				selection = new ActMapMusicSelection(profile, track);
				return true;
			}
			if (_reportedInvalidOverrides.Add(profile.Id + ":" + trackOverride))
			{
				warn("Ignored unknown run-music track override " + $"[profile={profile.Id}, track={trackOverride}].");
			}
		}
		int index = (int)(runSeed % (ulong)profile.DefaultTrackIds.Count);
		string trackId = profile.DefaultTrackIds[index];
		selection = new ActMapMusicSelection(profile, catalog.GetTrack(trackId));
		return true;
	}

	public bool TryResolveCuratedBoss(string encounterId, int phaseIndex, out CuratedBossMusicSelection selection)
	{
		if (BgmSource != 0 || BossBgmSource != 0 || !catalog.TryGetCuratedBoss(encounterId, out CuratedBossMusicProfile profile) || phaseIndex < 0 || phaseIndex >= profile.PhaseTrackIds.Count)
		{
			selection = null;
			return false;
		}
		string trackId = profile.PhaseTrackIds[phaseIndex];
		selection = new CuratedBossMusicSelection(profile, phaseIndex, catalog.GetTrack(trackId));
		return true;
	}

	public void SetEnabled(ActMapMusicProfile profile, bool enabled)
	{
		settings.SetEnabled(profile.Id, enabled);
		this.SettingsChanged?.Invoke();
	}

	public bool IsEnabled(ActMapMusicProfile profile)
	{
		return settings.IsEnabled(profile.Id, profile.EnabledByDefault);
	}

	public string? GetTrackOverride(ActMapMusicProfile profile)
	{
		return settings.GetTrackOverride(profile.Id);
	}

	public bool TrySetTrackOverride(ActMapMusicProfile profile, string? trackId)
	{
		if (!string.IsNullOrWhiteSpace(trackId) && !catalog.TryGetTrack(trackId, out MusicTrackProfile _))
		{
			return false;
		}
		settings.SetTrackOverride(profile.Id, trackId);
		this.SettingsChanged?.Invoke();
		return true;
	}
}
