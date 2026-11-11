using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal static class CombatCreatureUiDepthRuntime
{
	private static readonly NodePath HealthBarPath = new NodePath("%HealthBar");

	public static Node2D? ResolveVisualSurface(NCreature? creature)
	{
		if (!EnsureAbovePresentation(creature))
		{
			return null;
		}
		try
		{
			return (Node2D?)(object)(GodotObject.IsInstanceValid((GodotObject)(object)creature.Visuals) ? creature.Visuals : null);
		}
		catch
		{
			return null;
		}
	}

	public static bool EnsureAbovePresentation(NCreature? creature)
	{
		if (creature == null || !GodotObject.IsInstanceValid((GodotObject)(object)creature))
		{
			return false;
		}
		bool flag = false;
		try
		{
			CanvasItem nodeOrNull = ((Node)creature).GetNodeOrNull<CanvasItem>(HealthBarPath);
			flag |= ApplyCreatureOwnedDepth(nodeOrNull);
		}
		catch
		{
		}
		try
		{
			flag |= ApplySemanticUiDepth((CanvasItem?)(object)creature.IntentContainer);
		}
		catch
		{
		}
		return flag;
	}

	private static bool ApplyCreatureOwnedDepth(CanvasItem? item)
	{
		if (item == null || !GodotObject.IsInstanceValid((GodotObject)(object)item))
		{
			return false;
		}
		item.ZAsRelative = true;
		item.ZIndex = 0;
		return true;
	}

	private static bool ApplySemanticUiDepth(CanvasItem? item)
	{
		if (item == null || !GodotObject.IsInstanceValid((GodotObject)(object)item))
		{
			return false;
		}
		item.ZAsRelative = false;
		item.ZIndex = -1;
		return true;
	}
}
