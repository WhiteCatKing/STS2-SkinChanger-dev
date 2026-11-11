using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal sealed class CombatCreatureVisualAnchorResolver(CombatCreatureSpineResolver spines)
{
	public Node2D? Find(NCreature? creature)
	{
		if (creature == null || !GodotObject.IsInstanceValid((GodotObject)(object)creature))
		{
			return null;
		}
		Node2D val = spines.Find(creature);
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)val))
		{
			return val;
		}
		return FindBodyFallback(creature);
	}

	private static Node2D? FindBodyFallback(NCreature creature)
	{
		try
		{
			Node2D body = creature.Body;
			if (GodotNodeUtil.IsAlive((GodotObject?)(object)body))
			{
				return body;
			}
		}
		catch
		{
		}
		try
		{
			NCreatureVisuals visuals = creature.Visuals;
			return (Node2D?)(object)(GodotNodeUtil.IsAlive((GodotObject?)(object)visuals) ? visuals : null);
		}
		catch
		{
			return null;
		}
	}
}
