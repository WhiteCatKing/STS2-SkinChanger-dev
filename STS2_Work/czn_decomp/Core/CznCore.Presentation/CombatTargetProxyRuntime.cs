using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace CznCore.Presentation;

internal sealed class CombatTargetProxyRuntime : Node2D
{
	internal const string HitRequestedSignal = "czn_target_proxy_hit_requested";

	internal const string TerminalHitRequestedSignal = "czn_target_proxy_terminal_hit_requested";

	internal const string HitFreezeRequestedSignal = "czn_target_proxy_hit_freeze_requested";

	private static readonly string[] IdleFallbackAnimations = new string[10] { "idle_loop", "idle", "awake_loop", "idle_normal", "normal_idle", "neutral", "normal", "standby", "stand", "wait" };

	private static readonly string[] HitFallbackAnimations = new string[3] { "hurt", "hit", "normal_hit" };

	private readonly List<GodotObject> _activeHitEntries = new List<GodotObject>();

	internal void BindRouteCommandSignals()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)this).Connect(SignalName.Ready, Callable.From<bool>((Func<bool>)(() => RestoreMirroredState())), 4u);
		BindCommandSignal(StringName.op_Implicit("czn_target_proxy_hit_requested"), Callable.From((Action)HandleHitRequested));
		BindCommandSignal(StringName.op_Implicit("czn_target_proxy_terminal_hit_requested"), Callable.From((Action)HandleTerminalHitRequested));
		BindCommandSignal(StringName.op_Implicit("czn_target_proxy_hit_freeze_requested"), Callable.From((Action)HandleHitFreezeRequested));
	}

	private bool RestoreMirroredState()
	{
		bool restored = false;
		VisitMirroredSpines(delegate(Node spine)
		{
			restored |= TryRestoreSpine(spine);
		});
		return restored;
	}

	private int PlayMirroredHit(bool restoreState)
	{
		_activeHitEntries.Clear();
		int played = 0;
		VisitMirroredSpines(delegate(Node spine)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			bool restoreState2 = restoreState;
			Variant meta = ((GodotObject)this).GetMeta(StringName.op_Implicit("czn_proxy_debug"), Variant.op_Implicit(false));
			if (TryPlayHit(spine, restoreState2, ((Variant)(ref meta)).AsBool(), out GodotObject hitEntry))
			{
				played++;
				if (hitEntry != null)
				{
					_activeHitEntries.Add(hitEntry);
				}
			}
		});
		return played;
	}

	private int FreezeMirroredHit()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (GodotObject activeHitEntry in _activeHitEntries)
		{
			if (GodotObject.IsInstanceValid(activeHitEntry) && activeHitEntry.HasMethod(StringName.op_Implicit("set_time_scale")))
			{
				activeHitEntry.Call(StringName.op_Implicit("set_time_scale"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
				num++;
			}
		}
		return num;
	}

	private void BindCommandSignal(StringName signalName, Callable handler)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)this).AddUserSignal(StringName.op_Implicit(signalName), (Array)null);
		Error val = ((GodotObject)this).Connect(signalName, handler, 0u);
		if ((int)val != 0)
		{
			throw new InvalidOperationException("Target proxy command signal bind failed: " + StringName.op_Implicit(signalName) + " (" + ((object)(Error)(ref val)/*cast due to .constrained prefix*/).ToString() + ")");
		}
	}

	private void HandleHitRequested()
	{
		PlayMirroredHit(restoreState: true);
	}

	private void HandleTerminalHitRequested()
	{
		PlayMirroredHit(restoreState: false);
	}

	private void HandleHitFreezeRequested()
	{
		FreezeMirroredHit();
	}

	private void VisitMirroredSpines(Action<Node> visit)
	{
		VisitMirroredSpines((Node)(object)this, visit);
	}

	private static void VisitMirroredSpines(Node root, Action<Node> visit)
	{
		if (((GodotObject)root).HasMeta(StringName.op_Implicit("czn_spine_state")) && ((GodotObject)root).HasMethod(StringName.op_Implicit("get_animation_state")) && ((GodotObject)root).HasMethod(StringName.op_Implicit("get_skeleton")))
		{
			visit(root);
		}
		foreach (Node child in root.GetChildren(false))
		{
			VisitMirroredSpines(child, visit);
		}
	}

	private static bool TryRestoreSpine(Node spine)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!TryGetSpineContext(spine, out Dictionary visualState, out GodotObject animationState, out GodotObject skeleton, out GodotObject data))
			{
				return false;
			}
			ApplySkin(visualState, skeleton, data);
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			Variant val = default(Variant);
			if (visualState.TryGetValue(Variant.op_Implicit("tracks"), ref val))
			{
				Variant val5 = default(Variant);
				foreach (Variant item in ((Variant)(ref val)).AsGodotArray())
				{
					Variant current = item;
					Dictionary val2 = ((Variant)(ref current)).AsGodotDictionary();
					string @string = GetString(val2, "anim");
					if (!string.IsNullOrEmpty(@string) && HasAnimation(data, @string))
					{
						if (!flag && animationState.HasMethod(StringName.op_Implicit("clear_tracks")))
						{
							animationState.Call(StringName.op_Implicit("clear_tracks"), Array.Empty<Variant>());
							flag = true;
						}
						int @int = GetInt(val2, "track");
						bool @bool = GetBool(val2, "loop", defaultValue: true);
						Variant val3 = animationState.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
						{
							Variant.op_Implicit(@string),
							Variant.op_Implicit(@bool),
							Variant.op_Implicit(@int)
						});
						GodotObject val4 = ((Variant)(ref val3)).AsGodotObject();
						if (val4 != null && val2.TryGetValue(Variant.op_Implicit("time"), ref val5) && val4.HasMethod(StringName.op_Implicit("set_track_time")))
						{
							val4.Call(StringName.op_Implicit("set_track_time"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(Math.Max(0f, ((Variant)(ref val5)).AsSingle())) });
						}
						flag2 = true;
						flag3 = flag3 || @int == 0;
					}
				}
			}
			if (!flag3)
			{
				if (!flag && animationState.HasMethod(StringName.op_Implicit("clear_tracks")))
				{
					animationState.Call(StringName.op_Implicit("clear_tracks"), Array.Empty<Variant>());
				}
				flag3 = TryPlayIdleFallback(animationState, data);
			}
			SetPlaybackTimeScale(animationState, 1f);
			ApplyImmediately(spine, animationState, skeleton);
			return flag2 || flag3;
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Target proxy state restore failed: " + SkinLog.FormatException(ex));
			return false;
		}
	}

	private static bool TryPlayHit(Node spine, bool restoreState, bool diagnosticsEnabled, out GodotObject? hitEntry)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		hitEntry = null;
		try
		{
			if (!TryGetSpineContext(spine, out Dictionary visualState, out GodotObject animationState, out GodotObject skeleton, out GodotObject data))
			{
				return false;
			}
			string @string = GetString(visualState, "anim");
			string text = ResolveHitAnimation(data, @string);
			if (string.IsNullOrEmpty(text))
			{
				if (diagnosticsEnabled)
				{
					SkinLog.Info("Target proxy hit skipped node=" + NodePath.op_Implicit(spine.GetPath()) + " source=" + @string + " reason=no-compatible-animation");
				}
				return false;
			}
			SetPlaybackTimeScale(animationState, 1f);
			Variant val = animationState.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
			{
				Variant.op_Implicit(text),
				Variant.op_Implicit(false),
				Variant.op_Implicit(0)
			});
			hitEntry = ((Variant)(ref val)).AsGodotObject();
			if (restoreState && animationState.HasMethod(StringName.op_Implicit("add_animation")))
			{
				var (text2, flag) = ResolveRestoreAnimation(visualState, data, @string);
				if (!string.IsNullOrEmpty(text2))
				{
					animationState.Call(StringName.op_Implicit("add_animation"), (Variant[])(object)new Variant[4]
					{
						Variant.op_Implicit(text2),
						Variant.op_Implicit(flag),
						Variant.op_Implicit(0),
						Variant.op_Implicit(0f)
					});
				}
			}
			ApplyImmediately(spine, animationState, skeleton);
			if (diagnosticsEnabled)
			{
				SkinLog.Info("Target proxy hit node=" + NodePath.op_Implicit(spine.GetPath()) + " source=" + @string + " selected=" + text + " track=0 restoreState=" + restoreState + " entryValid=" + (hitEntry != null));
			}
			return true;
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Target proxy hit playback failed: " + SkinLog.FormatException(ex));
			return false;
		}
	}

	private static bool TryGetSpineContext(Node spine, out Dictionary visualState, out GodotObject animationState, out GodotObject skeleton, out GodotObject data)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		visualState = new Dictionary();
		animationState = null;
		skeleton = null;
		data = null;
		if (!((GodotObject)spine).HasMeta(StringName.op_Implicit("czn_spine_state")))
		{
			return false;
		}
		StringName obj = StringName.op_Implicit("czn_spine_state");
		Variant val = default(Variant);
		val = ((GodotObject)spine).GetMeta(obj, val);
		visualState = ((Variant)(ref val)).AsGodotDictionary();
		val = ((GodotObject)spine).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
		animationState = ((Variant)(ref val)).AsGodotObject();
		val = ((GodotObject)spine).Call(StringName.op_Implicit("get_skeleton"), Array.Empty<Variant>());
		skeleton = ((Variant)(ref val)).AsGodotObject();
		if (animationState == null || skeleton == null || !skeleton.HasMethod(StringName.op_Implicit("get_data")))
		{
			return false;
		}
		val = skeleton.Call(StringName.op_Implicit("get_data"), Array.Empty<Variant>());
		data = ((Variant)(ref val)).AsGodotObject();
		return data != null;
	}

	private static void ApplySkin(Dictionary visualState, GodotObject skeleton, GodotObject data)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		string @string = GetString(visualState, "skin");
		if (string.IsNullOrEmpty(@string) || !data.HasMethod(StringName.op_Implicit("find_skin")))
		{
			return;
		}
		Variant val = data.Call(StringName.op_Implicit("find_skin"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(@string) });
		if (((Variant)(ref val)).AsGodotObject() != null && skeleton.HasMethod(StringName.op_Implicit("set_skin_by_name")))
		{
			skeleton.Call(StringName.op_Implicit("set_skin_by_name"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(@string) });
			if (skeleton.HasMethod(StringName.op_Implicit("set_slots_to_setup_pose")))
			{
				skeleton.Call(StringName.op_Implicit("set_slots_to_setup_pose"), Array.Empty<Variant>());
			}
		}
	}

	private static string ResolveHitAnimation(GodotObject data, string mirroredAnimation)
	{
		if (mirroredAnimation.Contains("sleep", StringComparison.OrdinalIgnoreCase) && HasAnimation(data, "hurt_sleeping"))
		{
			return "hurt_sleeping";
		}
		if (mirroredAnimation.Contains("stun", StringComparison.OrdinalIgnoreCase) && HasAnimation(data, "hurt_stunned"))
		{
			return "hurt_stunned";
		}
		if (mirroredAnimation.Contains("awake", StringComparison.OrdinalIgnoreCase) && HasAnimation(data, "hurt_awake"))
		{
			return "hurt_awake";
		}
		string[] hitFallbackAnimations = HitFallbackAnimations;
		foreach (string text in hitFallbackAnimations)
		{
			if (HasAnimation(data, text))
			{
				return text;
			}
		}
		return string.Empty;
	}

	private static (string Animation, bool Loop) ResolveRestoreAnimation(Dictionary visualState, GodotObject data, string mirroredAnimation)
	{
		if (!string.IsNullOrEmpty(mirroredAnimation) && HasAnimation(data, mirroredAnimation))
		{
			return (Animation: mirroredAnimation, Loop: ResolveTrackZeroLoop(visualState));
		}
		return (Animation: ResolveIdleFallback(data), Loop: true);
	}

	private static bool ResolveTrackZeroLoop(Dictionary visualState)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Variant val = default(Variant);
		if (!visualState.TryGetValue(Variant.op_Implicit("tracks"), ref val))
		{
			return true;
		}
		foreach (Variant item in ((Variant)(ref val)).AsGodotArray())
		{
			Variant current = item;
			Dictionary dictionary = ((Variant)(ref current)).AsGodotDictionary();
			if (GetInt(dictionary, "track") == 0)
			{
				return GetBool(dictionary, "loop", defaultValue: true);
			}
		}
		return true;
	}

	private static bool TryPlayIdleFallback(GodotObject animationState, GodotObject data)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		string text = ResolveIdleFallback(data);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		animationState.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
		{
			Variant.op_Implicit(text),
			Variant.op_Implicit(true),
			Variant.op_Implicit(0)
		});
		return true;
	}

	private static string ResolveIdleFallback(GodotObject data)
	{
		string[] idleFallbackAnimations = IdleFallbackAnimations;
		foreach (string text in idleFallbackAnimations)
		{
			if (HasAnimation(data, text))
			{
				return text;
			}
		}
		return string.Empty;
	}

	private static bool HasAnimation(GodotObject data, string animation)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (data.HasMethod(StringName.op_Implicit("find_animation")))
		{
			Variant val = data.Call(StringName.op_Implicit("find_animation"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(animation) });
			return ((Variant)(ref val)).AsGodotObject() != null;
		}
		return false;
	}

	private static void SetPlaybackTimeScale(GodotObject animationState, float scale)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (animationState.HasMethod(StringName.op_Implicit("set_time_scale")))
		{
			animationState.Call(StringName.op_Implicit("set_time_scale"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(scale) });
		}
	}

	private static void ApplyImmediately(Node spine, GodotObject animationState, GodotObject skeleton)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (animationState.HasMethod(StringName.op_Implicit("apply")))
		{
			animationState.Call(StringName.op_Implicit("apply"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(skeleton) });
		}
		if (((GodotObject)spine).HasMethod(StringName.op_Implicit("update_skeleton")))
		{
			((GodotObject)spine).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
		}
	}

	private static string GetString(Dictionary dictionary, string key)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Variant val = default(Variant);
		if (!dictionary.TryGetValue(Variant.op_Implicit(key), ref val))
		{
			return string.Empty;
		}
		return ((Variant)(ref val)).AsString();
	}

	private static int GetInt(Dictionary dictionary, string key)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Variant val = default(Variant);
		if (!dictionary.TryGetValue(Variant.op_Implicit(key), ref val))
		{
			return 0;
		}
		return ((Variant)(ref val)).AsInt32();
	}

	private static bool GetBool(Dictionary dictionary, string key, bool defaultValue)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Variant val = default(Variant);
		if (!dictionary.TryGetValue(Variant.op_Implicit(key), ref val))
		{
			return defaultValue;
		}
		return ((Variant)(ref val)).AsBool();
	}
}
