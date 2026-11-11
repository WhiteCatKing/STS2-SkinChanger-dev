using System;
using Godot;

namespace CznCore.Godot;

public static class GodotScheduler
{
	public static void Deferred(Action action)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From(action);
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	public static void Delay(float seconds, Action action)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		((SceneTree)Engine.GetMainLoop()).CreateTimer((double)seconds, true, false, false).Timeout += action;
	}

	public static bool OneShot(Node parent, float seconds, Action action)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		if (!GodotNodeUtil.IsAlive((GodotObject?)(object)parent))
		{
			return false;
		}
		Timer timer = new Timer
		{
			OneShot = true,
			Autostart = false,
			WaitTime = seconds
		};
		parent.AddChild((Node)(object)timer, false, (InternalMode)0);
		timer.Timeout += delegate
		{
			if (GodotNodeUtil.IsAlive((GodotObject?)(object)timer))
			{
				((Node)timer).QueueFree();
			}
			action();
		};
		timer.Start(-1.0);
		return true;
	}

	public static void DeferredFrames(Node root, int frames, Action<Node> action)
	{
		if (frames <= 0 || !GodotNodeUtil.IsAlive((GodotObject?)(object)root))
		{
			return;
		}
		Deferred(delegate
		{
			if (GodotNodeUtil.IsAlive((GodotObject?)(object)root))
			{
				action(root);
				DeferredFrames(root, frames - 1, action);
			}
		});
	}
}
