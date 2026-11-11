using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Application;

internal sealed class RunMusicCatalog
{
	private readonly IReadOnlyDictionary<string, MusicTrackProfile> _tracksById;

	private readonly IReadOnlyDictionary<string, ActMapMusicProfile> _actMapsByActId;

	private readonly IReadOnlyDictionary<string, CuratedBossMusicProfile> _curatedBossesByEncounterId;

	public IReadOnlyCollection<MusicTrackProfile> Tracks { get; }

	public IReadOnlyCollection<ActMapMusicProfile> ActMapProfiles { get; }

	public IReadOnlyCollection<CuratedBossMusicProfile> CuratedBossProfiles { get; }

	public RunMusicCatalog(IEnumerable<MusicTrackProfile> tracks, IEnumerable<ActMapMusicProfile> actMapProfiles, IEnumerable<CuratedBossMusicProfile> curatedBossProfiles)
	{
		MusicTrackProfile[] source = tracks.ToArray();
		ActMapMusicProfile[] array = actMapProfiles.ToArray();
		CuratedBossMusicProfile[] array2 = curatedBossProfiles.ToArray();
		_tracksById = source.ToDictionary<MusicTrackProfile, string>((MusicTrackProfile track) => track.Id, StringComparer.Ordinal);
		Tracks = (IReadOnlyCollection<MusicTrackProfile>)(object)source;
		_actMapsByActId = array.ToDictionary<ActMapMusicProfile, string>((ActMapMusicProfile profile) => profile.ActId, StringComparer.Ordinal);
		_curatedBossesByEncounterId = array2.ToDictionary<CuratedBossMusicProfile, string>((CuratedBossMusicProfile profile) => profile.EncounterId, StringComparer.Ordinal);
		ActMapMusicProfile[] array3 = array;
		foreach (ActMapMusicProfile actMapMusicProfile in array3)
		{
			if (actMapMusicProfile.DefaultTrackIds.Count == 0)
			{
				throw new InvalidOperationException("Act map profile '" + actMapMusicProfile.Id + "' has no default track.");
			}
			foreach (string defaultTrackId in actMapMusicProfile.DefaultTrackIds)
			{
				if (!_tracksById.ContainsKey(defaultTrackId))
				{
					throw new InvalidOperationException($"Act map profile '{actMapMusicProfile.Id}' references unknown track '{defaultTrackId}'.");
				}
			}
		}
		ActMapProfiles = (IReadOnlyCollection<ActMapMusicProfile>)(object)array;
		CuratedBossMusicProfile[] array4 = array2;
		foreach (CuratedBossMusicProfile curatedBossMusicProfile in array4)
		{
			if (curatedBossMusicProfile.PhaseTrackIds.Count == 0)
			{
				throw new InvalidOperationException("Curated Boss profile '" + curatedBossMusicProfile.Id + "' has no track.");
			}
			foreach (string phaseTrackId in curatedBossMusicProfile.PhaseTrackIds)
			{
				if (!_tracksById.ContainsKey(phaseTrackId))
				{
					throw new InvalidOperationException($"Curated Boss profile '{curatedBossMusicProfile.Id}' references unknown track '{phaseTrackId}'.");
				}
			}
		}
		CuratedBossProfiles = (IReadOnlyCollection<CuratedBossMusicProfile>)(object)array2;
	}

	public bool TryGetTrack(string trackId, out MusicTrackProfile track)
	{
		return _tracksById.TryGetValue(trackId, out track);
	}

	public bool TryGetActMap(string actId, out ActMapMusicProfile profile)
	{
		return _actMapsByActId.TryGetValue(actId, out profile);
	}

	public bool TryGetCuratedBoss(string encounterId, out CuratedBossMusicProfile profile)
	{
		return _curatedBossesByEncounterId.TryGetValue(encounterId, out profile);
	}

	public MusicTrackProfile GetTrack(string trackId)
	{
		return _tracksById[trackId];
	}
}
