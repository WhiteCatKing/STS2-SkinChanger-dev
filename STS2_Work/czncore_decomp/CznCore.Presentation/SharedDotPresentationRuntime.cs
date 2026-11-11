using System;
using System.Collections.Generic;
using CznCore.Godot;
using CznCore.Settings;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznCore.Presentation;

internal sealed class SharedDotPresentationRuntime
{
	private static readonly CombatCreatureSpineResolver Spines = new CombatCreatureSpineResolver();

	private static readonly IReadOnlyDictionary<string, IReadOnlyList<CombatCameraShakeTrack>> Shakes = new Dictionary<string, IReadOnlyList<CombatCameraShakeTrack>>(StringComparer.Ordinal)
	{
		["rift"] = new _003C_003Ez__ReadOnlyArray<CombatCameraShakeTrack>(new CombatCameraShakeTrack[2]
		{
			new CombatCameraShakeTrack(0f, CombatOfficialCameraShakeProfiles.VibrationUp),
			new CombatCameraShakeTrack(0.133f, CombatOfficialCameraShakeProfiles.VibrationLeft)
		}),
		["pain"] = new _003C_003Ez__ReadOnlySingleElementList<CombatCameraShakeTrack>(new CombatCameraShakeTrack(0f, CombatOfficialCameraShakeProfiles.CurseImpact)),
		["pain_multi"] = new _003C_003Ez__ReadOnlySingleElementList<CombatCameraShakeTrack>(new CombatCameraShakeTrack(0f, CombatOfficialCameraShakeProfiles.CurseImpact))
	};

	private static readonly CombatRadialRgbBlurSpec CurseBlur = new CombatRadialRgbBlurSpec(1f, -1f, 10f, new Vector2(1000f, 1000f), GlowMask: true, 0f, "0,0,1,1", 0.01f, 0.0567f, 0f, "0.1,0.9111111,0.41944444,0.925");

	private readonly CombatPresentationAudiencePolicy _audience;

	private readonly Func<bool> _effectsEnabled;

	private readonly Func<bool> _particlesEnabled;

	private readonly Func<float> _brightness;

	private readonly Func<float> _opacity;

	private readonly Action<string> _warn;

	public CharacterEffectEngine Effects { get; }

	public SharedDotPresentationRuntime(string ownerId, SkinCharacterSettings settings, Func<Player?, bool> isCharacterPlayer, Action<string> warn)
		: this(ownerId, new CombatPresentationAudiencePolicy(settings, isCharacterPlayer), () => settings.EffectiveEffectsEnabled, () => settings.EffectiveParticlesEnabled, () => settings.EffectBrightness, () => settings.EffectOpacity, warn)
	{
	}

	public SharedDotPresentationRuntime(CombatPresentationAudiencePolicy audience, Func<bool> effectsEnabled, Func<bool> particlesEnabled, string settingsGroup, Action<Node2D> applySettings, Action<string> warn)
		: this(settingsGroup, audience, effectsEnabled, particlesEnabled, () => 1f, () => 1f, warn)
	{
	}

	private SharedDotPresentationRuntime(string ownerId, CombatPresentationAudiencePolicy audience, Func<bool> effectsEnabled, Func<bool> particlesEnabled, Func<float> brightness, Func<float> opacity, Action<string> warn)
	{
		_audience = audience;
		_effectsEnabled = effectsEnabled;
		_particlesEnabled = particlesEnabled;
		_brightness = brightness;
		_opacity = opacity;
		_warn = warn;
		Effects = new CharacterEffectEngine(new CharacterEffectEngineOptions(ownerId + "Dot", new SceneEffectCatalog(SharedDotRouteScenes.ScenePaths, Array.Empty<string>()), CombatWarmupPolicyProfile.Default, new SkinDiagnosticsRuntime(() => true, 16.7), effectsEnabled, delegate(Node2D effect, CombatPresentationEffectCue _)
		{
			Start(effect);
		})
		{
			BuildScene = SharedDotRouteScenes.Build
		});
	}

	public void Play(Creature target, Creature? applier, string route)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		if (!_effectsEnabled())
		{
			return;
		}
		Player val = ((applier != null) ? applier.Player : null);
		if (val != null && !_audience.Allows(val, CombatPresentationChannel.Effects))
		{
			return;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		NCreature val2 = ((instance != null) ? instance.GetCreatureNode(target) : null);
		if (val2 == null)
		{
			return;
		}
		Node2D val3 = null;
		int generation = 0;
		try
		{
			Node2D val4 = CombatTargetSurfaceAnchorRuntime.Resolve(val2, "CznDotAnchor", "CznCore DOT anchor: ", null, null, CombatTargetSurfaceAnchorMode.HitboxCenter);
			if (val4 == null)
			{
				return;
			}
			object obj;
			if (applier != null)
			{
				CombatCreatureSpineResolver spines = Spines;
				NCombatRoom instance2 = NCombatRoom.Instance;
				obj = spines.Find((instance2 != null) ? instance2.GetCreatureNode(applier) : null);
			}
			else
			{
				obj = null;
			}
			Node2D val5 = (Node2D)obj;
			Vector2 globalScale = val4.GlobalScale;
			Vector2 val6 = ((Vector2)(ref globalScale)).Abs();
			Vector2 val7;
			if (val5 == null || !(val6.X > 0.0001f) || !(val6.Y > 0.0001f))
			{
				val7 = Vector2.One;
			}
			else
			{
				globalScale = val5.GlobalScale;
				val7 = ((Vector2)(ref globalScale)).Abs() / val6;
			}
			Vector2 scale = val7;
			val3 = Effects.AcquireScene(SharedDotRouteScenes.ScenePath(route), out generation);
			if (val3 == null)
			{
				return;
			}
			val3.Scale = scale;
			SharedDotRouteScenes.Roll(val3);
			((Node)val4).AddChild((Node)(object)val3, false, (InternalMode)0);
			CombatEffectRuntime.ApplyRootVisualSettings((CanvasItem)(object)val3, _brightness(), _opacity());
			Start(val3);
			Node2D played = val3;
			int playedGeneration = generation;
			GodotScheduler.OneShot((Node)(object)val3, Math.Max(0.01f, SharedDotRouteScenes.Lifetime(val3)), delegate
			{
				Effects.Release(played, playedGeneration);
			});
			if (route != "rift")
			{
				CombatRadialRgbBlurRuntime.Play(val4, Vector2.Zero, scale, CurseBlur);
			}
			if (Shakes.TryGetValue(route, out IReadOnlyList<CombatCameraShakeTrack> value))
			{
				Node2D val8 = Spines.Find(val2);
				if (val8 != null)
				{
					_003C_003Ez__ReadOnlySingleElementList<Node2D> anchors = new _003C_003Ez__ReadOnlySingleElementList<Node2D>(val8);
					IReadOnlyList<CombatCameraShakeTrack> tracks = value;
					Rect2 viewportRect = ((CanvasItem)val2).GetViewportRect();
					CombatCameraShakeRuntime.PlayAdditivePath(anchors, tracks, ((Rect2)(ref viewportRect)).Size, 1080f, (TransitionType)7, (EaseType)2);
				}
			}
		}
		catch (Exception ex)
		{
			if (val3 != null)
			{
				Effects.Release(val3, generation);
			}
			_warn("DOT presentation failed; native damage continues: " + ex.Message);
		}
	}

	private void Start(Node2D effect)
	{
		CombatEffectRuntime.StartSpineAnimationsFromMetadata((Node)(object)effect, "czn_spine_animation_name", "animation", loop: false, 1f, _warn);
		if (_particlesEnabled())
		{
			CombatEffectRuntime.StartParticles((Node)(object)effect, _warn);
		}
		else
		{
			CombatEffectRuntime.StopParticles((Node)(object)effect);
		}
	}
}
