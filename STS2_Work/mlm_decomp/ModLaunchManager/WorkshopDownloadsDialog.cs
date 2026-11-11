using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;

namespace ModLaunchManager;

internal static class WorkshopDownloadsDialog
{
	private const double PollSeconds = 0.5;

	private static CanvasLayer? _layer;

	private static Control? _root;

	private static Label? _title;

	private static Label? _subtitle;

	private static Label? _summary;

	private static Label? _empty;

	private static ScrollContainer? _scroll;

	private static VBoxContainer? _list;

	private static Button? _close;

	private static Button? _clear;

	private static Button? _done;

	private static Action<ulong, string>? _openPage;

	private static Action<ulong, string, string>? _removeItem;

	private static Func<ulong, bool>? _canRemoveItem;

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

	internal static void Show(Action<ulong, string> openPage, Action<ulong, string, string>? removeItem = null, Func<ulong, bool>? canRemoveItem = null)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected O, but got Unknown
		_openPage = openPage;
		_removeItem = removeItem;
		_canRemoveItem = canRemoveItem;
		if (IsOpen)
		{
			Render();
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			_layer = new CanvasLayer
			{
				Name = StringName.op_Implicit("WorkshopDownloadsDialog"),
				Layer = 1150
			};
			_root = UiKit.FullRect<Control>(new Control
			{
				Name = StringName.op_Implicit("WorkshopDownloadsRoot"),
				MouseFilter = (MouseFilterEnum)0,
				Theme = UiKit.CreateTheme()
			});
			ColorRect control = new ColorRect
			{
				Color = UiKit.Alpha(UiKit.Canvas, 0.88f),
				MouseFilter = (MouseFilterEnum)0
			};
			((Node)_root).AddChild((Node)(object)UiKit.FullRect<ColorRect>(control), false, (InternalMode)0);
			PanelContainer val2 = UiKit.Card("DialogPanel");
			((Node)val2).Name = StringName.op_Implicit("WorkshopDownloadsCard");
			((Control)val2).AnchorLeft = 0.22f;
			((Control)val2).AnchorTop = 0.12f;
			((Control)val2).AnchorRight = 0.78f;
			((Control)val2).AnchorBottom = 0.88f;
			((Control)val2).OffsetLeft = 0f;
			((Control)val2).OffsetTop = 0f;
			((Control)val2).OffsetRight = 0f;
			((Control)val2).OffsetBottom = 0f;
			((Node)val2).AddChild((Node)(object)BuildContent(), false, (InternalMode)0);
			((Node)_root).AddChild((Node)(object)val2, false, (InternalMode)0);
			Timer val3 = new Timer
			{
				WaitTime = 0.5,
				Autostart = true
			};
			val3.Timeout += Render;
			((Node)_root).AddChild((Node)(object)val3, false, (InternalMode)0);
			((Node)_layer).AddChild((Node)(object)_root, false, (InternalMode)0);
			((Node)val.Root).AddChild((Node)(object)_layer, false, (InternalMode)0);
			LauncherController.Active?.AddScope(new LauncherController.Scope(_root, Close, () => (Control?)(object)_done));
			Refresh();
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
		_title = null;
		_subtitle = null;
		_summary = null;
		_empty = null;
		_scroll = null;
		_list = null;
		_close = null;
		_clear = null;
		_done = null;
		_openPage = null;
		_removeItem = null;
		_canRemoveItem = null;
		_signature = string.Empty;
	}

	internal static void Refresh()
	{
		if (IsOpen && _root != null)
		{
			_root.Theme = UiKit.CreateTheme();
			_signature = string.Empty;
			if (_title != null)
			{
				_title.Text = MlmLocManager.Get("downloads.title");
			}
			if (_subtitle != null)
			{
				_subtitle.Text = MlmLocManager.Get("downloads.subtitle");
			}
			if (_empty != null)
			{
				_empty.Text = MlmLocManager.Get("downloads.empty");
			}
			if (_clear != null)
			{
				_clear.Text = MlmLocManager.Get("downloads.clearFinished");
			}
			if (_done != null)
			{
				_done.Text = MlmLocManager.Get("common.action.close");
			}
			if (_close != null)
			{
				((Control)_close).TooltipText = MlmLocManager.Get("common.action.close");
			}
			Render();
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
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Expected O, but got Unknown
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Expected O, but got Unknown
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Expected O, but got Unknown
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Expected O, but got Unknown
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
		_title = UiKit.Label(MlmLocManager.Get("downloads.title"), "SectionLabel");
		_subtitle = UiKit.Label(MlmLocManager.Get("downloads.subtitle"), "DimLabel");
		_subtitle.AutowrapMode = (AutowrapMode)3;
		((Node)val3).AddChild((Node)(object)_title, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)_subtitle, false, (InternalMode)0);
		_close = UiKit.Button("✕", "AccentIconButton", 34f, 32f);
		((Control)_close).SizeFlagsVertical = (SizeFlags)0;
		((Control)_close).MouseDefaultCursorShape = (CursorShape)2;
		((BaseButton)_close).Pressed += Close;
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_close, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)UiKit.Divider(), false, (InternalMode)0);
		_summary = UiKit.Label(string.Empty, "MutedLabel");
		((Node)val).AddChild((Node)(object)_summary, false, (InternalMode)0);
		_empty = UiKit.Label(MlmLocManager.Get("downloads.empty"), "DimLabel");
		_empty.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)_empty, false, (InternalMode)0);
		_list = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_list).AddThemeConstantOverride(StringName.op_Implicit("separation"), 6);
		_scroll = new ScrollContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3,
			HorizontalScrollMode = (ScrollMode)0
		};
		((Node)_scroll).AddChild((Node)(object)_list, false, (InternalMode)0);
		UiKit.FollowFocus(_scroll);
		((Node)val).AddChild((Node)(object)_scroll, false, (InternalMode)0);
		HBoxContainer val4 = new HBoxContainer();
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_clear = UiKit.Button(MlmLocManager.Get("downloads.clearFinished"), "SecondaryButton", 0f, 34f);
		((Control)_clear).TooltipText = MlmLocManager.Get("downloads.clearFinished.tooltip");
		((BaseButton)_clear).Pressed += delegate
		{
			WorkshopDownloadCenter.ClearFinished();
			Render();
		};
		((Node)val4).AddChild((Node)(object)_clear, false, (InternalMode)0);
		((Node)val4).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		_done = UiKit.Button(MlmLocManager.Get("common.action.close"), "PrimaryButton", 96f, 34f);
		((BaseButton)_done).Pressed += Close;
		((Node)val4).AddChild((Node)(object)_done, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val4, false, (InternalMode)0);
		return (Control)(object)obj;
	}

	internal static void Render()
	{
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOpen || _list == null || _summary == null || _empty == null || _clear == null || _scroll == null)
		{
			return;
		}
		IReadOnlyList<WorkshopDownloadRecord> downloads = WorkshopDownloadCenter.Downloads;
		_summary.Text = MlmLocManager.Get("downloads.summary", ("active", WorkshopDownloadCenter.ActiveCount), ("complete", WorkshopDownloadCenter.CompleteCount), ("failed", WorkshopDownloadCenter.FailedCount));
		((CanvasItem)_summary).Visible = downloads.Count > 0;
		((CanvasItem)_empty).Visible = downloads.Count == 0;
		((CanvasItem)_scroll).Visible = downloads.Count > 0;
		((BaseButton)_clear).Disabled = WorkshopDownloadCenter.CompleteCount == 0;
		List<WorkshopDownloadRecord> list = downloads.Reverse().ToList();
		string text = string.Join('\u001f', list.Select((WorkshopDownloadRecord record) => $"{record.ItemId}\u001e{record.State}\u001e{(int)Math.Round(record.Progress * 100f)}\u001e{record.Name}\u001e{record.Origin}"));
		if (string.Equals(text, _signature, StringComparison.Ordinal))
		{
			return;
		}
		_signature = text;
		int scrollPosition = _scroll.ScrollVertical;
		foreach (Node child in ((Node)_list).GetChildren(false))
		{
			((Node)_list).RemoveChild(child);
			child.QueueFree();
		}
		foreach (WorkshopDownloadRecord item in list)
		{
			((Node)_list).AddChild((Node)(object)RowFor(item), false, (InternalMode)0);
		}
		ScrollContainer scroll = _scroll;
		Callable val = Callable.From((Action)delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)scroll))
			{
				scroll.ScrollVertical = scrollPosition;
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static Control RowFor(WorkshopDownloadRecord record)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Expected O, but got Unknown
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		PanelContainer val = UiKit.Card("InsetPanel");
		((Control)val).SizeFlagsVertical = (SizeFlags)0;
		HBoxContainer val2 = new HBoxContainer();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		VBoxContainer val3 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)4
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 3);
		string name = (string.IsNullOrWhiteSpace(record.Name) ? MlmLocManager.Get("collection.row.untitled", ("id", record.ItemId)) : record.Name.Trim());
		Label val4 = UiKit.Label(name);
		val4.TextOverrunBehavior = (OverrunBehavior)3;
		val4.ClipText = true;
		((Control)val4).TooltipText = name;
		((Control)val4).MouseFilter = (MouseFilterEnum)1;
		Label val5 = UiKit.Label(SourceText(record), "DimLabel");
		val5.TextOverrunBehavior = (OverrunBehavior)3;
		val5.ClipText = true;
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val5, false, (InternalMode)0);
		if (record.State == WorkshopDownloadState.Downloading)
		{
			((Node)val3).AddChild((Node)new ProgressBar
			{
				MinValue = 0.0,
				MaxValue = 100.0,
				Value = record.Progress * 100f,
				ShowPercentage = false,
				CustomMinimumSize = new Vector2(0f, 6f)
			}, false, (InternalMode)0);
		}
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		string text = StateText(record);
		Label val6 = UiKit.Label(text, record.State switch
		{
			WorkshopDownloadState.Complete => "SuccessLabel", 
			WorkshopDownloadState.Failed => "WarningLabel", 
			_ => "DimLabel", 
		});
		((Control)val6).CustomMinimumSize = new Vector2(150f, 0f);
		val6.HorizontalAlignment = (HorizontalAlignment)2;
		((Control)val6).SizeFlagsVertical = (SizeFlags)4;
		((Control)val6).MouseFilter = (MouseFilterEnum)1;
		if (record.State == WorkshopDownloadState.Failed)
		{
			((Control)val6).TooltipText = MlmLocManager.Get("downloads.state.failed.tooltip");
		}
		((Node)val2).AddChild((Node)(object)val6, false, (InternalMode)0);
		if (record.State == WorkshopDownloadState.Failed)
		{
			Button val7 = UiKit.Button(MlmLocManager.Get("downloads.retry"), "SecondaryButton", 0f, 30f);
			((Control)val7).SizeFlagsVertical = (SizeFlags)4;
			ulong itemId = record.ItemId;
			((BaseButton)val7).Pressed += delegate
			{
				WorkshopDownloadCenter.Retry(itemId);
				Render();
			};
			((Node)val2).AddChild((Node)(object)val7, false, (InternalMode)0);
		}
		Button val8 = UiKit.Button(MlmLocManager.Get("downloads.openPage"), "MiniButton", 0f, 30f);
		((Control)val8).TooltipText = MlmLocManager.Get("transfer.import.openWorkshop.tooltip");
		((Control)val8).SizeFlagsVertical = (SizeFlags)4;
		ulong pageId = record.ItemId;
		((BaseButton)val8).Pressed += delegate
		{
			_openPage?.Invoke(pageId, name);
		};
		((Node)val2).AddChild((Node)(object)val8, false, (InternalMode)0);
		if (_removeItem != null)
		{
			Button val9 = UiKit.Button(MlmLocManager.Get("workshop.remove.unsubscribe"), "SecondaryButton", 0f, 30f);
			((Control)val9).SizeFlagsVertical = (SizeFlags)4;
			Func<ulong, bool>? canRemoveItem = _canRemoveItem;
			((BaseButton)val9).Disabled = canRemoveItem != null && !canRemoveItem(pageId);
			((Control)val9).TooltipText = MlmLocManager.Get(((BaseButton)val9).Disabled ? "workshop.remove.locked" : "workshop.remove.tooltip");
			((BaseButton)val9).Pressed += delegate
			{
				_removeItem?.Invoke(pageId, name, MlmLocManager.Get("workshop.remove.unsubscribe"));
			};
			((Node)val2).AddChild((Node)(object)val9, false, (InternalMode)0);
		}
		((Node)val).AddChild((Node)(object)UiKit.Margin((Control)(object)val2, 12, 8), false, (InternalMode)0);
		return (Control)(object)val;
	}

	private static string SourceText(WorkshopDownloadRecord record)
	{
		return record.Source switch
		{
			WorkshopDownloadSource.Collection => string.IsNullOrWhiteSpace(record.Origin) ? MlmLocManager.Get("downloads.source.collectionUnnamed") : MlmLocManager.Get("downloads.source.collection", ("name", record.Origin)), 
			WorkshopDownloadSource.ModList => MlmLocManager.Get("downloads.source.modList"), 
			WorkshopDownloadSource.Friend => MlmLocManager.Get("downloads.source.friend"), 
			_ => MlmLocManager.Get("downloads.source.update"), 
		};
	}

	private static string StateText(WorkshopDownloadRecord record)
	{
		return record.State switch
		{
			WorkshopDownloadState.Downloading => (record.TotalBytes != 0) ? MlmLocManager.Get("downloads.state.downloadingBytes", ("progress", (int)Math.Round(record.Progress * 100f)), ("done", Megabytes(record.DownloadedBytes)), ("total", Megabytes(record.TotalBytes))) : MlmLocManager.Get("downloads.state.downloading", ("progress", (int)Math.Round(record.Progress * 100f))), 
			WorkshopDownloadState.Complete => MlmLocManager.Get("downloads.state.complete"), 
			WorkshopDownloadState.Failed => MlmLocManager.Get("downloads.state.failed"), 
			_ => MlmLocManager.Get("downloads.state.queued"), 
		};
	}

	private static string Megabytes(ulong bytes)
	{
		return ((double)bytes / 1048576.0).ToString((bytes >= 10485760) ? "0" : "0.0", CultureInfo.InvariantCulture);
	}
}
