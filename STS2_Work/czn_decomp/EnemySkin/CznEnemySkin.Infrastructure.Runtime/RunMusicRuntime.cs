using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Application;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class RunMusicRuntime
{
	private sealed record MusicSelection(string Surface, string ProfileId, MusicTrackProfile Track);

	private const string MusicBus = "Master";

	private const float MaxPlaybackGainLinear = 1.5f;

	private readonly RunMusicService _music;

	private readonly NativeMusicHostAdapter _nativeMusic;

	private readonly HashSet<string> _reportedFailures = new HashSet<string>(StringComparer.Ordinal);

	private AudioStreamPlayer? _player;

	private MusicTrackProfile? _activeTrack;

	private float _bgmVolume = 1f;

	private bool _nativeMusicSuppressed;

	private bool _combatActive;

	private EncounterModel? _activeEncounter;

	private int _activeBossMusicPhase;

	public RunMusicRuntime(RunMusicService music, NativeMusicHostAdapter nativeMusic)
	{
		_music = music;
		_nativeMusic = nativeMusic;
		InitializeVolume();
		_music.SettingsChanged += OnSettingsChanged;
		CombatManager.Instance.CombatBegan += OnCombatBegan;
		CombatManager.Instance.CombatEnded += OnCombatEnded;
	}

	public void UpdateBgmVolume(float volume)
	{
		_bgmVolume = Math.Clamp(volume, 0f, 1f);
		ApplyVolume();
	}

	public void SetBossMusicPhase(int phaseIndex)
	{
		if (_combatActive && _activeEncounter != null && phaseIndex >= 0 && phaseIndex != _activeBossMusicPhase)
		{
			_activeBossMusicPhase = phaseIndex;
			IReadOnlyList<MusicSelection> candidates = ResolveExactEncounterMusic(_activeEncounter, phaseIndex);
			if (!TryActivateCandidates(candidates))
			{
				UseNativeCombatMusic(_activeEncounter);
			}
		}
	}

	public void OnNativeMusicUpdated(NRunMusicController controller)
	{
		if (!_combatActive)
		{
			ResetNativeMusicSuppressionLease();
			PlayMapMusic(controller, nativeFallbackAlreadyAudible: true);
		}
	}

	public void OnNativeStopRequested()
	{
		_combatActive = false;
		_activeEncounter = null;
		_activeBossMusicPhase = 0;
		_nativeMusicSuppressed = false;
		DestroyPlayer();
	}

	private void OnSettingsChanged()
	{
		if (!_combatActive)
		{
			NRunMusicController instance = NRunMusicController.Instance;
			if (instance != null)
			{
				PlayMapMusic(instance, !_nativeMusicSuppressed);
			}
		}
	}

	private void OnCombatBegan(CombatState state)
	{
		_combatActive = true;
		_activeEncounter = state.Encounter;
		_activeBossMusicPhase = 0;
		EncounterModel? activeEncounter = _activeEncounter;
		if (activeEncounter != null && activeEncounter.HasBgm)
		{
			ResetNativeMusicSuppressionLease();
		}
		IReadOnlyList<MusicSelection> readOnlyList;
		if (_activeEncounter != null)
		{
			readOnlyList = ResolveCombatMusic(state, _activeEncounter);
		}
		else
		{
			IReadOnlyList<MusicSelection> readOnlyList2 = Array.Empty<MusicSelection>();
			readOnlyList = readOnlyList2;
		}
		IReadOnlyList<MusicSelection> candidates = readOnlyList;
		if (!TryActivateCandidates(candidates))
		{
			UseNativeCombatMusic(_activeEncounter);
		}
	}

	private void OnCombatEnded(CombatRoom _)
	{
		_combatActive = false;
		_activeEncounter = null;
		_activeBossMusicPhase = 0;
		NRunMusicController instance = NRunMusicController.Instance;
		if (instance == null)
		{
			ReleaseCznMusic(restoreNative: false);
		}
		else
		{
			PlayMapMusic(instance, nativeFallbackAlreadyAudible: false);
		}
	}

	private void PlayMapMusic(NRunMusicController controller, bool nativeFallbackAlreadyAudible)
	{
		MusicSelection musicSelection = ResolveMapMusic(controller);
		if (musicSelection == null || !Activate(musicSelection))
		{
			ReleaseCznMusic(!nativeFallbackAlreadyAudible);
		}
	}

	private MusicSelection? ResolveMapMusic(NRunMusicController controller)
	{
		if (!_nativeMusic.TryGetRunState(controller, out IRunState runState, out string reason))
		{
			LogOnce("run_state", "Run map music is unavailable: " + reason);
			return null;
		}
		if (!_music.TryResolveActMap(((AbstractModel)runState.Act).Id.Entry, runState.Rng.Seed, out ActMapMusicSelection selection))
		{
			return null;
		}
		return new MusicSelection("act_map", selection.Profile.Id, selection.Track);
	}

	private IReadOnlyList<MusicSelection> ResolveCombatMusic(CombatState state, EncounterModel encounter)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (Entry.EncounterBattlefields.TryResolve(((AbstractModel)encounter).Id.Entry, out EncounterBattlefieldProfile profile))
		{
			return ResolveExactEncounterMusic(encounter, _activeBossMusicPhase, profile);
		}
		if (encounter.HasBgm || !ActRoomScopeClassifier.TryClassify(encounter.RoomType, encounter.IsWeak, out var scope) || !Entry.ActPresentations.TryResolve(((AbstractModel)state.RunState.Act).Id.Entry, scope, out ActPresentationProfile profile2))
		{
			return Array.Empty<MusicSelection>();
		}
		MusicSelection musicSelection = BattlefieldSelection("act_battlefield", profile2.Id, profile2.Battlefield);
		if (!(musicSelection == null))
		{
			return new _003C_003Ez__ReadOnlySingleElementList<MusicSelection>(musicSelection);
		}
		return Array.Empty<MusicSelection>();
	}

	private IReadOnlyList<MusicSelection> ResolveExactEncounterMusic(EncounterModel encounter, int phaseIndex)
	{
		if (!Entry.EncounterBattlefields.TryResolve(((AbstractModel)encounter).Id.Entry, out EncounterBattlefieldProfile profile))
		{
			return Array.Empty<MusicSelection>();
		}
		return ResolveExactEncounterMusic(encounter, phaseIndex, profile);
	}

	private IReadOnlyList<MusicSelection> ResolveExactEncounterMusic(EncounterModel encounter, int phaseIndex, EncounterBattlefieldProfile encounterProfile)
	{
		List<MusicSelection> list = new List<MusicSelection>();
		if (_music.TryResolveCuratedBoss(((AbstractModel)encounter).Id.Entry, phaseIndex, out CuratedBossMusicSelection selection))
		{
			list.Add(new MusicSelection("curated_boss", selection.Profile.Id, selection.Track));
		}
		MusicSelection czn = BattlefieldSelection("encounter_battlefield", encounterProfile.Id, encounterProfile.Battlefield);
		if (czn != null && list.All((MusicSelection candidate) => candidate.Track.Id != czn.Track.Id))
		{
			list.Add(czn);
		}
		return list;
	}

	private MusicSelection? BattlefieldSelection(string surface, string profileId, BattlefieldPresentationProfile battlefield)
	{
		string trackId = battlefield.Bgm.TrackId;
		if (trackId == null || !_music.TryResolveTrack(trackId, out MusicTrackProfile track))
		{
			return null;
		}
		return new MusicSelection(surface, profileId, track);
	}

	private bool Activate(MusicSelection selection)
	{
		if (!SwitchTrack(selection))
		{
			return false;
		}
		if (_nativeMusicSuppressed)
		{
			return true;
		}
		if (_nativeMusic.TrySuppress(out string reason))
		{
			_nativeMusicSuppressed = true;
			return true;
		}
		LogOnce("native_suppress:" + selection.ProfileId, "Could not suppress STS2 music " + $"[profile={selection.ProfileId}, reason={reason}].");
		return false;
	}

	private bool TryActivateCandidates(IEnumerable<MusicSelection> candidates)
	{
		foreach (MusicSelection candidate in candidates)
		{
			if (Activate(candidate))
			{
				return true;
			}
		}
		return false;
	}

	private bool SwitchTrack(MusicSelection selection)
	{
		MusicTrackProfile track = selection.Track;
		if (track.Transport != BgmTransport.GodotStream)
		{
			LogTrackFailure(selection, "unsupported transport");
			return false;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_player) && GodotObject.IsInstanceValid((GodotObject)(object)_player.Stream) && _activeTrack?.Id == track.Id)
		{
			ApplyVolume();
			if (!_player.Playing)
			{
				return StartPlayer(selection);
			}
			return true;
		}
		AudioStreamOggVorbis val = LoadStream(selection);
		NRunMusicController instance = NRunMusicController.Instance;
		if (val == null || instance == null)
		{
			if (val != null)
			{
				((GodotObject)val).Dispose();
			}
			return false;
		}
		val.Loop = true;
		val.LoopOffset = track.LoopOffsetSeconds;
		EnsurePlayer(instance);
		AudioStream stream = _player.Stream;
		_player.Stop();
		_player.Stream = (AudioStream)(object)val;
		_activeTrack = track;
		ApplyVolume();
		if (stream != null)
		{
			((GodotObject)stream).Dispose();
		}
		if (!StartPlayer(selection))
		{
			return false;
		}
		Entry.Log.Info("Playing CZN run music " + $"[surface={selection.Surface}, profile={selection.ProfileId}, " + $"track={track.Id}, path={track.Locator}].", 1);
		return true;
	}

	private bool StartPlayer(MusicSelection selection)
	{
		_player.Play(0f);
		if (_player.Playing)
		{
			return true;
		}
		LogTrackFailure(selection, "Godot did not enter playback state");
		DestroyPlayer();
		return false;
	}

	private AudioStreamOggVorbis? LoadStream(MusicSelection selection)
	{
		try
		{
			AudioStreamOggVorbis obj = ResourceLoader.Load<AudioStreamOggVorbis>(selection.Track.Locator, (string)null, (CacheMode)0);
			if (obj == null)
			{
				LogTrackFailure(selection, "resource not found");
			}
			return obj;
		}
		catch (Exception ex)
		{
			LogTrackFailure(selection, ex.GetBaseException().Message);
			return null;
		}
	}

	private void UseNativeCombatMusic(EncounterModel? encounter)
	{
		bool flag = encounter != null && encounter.HasBgm;
		ReleaseCznMusic(_nativeMusicSuppressed && !flag);
	}

	private void ReleaseCznMusic(bool restoreNative)
	{
		DestroyPlayer();
		string reason;
		if (!restoreNative)
		{
			_nativeMusicSuppressed = false;
		}
		else if (!_nativeMusic.TryRestore(out reason))
		{
			LogOnce("native_restore", "Could not restore STS2 music: " + reason);
		}
		else
		{
			_nativeMusicSuppressed = false;
		}
	}

	private void EnsurePlayer(NRunMusicController controller)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		if (!GodotObject.IsInstanceValid((GodotObject)(object)_player))
		{
			_player = new AudioStreamPlayer
			{
				Name = StringName.op_Implicit("CznRunMusic"),
				Bus = StringName.op_Implicit("Master")
			};
			((Node)controller).AddChild((Node)(object)_player, false, (InternalMode)0);
		}
	}

	private void DestroyPlayer()
	{
		_activeTrack = null;
		if (!GodotObject.IsInstanceValid((GodotObject)(object)_player))
		{
			_player = null;
			return;
		}
		_player.Stop();
		AudioStream stream = _player.Stream;
		_player.Stream = null;
		if (stream != null)
		{
			((GodotObject)stream).Dispose();
		}
		((Node)_player).QueueFree();
		_player = null;
	}

	private void ApplyVolume()
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)_player) && !(_activeTrack == null))
		{
			float num = Mathf.Pow(_bgmVolume, 2f);
			_player.VolumeLinear = Math.Clamp(_activeTrack.PlaybackGainLinear * num, 0f, 1.5f);
		}
	}

	private void ResetNativeMusicSuppressionLease()
	{
		_nativeMusicSuppressed = false;
	}

	private void InitializeVolume()
	{
		try
		{
			_bgmVolume = Math.Clamp(SaveManager.Instance.SettingsSave.VolumeBgm, 0f, 1f);
		}
		catch
		{
		}
	}

	private void LogTrackFailure(MusicSelection selection, string reason)
	{
		LogOnce("track:" + selection.Track.Id, "CZN music track is unavailable; keeping STS2 music " + $"[profile={selection.ProfileId}, track={selection.Track.Id}, " + $"path={selection.Track.Locator}, reason={reason}].");
	}

	private void LogOnce(string key, string message)
	{
		if (_reportedFailures.Add(key))
		{
			Entry.Log.Warn(message, 1);
		}
	}
}
