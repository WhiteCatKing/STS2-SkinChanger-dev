using System;
using System.Collections.Generic;
using CznCore.Godot;
using CznCore.Settings;
using CznCore.Standalone;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;

namespace CznCore.Presentation;

internal sealed class CharacterDefeatLifecycle : IDisposable
{
	private readonly Func<Player, bool> _accepts;

	private readonly Func<SkinCharacterSettings> _settings;

	private readonly string _scene;

	private readonly string _deathVoice;

	private readonly string _failVoice;

	private readonly float _bodySeconds;

	private readonly CharacterSkinVoicePlayerRuntime _voice;

	private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

	private readonly Action<string> _warn;

	private Player? _deadPlayer;

	private Player? _finishedPlayer;

	private CanvasLayer? _overlay;

	private int _generation;

	private int _cgGeneration;

	private bool _disposed;

	public CharacterDefeatLifecycle(string owner, Func<Player, bool> accepts, Func<SkinCharacterSettings> settings, string scene, float bodySeconds, string deathVoice, string failVoice, Action<string> warn)
	{
		_accepts = accepts;
		_settings = settings;
		_scene = scene;
		_bodySeconds = bodySeconds;
		_deathVoice = deathVoice;
		_failVoice = failVoice;
		_warn = warn;
		_voice = new CharacterSkinVoicePlayerRuntime(settings(), owner + "DefeatVoice");
		settings().Subscribe(OnSettingsChanged);
		PresentationTransports.Ensure(PresentationTransport.CharacterLifecycle);
		PresentationTransports.Ensure(PresentationTransport.RoomsAndCombat);
		_subscriptions.Add(SkinEventHub.Subscribe<PlayerDeathResolvedEvent>(owner + " death", OnDeath));
		_subscriptions.Add(SkinEventHub.Subscribe<RunFinishedPresentationEvent>(owner + " failure", OnFinished));
		_subscriptions.Add(SkinEventHub.Subscribe<RoomEnteringEvent>(owner + " defeat cleanup", delegate
		{
			Cleanup();
		}));
		_subscriptions.Add(SkinEventHub.Subscribe<PresentationMainMenuEvent>(owner + " defeat menu cleanup", delegate
		{
			Cleanup();
		}));
		_subscriptions.Add(SkinEventHub.Subscribe(owner + " revived death reset", delegate(PlayerTurnStartedEvent e)
		{
			if (_deadPlayer == e.Player && e.Player.Creature.IsAlive)
			{
				_deadPlayer = null;
			}
		}));
		_subscriptions.Add(SkinEventHub.Subscribe<CombatStartingEvent>(owner + " defeat reset", delegate
		{
			Cleanup();
			_deadPlayer = null;
			_finishedPlayer = null;
		}));
		CharacterSkinModBootstrap.TrackInitializationRollback(Dispose);
	}

	private bool Accepts(Player player)
	{
		if (!_disposed && _accepts(player))
		{
			return LocalContext.IsMe(player);
		}
		return false;
	}

	private void OnDeath(PlayerDeathResolvedEvent e)
	{
		if (!e.IsAbandoned && Accepts(e.Player) && _deadPlayer != e.Player)
		{
			Cleanup();
			_deadPlayer = e.Player;
			PlayVoice(e.Player, _deathVoice);
			QueueCg(MathF.Max(e.DeathAnimationLength, _bodySeconds));
		}
	}

	private void OnFinished(RunFinishedPresentationEvent e)
	{
		if (!Accepts(e.Player) || _finishedPlayer == e.Player)
		{
			return;
		}
		_finishedPlayer = e.Player;
		if (_deadPlayer != e.Player)
		{
			QueueCg(_bodySeconds);
			if (e.IsFailure)
			{
				PlayVoice(e.Player, _failVoice);
			}
		}
	}

	private void PlayVoice(Player player, string path)
	{
		if (Accepts(player) && !string.IsNullOrWhiteSpace(path))
		{
			_voice.Configure(_settings());
			_voice.PlayRequiredPresentationForWithResult(player, path);
		}
	}

	private void QueueCg(float delay)
	{
		int cgGeneration = _cgGeneration;
		Schedule(delay, delegate
		{
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			if (cgGeneration == _cgGeneration)
			{
				SkinCharacterSettings skinCharacterSettings = _settings();
				if (skinCharacterSettings.ShowDefeatCgOverlay && skinCharacterSettings.DefeatCgOverlayMode != 0)
				{
					MainLoop mainLoop = Engine.GetMainLoop();
					SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
					if (val != null)
					{
						PackedScene val2 = ResourceLoader.Load<PackedScene>(_scene, (string)null, (CacheMode)1);
						if (val2 == null)
						{
							_warn("Missing defeat scene: " + _scene);
						}
						else
						{
							_overlay = val2.Instantiate<CanvasLayer>((GenEditState)0);
							int num = ((skinCharacterSettings.DefeatCgOverlayMode != DefeatCgOverlayMode.Type2 && (skinCharacterSettings.DefeatCgOverlayMode != DefeatCgOverlayMode.Random || Random.Shared.Next(2) != 1)) ? 1 : 2);
							((GodotObject)_overlay).Set(StringName.op_Implicit("variant"), Variant.op_Implicit(num));
							DefeatSceneSettings.Apply((Node)(object)_overlay, skinCharacterSettings);
							((Node)val.Root).AddChild((Node)(object)_overlay, false, (InternalMode)0);
						}
					}
				}
			}
		});
	}

	private void OnSettingsChanged(string? _)
	{
		if (_disposed)
		{
			return;
		}
		SkinCharacterSettings skinCharacterSettings = _settings();
		_voice.Configure(skinCharacterSettings);
		if (!skinCharacterSettings.EffectiveVoiceEnabled)
		{
			_voice.Stop();
		}
		if (!skinCharacterSettings.ShowDefeatCgOverlay || skinCharacterSettings.DefeatCgOverlayMode == DefeatCgOverlayMode.Off)
		{
			_cgGeneration++;
			if (GodotObject.IsInstanceValid((GodotObject)(object)_overlay))
			{
				_overlay.Visible = false;
				((Node)_overlay).QueueFree();
			}
			_overlay = null;
		}
		else if (GodotObject.IsInstanceValid((GodotObject)(object)_overlay))
		{
			DefeatSceneSettings.Apply((Node)(object)_overlay, skinCharacterSettings);
		}
	}

	private void Schedule(float delay, Action action)
	{
		int generation = _generation;
		if (delay <= 0f)
		{
			Run();
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			GodotScheduler.OneShot((Node)(object)val.Root, delay, Run);
		}
		void Run()
		{
			if (_disposed || generation != _generation)
			{
				return;
			}
			try
			{
				action();
			}
			catch (Exception ex)
			{
				_warn("Defeat presentation failed: " + ex.Message);
				Cleanup();
			}
		}
	}

	public void Cleanup()
	{
		_generation++;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_overlay))
		{
			_overlay.Visible = false;
			((Node)_overlay).QueueFree();
		}
		_overlay = null;
		_voice.TrimCache(stopPlayback: true);
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		Cleanup();
		_settings().Unsubscribe(OnSettingsChanged);
		foreach (IDisposable subscription in _subscriptions)
		{
			subscription.Dispose();
		}
		_subscriptions.Clear();
	}
}
