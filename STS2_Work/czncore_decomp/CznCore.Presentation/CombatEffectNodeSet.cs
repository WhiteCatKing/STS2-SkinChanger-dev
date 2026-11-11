using System;
using System.Collections.Generic;
using Godot;

namespace CznCore.Presentation;

public sealed class CombatEffectNodeSet
{
	private List<Node>? _spines;

	private List<Node>? _particles;

	private List<Node>? _nodes;

	private List<CanvasItem>? _canvas;

	public IReadOnlyList<Node> SpineSprites
	{
		get
		{
			IReadOnlyList<Node> spines = _spines;
			return spines ?? Array.Empty<Node>();
		}
	}

	public IReadOnlyList<Node> Particles
	{
		get
		{
			IReadOnlyList<Node> particles = _particles;
			return particles ?? Array.Empty<Node>();
		}
	}

	public IReadOnlyList<Node> Nodes
	{
		get
		{
			IReadOnlyList<Node> nodes = _nodes;
			return nodes ?? Array.Empty<Node>();
		}
	}

	public IReadOnlyList<CanvasItem> CanvasItems
	{
		get
		{
			IReadOnlyList<CanvasItem> canvas = _canvas;
			return canvas ?? Array.Empty<CanvasItem>();
		}
	}

	public static CombatEffectNodeSet Collect(Node root, bool reverseSiblings = false, bool includeCanvas = false)
	{
		CombatEffectNodeSet combatEffectNodeSet = new CombatEffectNodeSet();
		Stack<Node> stack = new Stack<Node>();
		stack.Push(root);
		while (stack.Count > 0)
		{
			Node val = stack.Pop();
			if (!GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				continue;
			}
			if ((val is GpuParticles2D || val is CpuParticles2D) ? true : false)
			{
				CombatEffectNodeSet combatEffectNodeSet2 = combatEffectNodeSet;
				(combatEffectNodeSet2._particles ?? (combatEffectNodeSet2._particles = new List<Node>())).Add(val);
			}
			else if (((GodotObject)val).GetClass() == "SpineSprite")
			{
				CombatEffectNodeSet combatEffectNodeSet2 = combatEffectNodeSet;
				(combatEffectNodeSet2._spines ?? (combatEffectNodeSet2._spines = new List<Node>())).Add(val);
			}
			if (includeCanvas)
			{
				CombatEffectNodeSet combatEffectNodeSet2 = combatEffectNodeSet;
				(combatEffectNodeSet2._nodes ?? (combatEffectNodeSet2._nodes = new List<Node>())).Add(val);
				CanvasItem val2 = (CanvasItem)(object)((val is CanvasItem) ? val : null);
				if (val2 != null)
				{
					combatEffectNodeSet2 = combatEffectNodeSet;
					(combatEffectNodeSet2._canvas ?? (combatEffectNodeSet2._canvas = new List<CanvasItem>())).Add(val2);
				}
			}
			int childCount = val.GetChildCount(false);
			for (int i = 0; i < childCount; i++)
			{
				stack.Push(val.GetChild(reverseSiblings ? i : (childCount - 1 - i), false));
			}
		}
		return combatEffectNodeSet;
	}
}
