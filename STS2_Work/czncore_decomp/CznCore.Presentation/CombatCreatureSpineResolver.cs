using System;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal sealed class CombatCreatureSpineResolver
{
	private const string SpineSpriteClassName = "SpineSprite";

	private readonly CombatNodeAnchorCache<NCreature> _anchors = new CombatNodeAnchorCache<NCreature>();

	private readonly FieldInfo? _visualsBodyField = typeof(NCreatureVisuals).GetField("_body", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	private readonly PropertyInfo? _spineBodyProperty = typeof(NCreatureVisuals).GetProperty("SpineBody", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

	public Node2D? Find(NCreature? creature)
	{
		if (creature == null || !GodotObject.IsInstanceValid((GodotObject)(object)creature))
		{
			return null;
		}
		return _anchors.GetOrResolve(creature, GetSearchRoot, FindUncached);
	}

	public void Clear()
	{
		_anchors.Clear();
	}

	public bool TrySetVisualsSpineBody(NCreatureVisuals? visuals, Node2D spine, Action<string>? warn = null)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if (visuals == null || !GodotObject.IsInstanceValid((GodotObject)(object)spine) || _spineBodyProperty == null)
		{
			return false;
		}
		try
		{
			_spineBodyProperty.SetValue(visuals, (object?)new MegaSprite(Variant.op_Implicit((GodotObject)(object)spine)));
			return true;
		}
		catch (Exception ex)
		{
			warn?.Invoke("Failed to restore NCreatureVisuals.SpineBody: " + SkinLog.FormatException(ex));
			return false;
		}
	}

	private static Node? GetSearchRoot(NCreature creature)
	{
		return (Node?)(((object)creature.Visuals) ?? ((object)creature));
	}

	private Node2D? FindUncached(NCreature creature)
	{
		Node searchRoot = GetSearchRoot(creature);
		if (searchRoot == null)
		{
			return null;
		}
		Node2D val = FindFromBody(SafeGetCreatureBody(creature));
		if (val != null)
		{
			return val;
		}
		NCreatureVisuals visuals = creature.Visuals;
		Node2D val2 = FindFromBody(SafeGetCurrentBody(visuals));
		if (val2 != null)
		{
			return val2;
		}
		Node2D val3 = FindFromBody(SafeGetBodyValue(_visualsBodyField, visuals));
		if (val3 != null)
		{
			return val3;
		}
		return FindRecursive(searchRoot);
	}

	private static Node2D? SafeGetCreatureBody(NCreature creature)
	{
		try
		{
			return creature.Body;
		}
		catch
		{
			return null;
		}
	}

	private static Node2D? SafeGetCurrentBody(NCreatureVisuals? visuals)
	{
		if (visuals == null)
		{
			return null;
		}
		try
		{
			return visuals.GetCurrentBody();
		}
		catch
		{
			return null;
		}
	}

	private static Node2D? SafeGetBodyValue(FieldInfo? field, NCreatureVisuals? visuals)
	{
		if (field == null || visuals == null)
		{
			return null;
		}
		try
		{
			object? value = field.GetValue(visuals);
			return (Node2D?)((value is Node2D) ? value : null);
		}
		catch
		{
			return null;
		}
	}

	private static Node2D? FindFromBody(Node2D? body)
	{
		if (body == null || !GodotObject.IsInstanceValid((GodotObject)(object)body))
		{
			return null;
		}
		if (IsSpineSprite((GodotObject)(object)body))
		{
			return body;
		}
		return FindRecursive((Node)(object)body);
	}

	private static Node2D? FindRecursive(Node root)
	{
		Node2D val = (Node2D)(object)((root is Node2D) ? root : null);
		if (val != null && IsSpineSprite((GodotObject)(object)root))
		{
			return val;
		}
		foreach (Node child in root.GetChildren(false))
		{
			Node2D val2 = FindRecursive(child);
			if (val2 != null)
			{
				return val2;
			}
		}
		return null;
	}

	private static bool IsSpineSprite(GodotObject node)
	{
		return node.GetClass() == "SpineSprite";
	}
}
