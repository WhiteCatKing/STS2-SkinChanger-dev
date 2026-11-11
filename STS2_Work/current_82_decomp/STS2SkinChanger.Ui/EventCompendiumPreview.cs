using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.addons.mega_text;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class EventCompendiumPreview
{
	private static readonly ConditionalWeakTable<Node, object> PreviewNodes = new ConditionalWeakTable<Node, object>();

	public static bool IsPreview(Node node)
	{
		object value;
		return PreviewNodes.TryGetValue(node, out value);
	}

	public static EventModel[] Events()
	{
		List<EventModel> list = new List<EventModel>();
		try
		{
			Type[] allAbstractModelSubtypes = ModelDb.AllAbstractModelSubtypes;
			foreach (Type type in allAbstractModelSubtypes)
			{
				if (type.IsAbstract || type.IsGenericTypeDefinition || !typeof(EventModel).IsAssignableFrom(type))
				{
					continue;
				}
				try
				{
					EventModel byIdOrNull = ModelDb.GetByIdOrNull<EventModel>(ModelDb.GetId(type));
					if (byIdOrNull != null)
					{
						list.Add(byIdOrNull);
					}
				}
				catch
				{
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("枚举事件失败：" + ex.GetBaseException().Message);
		}
		if (list.Count == 0)
		{
			list.AddRange(ModelDb.AllEvents);
		}
		return list.Where((EventModel model) => !(model is AncientEventModel) && ((AbstractModel)model).Id.Entry != "FAKE_MERCHANT").DistinctBy((EventModel model) => ((AbstractModel)model).Id).OrderBy<EventModel, string>((EventModel model) => Title(model), StringComparer.CurrentCulture)
			.ToArray();
	}

	private static string? ReadCustomString(EventModel model, string propertyName)
	{
		try
		{
			return ((object)model).GetType().GetProperty(propertyName)?.GetValue(model) as string;
		}
		catch
		{
			return null;
		}
	}

	public static string Title(EventModel model)
	{
		string text = Format(model, ((AbstractModel)model).Id.Entry + ".title");
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		return ((AbstractModel)model).Id.Entry;
	}

	public static Control Create(EventModel model, string? selectedPage = null)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Expected O, but got Unknown
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Expected O, but got Unknown
		string groupId = EventSkinPolicy.GroupId(((AbstractModel)model).Id.Entry);
		bool groupExists = EventSkinRuntime.IsManaged(model);
		Control host = new Control
		{
			Name = StringName.op_Implicit("EventSkinPreview"),
			Size = new Vector2(1920f, 1080f)
		};
		NEventLayout layout = PreloadManager.Cache.GetScene("res://scenes/events/default_event_layout.tscn").Instantiate<NEventLayout>((GenEditState)0);
		PreviewNodes.Add((Node)(object)layout, new object());
		((Node)host).AddChild((Node)(object)layout, false, (InternalMode)0);
		string[] keys = LocManager.Instance.GetTable(model.LocTable).Keys.ToArray();
		string[] pages = EventPreviewPolicy.Pages(((AbstractModel)model).Id.Entry, keys);
		if (pages.Length == 0)
		{
			pages = new string[1] { "INITIAL" };
		}
		string page = ((selectedPage != null && pages.Contains<string>(selectedPage)) ? selectedPage : pages[0]);
		OptionButton val = new OptionButton
		{
			Position = new Vector2(70f, 80f),
			CustomMinimumSize = new Vector2(150f, 44f),
			FitToLongestItem = false
		};
		ContextualSkinControls.ApplyGameTheme(val);
		for (int i = 0; i < pages.Length; i++)
		{
			val.AddItem($"{i + 1}/{pages.Length}", -1);
		}
		val.Select(Array.IndexOf(pages, page));
		((CanvasItem)val).Visible = pages.Length > 1;
		((Node)host).AddChild((Node)(object)val, false, (InternalMode)0);
		ScrollContainer pageScroll = null;
		Action resizeText = null;
		val.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			Populate(pages[checked((int)index)]);
		};
		((Node)host).Ready += delegate
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Expected O, but got Unknown
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
			VBoxContainer column = ((Node)layout).GetNode<VBoxContainer>(NodePath.op_Implicit("VBoxContainer"));
			Rect2 nativeBounds = ((Control)column).GetRect();
			Rect2 val2 = EventPreviewPolicy.TextBounds(host.Size, nativeBounds, 0f);
			ScrollContainer scroll = new ScrollContainer
			{
				Name = StringName.op_Implicit("EventPageScroll"),
				Position = ((Rect2)(ref val2)).Position,
				Size = ((Rect2)(ref val2)).Size,
				HorizontalScrollMode = (ScrollMode)0
			};
			((Node)layout).AddChild((Node)(object)scroll, false, (InternalMode)0);
			((Node)column).Reparent((Node)(object)scroll, false);
			((Control)column).SetAnchorsAndOffsetsPreset((LayoutPreset)0, (LayoutPresetMode)0, 0);
			((Control)column).CustomMinimumSize = new Vector2(800f, 0f);
			((Control)column).SizeFlagsHorizontal = (SizeFlags)3;
			pageScroll = scroll;
			bool resizeQueued = false;
			resizeText = delegate
			{
				//IL_002f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				if (!resizeQueued)
				{
					resizeQueued = true;
					Callable val4 = Callable.From((Action)delegate
					{
						//IL_0049: Unknown result type (might be due to invalid IL or missing references)
						//IL_004f: Unknown result type (might be due to invalid IL or missing references)
						//IL_005a: Unknown result type (might be due to invalid IL or missing references)
						//IL_0064: Unknown result type (might be due to invalid IL or missing references)
						//IL_0069: Unknown result type (might be due to invalid IL or missing references)
						//IL_0072: Unknown result type (might be due to invalid IL or missing references)
						//IL_0084: Unknown result type (might be due to invalid IL or missing references)
						resizeQueued = false;
						if (GodotObject.IsInstanceValid((GodotObject)(object)host) && ((Node)host).IsInsideTree() && !((GodotObject)host).IsQueuedForDeletion())
						{
							Rect2 val3 = EventPreviewPolicy.TextBounds(host.Size, nativeBounds, ((Control)column).GetCombinedMinimumSize().Y);
							((Control)scroll).Position = ((Rect2)(ref val3)).Position;
							((Control)scroll).Size = ((Rect2)(ref val3)).Size;
						}
					});
					((Callable)(ref val4)).CallDeferred(Array.Empty<Variant>());
				}
			};
			((Control)column).MinimumSizeChanged += delegate
			{
				resizeText();
			};
			host.Resized += delegate
			{
				resizeText();
			};
			try
			{
				string text = "res://images/events/" + ((AbstractModel)model).Id.Entry.ToLowerInvariant() + ".png";
				Texture2D val5 = null;
				try
				{
					val5 = model.CreateInitialPortrait();
				}
				catch
				{
				}
				if (val5 == null)
				{
					string text2 = ReadCustomString(model, "CustomInitialPortraitPath");
					string text3 = ((!string.IsNullOrWhiteSpace(text2) && ResourceLoader.Exists(text2, "")) ? text2 : text);
					if (ResourceLoader.Exists(text3, ""))
					{
						val5 = (Texture2D)((groupExists && text3.Equals(text, StringComparison.OrdinalIgnoreCase)) ? ((object)EventSkinRuntime.LoadTexture(groupId, text3)) : ((object)PreloadManager.Cache.GetTexture2D(text3)));
					}
				}
				if (val5 != null)
				{
					layout.SetPortrait(val5, (Texture2D)null);
				}
				PackedScene val6 = null;
				try
				{
					val6 = model.CreateBackgroundScene();
				}
				catch
				{
				}
				if (val6 == null)
				{
					string text4 = ReadCustomString(model, "CustomBackgroundScenePath");
					if (!string.IsNullOrWhiteSpace(text4) && ResourceLoader.Exists(text4, ""))
					{
						val6 = PreloadManager.Cache.GetScene(text4);
					}
				}
				if (val6 != null)
				{
					Control val7 = val6.Instantiate<Control>((GenEditState)0);
					val7.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
					val7.MouseFilter = (MouseFilterEnum)2;
					((Node)layout).AddChild((Node)(object)val7, false, (InternalMode)0);
					((Node)layout).MoveChild((Node)(object)val7, 0);
				}
				if (model.HasVfx)
				{
					Node2D val8 = model.CreateVfx();
					layout.AddVfxAnchoredToPortrait((Node)(object)val8);
					val8.Position = EventModel.VfxOffset;
				}
			}
			catch (Exception value)
			{
				ModLog.Error($"事件图鉴预览 {((AbstractModel)model).Id.Entry} 失败：{value}");
			}
			Populate(page);
		};
		return host;
		void Populate(string pageId)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			MegaLabel nodeOrNull = ((Node)layout).GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("%Title"));
			if (nodeOrNull != null)
			{
				((Label)nodeOrNull).Text = Title(model);
				((CanvasItem)nodeOrNull).Modulate = Colors.White;
			}
			MegaRichTextLabel nodeOrNull2 = ((Node)layout).GetNodeOrNull<MegaRichTextLabel>(NodePath.op_Implicit("%EventDescription"));
			if (nodeOrNull2 != null)
			{
				nodeOrNull2.SetTextAutoSize(Format(model, ((AbstractModel)model).Id.Entry + ".pages." + pageId + ".description"));
				((CanvasItem)nodeOrNull2).Modulate = Colors.White;
				((RichTextLabel)nodeOrNull2).VisibleRatio = 1f;
			}
			Control nodeOrNull3 = ((Node)layout).GetNodeOrNull<Control>(NodePath.op_Implicit("%SharedEventLabel"));
			if (nodeOrNull3 != null)
			{
				((CanvasItem)nodeOrNull3).Visible = false;
			}
			VBoxContainer node = ((Node)layout).GetNode<VBoxContainer>(NodePath.op_Implicit("%OptionsContainer"));
			foreach (Node child in ((Node)node).GetChildren(false))
			{
				((Node)node).RemoveChild(child);
				child.QueueFree();
			}
			string[] array = EventPreviewPolicy.Options(((AbstractModel)model).Id.Entry, pageId, keys);
			foreach (string text5 in array)
			{
				NEventOptionButton val9 = PreloadManager.Cache.GetScene("res://scenes/events/event_option_button.tscn").Instantiate<NEventOptionButton>((GenEditState)0);
				PreviewNodes.Add((Node)(object)val9, new object());
				((Control)val9).MouseFilter = (MouseFilterEnum)2;
				((Control)val9).FocusMode = (FocusModeEnum)0;
				((Node)val9).ProcessMode = (ProcessModeEnum)4;
				((Node)node).AddChild((Node)(object)val9, false, (InternalMode)0);
				string text6 = Format(model, text5 + ".title");
				string text7 = Format(model, text5 + ".description");
				((Node)val9).GetNode<MegaRichTextLabel>(NodePath.op_Implicit("%Text")).SetTextAutoSize("[gold][b]" + text6 + "[/b][/gold]\n" + text7);
			}
			if (pageScroll != null)
			{
				pageScroll.ScrollVertical = 0;
			}
			resizeText?.Invoke();
		}
	}

	private static string Format(EventModel model, string key)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		if (!LocString.Exists(model.LocTable, key))
		{
			return "";
		}
		LocString text = new LocString(model.LocTable, key);
		model.DynamicVars.AddTo(text);
		((CharacterModel)ModelDb.Character<Ironclad>()).AddDetailsTo(text);
		text.Add("IsMultiplayer", false);
		string rawText = text.GetRawText();
		if (Regex.Matches(rawText, "(?<!\\{)\\{([A-Za-z_]\\w*)").Any((Match match) => !text.Variables.ContainsKey(match.Groups[1].Value)))
		{
			return rawText;
		}
		return text.GetFormattedText();
	}
}
