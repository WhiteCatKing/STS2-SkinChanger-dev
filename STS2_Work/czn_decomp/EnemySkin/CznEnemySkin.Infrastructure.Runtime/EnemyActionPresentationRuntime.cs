using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class EnemyActionPresentationRuntime
{
	private sealed class ActorPresentationState
	{
		public int Generation { get; set; }

		public HashSet<Node2D> ActiveEffects { get; } = new HashSet<Node2D>();

		public HashSet<AudioStreamPlayer> ActiveAudioPlayers { get; } = new HashSet<AudioStreamPlayer>();
	}

	private sealed record AudioOwnership(string ProfileId, ActorPresentationState? ActorState, int Generation);

	private static readonly StringName SfxBus = new StringName("SFX");

	private static readonly StringName EmittingProperty = new StringName("emitting");

	private static readonly StringName RestartMethod = new StringName("restart");

	private static readonly StringName ParticleStartDelayMeta = new StringName("czn_particle_start_delay_seconds");

	private static readonly StringName ParticleEmissionWindowMeta = new StringName("czn_enemy_emission_window_seconds");

	private readonly Dictionary<string, AudioStream> _audioCache = new Dictionary<string, AudioStream>(StringComparer.Ordinal);

	private readonly Dictionary<string, PackedScene> _sceneCache = new Dictionary<string, PackedScene>(StringComparer.Ordinal);

	private readonly HashSet<string> _missingResources = new HashSet<string>(StringComparer.Ordinal);

	private readonly HashSet<string> _reportedSynchronousLoads = new HashSet<string>(StringComparer.Ordinal);

	private readonly EnemyEffectMaterialRuntime _effectMaterials;

	private readonly List<AudioStreamPlayer> _audioPlayers = new List<AudioStreamPlayer>();

	private readonly List<AudioStreamPlayer> _priorityAudioPlayers = new List<AudioStreamPlayer>();

	private readonly Dictionary<AudioStreamPlayer, AudioOwnership> _nonPriorityAudioOwners = new Dictionary<AudioStreamPlayer, AudioOwnership>();

	private readonly ConditionalWeakTable<NCreature, ActorPresentationState> _actorPresentationStates = new ConditionalWeakTable<NCreature, ActorPresentationState>();

	private readonly Dictionary<(string ProfileId, EnemyAction Action), int> _audioChoiceCursor = new Dictionary<(string, EnemyAction), int>();

	private Node? _audioParent;

	private readonly Dictionary<Node2D, EnemyAction> _activeEffects = new Dictionary<Node2D, EnemyAction>();

	private bool _effectsEnabled;

	private int _effectsGeneration;

	private int _nextPlayerIndex;

	private int _nextPriorityPlayerIndex;

	private readonly EnemyEffectRangeRuntime _effectRanges = new EnemyEffectRangeRuntime();

	internal int EffectsGeneration => _effectsGeneration;

	public EnemyActionPresentationRuntime(EnemyEffectMaterialRuntime effectMaterials, bool effectsEnabled = true)
	{
		_effectMaterials = effectMaterials;
		_effectsEnabled = effectsEnabled;
	}

	public void SetEffectsEnabled(bool enabled)
	{
		if (_effectsEnabled == enabled)
		{
			return;
		}
		_effectsEnabled = enabled;
		_effectsGeneration++;
		if (enabled)
		{
			return;
		}
		KeyValuePair<Node2D, EnemyAction>[] array = _activeEffects.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<Node2D, EnemyAction> keyValuePair = array[i];
			var (val2, action) = (KeyValuePair<Node2D, EnemyAction>)(ref keyValuePair);
			if (EnemyActionEffectPolicy.IsAttackOrSkill(action))
			{
				_activeEffects.Remove(val2);
				if (GodotObject.IsInstanceValid((GodotObject)(object)val2) && !((GodotObject)val2).IsQueuedForDeletion())
				{
					((CanvasItem)val2).Visible = false;
					((Node)val2).ProcessMode = (ProcessModeEnum)4;
					((Node)val2).QueueFree();
				}
			}
		}
		_sceneCache.Clear();
	}

	public void Prewarm(EnemyReplacementProfile profile)
	{
		foreach (EnemyActionPresentation value in profile.Monster.ActionPresentations.Values)
		{
			AudioStream stream;
			foreach (EnemyAudioCue audioChoice in value.AudioChoices)
			{
				TryLoadAudio(audioChoice.ResourcePath, out stream);
			}
			foreach (EnemyAudioLayerCue item in value.AudioLayers ?? Array.Empty<EnemyAudioLayerCue>())
			{
				TryLoadAudio(item.ResourcePath, out stream);
			}
		}
		foreach (EnemyAction prewarmEffectAction in profile.Monster.PrewarmEffectActions)
		{
			if (!EnemyActionEffectPolicy.ShouldPlay(prewarmEffectAction, _effectsEnabled))
			{
				continue;
			}
			foreach (EnemyEffectCue effect in profile.Monster.ActionPresentations[prewarmEffectAction].Effects)
			{
				TryLoadScene(effect.ScenePath, out PackedScene _);
			}
		}
	}

	public void ReleaseCachedResources()
	{
		if (_sceneCache.Count != 0 || _audioCache.Count != 0)
		{
			Entry.Log.Info("Released cached enemy presentation resources at a room boundary " + $"[scenes={_sceneCache.Count}, audio={_audioCache.Count}].", 1);
			_sceneCache.Clear();
			_audioCache.Clear();
		}
	}

	public bool CanPlayAudio(EnemyReplacementProfile profile, EnemyAction action)
	{
		if (profile.Monster.ActionPresentations.TryGetValue(action, out EnemyActionPresentation value))
		{
			AudioStream stream2;
			if (!value.AudioChoices.Any((EnemyAudioCue cue) => TryLoadAudio(cue.ResourcePath, out AudioStream _)))
			{
				return (value.AudioLayers ?? Array.Empty<EnemyAudioLayerCue>()).Any((EnemyAudioLayerCue cue) => TryLoadAudio(cue.ResourcePath, out stream2));
			}
			return true;
		}
		return false;
	}

	public bool CanPlayEffects(EnemyReplacementProfile profile, EnemyAction action)
	{
		PackedScene scene;
		if (EnemyActionEffectPolicy.ShouldPlay(action, _effectsEnabled) && profile.Monster.ActionPresentations.TryGetValue(action, out EnemyActionPresentation value) && value.Effects.Count > 0)
		{
			return value.Effects.All((EnemyEffectCue cue) => TryLoadScene(cue.ScenePath, out scene));
		}
		return false;
	}

	public void StopNonPriorityAudioForProfiles(IReadOnlySet<string> profileIds)
	{
		RemoveInvalidPlayers(_audioPlayers);
		foreach (AudioStreamPlayer audioPlayer in _audioPlayers)
		{
			if (_nonPriorityAudioOwners.TryGetValue(audioPlayer, out AudioOwnership value) && profileIds.Contains(value.ProfileId))
			{
				audioPlayer.Stop();
				audioPlayer.Stream = null;
				ForgetNonPriorityAudioOwnership(audioPlayer);
			}
		}
	}

	public void Play(NCreature creature, EnemyReplacementProfile profile, EnemyAction action, bool includeAudio = true, EnemyPresentationStance stance = EnemyPresentationStance.Normal)
	{
		PlayCore(creature, profile, action, includeAudio, null, 0, stance);
	}

	public void PlayActorIsolated(NCreature creature, EnemyReplacementProfile profile, EnemyAction action, bool includeAudio = true)
	{
		ActorPresentationState orCreateValue = _actorPresentationStates.GetOrCreateValue(creature);
		int generation = ResetActorPresentation(orCreateValue);
		PlayCore(creature, profile, action, includeAudio, orCreateValue, generation);
	}

	public void CancelActorPresentation(NCreature creature)
	{
		if (_actorPresentationStates.TryGetValue(creature, out ActorPresentationState value))
		{
			ResetActorPresentation(value);
		}
	}

	private void PlayCore(NCreature creature, EnemyReplacementProfile profile, EnemyAction action, bool includeAudio, ActorPresentationState? actorState, int generation, EnemyPresentationStance stance = EnemyPresentationStance.Normal)
	{
		if (!profile.Monster.ActionPresentations.TryGetValue(action, out EnemyActionPresentation value))
		{
			return;
		}
		if (includeAudio)
		{
			PlayNextAudio(creature, profile, value, actorState, generation);
			PlayAudioLayers(creature, profile, value, actorState, generation);
		}
		if (!EnemyActionEffectPolicy.ShouldPlay(action, _effectsEnabled))
		{
			return;
		}
		Node2D val = ResolveEffectHost(creature, profile, action);
		if (val == null)
		{
			return;
		}
		foreach (EnemyEffectCue effect in value.Effects)
		{
			if (!effect.TargetHit && effect.MatchesStance(stance))
			{
				SpawnEffect(val, profile, action, effect, actorState, generation);
			}
		}
	}

	internal bool TryPlayTargetEffects(Node container, Vector2 globalPosition, EnemyReplacementProfile profile, EnemyAction action, int effectsGeneration, Node? backContainer = null, CanvasItem? followTarget = null, bool atEntry = false, bool centerOnly = false, IReadOnlyList<NCreature>? followSide = null, bool screenSpace = false, int? nativeHitIndex = null, EnemyPresentationStance stance = EnemyPresentationStance.Normal, Vector2? bodyPosition = null)
	{
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Expected O, but got Unknown
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		if (!_effectsEnabled || effectsGeneration != _effectsGeneration || !Live(container) || !profile.Monster.ActionPresentations.TryGetValue(action, out EnemyActionPresentation value))
		{
			return false;
		}
		EnemyEffectCue[] array = value.Effects.Where((EnemyEffectCue c) => c.MatchesTargetSurface(atEntry, centerOnly) && c.MatchesNativeHit(nativeHitIndex) && c.MatchesStance(stance)).ToArray();
		if (array.Length == 0 || array.Any((EnemyEffectCue c) => !TryLoadScene(c.ScenePath, out PackedScene _)) || (array.Any((EnemyEffectCue c) => c.ZIndex < 0) && !Live(backContainer)) || (followTarget != null && !Live((Node?)(object)followTarget)))
		{
			return false;
		}
		List<Node2D> list = new List<Node2D>();
		try
		{
			foreach (IGrouping<(bool, EnemyTargetOrigin), EnemyEffectCue> item in from c in array
				group c by (Back: c.ZIndex < 0, Origin: c.TargetOrigin))
			{
				Node2D anchor = new Node2D
				{
					Name = StringName.op_Implicit("CznEnemyTargetImpact")
				};
				list.Add(anchor);
				((Node)(item.Key.Item1 ? ((object)backContainer) : ((object)container))).AddChild((Node)(object)anchor, false, (InternalMode)0);
				anchor.GlobalPosition = ((item.Key.Item2 == EnemyTargetOrigin.Body) ? (bodyPosition ?? globalPosition) : globalPosition);
				if (screenSpace)
				{
					EnemyTargetAnchor.LockToScreen(anchor);
				}
				else if (followSide != null)
				{
					EnemyTargetAnchor.FollowSide(anchor, followSide);
				}
				else
				{
					NCreature val = (NCreature)(object)((followTarget is NCreature) ? followTarget : null);
					if (val != null)
					{
						EnemyTargetAnchor.FollowCreature(anchor, val, item.Key.Item2);
					}
					else if (followTarget != null)
					{
						EnemyTargetAnchor.Follow(anchor, followTarget);
					}
				}
				_activeEffects.Add(anchor, action);
				((Node)anchor).TreeExiting += delegate
				{
					_activeEffects.Remove(anchor);
				};
				foreach (EnemyEffectCue item2 in item)
				{
					SpawnEffect(anchor, profile, action, item2, null, 0);
				}
				if (((Node)anchor).GetChildCount(false) != item.Count((EnemyEffectCue c) => c.DelaySeconds <= 0f))
				{
					throw new InvalidOperationException("Incomplete immediate target group.");
				}
				Schedule((Node)(object)anchor, item.Max((EnemyEffectCue c) => c.DelaySeconds + c.LifetimeSeconds), delegate
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)anchor) && !((GodotObject)anchor).IsQueuedForDeletion())
					{
						((Node)anchor).QueueFree();
					}
				});
			}
			return true;
		}
		catch (Exception ex)
		{
			foreach (Node2D item3 in list)
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)item3))
				{
					((GodotObject)item3).Free();
				}
			}
			Entry.Log.Warn("Enemy target effect fallback: " + ex.Message, 1);
			return false;
		}
		static bool Live(Node? node)
		{
			if (node != null && GodotObject.IsInstanceValid((GodotObject)(object)node) && node.IsInsideTree())
			{
				return !((GodotObject)node).IsQueuedForDeletion();
			}
			return false;
		}
	}

	private void PlayAudioLayers(NCreature creature, EnemyReplacementProfile profile, EnemyActionPresentation presentation, ActorPresentationState? actorState, int generation)
	{
		foreach (EnemyAudioLayerCue item in presentation.AudioLayers ?? Array.Empty<EnemyAudioLayerCue>())
		{
			EnemyAudioLayerCue layer = item;
			if (layer.DelaySeconds <= 0.001f)
			{
				PlayLayer();
			}
			else
			{
				Schedule((Node)(object)creature, layer.DelaySeconds, PlayLayer);
			}
			void PlayLayer()
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)creature) && IsActorPresentationCurrent(actorState, generation))
				{
					PlayAudio(creature, new EnemyAudioCue(layer.ResourcePath, layer.VolumeLinear), isPriority: false, profile.Id, actorState, generation);
				}
			}
		}
	}

	public bool PlayAudioChoice(NCreature creature, EnemyReplacementProfile profile, EnemyAction action, int choiceIndex)
	{
		if (!profile.Monster.ActionPresentations.TryGetValue(action, out EnemyActionPresentation value) || value.AudioChoices.Count == 0)
		{
			return false;
		}
		int index = Math.Clamp(choiceIndex, 0, value.AudioChoices.Count - 1);
		return PlayAudio(creature, value.AudioChoices[index], action == EnemyAction.Death, profile.Id);
	}

	private void PlayNextAudio(NCreature creature, EnemyReplacementProfile profile, EnemyActionPresentation presentation, ActorPresentationState? actorState, int generation)
	{
		if (presentation.AudioChoices.Count != 0)
		{
			(string, EnemyAction) key = (profile.Id, presentation.Action);
			_audioChoiceCursor.TryGetValue(key, out var value);
			int num = ((presentation.AudioChoices.Count != 1) ? (value % presentation.AudioChoices.Count) : 0);
			_audioChoiceCursor[key] = (num + 1) % presentation.AudioChoices.Count;
			PlayAudio(creature, presentation.AudioChoices[num], presentation.Action == EnemyAction.Death, profile.Id, actorState, generation);
		}
	}

	internal static bool CanEmitAudio(NCreature creature)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)creature) && !((GodotObject)creature).IsQueuedForDeletion() && ((Node)creature).IsInsideTree())
		{
			return ((CanvasItem)creature).IsVisibleInTree();
		}
		return false;
	}

	private bool PlayAudio(NCreature creature, EnemyAudioCue cue, bool isPriority, string profileId, ActorPresentationState? actorState = null, int generation = 0)
	{
		if (!CanEmitAudio(creature) || !TryLoadAudio(cue.ResourcePath, out AudioStream stream))
		{
			return false;
		}
		for (int i = 0; i < 2; i++)
		{
			AudioStreamPlayer val = EnsureAudioPlayer(creature, isPriority);
			if (val == null)
			{
				return false;
			}
			try
			{
				val.Stop();
				val.Stream = stream;
				val.VolumeLinear = cue.VolumeLinear;
				val.Bus = SfxBus;
				val.Play(0f);
				if (!isPriority)
				{
					ForgetNonPriorityAudioOwnership(val);
					_nonPriorityAudioOwners[val] = new AudioOwnership(profileId, actorState, generation);
					actorState?.ActiveAudioPlayers.Add(val);
				}
				return true;
			}
			catch (ObjectDisposedException)
			{
				ForgetAudioPlayer(val);
			}
		}
		return false;
	}

	private bool TryLoadAudio(string resourcePath, out AudioStream? stream)
	{
		if (_audioCache.TryGetValue(resourcePath, out stream) && GodotObject.IsInstanceValid((GodotObject)(object)stream))
		{
			return true;
		}
		stream = null;
		if (_missingResources.Contains(resourcePath))
		{
			return false;
		}
		try
		{
			stream = ResourceLoader.Load<AudioStream>(resourcePath, (string)null, (CacheMode)1);
			if (stream != null)
			{
				_audioCache[resourcePath] = stream;
				return true;
			}
		}
		catch (Exception ex)
		{
			LogMissingResource(resourcePath, ex.Message);
			return false;
		}
		LogMissingResource(resourcePath, null);
		return false;
	}

	private AudioStreamPlayer? EnsureAudioPlayer(NCreature creature, bool isPriority)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		Node val = ResolveAudioParent(creature);
		if (val == null)
		{
			return null;
		}
		if (_audioParent != val || !IsUsableNode(_audioParent))
		{
			ResetAudioPlayerPools(val);
		}
		List<AudioStreamPlayer> list = (isPriority ? _priorityAudioPlayers : _audioPlayers);
		RemoveInvalidPlayers(list);
		foreach (AudioStreamPlayer item in list)
		{
			if (!item.Playing)
			{
				if (!isPriority)
				{
					ForgetNonPriorityAudioOwnership(item);
				}
				return item;
			}
		}
		if (list.Count < 4)
		{
			AudioStreamPlayer val2 = new AudioStreamPlayer
			{
				Name = StringName.op_Implicit(isPriority ? ("CznEnemySkinPriorityAudio_" + list.Count) : ("CznEnemySkinAudio_" + list.Count))
			};
			val.AddChild((Node)(object)val2, false, (InternalMode)0);
			list.Add(val2);
			_audioParent = val;
			return val2;
		}
		if (isPriority)
		{
			_nextPriorityPlayerIndex = (_nextPriorityPlayerIndex + 1) % list.Count;
			return list[_nextPriorityPlayerIndex];
		}
		_nextPlayerIndex = (_nextPlayerIndex + 1) % list.Count;
		return list[_nextPlayerIndex];
	}

	private static Node? ResolveAudioParent(NCreature creature)
	{
		Node instance = (Node)(object)NCombatRoom.Instance;
		if (IsUsableNode(instance))
		{
			return instance;
		}
		Node instance2 = (Node)(object)NGame.Instance;
		if (IsUsableNode(instance2))
		{
			return instance2;
		}
		try
		{
			SceneTree tree = ((Node)creature).GetTree();
			Node val = (Node)(object)((tree != null) ? tree.Root : null);
			return IsUsableNode(val) ? val : null;
		}
		catch (ObjectDisposedException)
		{
			return null;
		}
	}

	private void ResetAudioPlayerPools(Node targetParent)
	{
		AudioStreamPlayer[] array = _nonPriorityAudioOwners.Keys.ToArray();
		foreach (AudioStreamPlayer player in array)
		{
			ForgetNonPriorityAudioOwnership(player);
		}
		_audioPlayers.Clear();
		_priorityAudioPlayers.Clear();
		_nextPlayerIndex = 0;
		_nextPriorityPlayerIndex = 0;
		_audioParent = targetParent;
	}

	private void ForgetAudioPlayer(AudioStreamPlayer player)
	{
		RemovePlayerReference(_audioPlayers, player);
		RemovePlayerReference(_priorityAudioPlayers, player);
		ForgetNonPriorityAudioOwnership(player);
		_nextPlayerIndex = 0;
		_nextPriorityPlayerIndex = 0;
	}

	private void RemoveInvalidPlayers(List<AudioStreamPlayer> players)
	{
		for (int num = players.Count - 1; num >= 0; num--)
		{
			if (!IsUsableNode((Node?)(object)players[num]))
			{
				ForgetNonPriorityAudioOwnership(players[num]);
				players.RemoveAt(num);
			}
		}
	}

	private static void RemovePlayerReference(List<AudioStreamPlayer> players, AudioStreamPlayer target)
	{
		for (int num = players.Count - 1; num >= 0; num--)
		{
			if (players[num] == target)
			{
				players.RemoveAt(num);
			}
		}
	}

	private static bool IsUsableNode(Node? node)
	{
		if (node == null)
		{
			return false;
		}
		try
		{
			return GodotObject.IsInstanceValid((GodotObject)(object)node) && !((GodotObject)node).IsQueuedForDeletion();
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
	}

	private int ResetActorPresentation(ActorPresentationState actorState)
	{
		actorState.Generation++;
		Node2D[] array = actorState.ActiveEffects.ToArray();
		foreach (Node2D val in array)
		{
			if (IsUsableNode((Node?)(object)val))
			{
				((CanvasItem)val).Visible = false;
				((Node)val).QueueFree();
			}
		}
		actorState.ActiveEffects.Clear();
		AudioStreamPlayer[] array2 = actorState.ActiveAudioPlayers.ToArray();
		foreach (AudioStreamPlayer val2 in array2)
		{
			if (_nonPriorityAudioOwners.TryGetValue(val2, out AudioOwnership value) && value.ActorState == actorState)
			{
				if (IsUsableNode((Node?)(object)val2))
				{
					val2.Stop();
					val2.Stream = null;
				}
				_nonPriorityAudioOwners.Remove(val2);
			}
		}
		actorState.ActiveAudioPlayers.Clear();
		return actorState.Generation;
	}

	private void ForgetNonPriorityAudioOwnership(AudioStreamPlayer player)
	{
		if (_nonPriorityAudioOwners.Remove(player, out AudioOwnership value))
		{
			value.ActorState?.ActiveAudioPlayers.Remove(player);
		}
	}

	private static bool IsActorPresentationCurrent(ActorPresentationState? actorState, int generation)
	{
		if (actorState != null)
		{
			return actorState.Generation == generation;
		}
		return true;
	}

	private void SpawnEffect(Node2D host, EnemyReplacementProfile profile, EnemyAction action, EnemyEffectCue cue, ActorPresentationState? actorState, int generation)
	{
		int effectsGeneration = _effectsGeneration;
		if (cue.DelaySeconds <= 0.001f)
		{
			SpawnNow();
		}
		else
		{
			Schedule((Node)(object)host, cue.DelaySeconds, SpawnNow);
		}
		void SpawnNow()
		{
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			if ((EnemyActionEffectPolicy.IsAttackOrSkill(action) && (!_effectsEnabled || effectsGeneration != _effectsGeneration)) || !IsActorPresentationCurrent(actorState, generation) || !GodotObject.IsInstanceValid((GodotObject)(object)host) || ((GodotObject)host).IsQueuedForDeletion() || !TryLoadScene(cue.ScenePath, out PackedScene scene))
			{
				return;
			}
			Node val = null;
			try
			{
				val = scene.Instantiate((GenEditState)0);
				Node2D effect = (Node2D)(object)((val is Node2D) ? val : null);
				if (effect == null)
				{
					throw new InvalidOperationException("Enemy effect scene root must be Node2D.");
				}
				EnemyNodeSet nodes = EnemyNodeTraversal.Collect((Node)(object)effect);
				_effectMaterials.Apply((Node)(object)effect, nodes, cue.MaterialPolicy);
				Color modulate = ((CanvasItem)effect).Modulate;
				modulate.A *= cue.Opacity;
				((CanvasItem)effect).Modulate = modulate;
				((Node)effect).Name = StringName.op_Implicit($"CznEnemySkin_{action}_{((Node)effect).Name}");
				effect.Position = new Vector2(cue.OfficialPositionX, 0f - cue.OfficialPositionY);
				effect.Scale = new Vector2(cue.ScaleX, cue.ScaleY);
				effect.RotationDegrees = 0f - cue.OfficialRotationDegrees;
				_effectRanges.Apply(effect);
				EnemyPresentationDepthPolicy.PlaceEffect(host, effect, cue.ZIndex);
				val = null;
				actorState?.ActiveEffects.Add(effect);
				_activeEffects.Add(effect, action);
				((Node)effect).TreeExiting += delegate
				{
					_activeEffects.Remove(effect);
					actorState?.ActiveEffects.Remove(effect);
				};
				if (cue.SpineAnimationName != null)
				{
					StartSpineAnimations((Node)(object)effect, nodes, cue.SpineAnimationName, profile.Id);
				}
				if (cue.StartParticles)
				{
					StartParticles((Node)(object)effect, nodes, profile.Id);
				}
				Schedule((Node)(object)effect, cue.LifetimeSeconds, delegate
				{
					actorState?.ActiveEffects.Remove(effect);
					if (GodotObject.IsInstanceValid((GodotObject)(object)effect))
					{
						((Node)effect).QueueFree();
					}
				});
			}
			catch (Exception ex)
			{
				if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					((GodotObject)val).Free();
				}
				Entry.Log.Warn("Enemy action effect failed " + $"[profile={profile.Id}, action={action}, " + $"scene={cue.ScenePath}, error={ex.Message}].", 1);
			}
		}
	}

	private bool TryLoadScene(string resourcePath, out PackedScene? scene)
	{
		if (_sceneCache.TryGetValue(resourcePath, out scene) && GodotObject.IsInstanceValid((GodotObject)(object)scene))
		{
			return true;
		}
		scene = null;
		if (_missingResources.Contains(resourcePath))
		{
			return false;
		}
		try
		{
			ReportSynchronousLoad(resourcePath);
			scene = ResourceLoader.Load<PackedScene>(resourcePath, (string)null, (CacheMode)1);
			if (scene != null)
			{
				_sceneCache[resourcePath] = scene;
				return true;
			}
		}
		catch (Exception ex)
		{
			LogMissingResource(resourcePath, ex.Message);
			return false;
		}
		LogMissingResource(resourcePath, null);
		return false;
	}

	private static Node2D? ResolveEffectHost(NCreature creature, EnemyReplacementProfile profile, EnemyAction action)
	{
		if (creature.Visuals == null || !GodotObject.IsInstanceValid((GodotObject)(object)creature.Visuals))
		{
			return null;
		}
		if (TunnelerCompoundBodyAdapter.TryResolvePresentationHost(creature, profile, action, out Node2D host))
		{
			return host;
		}
		return ((Node)creature.Visuals).GetNodeOrNull<Node2D>(NodePath.op_Implicit("%Visuals"));
	}

	private static void StartSpineAnimations(Node effectRoot, EnemyNodeSet nodes, string animation, string profileId)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		foreach (Node spineSprite in nodes.SpineSprites)
		{
			MegaSprite val = new MegaSprite(Variant.op_Implicit((GodotObject)(object)spineSprite));
			SpineNodeExtensions.RunWhenSpineReady(effectRoot, val, (Action<MegaAnimationState>)delegate(MegaAnimationState state)
			{
				try
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)effectRoot) && !((GodotObject)effectRoot).IsQueuedForDeletion())
					{
						state.SetAnimation(animation, false, 0);
					}
				}
				catch (Exception ex)
				{
					Entry.Log.Warn("Enemy effect Spine playback failed " + $"[profile={profileId}, animation={animation}, " + "error=" + ex.Message + "].", 1);
				}
			});
		}
	}

	private static void StartParticles(Node effectRoot, EnemyNodeSet nodes, string profileId)
	{
		foreach (Node particle2 in nodes.Particles)
		{
			Node particle = particle2;
			float num = ReadFloatMeta(particle, ParticleStartDelayMeta);
			if (num <= 0.001f)
			{
				Start();
			}
			else
			{
				Schedule(effectRoot, num, Start);
			}
			void Start()
			{
				//IL_0052: Unknown result type (might be due to invalid IL or missing references)
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_007e: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)effectRoot) && !((GodotObject)effectRoot).IsQueuedForDeletion() && GodotObject.IsInstanceValid((GodotObject)(object)particle) && !((GodotObject)particle).IsQueuedForDeletion())
					{
						Node obj = particle;
						StringName emittingProperty = EmittingProperty;
						bool flag = false;
						((GodotObject)obj).Set(emittingProperty, Variant.From<bool>(ref flag));
						if (((GodotObject)particle).HasMethod(RestartMethod))
						{
							((GodotObject)particle).Call(RestartMethod, Array.Empty<Variant>());
						}
						Node obj2 = particle;
						StringName emittingProperty2 = EmittingProperty;
						flag = true;
						((GodotObject)obj2).Set(emittingProperty2, Variant.From<bool>(ref flag));
						float num2 = ReadFloatMeta(particle, ParticleEmissionWindowMeta);
						if (num2 > 0f)
						{
							Schedule(effectRoot, num2, delegate
							{
								//IL_0029: Unknown result type (might be due to invalid IL or missing references)
								if (GodotObject.IsInstanceValid((GodotObject)(object)particle) && !((GodotObject)particle).IsQueuedForDeletion())
								{
									Node obj3 = particle;
									StringName emittingProperty3 = EmittingProperty;
									bool flag2 = false;
									((GodotObject)obj3).Set(emittingProperty3, Variant.From<bool>(ref flag2));
								}
							});
						}
					}
				}
				catch (Exception ex)
				{
					Entry.Log.Warn("Enemy particle playback failed " + $"[profile={profileId}, node={particle.Name}, " + "error=" + ex.Message + "].", 1);
				}
			}
		}
	}

	private static float ReadFloatMeta(Node node, StringName key)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (((GodotObject)node).HasMeta(key))
			{
				Variant val = default(Variant);
				val = ((GodotObject)node).GetMeta(key, val);
				return Math.Max(0f, ((Variant)(ref val)).AsSingle());
			}
		}
		catch
		{
		}
		return 0f;
	}

	private static void Schedule(Node owner, float seconds, Action action)
	{
		SceneTree tree = owner.GetTree();
		if (tree != null)
		{
			tree.CreateTimer((double)Math.Max(0.001f, seconds), false, false, false).Timeout += action;
		}
	}

	private void ReportSynchronousLoad(string resourcePath)
	{
		try
		{
			if (!ResourceLoader.HasCached(resourcePath) && _reportedSynchronousLoads.Add(resourcePath))
			{
				Entry.Log.Warn("Enemy presentation resource was not preloaded and is being loaded synchronously [path=" + resourcePath + "].", 1);
			}
		}
		catch
		{
		}
	}

	private void LogMissingResource(string resourcePath, string? error)
	{
		if (_missingResources.Add(resourcePath))
		{
			Entry.Log.Warn("Enemy action resource is unavailable [path=" + resourcePath + (string.IsNullOrWhiteSpace(error) ? "]." : (", error=" + error + "].")), 1);
		}
	}
}
