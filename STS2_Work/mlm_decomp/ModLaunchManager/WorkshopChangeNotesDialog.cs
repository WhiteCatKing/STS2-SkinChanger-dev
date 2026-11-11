using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ModLaunchManager;

internal static class WorkshopChangeNotesDialog
{
	private const int NoteLength = 4000;

	private const string FocusKey = "change_note_action";

	private static CanvasLayer? _layer;

	private static Control? _root;

	private static Label? _summary;

	private static ScrollContainer? _scroll;

	private static VBoxContainer? _list;

	private static Button? _close;

	private static Button? _translateAll;

	private static Func<ulong, string>? _nameFor;

	private static Action<ulong>? _openPage;

	private static string _signature = string.Empty;

	internal static bool IsOpen
	{
		get
		{
			if (_layer != null)
			{
				return GodotObject.IsInstanceValid((GodotObject)(object)_layer);
			}
			return false;
		}
	}

	internal static void Show(Func<ulong, string> nameFor, Action<ulong> openPage)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		_nameFor = nameFor;
		_openPage = openPage;
		if (IsOpen)
		{
			Refresh();
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			_layer = new CanvasLayer
			{
				Name = StringName.op_Implicit("WorkshopChangeNotesDialog"),
				Layer = 1100
			};
			_root = UiKit.FullRect<Control>(new Control
			{
				Name = StringName.op_Implicit("WorkshopChangeNotesRoot"),
				MouseFilter = (MouseFilterEnum)0,
				Theme = UiKit.CreateTheme()
			});
			((Node)_root).AddChild((Node)(object)UiKit.FullRect<ColorRect>(new ColorRect
			{
				Color = UiKit.Alpha(UiKit.Canvas, 0.88f),
				MouseFilter = (MouseFilterEnum)0
			}), false, (InternalMode)0);
			PanelContainer val2 = UiKit.Card("DialogPanel");
			((Node)val2).Name = StringName.op_Implicit("WorkshopChangeNotesCard");
			((Control)val2).AnchorLeft = 0.16f;
			((Control)val2).AnchorRight = 0.84f;
			((Control)val2).AnchorTop = 0.1f;
			((Control)val2).AnchorBottom = 0.9f;
			((Node)val2).AddChild((Node)(object)BuildContent(), false, (InternalMode)0);
			((Node)_root).AddChild((Node)(object)val2, false, (InternalMode)0);
			UiKit.FitDialogHeight((Control)(object)val2, _scroll, 0.8f);
			((Node)_layer).AddChild((Node)(object)_root, false, (InternalMode)0);
			((Node)val.Root).AddChild((Node)(object)_layer, false, (InternalMode)0);
			LauncherController.Active?.AddScope(new LauncherController.Scope(_root, Close, () => (Control?)(object)_close)
			{
				ScrollTarget = () => _scroll
			});
			_signature = string.Empty;
			Refresh();
			Button? close = _close;
			if (close != null)
			{
				((Control)close).GrabFocus();
			}
		}
	}

	internal static void Close()
	{
		if (_layer != null && GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			((Node)_layer).QueueFree();
		}
		_layer = null;
		_root = null;
		_summary = null;
		_scroll = null;
		_list = null;
		_close = null;
		_translateAll = null;
		_nameFor = null;
		_openPage = null;
		_signature = string.Empty;
	}

	internal static void Refresh()
	{
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOpen || _list == null || _summary == null || _nameFor == null)
		{
			return;
		}
		IReadOnlyList<WorkshopChangeNoteItem> updates = WorkshopChangeNoteFeed.Updates;
		string text = MlmLocManager.EffectiveLocale + "|" + WorkshopChangeNoteTranslations.Language + "|" + WorkshopChangeNoteTranslations.Busy + "|" + string.Join(";", updates.Select((WorkshopChangeNoteItem item) => $"{item.ItemId}:{item.State}:{item.Notes.Count}:{item.Truncated}:" + WorkshopChangeNoteTranslations.ViewFor(item.ItemId)));
		_summary.Text = WorkshopChangeNoteText.Summary(updates);
		if (_translateAll != null)
		{
			List<ulong> itemIds = updates.Select((WorkshopChangeNoteItem item) => item.ItemId).ToList();
			((CanvasItem)_translateAll).Visible = WorkshopChangeNoteTranslations.CanTranslateAny(itemIds) || WorkshopChangeNoteTranslations.Busy;
			((BaseButton)_translateAll).Disabled = WorkshopChangeNoteTranslations.Busy;
			_translateAll.Text = MlmLocManager.Get(WorkshopChangeNoteTranslations.Busy ? "changeNotes.ai.translating" : "changeNotes.ai.translateAll");
			((Control)_translateAll).TooltipText = MlmLocManager.Get("changeNotes.ai.translateAll.tooltip", ("language", WorkshopChangeNoteTranslations.LanguageName(WorkshopChangeNoteTranslations.Language)));
		}
		if (text == _signature)
		{
			return;
		}
		_signature = text;
		ScrollContainer? scroll = _scroll;
		double scroll2 = ((scroll != null) ? scroll.ScrollVertical : 0);
		Control? root = _root;
		object obj;
		if (root == null)
		{
			obj = null;
		}
		else
		{
			Viewport viewport = ((Node)root).GetViewport();
			obj = ((viewport != null) ? viewport.GuiGetFocusOwner() : null);
		}
		Control val = (Control)obj;
		object obj2;
		if (val == null || !((Node)_list).IsAncestorOf((Node)(object)val) || !((GodotObject)val).HasMeta(StringName.op_Implicit("change_note_action")))
		{
			obj2 = null;
		}
		else
		{
			StringName obj3 = StringName.op_Implicit("change_note_action");
			Variant val2 = default(Variant);
			val2 = ((GodotObject)val).GetMeta(obj3, val2);
			obj2 = ((Variant)(ref val2)).AsString();
		}
		string refocus = (string)obj2;
		foreach (Node child in ((Node)_list).GetChildren(false))
		{
			((Node)_list).RemoveChild(child);
			child.QueueFree();
		}
		if (updates.Count == 0)
		{
			Label val3 = UiKit.Label(MlmLocManager.Get("changeNotes.dialog.empty"), "DimLabel");
			val3.AutowrapMode = (AutowrapMode)3;
			((Node)_list).AddChild((Node)(object)val3, false, (InternalMode)0);
		}
		for (int i = 0; i < updates.Count; i++)
		{
			if (i > 0)
			{
				((Node)_list).AddChild((Node)(object)UiKit.Divider(), false, (InternalMode)0);
			}
			((Node)_list).AddChild((Node)(object)BuildItem(updates[i]), false, (InternalMode)0);
		}
		if (_scroll != null)
		{
			ScrollContainer target = _scroll;
			Callable val4 = Callable.From((Action)delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)target))
				{
					target.ScrollVertical = (int)scroll2;
				}
			});
			((Callable)(ref val4)).CallDeferred(Array.Empty<Variant>());
		}
		if (refocus == null)
		{
			return;
		}
		Button val6 = ((IEnumerable)((Node)_list).FindChildren("*", "Button", true, false)).Cast<Button>().FirstOrDefault((Func<Button, bool>)delegate(Button button)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (((GodotObject)button).HasMeta(StringName.op_Implicit("change_note_action")))
			{
				StringName obj4 = StringName.op_Implicit("change_note_action");
				Variant val5 = default(Variant);
				val5 = ((GodotObject)button).GetMeta(obj4, val5);
				return ((Variant)(ref val5)).AsString() == refocus;
			}
			return false;
		});
		if (val6 != null)
		{
			((Control)val6).GrabFocus();
		}
	}

	private static Control BuildContent()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Expected O, but got Unknown
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Expected O, but got Unknown
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Expected O, but got Unknown
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Expected O, but got Unknown
		MarginContainer obj = UiKit.Margin((Control)new VBoxContainer(), 20, 18);
		VBoxContainer val = (VBoxContainer)((Node)obj).GetChild(0, false);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		HBoxContainer val2 = new HBoxContainer();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		VBoxContainer val3 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 2);
		Label val4 = UiKit.Label(MlmLocManager.Get("changeNotes.dialog.title"), "SectionLabel");
		val4.AutowrapMode = (AutowrapMode)3;
		_summary = UiKit.Label(string.Empty, "DimLabel");
		_summary.AutowrapMode = (AutowrapMode)3;
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)_summary, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		_translateAll = UiKit.Button(MlmLocManager.Get("changeNotes.ai.translateAll"), "SecondaryButton", 0f, 34f);
		((Node)_translateAll).Name = StringName.op_Implicit("TranslateAllWorkshopChangeNotes");
		((Control)_translateAll).SizeFlagsVertical = (SizeFlags)0;
		((BaseButton)_translateAll).Pressed += delegate
		{
			WorkshopChangeNoteTranslations.TranslateAll(WorkshopChangeNoteFeed.Updates.Select((WorkshopChangeNoteItem item) => item.ItemId).ToList());
		};
		((Node)val2).AddChild((Node)(object)_translateAll, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)UiKit.Divider(), false, (InternalMode)0);
		_scroll = new ScrollContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3,
			HorizontalScrollMode = (ScrollMode)0
		};
		_list = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_list).AddThemeConstantOverride(StringName.op_Implicit("separation"), 14);
		((Node)_scroll).AddChild((Node)(object)_list, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)_scroll, false, (InternalMode)0);
		HBoxContainer val5 = new HBoxContainer();
		((Node)val5).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)2
		}, false, (InternalMode)0);
		_close = UiKit.Button(MlmLocManager.Get("changeNotes.dialog.close"), "PrimaryButton", 100f, 38f);
		((Node)_close).Name = StringName.op_Implicit("CloseWorkshopChangeNotes");
		((BaseButton)_close).Pressed += Close;
		((Node)val5).AddChild((Node)(object)_close, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val5, false, (InternalMode)0);
		return (Control)(object)obj;
	}

	private static Control BuildItem(WorkshopChangeNoteItem item)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		VBoxContainer val = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		HBoxContainer val2 = new HBoxContainer();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		Label val3 = UiKit.Label(_nameFor?.Invoke(item.ItemId) ?? item.ItemId.ToString(), "MetricLabel");
		((Control)val3).SizeFlagsHorizontal = (SizeFlags)3;
		val3.AutowrapMode = (AutowrapMode)3;
		ulong itemId = item.ItemId;
		Button val4 = UiKit.Button(string.Empty, "MiniButton", 0f, 30f);
		((Control)val4).SizeFlagsVertical = (SizeFlags)0;
		((GodotObject)val4).SetMeta(StringName.op_Implicit("change_note_action"), Variant.op_Implicit($"translate:{itemId}"));
		WorkshopChangeNoteText.ConfigureTranslateButton(val4, WorkshopChangeNoteTranslations.ViewFor(itemId), locked: false);
		((BaseButton)val4).Pressed += delegate
		{
			WorkshopChangeNoteTranslations.Activate(itemId);
		};
		Button val5 = UiKit.Button(MlmLocManager.Get("changeNotes.action.viewAll"), "MiniButton", 0f, 30f);
		((Control)val5).TooltipText = MlmLocManager.Get("changeNotes.action.viewAll.tooltip");
		((Control)val5).SizeFlagsVertical = (SizeFlags)0;
		((GodotObject)val5).SetMeta(StringName.op_Implicit("change_note_action"), Variant.op_Implicit($"page:{itemId}"));
		((BaseButton)val5).Pressed += delegate
		{
			_openPage?.Invoke(itemId);
		};
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		string text = WorkshopChangeNoteText.StateMessage(item);
		if (text != null)
		{
			Label val6 = UiKit.Label(text, (item.State == WorkshopChangeNoteState.Failed) ? "WarningLabel" : "DimLabel");
			val6.AutowrapMode = (AutowrapMode)3;
			((Node)val).AddChild((Node)(object)val6, false, (InternalMode)0);
		}
		foreach (WorkshopChangeNote note in item.Notes)
		{
			((Node)val).AddChild((Node)(object)WorkshopChangeNoteText.NoteView(itemId, note, 4000), false, (InternalMode)0);
		}
		if (item.Truncated)
		{
			Label val7 = UiKit.Label(MlmLocManager.Get("changeNotes.note.more"), "DimLabel");
			val7.AutowrapMode = (AutowrapMode)3;
			((Node)val).AddChild((Node)(object)val7, false, (InternalMode)0);
		}
		return (Control)(object)val;
	}
}
