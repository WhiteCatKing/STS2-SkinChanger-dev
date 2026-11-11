using System;
using CznCore.Godot;
using Godot;

namespace CznCore.Presentation.Vfx;

internal static class RenderingServerVfxLayerMount
{
	public static RenderingServerVfxLayer? EnsureMounted(ref RenderingServerVfxLayer? layer, RenderingServerVfxLayerMountSpec spec, Action<string>? debug = null, Action<string>? warn = null)
	{
		if (spec == null)
		{
			throw new ArgumentNullException("spec");
		}
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)layer))
		{
			return layer;
		}
		layer = null;
		Node val = ResolveParent(spec.Parent);
		if (val == null)
		{
			warn?.Invoke("RenderingServer VFX layer mount skipped: SceneTree root unavailable.");
			return null;
		}
		int num = Math.Max(0, spec.MaxCapacity);
		int prewarmCount = Math.Clamp(spec.PrewarmCount, 0, num);
		RenderingServerVfxLayer renderingServerVfxLayer = new RenderingServerVfxLayer();
		((Node)renderingServerVfxLayer).Name = StringName.op_Implicit(spec.Name);
		renderingServerVfxLayer.Enabled = spec.Enabled;
		renderingServerVfxLayer.PrewarmCount = prewarmCount;
		renderingServerVfxLayer.MaxCapacity = num;
		renderingServerVfxLayer.BaseZIndex = spec.BaseZIndex;
		((CanvasItem)renderingServerVfxLayer).ZAsRelative = spec.ZAsRelative;
		renderingServerVfxLayer.StatsLogIntervalSeconds = Math.Max(0f, spec.StatsLogIntervalSeconds);
		RenderingServerVfxLayer renderingServerVfxLayer2 = renderingServerVfxLayer;
		val.AddChild((Node)(object)renderingServerVfxLayer2, false, (InternalMode)0);
		layer = renderingServerVfxLayer2;
		debug?.Invoke("RenderingServer VFX layer mounted name=" + spec.Name + " prewarm=" + prewarmCount + " capacity=" + num + " z=" + spec.BaseZIndex);
		return renderingServerVfxLayer2;
	}

	public static void Release(ref RenderingServerVfxLayer? layer)
	{
		RenderingServerVfxLayer? node = layer;
		layer = null;
		GodotNodeUtil.SafeQueueFree((Node?)(object)node);
	}

	private static Node? ResolveParent(Node? parent)
	{
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)parent))
		{
			return parent;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null && val.Root != null)
		{
			return (Node?)(object)val.Root;
		}
		return null;
	}
}
