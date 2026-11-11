using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class EnemyPresentationDepthPolicy
{
	private readonly record struct NormalizedDepth(int SourceZIndex, int SafeZIndex);

	private readonly record struct DepthEntry(CanvasItem Item, int SourceZIndex, int SafeZIndex, int Index);

	public const int CreatureIntentZIndex = -1;

	private static readonly StringName OriginalZIndexMeta = new StringName("czn_enemy_skin_original_z_index");

	private static readonly StringName OriginalZAsRelativeMeta = new StringName("czn_enemy_skin_original_z_as_relative");

	private static readonly StringName EffectiveSourceZIndexMeta = new StringName("czn_enemy_skin_effective_source_z_index");

	private static readonly StringName EffectiveSafeZIndexMeta = new StringName("czn_enemy_skin_effective_safe_z_index");

	private static readonly NodePath HealthBarPath = new NodePath("%HealthBar");

	public static void NormalizeVisualSubtree(Node root)
	{
		NormalizeVisualNode(root, 0, 0, 0);
	}

	public static void PlaceEffect(Node2D host, Node2D effect, int sourceZIndex)
	{
		NormalizeVisualNode((Node)(object)effect, 0, 0, sourceZIndex);
		((Node)host).AddChild((Node)(object)effect, false, (InternalMode)0);
		SortNormalizedChildren((Node)(object)host, CollectNormalizedChildren((Node)(object)host));
	}

	public static void EnsureCreatureUiAbovePresentation(NCreature creature)
	{
		ApplyCreatureOwnedDepth(((Node)creature).GetNodeOrNull<CanvasItem>(HealthBarPath));
		ApplySemanticUiDepth((CanvasItem?)(object)creature.IntentContainer);
	}

	private static NormalizedDepth NormalizeVisualNode(Node node, int parentSourceZIndex, int parentSafeZIndex, int? rootSourceZIndex = null)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		int num = parentSourceZIndex;
		int num2 = parentSafeZIndex;
		CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
		if (val != null)
		{
			(int ZIndex, bool ZAsRelative) tuple = ReadOrCaptureOriginalDepth(val);
			int item = tuple.ZIndex;
			bool item2 = tuple.ZAsRelative;
			num = rootSourceZIndex ?? (item2 ? (parentSourceZIndex + item) : item);
			num2 = Math.Sign(num);
			val.ZAsRelative = true;
			val.ZIndex = num2 - parentSafeZIndex;
			((GodotObject)val).SetMeta(EffectiveSourceZIndexMeta, Variant.From<int>(ref num));
			((GodotObject)val).SetMeta(EffectiveSafeZIndexMeta, Variant.From<int>(ref num2));
		}
		Array<Node> children = node.GetChildren(false);
		List<DepthEntry> list = null;
		for (int i = 0; i < children.Count; i++)
		{
			Node obj = children[i];
			NormalizedDepth normalizedDepth = NormalizeVisualNode(obj, num, num2, null);
			CanvasItem val2 = (CanvasItem)(object)((obj is CanvasItem) ? obj : null);
			if (val2 != null)
			{
				if (list == null)
				{
					list = new List<DepthEntry>(children.Count);
				}
				list.Add(new DepthEntry(val2, normalizedDepth.SourceZIndex, normalizedDepth.SafeZIndex, i));
			}
		}
		if (list != null)
		{
			SortNormalizedChildren(node, list);
		}
		return new NormalizedDepth(num, num2);
	}

	private static void ApplyCreatureOwnedDepth(CanvasItem? item)
	{
		if (item != null && GodotObject.IsInstanceValid((GodotObject)(object)item) && (!item.ZAsRelative || item.ZIndex != 0))
		{
			item.ZAsRelative = true;
			item.ZIndex = 0;
		}
	}

	private static void ApplySemanticUiDepth(CanvasItem? item)
	{
		if (item != null && GodotObject.IsInstanceValid((GodotObject)(object)item) && (item.ZAsRelative || item.ZIndex != -1))
		{
			item.ZAsRelative = false;
			item.ZIndex = -1;
		}
	}

	private static (int ZIndex, bool ZAsRelative) ReadOrCaptureOriginalDepth(CanvasItem item)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (((GodotObject)item).HasMeta(OriginalZIndexMeta) && ((GodotObject)item).HasMeta(OriginalZAsRelativeMeta))
			{
				StringName originalZIndexMeta = OriginalZIndexMeta;
				Variant val = default(Variant);
				val = ((GodotObject)item).GetMeta(originalZIndexMeta, val);
				int item2 = ((Variant)(ref val)).AsInt32();
				StringName originalZAsRelativeMeta = OriginalZAsRelativeMeta;
				val = default(Variant);
				val = ((GodotObject)item).GetMeta(originalZAsRelativeMeta, val);
				return (ZIndex: item2, ZAsRelative: ((Variant)(ref val)).AsBool());
			}
		}
		catch
		{
		}
		int zIndex = item.ZIndex;
		bool zAsRelative = item.ZAsRelative;
		((GodotObject)item).SetMeta(OriginalZIndexMeta, Variant.From<int>(ref zIndex));
		((GodotObject)item).SetMeta(OriginalZAsRelativeMeta, Variant.From<bool>(ref zAsRelative));
		return (ZIndex: zIndex, ZAsRelative: zAsRelative);
	}

	private static List<DepthEntry> CollectNormalizedChildren(Node parent)
	{
		Array<Node> children = parent.GetChildren(false);
		List<DepthEntry> list = new List<DepthEntry>(children.Count);
		for (int i = 0; i < children.Count; i++)
		{
			Node obj = children[i];
			CanvasItem val = (CanvasItem)(object)((obj is CanvasItem) ? obj : null);
			if (val != null && ((GodotObject)val).HasMeta(EffectiveSourceZIndexMeta) && ((GodotObject)val).HasMeta(EffectiveSafeZIndexMeta))
			{
				list.Add(new DepthEntry(val, ReadDepthMeta(val, EffectiveSourceZIndexMeta), ReadDepthMeta(val, EffectiveSafeZIndexMeta), i));
			}
		}
		return list;
	}

	private static void SortNormalizedChildren(Node parent, List<DepthEntry> entries)
	{
		if (entries.Count == 0)
		{
			return;
		}
		entries.Sort(delegate(DepthEntry left, DepthEntry right)
		{
			int num = left.SafeZIndex.CompareTo(right.SafeZIndex);
			if (num != 0)
			{
				return num;
			}
			int num2 = left.SourceZIndex.CompareTo(right.SourceZIndex);
			return (num2 == 0) ? left.Index.CompareTo(right.Index) : num2;
		});
		int num3 = parent.GetChildCount(false) - entries.Count;
		bool flag = true;
		for (int i = 0; i < entries.Count; i++)
		{
			if (entries[i].Index != num3 + i)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			return;
		}
		foreach (DepthEntry entry in entries)
		{
			parent.MoveChild((Node)(object)entry.Item, parent.GetChildCount(false) - 1);
		}
	}

	private static int ReadDepthMeta(CanvasItem item, StringName metaName)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Variant val = default(Variant);
			val = ((GodotObject)item).GetMeta(metaName, val);
			return ((Variant)(ref val)).AsInt32();
		}
		catch
		{
			return 0;
		}
	}
}
