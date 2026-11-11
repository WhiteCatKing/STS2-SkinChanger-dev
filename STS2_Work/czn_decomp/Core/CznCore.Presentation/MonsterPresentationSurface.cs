using System;
using Godot;

namespace CznCore.Presentation;

internal static class MonsterPresentationSurface
{
	internal static readonly StringName ScaleMeta = new StringName("czn_monster_system_effect_scale");

	internal static bool TryResolveDefense(Node2D? primary, string route, out Node2D? anchor, out Vector2 scale)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		anchor = null;
		scale = Vector2.One;
		Node2D val = FindVisibleBody(primary);
		if (val == null)
		{
			return false;
		}
		StringName scaleMeta = ScaleMeta;
		Variant val2 = default(Variant);
		val2 = ((GodotObject)val).GetMeta(scaleMeta, val2);
		float num = ((Variant)(ref val2)).AsSingle();
		if (!float.IsFinite(num) || num <= 0f)
		{
			return false;
		}
		string bone = ((route == "system_hit_shield_enemy") ? "target" : "root");
		anchor = ResolveBone(val, bone);
		if (anchor == null)
		{
			return false;
		}
		scale = Vector2.One * num;
		return true;
	}

	internal static Node2D? FindVisibleBody(Node2D? primary)
	{
		if (primary == null || !GodotObject.IsInstanceValid((GodotObject)(object)primary))
		{
			return null;
		}
		Node2D val = (Accepts(primary) ? primary : null);
		if (val == null)
		{
			Node parent = ((Node)primary).GetParent();
			if (parent != null)
			{
				foreach (Node child in parent.GetChildren(false))
				{
					Node2D val2 = (Node2D)(object)((child is Node2D) ? child : null);
					if (val2 != null && Accepts(val2))
					{
						val = val2;
						break;
					}
				}
			}
		}
		return val;
	}

	internal static Node2D? ResolveBone(Node2D body, string bone)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)body).Call(StringName.op_Implicit("get_skeleton"), Array.Empty<Variant>());
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		object obj2;
		if (obj == null)
		{
			obj2 = null;
		}
		else
		{
			val = obj.Call(StringName.op_Implicit("find_bone"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(bone) });
			obj2 = ((Variant)(ref val)).AsGodotObject();
		}
		GodotObject val2 = (GodotObject)obj2;
		if (val2 == null)
		{
			return null;
		}
		string text = "CznDefenseBone_" + bone;
		Node2D val3 = ((Node)body).GetNodeOrNull<Node2D>(NodePath.op_Implicit(text));
		if (val3 == null)
		{
			val = ClassDB.Instantiate(StringName.op_Implicit("SpineBoneNode"));
			GodotObject obj3 = ((Variant)(ref val)).AsGodotObject();
			val3 = (Node2D)(object)((obj3 is Node2D) ? obj3 : null);
			if (val3 == null)
			{
				return null;
			}
			((Node)val3).Name = StringName.op_Implicit(text);
			((Node)body).AddChild((Node)(object)val3, false, (InternalMode)0);
			((GodotObject)val3).Set(StringName.op_Implicit("bone_name"), Variant.op_Implicit(bone));
		}
		Node2D obj4 = val3;
		val = val2.Call(StringName.op_Implicit("get_global_transform"), Array.Empty<Variant>());
		obj4.GlobalTransform = ((Variant)(ref val)).AsTransform2D();
		return val3;
	}

	private static bool Accepts(Node2D body)
	{
		if (((GodotObject)body).GetClass() == "SpineSprite" && ((GodotObject)body).HasMeta(ScaleMeta))
		{
			return ((CanvasItem)body).IsVisibleInTree();
		}
		return false;
	}
}
