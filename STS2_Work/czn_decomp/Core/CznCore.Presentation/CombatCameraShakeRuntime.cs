using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;

namespace CznCore.Presentation;

internal static class CombatCameraShakeRuntime
{
	private sealed class AdditivePlaybackState
	{
		private Vector2 _appliedOffset;

		private Vector2 _lastWrittenPosition;

		public Tween? Tween { get; set; }

		public bool IsActive { get; private set; }

		private int Generation { get; set; }

		public int Begin()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			_appliedOffset = Vector2.Zero;
			Tween = null;
			IsActive = true;
			return ++Generation;
		}

		public bool IsCurrent(int generation)
		{
			if (IsActive)
			{
				return generation == Generation;
			}
			return false;
		}

		public void ApplyOffset(Node2D anchor, Vector2 offset)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = (WrittenSinceLastStep(anchor) ? anchor.Position : (anchor.Position - _appliedOffset));
			anchor.Position = val + offset;
			_lastWrittenPosition = anchor.Position;
			_appliedOffset = offset;
		}

		public void RemoveOffset(Node2D anchor)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!WrittenSinceLastStep(anchor))
			{
				anchor.Position -= _appliedOffset;
			}
			_appliedOffset = Vector2.Zero;
			_lastWrittenPosition = anchor.Position;
		}

		private bool WrittenSinceLastStep(Node2D anchor)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			if (_appliedOffset != Vector2.Zero)
			{
				Vector2 position = anchor.Position;
				return !((Vector2)(ref position)).IsEqualApprox(_lastWrittenPosition);
			}
			return false;
		}

		public bool Complete(int generation)
		{
			if (!IsCurrent(generation))
			{
				return false;
			}
			Tween = null;
			IsActive = false;
			return true;
		}

		public void Cancel()
		{
			Tween? tween = Tween;
			if (tween != null)
			{
				tween.Kill();
			}
			Tween = null;
			IsActive = false;
			Generation++;
		}
	}

	private const float TimeEpsilonSeconds = 0.0005f;

	private const float DefaultReferenceHeight = 1080f;

	private static readonly ConditionalWeakTable<Node2D, AdditivePlaybackState> AdditivePlaybackStates = new ConditionalWeakTable<Node2D, AdditivePlaybackState>();

	public static int PlayAdditivePath(IReadOnlyList<Node2D?> anchors, IReadOnlyList<CombatCameraShakeTrack> tracks, Vector2 viewportSize, float referenceHeight = 1080f, TransitionType transition = 7L, EaseType ease = 2L)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		float viewportScale = ((viewportSize.Y > 0f && referenceHeight > 0f) ? MathF.Max(1f, viewportSize.Y / referenceHeight) : 1f);
		List<Node2D> list = new List<Node2D>();
		int num = 0;
		foreach (Node2D anchor in anchors)
		{
			if (anchor != null && GodotObject.IsInstanceValid((GodotObject)(object)anchor) && !list.Contains(anchor))
			{
				list.Add(anchor);
				if (PlayAdditivePath(anchor, anchor.Position, tracks, viewportScale, transition, ease))
				{
					num++;
				}
			}
		}
		return num;
	}

	public static bool PlayAdditivePath(Node2D? anchor, Vector2 basePosition, IReadOnlyList<CombatCameraShakeTrack> tracks, float viewportScale = 1f, TransitionType transition = 7L, EaseType ease = 2L)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		if (anchor == null || !GodotObject.IsInstanceValid((GodotObject)(object)anchor))
		{
			return false;
		}
		IReadOnlyList<CombatCameraShakeKey> readOnlyList = BuildAdditivePath(tracks);
		if (readOnlyList.Count == 0)
		{
			return false;
		}
		float num = MathF.Max(0f, viewportScale);
		AdditivePlaybackState state = AdditivePlaybackStates.GetOrCreateValue(anchor);
		if (state.IsActive)
		{
			state.Cancel();
			state.RemoveOffset(anchor);
		}
		int generation = state.Begin();
		state.ApplyOffset(anchor, readOnlyList[0].Offset * num);
		if (readOnlyList.Count == 1)
		{
			CompleteAdditivePlayback(anchor, state, generation);
			return true;
		}
		Tween val = ((Node)anchor).CreateTween();
		val.SetTrans(transition);
		val.SetEase(ease);
		for (int i = 1; i < readOnlyList.Count; i++)
		{
			float y = readOnlyList[i].TimeSeconds - readOnlyList[i - 1].TimeSeconds;
			val.TweenMethod(Callable.From<Vector2>((Action<Vector2>)delegate(Vector2 offset)
			{
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				if (state.IsCurrent(generation) && GodotObject.IsInstanceValid((GodotObject)(object)anchor))
				{
					state.ApplyOffset(anchor, offset);
				}
			}), Variant.op_Implicit(readOnlyList[i - 1].Offset * num), Variant.op_Implicit(readOnlyList[i].Offset * num), (double)MathF.Max(0.001f, y));
		}
		state.Tween = val;
		val.TweenCallback(Callable.From((Action)delegate
		{
			CompleteAdditivePlayback(anchor, state, generation);
		}));
		return true;
	}

	public static bool CancelAdditivePath(Node2D? anchor)
	{
		if (anchor == null || !GodotObject.IsInstanceValid((GodotObject)(object)anchor) || !AdditivePlaybackStates.TryGetValue(anchor, out AdditivePlaybackState value) || !value.IsActive)
		{
			return false;
		}
		value.Cancel();
		value.RemoveOffset(anchor);
		AdditivePlaybackStates.Remove(anchor);
		return true;
	}

	public static bool PlayPath(Node2D? anchor, Vector2 basePosition, IReadOnlyList<CombatCameraShakeKey> keys, float viewportScale = 1f, TransitionType transition = 7L, EaseType ease = 2L)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (anchor == null || !GodotObject.IsInstanceValid((GodotObject)(object)anchor) || keys.Count == 0)
		{
			return false;
		}
		PlayPathCore(anchor, basePosition, keys, viewportScale, transition, ease);
		return true;
	}

	private static Tween? PlayPathCore(Node2D anchor, Vector2 basePosition, IReadOnlyList<CombatCameraShakeKey> keys, float viewportScale, TransitionType transition, EaseType ease)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		float num = MathF.Max(0f, viewportScale);
		anchor.Position = basePosition + keys[0].Offset * num;
		if (keys.Count == 1)
		{
			return null;
		}
		Tween val = ((Node)anchor).CreateTween();
		val.SetTrans(transition);
		val.SetEase(ease);
		for (int i = 1; i < keys.Count; i++)
		{
			float y = keys[i].TimeSeconds - keys[i - 1].TimeSeconds;
			Vector2 val2 = basePosition + keys[i].Offset * num;
			val.TweenProperty((GodotObject)(object)anchor, NodePath.op_Implicit("position"), Variant.op_Implicit(val2), (double)MathF.Max(0.001f, y));
		}
		return val;
	}

	private static void CompleteAdditivePlayback(Node2D anchor, AdditivePlaybackState state, int generation)
	{
		if (state.Complete(generation))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)anchor))
			{
				state.RemoveOffset(anchor);
			}
			AdditivePlaybackStates.Remove(anchor);
		}
	}

	private static IReadOnlyList<CombatCameraShakeKey> BuildAdditivePath(IReadOnlyList<CombatCameraShakeTrack> tracks)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		if (tracks.Count == 0)
		{
			return Array.Empty<CombatCameraShakeKey>();
		}
		List<float> list = new List<float>();
		foreach (CombatCameraShakeTrack track in tracks)
		{
			if (track.Keys == null || track.Keys.Count == 0)
			{
				continue;
			}
			foreach (CombatCameraShakeKey key in track.Keys)
			{
				AddUniqueTime(list, track.StartSeconds + key.TimeSeconds);
			}
		}
		if (list.Count == 0)
		{
			return Array.Empty<CombatCameraShakeKey>();
		}
		AddUniqueTime(list, 0f);
		list.Sort();
		List<CombatCameraShakeKey> list2 = new List<CombatCameraShakeKey>(list.Count);
		foreach (float item in list)
		{
			Vector2 val = Vector2.Zero;
			foreach (CombatCameraShakeTrack track2 in tracks)
			{
				val += SampleTrack(track2, item);
			}
			list2.Add(new CombatCameraShakeKey(item, val));
		}
		return list2;
	}

	private static void AddUniqueTime(List<float> times, float time)
	{
		for (int i = 0; i < times.Count; i++)
		{
			if (MathF.Abs(times[i] - time) <= 0.0005f)
			{
				return;
			}
		}
		times.Add(time);
	}

	private static Vector2 SampleTrack(CombatCameraShakeTrack track, float absoluteTimeSeconds)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<CombatCameraShakeKey> keys = track.Keys;
		if (keys == null || keys.Count == 0)
		{
			return Vector2.Zero;
		}
		float num = absoluteTimeSeconds - track.StartSeconds;
		if (num < keys[0].TimeSeconds - 0.0005f || num > keys[keys.Count - 1].TimeSeconds + 0.0005f)
		{
			return Vector2.Zero;
		}
		if (num <= keys[0].TimeSeconds + 0.0005f)
		{
			return keys[0].Offset;
		}
		for (int i = 1; i < keys.Count; i++)
		{
			CombatCameraShakeKey combatCameraShakeKey = keys[i - 1];
			CombatCameraShakeKey combatCameraShakeKey2 = keys[i];
			if (num <= combatCameraShakeKey2.TimeSeconds + 0.0005f)
			{
				float num2 = combatCameraShakeKey2.TimeSeconds - combatCameraShakeKey.TimeSeconds;
				if (num2 <= 0.0005f)
				{
					return combatCameraShakeKey2.Offset;
				}
				float num3 = Math.Clamp((num - combatCameraShakeKey.TimeSeconds) / num2, 0f, 1f);
				Vector2 offset = combatCameraShakeKey.Offset;
				return ((Vector2)(ref offset)).Lerp(combatCameraShakeKey2.Offset, num3);
			}
		}
		return keys[keys.Count - 1].Offset;
	}
}
