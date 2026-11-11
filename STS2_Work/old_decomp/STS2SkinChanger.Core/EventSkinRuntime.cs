using System;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.addons.mega_text;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class EventSkinRuntime
{
	internal const string TextureSourceMeta = "sc_event_texture_source";

	internal const string VfxSourceMeta = "sc_event_vfx_source";

	private const string PortraitMeta = "sc_event_portrait";

	private const string PhobiaPortraitMeta = "sc_event_phobia_portrait";

	private static readonly FieldInfo EventField = AccessTools.Field(typeof(NEventLayout), "_event");

	public static bool IsManaged(EventModel model)
	{
		if (!(model is AncientEventModel))
		{
			return SkinService.Catalog?.Groups.Any((SkinGroup group) => group.Id == EventSkinPolicy.GroupId(((AbstractModel)model).Id.Entry)) ?? false;
		}
		return false;
	}

	public static void RememberPortrait(NEventLayout layout, Texture2D portrait, Texture2D? phobia)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		string text = Source(portrait);
		if (EventSkinPolicy.IsEventGroup(SkinService.Catalog?.FindGroupIdForResourcePath(text) ?? ""))
		{
			((GodotObject)layout).SetMeta(StringName.op_Implicit("sc_event_portrait"), Variant.op_Implicit(text));
			((GodotObject)layout).SetMeta(StringName.op_Implicit("sc_event_phobia_portrait"), Variant.op_Implicit((phobia == null) ? "" : Source(phobia)));
		}
		static string Source(Texture2D texture)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			Variant meta = ((GodotObject)texture).GetMeta(StringName.op_Implicit("sc_event_texture_source"), Variant.op_Implicit(((Resource)texture).ResourcePath));
			return ((Variant)(ref meta)).AsString();
		}
	}

	public static void RefreshCurrent(string groupId)
	{
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		if (!EventSkinPolicy.IsEventGroup(groupId))
		{
			return;
		}
		NEventRoom instance = NEventRoom.Instance;
		NEventLayout val = ((instance != null) ? instance.Layout : null);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return;
		}
		object? value = EventField.GetValue(val);
		EventModel val2 = (EventModel)((value is EventModel) ? value : null);
		if (val2 == null || EventSkinPolicy.GroupId(((AbstractModel)val2).Id.Entry) != groupId)
		{
			return;
		}
		try
		{
			val.SetTitle(val2.Title.GetFormattedText());
			if (val2.Description != null)
			{
				MegaRichTextLabel nodeOrNull = ((Node)val).GetNodeOrNull<MegaRichTextLabel>(NodePath.op_Implicit("%EventDescription"));
				if (nodeOrNull != null)
				{
					nodeOrNull.SetTextAutoSize(val2.Description.GetFormattedText());
				}
			}
			foreach (NEventOptionButton optionButton in val.OptionButtons)
			{
				LocString title = optionButton.Option.Title;
				string text = ((title != null) ? title.GetFormattedText() : null) ?? "";
				LocString description = optionButton.Option.Description;
				string text2 = ((description != null) ? description.GetFormattedText() : null) ?? "";
				string value2 = (optionButton.Option.IsLocked ? "red" : "gold");
				string textAutoSize = ((text.Length == 0) ? text2 : $"[{value2}][b]{text}[/b][/{value2}]\n{text2}");
				MegaRichTextLabel nodeOrNull2 = ((Node)optionButton).GetNodeOrNull<MegaRichTextLabel>(NodePath.op_Implicit("%Text"));
				if (nodeOrNull2 != null)
				{
					nodeOrNull2.SetTextAutoSize(textAutoSize);
				}
			}
			Variant meta = ((GodotObject)val).GetMeta(StringName.op_Implicit("sc_event_portrait"), Variant.op_Implicit(""));
			string text3 = ((Variant)(ref meta)).AsString();
			if (text3.Length > 0)
			{
				string text4 = (val2.HasPhobiaModePortrait ? ("res://images/events/" + ((AbstractModel)val2).Id.Entry.ToLowerInvariant() + "_phobia_mode.png") : "");
				val.SetPortrait(LoadTexture(groupId, text3), (text4.Length == 0) ? null : LoadTexture(groupId, text4));
			}
			if (!RefreshVfx((Node)(object)val, groupId, val2.HasVfx) && val2.HasVfx)
			{
				Node2D val3 = val2.CreateVfx();
				val.AddVfxAnchoredToPortrait((Node)(object)val3);
				val3.Position = EventModel.VfxOffset;
			}
		}
		catch (Exception value3)
		{
			ModLog.Error($"刷新事件外观 {((AbstractModel)val2).Id.Entry} 失败：{value3}");
		}
	}

	internal static Texture2D LoadTexture(string groupId, string path)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		Texture2D val = (Texture2D)SkinService.GetOrLoadRuntimeResource(groupId, path);
		((GodotObject)val).SetMeta(StringName.op_Implicit("sc_event_texture_source"), Variant.op_Implicit(path));
		return val;
	}

	private static bool RefreshVfx(Node parent, string groupId, bool enabled)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		foreach (Node child in parent.GetChildren(false))
		{
			Node2D val = (Node2D)(object)((child is Node2D) ? child : null);
			if (val != null && ((GodotObject)child).HasMeta(StringName.op_Implicit("sc_event_vfx_source")))
			{
				flag = true;
				if (enabled)
				{
					StringName obj = StringName.op_Implicit("sc_event_vfx_source");
					Variant val2 = default(Variant);
					val2 = ((GodotObject)child).GetMeta(obj, val2);
					string text = ((Variant)(ref val2)).AsString();
					Node2D val3 = SkinService.InstantiateRuntimeScene<Node2D>(groupId, text, (Action?)null);
					((GodotObject)val3).SetMeta(StringName.op_Implicit("sc_event_vfx_source"), Variant.op_Implicit(text));
					val3.Transform = val.Transform;
					((CanvasItem)val3).ZIndex = ((CanvasItem)val).ZIndex;
					parent.AddChild((Node)(object)val3, false, (InternalMode)0);
					parent.MoveChild((Node)(object)val3, ((Node)val).GetIndex(false));
				}
				parent.RemoveChild((Node)(object)val);
				((Node)val).QueueFree();
			}
			else
			{
				flag |= RefreshVfx(child, groupId, enabled);
			}
		}
		return flag;
	}
}
