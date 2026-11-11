using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznCore.Presentation;

internal sealed class AudioStreamPlayerRuntime
{
	private static readonly StringName SfxBus = new StringName("SFX");

	private readonly Dictionary<string, AudioStream> _cache = new Dictionary<string, AudioStream>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, long> _missingAssetNextLogAt = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _threadedLoads = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private readonly CacheMode? _cacheMode;

	private readonly string _missingAssetPrefix;

	private readonly string _loadFailedPrefix;

	private readonly Action<string, int> _warn;

	private readonly int _maxPlayers;

	private readonly List<AudioStreamPlayer> _players = new List<AudioStreamPlayer>();

	private string _playerName;

	private Node? _attachedTo;

	private readonly Node? _owner;

	private int _nextPlayerIndex;

	public bool IsPlaying
	{
		get
		{
			foreach (AudioStreamPlayer player in _players)
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)player) && player.Playing)
				{
					return true;
				}
			}
			return false;
		}
	}

	private CacheMode EffectiveCacheMode => (CacheMode)(((_003F?)_cacheMode) ?? 1);

	public AudioStreamPlayerRuntime(string playerName, string missingAssetPrefix, string loadFailedPrefix, CacheMode? cacheMode = null, Action<string, int>? warn = null, int maxPlayers = 1, Node? owner = null)
	{
		_playerName = playerName;
		_missingAssetPrefix = missingAssetPrefix;
		_loadFailedPrefix = loadFailedPrefix;
		_cacheMode = cacheMode;
		_warn = warn ?? new Action<string, int>(NoopWarn);
		_maxPlayers = Math.Max(1, maxPlayers);
		_owner = owner;
	}

	public void SetPlayerName(string playerName)
	{
		_playerName = playerName;
		for (int i = 0; i < _players.Count; i++)
		{
			AudioStreamPlayer val = _players[i];
			if (GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((Node)val).Name = StringName.op_Implicit(FormatPlayerName(i));
			}
		}
	}

	public bool Prewarm(string resPath)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetCachedStream(resPath, out AudioStream stream))
		{
			return true;
		}
		if (_threadedLoads.Contains(resPath))
		{
			return TryCompleteThreadedLoad(resPath, allowBlocking: false);
		}
		try
		{
			if (!ResourceLoader.Exists(resPath, ""))
			{
				if (TryCacheRawAudioStream(resPath, out stream))
				{
					return true;
				}
				LogMissingAsset(resPath, null);
				return true;
			}
			Error val = ResourceLoader.LoadThreadedRequest(resPath, "", false, EffectiveCacheMode);
			if ((int)val != 0)
			{
				if (TryCacheRawAudioStream(resPath, out stream))
				{
					return true;
				}
				LogMissingAsset(resPath, "threaded request failed: " + ((object)(Error)(ref val)/*cast due to .constrained prefix*/).ToString());
				return true;
			}
			_threadedLoads.Add(resPath);
			return TryCompleteThreadedLoad(resPath, allowBlocking: false);
		}
		catch (Exception ex)
		{
			LogMissingAsset(resPath, ex.Message);
			return true;
		}
	}

	public bool Play(string resPath, float volumeLinear)
	{
		return PlayWithResult(resPath, volumeLinear).Played;
	}

	public AudioPlaybackResult PlayWithResult(string resPath, float volumeLinear)
	{
		return PlayWithResult(resPath, volumeLinear, replaceCurrent: false);
	}

	public AudioPlaybackResult PlayWithResult(string resPath, float volumeLinear, bool replaceCurrent)
	{
		AudioStream stream = GetStream(resPath);
		if (stream == null)
		{
			return AudioPlaybackResult.NotPlayed;
		}
		AudioStreamPlayer val = EnsurePlayer();
		if (val == null)
		{
			return AudioPlaybackResult.NotPlayed;
		}
		if (replaceCurrent)
		{
			Stop();
		}
		else
		{
			val.Stop();
		}
		val.Stream = stream;
		val.VolumeLinear = volumeLinear;
		val.Bus = SfxBus;
		val.Play(0f);
		return new AudioPlaybackResult(Played: true, GetStreamLengthSeconds(stream));
	}

	public int PrewarmPlayers(int count = 0)
	{
		Node val = ResolveTargetParent();
		if (val == null)
		{
			return _players.Count;
		}
		RemoveInvalidPlayers();
		EnsurePlayersAttached(val);
		int num = ((count <= 0) ? _maxPlayers : Math.Clamp(count, 1, _maxPlayers));
		while (_players.Count < num)
		{
			AudioStreamPlayer val2 = CreatePlayer(_players.Count);
			val.AddChild((Node)(object)val2, false, (InternalMode)0);
			_players.Add(val2);
		}
		_attachedTo = val;
		return _players.Count;
	}

	public void Stop()
	{
		foreach (AudioStreamPlayer player in _players)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)player))
			{
				player.Stop();
			}
		}
	}

	public void TrimCache(bool stopPlayback = false)
	{
		_cache.Clear();
		_missingAssetNextLogAt.Clear();
		foreach (AudioStreamPlayer player in _players)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)player))
			{
				if (stopPlayback && player.Playing)
				{
					player.Stop();
				}
				if (stopPlayback || !player.Playing)
				{
					player.Stream = null;
				}
			}
		}
	}

	private AudioStreamPlayer? EnsurePlayer()
	{
		Node val = ResolveTargetParent();
		if (val == null)
		{
			return null;
		}
		RemoveInvalidPlayers();
		EnsurePlayersAttached(val);
		for (int i = 0; i < _players.Count; i++)
		{
			AudioStreamPlayer val2 = _players[i];
			if (!val2.Playing)
			{
				return val2;
			}
		}
		if (_players.Count < _maxPlayers)
		{
			AudioStreamPlayer val3 = CreatePlayer(_players.Count);
			val.AddChild((Node)(object)val3, false, (InternalMode)0);
			_players.Add(val3);
			_attachedTo = val;
			return val3;
		}
		_nextPlayerIndex = (_nextPlayerIndex + 1) % _players.Count;
		AudioStreamPlayer obj = _players[_nextPlayerIndex];
		obj.Stop();
		return obj;
	}

	public void RefreshVolume(float volumeLinear)
	{
		foreach (AudioStreamPlayer player in _players)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)player))
			{
				player.VolumeLinear = volumeLinear;
			}
		}
	}

	private Node? ResolveTargetParent()
	{
		if (_owner != null)
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)_owner) || !_owner.IsInsideTree())
			{
				return null;
			}
			return _owner;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		object obj = ((instance != null) ? instance.Ui : null);
		if (obj == null)
		{
			obj = NCombatRoom.Instance;
			if (obj == null)
			{
				obj = NGame.Instance;
				if (obj == null)
				{
					MainLoop mainLoop = Engine.GetMainLoop();
					MainLoop obj2 = ((mainLoop is SceneTree) ? mainLoop : null);
					if (obj2 == null)
					{
						return null;
					}
					obj = ((SceneTree)obj2).Root;
				}
			}
		}
		return (Node?)obj;
	}

	private void RemoveInvalidPlayers()
	{
		for (int num = _players.Count - 1; num >= 0; num--)
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)_players[num]))
			{
				_players.RemoveAt(num);
			}
		}
		if (_players.Count == 0)
		{
			_nextPlayerIndex = 0;
		}
		else if (_nextPlayerIndex >= _players.Count)
		{
			_nextPlayerIndex = 0;
		}
	}

	private void EnsurePlayersAttached(Node targetParent)
	{
		if (_attachedTo == targetParent && GodotObject.IsInstanceValid((GodotObject)(object)_attachedTo))
		{
			return;
		}
		for (int i = 0; i < _players.Count; i++)
		{
			AudioStreamPlayer val = _players[i];
			Node parent = ((Node)val).GetParent();
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)val);
			}
			((Node)val).Name = StringName.op_Implicit(FormatPlayerName(i));
			targetParent.AddChild((Node)(object)val, false, (InternalMode)0);
		}
		_attachedTo = targetParent;
	}

	private AudioStreamPlayer CreatePlayer(int index)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		return new AudioStreamPlayer
		{
			Name = StringName.op_Implicit(FormatPlayerName(index))
		};
	}

	private string FormatPlayerName(int index)
	{
		if (_maxPlayers > 1)
		{
			return _playerName + "_" + index.ToString(CultureInfo.InvariantCulture);
		}
		return _playerName;
	}

	private AudioStream? GetStream(string resPath)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetCachedStream(resPath, out AudioStream stream))
		{
			return stream;
		}
		if (_threadedLoads.Contains(resPath) && TryCompleteThreadedLoad(resPath, allowBlocking: true))
		{
			if (!TryGetCachedStream(resPath, out stream))
			{
				return null;
			}
			return stream;
		}
		try
		{
			if (!ResourceLoader.Exists(resPath, ""))
			{
				if (TryCacheRawAudioStream(resPath, out stream))
				{
					return stream;
				}
				LogMissingAsset(resPath, null);
				return null;
			}
			stream = ResourceLoader.Load<AudioStream>(resPath, (string)null, EffectiveCacheMode);
			if (stream == null)
			{
				if (TryCacheRawAudioStream(resPath, out stream))
				{
					return stream;
				}
				LogMissingAsset(resPath, null);
				return null;
			}
			_cache[resPath] = stream;
			return stream;
		}
		catch (Exception ex)
		{
			LogMissingAsset(resPath, ex.Message);
			return null;
		}
	}

	private bool TryGetCachedStream(string resPath, out AudioStream? stream)
	{
		if (_cache.TryGetValue(resPath, out stream) && stream != null && GodotObject.IsInstanceValid((GodotObject)(object)stream))
		{
			return true;
		}
		stream = null;
		_cache.Remove(resPath);
		return false;
	}

	private bool TryCompleteThreadedLoad(string resPath, bool allowBlocking)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I8
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I8
		ThreadLoadStatus val = ResourceLoader.LoadThreadedGetStatus(resPath, (Array)null);
		if ((long)val == 1)
		{
			if (!allowBlocking)
			{
				return false;
			}
			return CompleteThreadedLoad(resPath);
		}
		if ((long)val == 3)
		{
			return CompleteThreadedLoad(resPath);
		}
		_threadedLoads.Remove(resPath);
		return true;
	}

	private bool CompleteThreadedLoad(string resPath)
	{
		Resource obj = ResourceLoader.LoadThreadedGet(resPath);
		_threadedLoads.Remove(resPath);
		AudioStream val = (AudioStream)(object)((obj is AudioStream) ? obj : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			_cache[resPath] = val;
			return true;
		}
		if (TryCacheRawAudioStream(resPath, out AudioStream _))
		{
			return true;
		}
		LogMissingAsset(resPath, null);
		return true;
	}

	private bool TryCacheRawAudioStream(string resPath, out AudioStream? stream)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		stream = null;
		if (!IsRawWavPath(resPath) || !FileAccess.FileExists(resPath))
		{
			return false;
		}
		stream = (AudioStream?)(object)AudioStreamWav.LoadFromFile(resPath, new Dictionary());
		if (stream == null || !GodotObject.IsInstanceValid((GodotObject)(object)stream))
		{
			stream = null;
			return false;
		}
		_cache[resPath] = stream;
		return true;
	}

	private static bool IsRawWavPath(string resPath)
	{
		return resPath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase);
	}

	private static float GetStreamLengthSeconds(AudioStream stream)
	{
		try
		{
			return Math.Max(0f, (float)stream.GetLength());
		}
		catch
		{
			return 0f;
		}
	}

	private void LogMissingAsset(string resPath, string? error)
	{
		long tickCount = Environment.TickCount64;
		if (!_missingAssetNextLogAt.TryGetValue(resPath, out var value) || tickCount >= value)
		{
			_missingAssetNextLogAt[resPath] = tickCount + 60000;
			if (string.IsNullOrWhiteSpace(error))
			{
				_warn(_missingAssetPrefix + resPath, 2);
			}
			else
			{
				_warn(_loadFailedPrefix + resPath + " err=" + error, 2);
			}
		}
	}

	private static void NoopWarn(string message, int priority)
	{
	}
}
