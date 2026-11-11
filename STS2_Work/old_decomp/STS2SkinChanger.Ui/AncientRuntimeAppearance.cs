using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class AncientRuntimeAppearance
{
	private static readonly FieldInfo AncientEventField = AccessTools.Field(typeof(NAncientEventLayout), "_ancientEvent");

	internal static bool TryGetCurrent(out AncientEventModel ancient, out NAncientEventLayout layout, out SkinGroup group)
	{
		ancient = null;
		layout = null;
		group = null;
		NEventRoom instance = NEventRoom.Instance;
		NEventLayout obj = ((instance != null) ? instance.Layout : null);
		NAncientEventLayout val = (NAncientEventLayout)(object)((obj is NAncientEventLayout) ? obj : null);
		if (val == null || !TryGet(val, out AncientEventModel ancient2, out SkinGroup group2))
		{
			return false;
		}
		ancient = ancient2;
		layout = val;
		group = group2;
		return true;
	}

	internal static bool TryGet(NAncientEventLayout layout, out AncientEventModel ancient, out SkinGroup group)
	{
		ancient = null;
		group = null;
		object? value = AncientEventField.GetValue(layout);
		AncientEventModel val = (AncientEventModel)((value is AncientEventModel) ? value : null);
		if (val == null)
		{
			return false;
		}
		SkinGroup skinGroup = AncientCompendiumEntry.FindGroup(((AbstractModel)val).Id.Entry);
		if (skinGroup == null)
		{
			return false;
		}
		ancient = val;
		group = skinGroup;
		return true;
	}

	internal static Control? GetBackgroundTarget(NAncientEventLayout layout)
	{
		return ((Node)layout).GetNodeOrNull<Control>(NodePath.op_Implicit("%AncientBgContainer"));
	}

	internal static bool TryRefresh(string groupId, out string? error)
	{
		error = null;
		if (!TryGetCurrent(out AncientEventModel ancient, out NAncientEventLayout layout, out SkinGroup group) || !group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		Node nodeOrNull = ((Node)layout).GetNodeOrNull<Node>(NodePath.op_Implicit("%AncientBgContainer"));
		if (nodeOrNull == null)
		{
			error = "Ancient background container is unavailable";
			return false;
		}
		Node[] array = ((IEnumerable)nodeOrNull.GetChildren(false)).Cast<Node>().ToArray();
		Node val = null;
		try
		{
			val = (Node)(object)((EventModel)ancient).CreateBackgroundScene().Instantiate<Control>((GenEditState)0);
			if (array.Length != 0)
			{
				val.Name = array[0].Name;
			}
			Node[] array2 = array;
			foreach (Node val2 in array2)
			{
				nodeOrNull.RemoveChild(val2);
			}
			nodeOrNull.AddChild(val, false, (InternalMode)0);
			ManagedAncientStaticBackground.Fit(val);
			ManagedAncientLayeredImage.TryApply(group.Id, val);
			ManagedAncientSceneAnimation.TryStart(group.Id, val);
			array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].QueueFree();
			}
			return true;
		}
		catch (Exception ex)
		{
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				Node parent = val.GetParent();
				if (parent != null)
				{
					parent.RemoveChild(val);
				}
				val.QueueFree();
			}
			foreach (Node item in ((IEnumerable<Node>)array).Where((Func<Node, bool>)GodotObject.IsInstanceValid))
			{
				if (item.GetParent() == null)
				{
					nodeOrNull.AddChild(item, false, (InternalMode)0);
				}
			}
			error = ex.GetBaseException().Message;
			ModLog.Error("热重载当前先古场景失败：" + ex);
			return false;
		}
	}
}
