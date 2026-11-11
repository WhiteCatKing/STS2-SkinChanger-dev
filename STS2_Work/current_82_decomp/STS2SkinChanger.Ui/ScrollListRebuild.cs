using System;
using Godot;

namespace STS2SkinChanger.Ui;

internal static class ScrollListRebuild
{
	private const string ScrollName = "SkinChangerRetainedListScroll";

	private const string ContextMeta = "skin_changer_list_context";

	private const string PositionMeta = "skin_changer_list_scroll_position";

	private const string GenerationMeta = "skin_changer_list_scroll_generation";

	internal static ScrollContainer Begin(VBoxContainer content, string contextId)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		ScrollContainer val = ((Node)content).GetNodeOrNull<ScrollContainer>(NodePath.op_Implicit("SkinChangerRetainedListScroll"));
		if (val == null)
		{
			val = new ScrollContainer
			{
				Name = StringName.op_Implicit("SkinChangerRetainedListScroll")
			};
			((Node)content).AddChild((Node)(object)val, false, (InternalMode)0);
		}
		Variant meta = ((GodotObject)val).GetMeta(StringName.op_Implicit("skin_changer_list_context"), Variant.op_Implicit(string.Empty));
		int num = (((Variant)(ref meta)).AsString().Equals(contextId, StringComparison.OrdinalIgnoreCase) ? val.ScrollVertical : 0);
		foreach (Node child in ((Node)content).GetChildren(false))
		{
			if (child != val)
			{
				((Node)content).RemoveChild(child);
				child.QueueFree();
			}
		}
		foreach (Node child2 in ((Node)val).GetChildren(false))
		{
			((Node)val).RemoveChild(child2);
			child2.QueueFree();
		}
		((GodotObject)val).SetMeta(StringName.op_Implicit("skin_changer_list_scroll_position"), Variant.op_Implicit(num));
		ScrollContainer obj = val;
		StringName obj2 = StringName.op_Implicit("skin_changer_list_scroll_generation");
		meta = ((GodotObject)val).GetMeta(StringName.op_Implicit("skin_changer_list_scroll_generation"), Variant.op_Implicit(0L));
		((GodotObject)obj).SetMeta(obj2, Variant.op_Implicit(((Variant)(ref meta)).AsInt64() + 1));
		val.ScrollHorizontal = 0;
		((GodotObject)val).SetMeta(StringName.op_Implicit("skin_changer_list_context"), Variant.op_Implicit(contextId));
		return val;
	}

	internal static void PlaceAfterHeader(ScrollContainer scroll)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		Node parent = ((Node)scroll).GetParent();
		parent.MoveChild((Node)(object)scroll, parent.GetChildCount(false) - 1);
		Variant meta = ((GodotObject)scroll).GetMeta(StringName.op_Implicit("skin_changer_list_scroll_generation"), Variant.op_Implicit(0L));
		long generation = ((Variant)(ref meta)).AsInt64();
		meta = ((GodotObject)scroll).GetMeta(StringName.op_Implicit("skin_changer_list_scroll_position"), Variant.op_Implicit(0));
		int position = ((Variant)(ref meta)).AsInt32();
		Callable val2 = Callable.From((Action)delegate
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)scroll))
			{
				Variant meta2 = ((GodotObject)scroll).GetMeta(StringName.op_Implicit("skin_changer_list_scroll_generation"), Variant.op_Implicit(0L));
				if (((Variant)(ref meta2)).AsInt64() == generation)
				{
					((Container)scroll).QueueSort();
					Callable val = Callable.From((Action)delegate
					{
						//IL_001f: Unknown result type (might be due to invalid IL or missing references)
						//IL_0024: Unknown result type (might be due to invalid IL or missing references)
						//IL_0029: Unknown result type (might be due to invalid IL or missing references)
						if (GodotObject.IsInstanceValid((GodotObject)(object)scroll))
						{
							Variant meta3 = ((GodotObject)scroll).GetMeta(StringName.op_Implicit("skin_changer_list_scroll_generation"), Variant.op_Implicit(0L));
							if (((Variant)(ref meta3)).AsInt64() == generation)
							{
								scroll.ScrollVertical = position;
							}
						}
					});
					((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
				}
			}
		});
		((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
	}
}
