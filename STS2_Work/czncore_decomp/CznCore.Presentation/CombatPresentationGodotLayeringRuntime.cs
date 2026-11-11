using System;
using System.Collections.Generic;
using Godot;

namespace CznCore.Presentation;

internal static class CombatPresentationGodotLayeringRuntime
{
	public const int CanvasItemZIndexMin = -4096;

	public const int CanvasItemZIndexMax = 4096;

	private const string OriginalZIndexMeta = "czn_core_presentation_original_z_index";

	private const string OriginalZAsRelativeMeta = "czn_core_presentation_original_z_as_relative";

	private const string EffectiveSourceZIndexMeta = "czn_core_presentation_effective_source_z_index";

	private const string EffectiveSafeZIndexMeta = "czn_core_presentation_effective_safe_z_index";

	public static int ResolveCueZIndex(CombatPresentationEffectCue cue)
	{
		return CombatPresentationDepthRuntime.ResolveAttachedEffectZIndex(cue);
	}

	public static void PlaceEffectRoot(Node2D parent, Node2D effectRoot, CombatPresentationEffectCue cue)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)parent) && GodotObject.IsInstanceValid((GodotObject)(object)effectRoot))
		{
			int value = ((!CombatPresentationDepthRuntime.IsScreenAnchor(cue.Anchor)) ? (cue.GodotZIndex ?? cue.ZIndex) : 0);
			bool flag = !CombatPresentationDepthRuntime.IsScreenAnchor(cue.Anchor) && !cue.ProjectedTargetDepth.HasValue && CombatPresentationEffectCueProjection.ResolveOrdinaryTargetDepth(parent).HasValue;
			NormalizeEffectSubtree((Node)(object)effectRoot, 0, 0, value, cue.ProjectedTargetDepth.HasValue, straddleNativeBody: false, flag);
			if (!((GodotObject)effectRoot).HasMeta(StringName.op_Implicit("czn_core_presentation_original_behind_parent")))
			{
				((GodotObject)effectRoot).SetMeta(StringName.op_Implicit("czn_core_presentation_original_behind_parent"), Variant.op_Implicit(((CanvasItem)effectRoot).ShowBehindParent));
			}
			int showBehindParent;
			if (!flag)
			{
				StringName obj = StringName.op_Implicit("czn_core_presentation_original_behind_parent");
				Variant val = default(Variant);
				val = ((GodotObject)effectRoot).GetMeta(obj, val);
				showBehindParent = (((Variant)(ref val)).AsBool() ? 1 : 0);
			}
			else
			{
				showBehindParent = 1;
			}
			((CanvasItem)effectRoot).ShowBehindParent = (byte)showBehindParent != 0;
			int? projectedTargetDepth = cue.ProjectedTargetDepth;
			if (projectedTargetDepth.HasValue)
			{
				int valueOrDefault = projectedTargetDepth.GetValueOrDefault();
				((CanvasItem)effectRoot).ZAsRelative = false;
				((CanvasItem)effectRoot).ZIndex = ClampCanvasItemZIndex(valueOrDefault + ((cue.Anchor != CombatPresentationEffectAnchor.ScreenBack) ? 1 : 0));
			}
			SortNormalizedChildren((Node)(object)parent);
		}
	}

	public static int ClampCanvasItemZIndex(int zIndex)
	{
		return Math.Clamp(zIndex, -4096, 4096);
	}

	public static void PlaceNativeForeground(Node2D effectRoot, Node2D referenceSurface)
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)effectRoot) || !GodotObject.IsInstanceValid((GodotObject)(object)referenceSurface))
		{
			return;
		}
		int? num = CombatPresentationEffectCueProjection.ResolveOrdinaryTargetDepth(referenceSurface);
		if (!num.HasValue)
		{
			return;
		}
		int valueOrDefault = num.GetValueOrDefault();
		if (((Node)effectRoot).GetParent() == null)
		{
			return;
		}
		CanvasItem val = null;
		Node val2 = (Node)(object)referenceSurface;
		while (val2 != null && (val = val2.GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("BackCombatVfxContainer"))) == null)
		{
			val2 = val2.GetParent();
		}
		if (val != null)
		{
			if (((Node)effectRoot).GetParent() != val)
			{
				((Node)effectRoot).Reparent((Node)(object)val, true);
			}
			int num2;
			if (!((GodotObject)effectRoot).HasMeta(StringName.op_Implicit("czn_effect_source_root_z")))
			{
				num2 = 0;
			}
			else
			{
				StringName obj = StringName.op_Implicit("czn_effect_source_root_z");
				Variant val3 = default(Variant);
				val3 = ((GodotObject)effectRoot).GetMeta(obj, val3);
				num2 = ((Variant)(ref val3)).AsInt32();
			}
			int value = num2;
			NormalizeEffectSubtree((Node)(object)effectRoot, 0, 0, value, projectedTarget: false, straddleNativeBody: false, nativeAttached: true);
			((CanvasItem)effectRoot).ZAsRelative = false;
			((CanvasItem)effectRoot).ZIndex = ClampCanvasItemZIndex(valueOrDefault);
			Node parent = ((Node)effectRoot).GetParent();
			if (parent != null)
			{
				SortNormalizedChildren(parent);
			}
		}
	}

	private static void NormalizeEffectSubtree(Node node, int parentSourceZIndex, int parentSafeZIndex, int? rootSourceZIndex = null, bool projectedTarget = false, bool straddleNativeBody = false, bool nativeAttached = false, bool parentNativeRear = false)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		int num = parentSourceZIndex;
		int num2 = parentSafeZIndex;
		bool flag = parentNativeRear;
		CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
		if (val != null)
		{
			(int ZIndex, bool ZAsRelative) tuple = ReadOrCaptureOriginalDepth(val);
			int item = tuple.ZIndex;
			bool item2 = tuple.ZAsRelative;
			num = rootSourceZIndex ?? (item2 ? (parentSourceZIndex + item) : item);
			num2 = Math.Sign(num);
			if (projectedTarget)
			{
				num2 = (straddleNativeBody ? (Math.Min(0, num2) * 2) : 0);
			}
			else if (nativeAttached)
			{
				flag = !rootSourceZIndex.HasValue && (num < 0 || val.ShowBehindParent || (parentNativeRear && item == 0));
				num2 = ((!(rootSourceZIndex.HasValue || flag)) ? 1 : 0);
			}
			val.ZAsRelative = true;
			val.ZIndex = num2 - parentSafeZIndex;
			((GodotObject)val).SetMeta(StringName.op_Implicit("czn_core_presentation_effective_source_z_index"), Variant.From<int>(ref num));
			((GodotObject)val).SetMeta(StringName.op_Implicit("czn_core_presentation_effective_safe_z_index"), Variant.From<int>(ref num2));
		}
		foreach (Node child in node.GetChildren(false))
		{
			int parentSourceZIndex2 = num;
			int parentSafeZIndex2 = num2;
			bool projectedTarget2 = projectedTarget;
			bool straddleNativeBody2 = straddleNativeBody;
			bool nativeAttached2 = nativeAttached;
			bool parentNativeRear2 = flag;
			NormalizeEffectSubtree(child, parentSourceZIndex2, parentSafeZIndex2, null, projectedTarget2, straddleNativeBody2, nativeAttached2, parentNativeRear2);
		}
		SortNormalizedChildren(node);
	}

	private static (int ZIndex, bool ZAsRelative) ReadOrCaptureOriginalDepth(CanvasItem item)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (((GodotObject)item).HasMeta(StringName.op_Implicit("czn_core_presentation_original_z_index")) && ((GodotObject)item).HasMeta(StringName.op_Implicit("czn_core_presentation_original_z_as_relative")))
			{
				StringName obj = StringName.op_Implicit("czn_core_presentation_original_z_index");
				Variant val = default(Variant);
				val = ((GodotObject)item).GetMeta(obj, val);
				int item2 = ((Variant)(ref val)).AsInt32();
				StringName obj2 = StringName.op_Implicit("czn_core_presentation_original_z_as_relative");
				val = default(Variant);
				val = ((GodotObject)item).GetMeta(obj2, val);
				return (ZIndex: item2, ZAsRelative: ((Variant)(ref val)).AsBool());
			}
		}
		catch
		{
		}
		int zIndex = item.ZIndex;
		bool zAsRelative = item.ZAsRelative;
		((GodotObject)item).SetMeta(StringName.op_Implicit("czn_core_presentation_original_z_index"), Variant.From<int>(ref zIndex));
		((GodotObject)item).SetMeta(StringName.op_Implicit("czn_core_presentation_original_z_as_relative"), Variant.From<bool>(ref zAsRelative));
		return (ZIndex: zIndex, ZAsRelative: zAsRelative);
	}

	private static void SortNormalizedChildren(Node parent)
	{
		List<CanvasItem> list = new List<CanvasItem>();
		List<int> list2 = new List<int>();
		List<Node> list3 = new List<Node>();
		foreach (Node child in parent.GetChildren(false))
		{
			list3.Add(child);
			CanvasItem val = (CanvasItem)(object)((child is CanvasItem) ? child : null);
			if (val != null && ((GodotObject)val).HasMeta(StringName.op_Implicit("czn_core_presentation_effective_source_z_index")) && ((GodotObject)val).HasMeta(StringName.op_Implicit("czn_core_presentation_effective_safe_z_index")))
			{
				list.Add(val);
				list2.Add(((Node)val).GetIndex(false));
			}
		}
		list.Sort(delegate(CanvasItem left, CanvasItem right)
		{
			int num = ReadDepthMeta(left, "czn_core_presentation_effective_safe_z_index").CompareTo(ReadDepthMeta(right, "czn_core_presentation_effective_safe_z_index"));
			if (num != 0)
			{
				return num;
			}
			int num2 = ReadDepthMeta(left, "czn_core_presentation_effective_source_z_index").CompareTo(ReadDepthMeta(right, "czn_core_presentation_effective_source_z_index"));
			return (num2 == 0) ? ((Node)left).GetIndex(false).CompareTo(((Node)right).GetIndex(false)) : num2;
		});
		for (int i = 0; i < list.Count; i++)
		{
			list3[list2[i]] = (Node)(object)list[i];
		}
		for (int j = 0; j < list3.Count; j++)
		{
			if (list3[j].GetIndex(false) != j)
			{
				parent.MoveChild(list3[j], j);
			}
		}
	}

	private static int ReadDepthMeta(CanvasItem item, string metaName)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			StringName obj = StringName.op_Implicit(metaName);
			Variant val = default(Variant);
			val = ((GodotObject)item).GetMeta(obj, val);
			return ((Variant)(ref val)).AsInt32();
		}
		catch
		{
			return 0;
		}
	}
}
