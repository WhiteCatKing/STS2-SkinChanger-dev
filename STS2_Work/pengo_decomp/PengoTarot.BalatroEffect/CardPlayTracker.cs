using System.Collections.Generic;
using Godot;

namespace PengoTarot.BalatroEffect;

public static class CardPlayTracker
{
	private sealed class PlayState
	{
		public float DragStartTime;

		public float? PlayEndTime;

		public int AxisMode;
	}

	public const float AttackTime = 0.12f;

	public const float ReleaseTime = 0.4f;

	public const float RotationAmplitude = 15f;

	public const float BaseSpeed = 2f;

	private static readonly Dictionary<string, PlayState> _states = new Dictionary<string, PlayState>();

	private static readonly HashSet<string> _pendingRefresh = new HashSet<string>();

	private static bool _refreshScheduled;

	private static float Now => (float)Time.GetTicksMsec() / 1000f;

	public static void MarkDragStarted(string cardId)
	{
		_states[cardId] = new PlayState
		{
			DragStartTime = Now,
			AxisMode = (int)(Time.GetTicksMsec() & 3)
		};
		DeferRefresh(cardId);
	}

	public static void MarkPlayFinished(string cardId)
	{
		if (_states.TryGetValue(cardId, out PlayState value) && !value.PlayEndTime.HasValue)
		{
			value.PlayEndTime = Now;
			DeferRefresh(cardId);
		}
	}

	private static void DeferRefresh(string cardId)
	{
		_pendingRefresh.Add(cardId);
		if (!_refreshScheduled)
		{
			_refreshScheduled = true;
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val == null)
			{
				FlushPendingRefresh();
			}
			else
			{
				val.ProcessFrame += FlushPendingRefresh;
			}
		}
	}

	private static void FlushPendingRefresh()
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			val.ProcessFrame -= FlushPendingRefresh;
		}
		_refreshScheduled = false;
		if (_pendingRefresh.Count == 0)
		{
			return;
		}
		List<string> list = new List<string>(_pendingRefresh);
		_pendingRefresh.Clear();
		foreach (string item in list)
		{
			ShaderController.RefreshAllCardsWithId(item);
		}
	}

	public static (float xRot, float yRot)? GetPlayRotation(string cardId)
	{
		if (!_states.TryGetValue(cardId, out PlayState value))
		{
			return null;
		}
		float now = Now;
		float num = now - value.DragStartTime;
		if (value.PlayEndTime.HasValue && now > value.PlayEndTime.Value + 0.4f)
		{
			_states.Remove(cardId);
			return null;
		}
		float num2 = ComputeIntensity(num, value);
		int axisMode = value.AxisMode;
		float num3 = num * 2f * num2;
		float num4 = 15f * num2;
		float num5 = Mathf.Sin(num3 * 2.3f) * 0.25f;
		var (num6, num7) = axisMode switch
		{
			0 => (Mathf.Sin(num3) * num4, Mathf.Cos(num3) * num4), 
			1 => (Mathf.Cos(num3) * num4, Mathf.Sin(num3) * num4), 
			2 => (Mathf.Sin(num3) * num4, (Mathf.Sin(num3) + num5) * num4), 
			3 => (Mathf.Sin(num3) * num4, (Mathf.Sin(0f - num3) + num5) * num4), 
			_ => (0f, 0f), 
		};
		return (num6 * 0.6f, num7 * 0.6f);
	}

	private static float ComputeIntensity(float elapsed, PlayState state)
	{
		if (elapsed < 0.12f)
		{
			return SmoothStep(elapsed / 0.12f);
		}
		if (!state.PlayEndTime.HasValue)
		{
			return 1f;
		}
		float num = Now - state.PlayEndTime.Value;
		return SmoothStep(1f - Mathf.Min(num / 0.4f, 1f));
	}

	private static float SmoothStep(float t)
	{
		return t * t * (3f - 2f * t);
	}
}
