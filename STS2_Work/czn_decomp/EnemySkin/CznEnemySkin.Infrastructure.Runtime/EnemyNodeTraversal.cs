using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class EnemyNodeTraversal
{
	private const string SpineSpriteClassName = "SpineSprite";

	public static EnemyNodeSet Collect(Node root)
	{
		EnemyNodeSet enemyNodeSet = new EnemyNodeSet();
		Stack<Node> stack = new Stack<Node>();
		stack.Push(root);
		while (stack.Count > 0)
		{
			Node val = stack.Pop();
			if (GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				if ((val is GpuParticles2D || val is CpuParticles2D) ? true : false)
				{
					enemyNodeSet.AddParticles(val);
				}
				else if (((GodotObject)val).GetClass() == "SpineSprite")
				{
					enemyNodeSet.AddSpineSprite(val);
				}
				Array<Node> children = val.GetChildren(false);
				for (int num = children.Count - 1; num >= 0; num--)
				{
					stack.Push(children[num]);
				}
			}
		}
		return enemyNodeSet;
	}
}
