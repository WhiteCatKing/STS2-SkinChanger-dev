using System;
using CznCore.Godot;
using Godot;

namespace CznCore.Presentation;

internal static class CombatEntryVisibility
{
	private static long _generation;

	public static void Hide(Node owner, CanvasItem body, string key, Action reveal, float fallbackSeconds = 2f)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (((GodotObject)owner).HasMeta(StringName.op_Implicit(key)))
		{
			return;
		}
		long generation = ++_generation;
		body.Visible = false;
		((GodotObject)owner).SetMeta(StringName.op_Implicit(key), Variant.op_Implicit(generation));
		if (fallbackSeconds <= 0f)
		{
			return;
		}
		GodotScheduler.Delay(fallbackSeconds, delegate
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)owner) && owner.IsInsideTree() && ((GodotObject)owner).HasMeta(StringName.op_Implicit(key)))
			{
				Node obj = owner;
				StringName obj2 = StringName.op_Implicit(key);
				Variant val = default(Variant);
				val = ((GodotObject)obj).GetMeta(obj2, val);
				if (((Variant)(ref val)).AsInt64() == generation)
				{
					reveal();
				}
			}
		});
	}

	public static void Reveal(Node owner, CanvasItem body, Node2D? spine, string key)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)owner) && owner.IsInsideTree() && ((GodotObject)owner).HasMeta(StringName.op_Implicit(key)) && GodotObject.IsInstanceValid((GodotObject)(object)body))
		{
			body.Visible = true;
			if (spine != null && GodotObject.IsInstanceValid((GodotObject)(object)spine))
			{
				((GodotObject)spine).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
				((GodotObject)spine).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
			}
			((GodotObject)owner).RemoveMeta(StringName.op_Implicit(key));
		}
	}
}
