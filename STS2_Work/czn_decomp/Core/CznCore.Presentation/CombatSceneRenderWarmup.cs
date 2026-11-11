using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace CznCore.Presentation;

public static class CombatSceneRenderWarmup
{
	public static async Task<int> WarmAsync(string surfaceName, string primedMetadata, IEnumerable<string> scenePaths, Action<Node2D> prepare, Func<bool> isCurrent, int batchSize = 1)
	{
		if (isCurrent() && !(DisplayServer.GetName() == "headless"))
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree tree = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (tree != null)
			{
				List<PackedScene> pending = new List<PackedScene>();
				await RunStartResourcePreloader.PreloadAllAsync((Node)(object)tree.Root, scenePaths, isCurrent, delegate(string _, Resource resource)
				{
					PackedScene val = (PackedScene)(object)((resource is PackedScene) ? resource : null);
					if (val != null && !((GodotObject)val).HasMeta(StringName.op_Implicit(primedMetadata)))
					{
						pending.Add(val);
					}
				});
				await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
				if (pending.Count == 0 || !isCurrent())
				{
					return 0;
				}
				Node2D anchor = CombatRenderWarmupSurface.Resolve(surfaceName);
				if (anchor == null)
				{
					return 0;
				}
				Node lease = new Node();
				((Node)anchor).AddChild(lease, false, (InternalMode)0);
				List<Node2D> roots = new List<Node2D>(batchSize);
				int completed = 0;
				try
				{
					while (completed < pending.Count && isCurrent())
					{
						int end = Math.Min(pending.Count, completed + batchSize);
						for (int i = completed; i < end; i++)
						{
							Node2D val2 = pending[i].Instantiate<Node2D>((GenEditState)0);
							roots.Add(val2);
							((Node)anchor).AddChild((Node)(object)val2, false, (InternalMode)0);
							prepare(val2);
						}
						await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
						await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
						if (!isCurrent())
						{
							break;
						}
						for (int j = completed; j < end; j++)
						{
							((GodotObject)pending[j]).SetMeta(StringName.op_Implicit(primedMetadata), Variant.op_Implicit(true));
						}
						completed = end;
						FreeRoots(roots);
					}
					return completed;
				}
				finally
				{
					FreeRoots(roots);
					lease.QueueFree();
				}
			}
		}
		return 0;
	}

	private static void FreeRoots(List<Node2D> roots)
	{
		foreach (Node2D root in roots)
		{
			((Node)root).GetParent().RemoveChild((Node)(object)root);
			((Node)root).QueueFree();
		}
		roots.Clear();
	}
}
