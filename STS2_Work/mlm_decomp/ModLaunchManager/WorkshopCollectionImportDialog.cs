using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;

namespace ModLaunchManager;

internal static class WorkshopCollectionImportDialog
{
	private enum RowState
	{
		Ready,
		NeedsDownload,
		Queued,
		Downloading,
		Recognizing,
		NoMods,
		Failed,
		Unavailable,
		OtherGame,
		NotAMod
	}

	private enum MineState
	{
		Hidden,
		Loading,
		Ready,
		Failed
	}

	private sealed class Row(CollectionMember member)
	{
		internal CollectionMember Member { get; } = member;

		internal bool Selected { get; set; }

		internal double WaitedForDiscovery { get; set; }
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<Row, bool> _003C_003E9__59_0;

		public static Func<Control?> _003C_003E9__60_0;

		public static TextSubmittedEventHandler _003C_003E9__63_0;

		public static TextChangedEventHandler _003C_003E9__63_1;

		public static Action _003C_003E9__63_2;

		public static Action _003C_003E9__63_3;

		public static Action _003C_003E9__63_4;

		public static Func<CollectionChoice, bool> _003C_003E9__69_1;

		public static Func<CollectionChoice, bool> _003C_003E9__69_2;

		public static Func<Row, Row> _003C_003E9__79_0;

		public static Func<Row, ulong> _003C_003E9__79_3;

		public static Func<(WorkshopDownloadState State, float Progress), float> _003C_003E9__79_4;

		public static Func<Row, ulong> _003C_003E9__79_7;

		public static Func<(WorkshopDownloadState State, float Progress), bool> _003C_003E9__80_0;

		public static Func<RowState, bool> _003C_003E9__81_0;

		public static Func<RowState, bool> _003C_003E9__81_1;

		public static Func<LaunchEntry, string> _003C_003E9__82_2;

		public static Func<LaunchEntry, string> _003C_003E9__86_0;

		public static Func<Row, bool> _003C_003E9__89_0;

		public static Func<Row, bool> _003C_003E9__90_0;

		public static Func<Row, ulong> _003C_003E9__90_1;

		internal bool _003CActiveDownloads_003Eb__59_0(Row row)
		{
			WorkshopDownloadState? workshopDownloadState = JobOf(row)?.State;
			if (workshopDownloadState.HasValue)
			{
				WorkshopDownloadState valueOrDefault = workshopDownloadState.GetValueOrDefault();
				if ((uint)valueOrDefault <= 1u)
				{
					return true;
				}
			}
			return false;
		}

		internal Control? _003CShow_003Eb__60_0()
		{
			return (Control?)(object)_link;
		}

		internal void _003CBuildContent_003Eb__63_0(string _)
		{
			Read();
		}

		internal void _003CBuildContent_003Eb__63_1(string _)
		{
			_error = null;
			Render();
		}

		internal void _003CBuildContent_003Eb__63_2()
		{
			_host?.OpenDownloads();
		}

		internal void _003CBuildContent_003Eb__63_3()
		{
			Save(apply: false);
		}

		internal void _003CBuildContent_003Eb__63_4()
		{
			Save(apply: true);
		}

		internal bool _003COnMine_003Eb__69_1(CollectionChoice choice)
		{
			return !choice.Favorited;
		}

		internal bool _003COnMine_003Eb__69_2(CollectionChoice choice)
		{
			return choice.Favorited;
		}

		internal Row _003CRender_003Eb__79_0(Row row)
		{
			return row;
		}

		internal ulong _003CRender_003Eb__79_3(Row row)
		{
			return row.Member.ItemId;
		}

		internal float _003CRender_003Eb__79_4((WorkshopDownloadState State, float Progress) job)
		{
			return job.Progress;
		}

		internal ulong _003CRender_003Eb__79_7(Row row)
		{
			return row.Member.ItemId;
		}

		internal bool _003CBlocker_003Eb__80_0((WorkshopDownloadState State, float Progress) job)
		{
			return job.State == WorkshopDownloadState.Complete;
		}

		internal bool _003CRenderSummary_003Eb__81_0(RowState state)
		{
			return state == RowState.Ready;
		}

		internal bool _003CRenderSummary_003Eb__81_1(RowState state)
		{
			if (CanSelect(state))
			{
				return state == RowState.NoMods;
			}
			return true;
		}

		internal string _003CRebuildList_003Eb__82_2(LaunchEntry entry)
		{
			return entry.Key;
		}

		internal string _003CDetail_003Eb__86_0(LaunchEntry entry)
		{
			return entry.DisplayName;
		}

		internal bool _003CDownloadMissing_003Eb__89_0(Row row)
		{
			return row.Selected;
		}

		internal bool _003CSave_003Eb__90_0(Row row)
		{
			if (row.Selected)
			{
				return StateOf(row) == RowState.Ready;
			}
			return false;
		}

		internal ulong _003CSave_003Eb__90_1(Row row)
		{
			return row.Member.ItemId;
		}
	}

	private const double PollSeconds = 0.5;

	private const double ReadTimeoutSeconds = 30.0;

	private const double DiscoverySeconds = 30.0;

	private static readonly List<Row> Rows = new List<Row>();

	private static readonly WorkshopDownloadQueue DownloadQueue = new WorkshopDownloadQueue(OnDownloadsChanged, null);

	private static CanvasLayer? _layer;

	private static Control? _root;

	private static Label? _title;

	private static Label? _subtitle;

	private static Label? _linkCaption;

	private static LineEdit? _link;

	private static Button? _read;

	private static Button? _close;

	private static Label? _mineCaption;

	private static HFlowContainer? _mine;

	private static Control? _mineSpacer;

	private static WorkshopCollectionQuery? _mineQuery;

	private static IReadOnlyList<CollectionChoice>? _mineChoices;

	private static MineState _mineState;

	private static double _mineElapsed;

	private static readonly StringName MineMeta = StringName.op_Implicit("collection_id");

	private static Label? _status;

	private static VBoxContainer? _preview;

	private static Label? _collectionTitle;

	private static Label? _summary;

	private static ScrollContainer? _scroll;

	private static VBoxContainer? _list;

	private static Label? _nameCaption;

	private static LineEdit? _name;

	private static Label? _note;

	private static ProgressBar? _progress;

	private static Control? _spacer;

	private static Button? _download;

	private static Button? _viewDownloads;

	private static Button? _cancel;

	private static Button? _save;

	private static Button? _saveApply;

	private static LaunchPlan? _plan;

	private static WorkshopCollectionHost? _host;

	private static WorkshopCollectionQuery? _query;

	private static WorkshopCollectionReader? _reader;

	private static CollectionSnapshot? _snapshot;

	private static double _readElapsed;

	private static string? _error;

	private static string _listSignature = string.Empty;

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

	internal static IEnumerable<Control?> TourSource => new global::_003C_003Ez__ReadOnlyArray<Control>((Control[])(object)new Control[5]
	{
		(Control)_linkCaption,
		(Control)_link,
		(Control)_read,
		(Control)_mineCaption,
		(Control)_mine
	});

	internal static IEnumerable<Control?> TourItems => new global::_003C_003Ez__ReadOnlyArray<Control>((Control[])(object)new Control[3]
	{
		(Control)_collectionTitle,
		(Control)_summary,
		(Control)_scroll
	});

	internal static IEnumerable<Control?> TourActions => new global::_003C_003Ez__ReadOnlyArray<Control>((Control[])(object)new Control[4]
	{
		(Control)_download,
		(Control)_cancel,
		(Control)_save,
		(Control)_saveApply
	});

	internal static bool Busy
	{
		get
		{
			if (IsOpen)
			{
				WorkshopCollectionReader? reader = _reader;
				if (reader == null || !reader.Running)
				{
					return ActiveDownloads() > 0;
				}
				return true;
			}
			return false;
		}
	}

	private static (WorkshopDownloadState State, float Progress)? JobOf(Row row)
	{
		WorkshopDownloadJob workshopDownloadJob = DownloadQueue.Find(row.Member.ItemId);
		if (workshopDownloadJob != null)
		{
			return (workshopDownloadJob.State, workshopDownloadJob.Progress);
		}
		WorkshopDownloadRecord workshopDownloadRecord = WorkshopDownloadCenter.Find(row.Member.ItemId);
		if (workshopDownloadRecord == null)
		{
			return null;
		}
		return (workshopDownloadRecord.State, workshopDownloadRecord.Progress);
	}

	private static int ActiveDownloads()
	{
		return Rows.Count(delegate(Row row)
		{
			WorkshopDownloadState? workshopDownloadState = JobOf(row)?.State;
			if (workshopDownloadState.HasValue)
			{
				WorkshopDownloadState valueOrDefault = workshopDownloadState.GetValueOrDefault();
				if ((uint)valueOrDefault <= 1u)
				{
					return true;
				}
			}
			return false;
		});
	}

	internal static void Show(LaunchPlan plan, WorkshopCollectionHost host)
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
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		_plan = plan;
		_host = host;
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
				Name = StringName.op_Implicit("WorkshopCollectionImportDialog"),
				Layer = 1100
			};
			_root = UiKit.FullRect<Control>(new Control
			{
				Name = StringName.op_Implicit("WorkshopCollectionImportRoot"),
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
			((Node)val2).Name = StringName.op_Implicit("WorkshopCollectionImportCard");
			((Control)val2).AnchorLeft = 0.17f;
			((Control)val2).AnchorTop = 0.08f;
			((Control)val2).AnchorRight = 0.83f;
			((Control)val2).AnchorBottom = 0.92f;
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
			val3.Timeout += Poll;
			((Node)_root).AddChild((Node)(object)val3, false, (InternalMode)0);
			((Node)_layer).AddChild((Node)(object)_root, false, (InternalMode)0);
			((Node)val.Root).AddChild((Node)(object)_layer, false, (InternalMode)0);
			LauncherController.Active?.AddScope(new LauncherController.Scope(_root, Close, () => (Control?)(object)_link));
			if (WorkshopUpdates.IsAvailable)
			{
				ListMine();
			}
			Refresh();
			LineEdit? link = _link;
			if (link != null)
			{
				((GodotObject)link).CallDeferred(MethodName.GrabFocus, Array.Empty<Variant>());
			}
		}
	}

	internal static void Close()
	{
		_reader?.Cancel();
		_query?.Dispose();
		_mineQuery?.Dispose();
		if (_layer != null && GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			((Node)_layer).QueueFree();
		}
		_layer = null;
		_root = null;
		_title = null;
		_subtitle = null;
		_linkCaption = null;
		_link = null;
		_read = null;
		_close = null;
		_mineCaption = null;
		_mine = null;
		_mineSpacer = null;
		_mineQuery = null;
		_mineChoices = null;
		_mineState = MineState.Hidden;
		_mineElapsed = 0.0;
		_status = null;
		_preview = null;
		_collectionTitle = null;
		_summary = null;
		_scroll = null;
		_list = null;
		_nameCaption = null;
		_name = null;
		_note = null;
		_progress = null;
		_spacer = null;
		_download = null;
		_viewDownloads = null;
		_cancel = null;
		_save = null;
		_saveApply = null;
		_plan = null;
		_host = null;
		_query = null;
		_reader = null;
		_snapshot = null;
		_readElapsed = 0.0;
		_error = null;
		_listSignature = string.Empty;
		Rows.Clear();
		DownloadQueue.Clear();
	}

	internal static void Refresh()
	{
		if (IsOpen && _root != null && GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			_root.Theme = UiKit.CreateTheme();
			_listSignature = string.Empty;
			if (_title != null)
			{
				_title.Text = MlmLocManager.Get("collection.title");
			}
			if (_subtitle != null)
			{
				_subtitle.Text = MlmLocManager.Get("collection.subtitle");
			}
			if (_linkCaption != null)
			{
				_linkCaption.Text = MlmLocManager.Get("collection.link.label");
			}
			if (_link != null)
			{
				_link.PlaceholderText = MlmLocManager.Get("collection.link.placeholder");
			}
			if (_mineCaption != null)
			{
				_mineCaption.Text = MlmLocManager.Get("collection.mine.label");
			}
			if (_nameCaption != null)
			{
				_nameCaption.Text = MlmLocManager.Get("collection.name.label");
			}
			if (_name != null)
			{
				_name.PlaceholderText = MlmLocManager.Get("profiles.name.placeholder");
			}
			if (_note != null)
			{
				_note.Text = MlmLocManager.Get("collection.note");
			}
			if (_close != null)
			{
				((Control)_close).TooltipText = MlmLocManager.Get("common.action.close");
			}
			if (_cancel != null)
			{
				_cancel.Text = MlmLocManager.Get("common.action.cancel");
			}
			RenderMine();
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
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Expected O, but got Unknown
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Expected O, but got Unknown
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Expected O, but got Unknown
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Expected O, but got Unknown
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Expected O, but got Unknown
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Expected O, but got Unknown
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Expected O, but got Unknown
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0458: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Expected O, but got Unknown
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Expected O, but got Unknown
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0512: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Expected O, but got Unknown
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0606: Unknown result type (might be due to invalid IL or missing references)
		//IL_0611: Unknown result type (might be due to invalid IL or missing references)
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0627: Expected O, but got Unknown
		//IL_0627: Unknown result type (might be due to invalid IL or missing references)
		//IL_0635: Unknown result type (might be due to invalid IL or missing references)
		//IL_063a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0642: Unknown result type (might be due to invalid IL or missing references)
		//IL_064f: Expected O, but got Unknown
		//IL_064f: Unknown result type (might be due to invalid IL or missing references)
		//IL_065d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0664: Expected O, but got Unknown
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_055a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Expected O, but got Unknown
		//IL_076a: Unknown result type (might be due to invalid IL or missing references)
		//IL_076f: Unknown result type (might be due to invalid IL or missing references)
		//IL_077f: Expected O, but got Unknown
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
		_title = UiKit.Label(MlmLocManager.Get("collection.title"), "SectionLabel");
		_subtitle = UiKit.Label(MlmLocManager.Get("collection.subtitle"), "DimLabel");
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
		GridContainer val4 = new GridContainer
		{
			Columns = 3
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 8);
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 8);
		_linkCaption = UiKit.Label(MlmLocManager.Get("collection.link.label"), "MutedLabel");
		((Control)_linkCaption).SizeFlagsVertical = (SizeFlags)4;
		_link = new LineEdit
		{
			ClearButtonEnabled = true,
			PlaceholderText = MlmLocManager.Get("collection.link.placeholder"),
			SizeFlagsHorizontal = (SizeFlags)3,
			CustomMinimumSize = new Vector2(0f, 34f)
		};
		LineEdit? link = _link;
		object obj2 = _003C_003Ec._003C_003E9__63_0;
		if (obj2 == null)
		{
			TextSubmittedEventHandler val5 = delegate
			{
				Read();
			};
			_003C_003Ec._003C_003E9__63_0 = val5;
			obj2 = (object)val5;
		}
		link.TextSubmitted += (TextSubmittedEventHandler)obj2;
		_read = UiKit.Button(MlmLocManager.Get("collection.read"), "SecondaryButton", 110f, 34f);
		((BaseButton)_read).Pressed += Read;
		((Node)val4).AddChild((Node)(object)_linkCaption, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_link, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_read, false, (InternalMode)0);
		_mineCaption = UiKit.Label(MlmLocManager.Get("collection.mine.label"), "MutedLabel");
		((Control)_mineCaption).SizeFlagsVertical = (SizeFlags)4;
		_mine = new HFlowContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_mine).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 7);
		((Control)_mine).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 7);
		_mineSpacer = new Control
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val4).AddChild((Node)(object)_mineCaption, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_mine, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_mineSpacer, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val4, false, (InternalMode)0);
		_status = UiKit.Label(string.Empty, "DimLabel");
		_status.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)_status, false, (InternalMode)0);
		_preview = new VBoxContainer
		{
			Visible = false,
			SizeFlagsVertical = (SizeFlags)3
		};
		((Control)_preview).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		VBoxContainer val6 = new VBoxContainer();
		((Control)val6).AddThemeConstantOverride(StringName.op_Implicit("separation"), 2);
		_collectionTitle = UiKit.Label(string.Empty, "CaptionLabel");
		_collectionTitle.TextOverrunBehavior = (OverrunBehavior)3;
		_collectionTitle.ClipText = true;
		_summary = UiKit.Label(string.Empty, "DimLabel");
		_summary.AutowrapMode = (AutowrapMode)3;
		((Node)val6).AddChild((Node)(object)_collectionTitle, false, (InternalMode)0);
		((Node)val6).AddChild((Node)(object)_summary, false, (InternalMode)0);
		((Node)_preview).AddChild((Node)(object)val6, false, (InternalMode)0);
		_list = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_list).AddThemeConstantOverride(StringName.op_Implicit("separation"), 6);
		_scroll = new ScrollContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3,
			CustomMinimumSize = new Vector2(0f, 150f),
			HorizontalScrollMode = (ScrollMode)0
		};
		((Node)_scroll).AddChild((Node)(object)_list, false, (InternalMode)0);
		UiKit.FollowFocus(_scroll);
		((Node)_preview).AddChild((Node)(object)_scroll, false, (InternalMode)0);
		HBoxContainer val7 = new HBoxContainer();
		((Control)val7).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_nameCaption = UiKit.Label(MlmLocManager.Get("collection.name.label"), "MutedLabel");
		((Control)_nameCaption).SizeFlagsVertical = (SizeFlags)4;
		_name = new LineEdit
		{
			PlaceholderText = MlmLocManager.Get("profiles.name.placeholder"),
			SizeFlagsHorizontal = (SizeFlags)3,
			CustomMinimumSize = new Vector2(0f, 34f),
			MaxLength = 64
		};
		LineEdit? name = _name;
		object obj3 = _003C_003Ec._003C_003E9__63_1;
		if (obj3 == null)
		{
			TextChangedEventHandler val8 = delegate
			{
				_error = null;
				Render();
			};
			_003C_003Ec._003C_003E9__63_1 = val8;
			obj3 = (object)val8;
		}
		name.TextChanged += (TextChangedEventHandler)obj3;
		((Node)val7).AddChild((Node)(object)_nameCaption, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)_name, false, (InternalMode)0);
		((Node)_preview).AddChild((Node)(object)val7, false, (InternalMode)0);
		_note = UiKit.Label(MlmLocManager.Get("collection.note"), "DimLabel");
		_note.AutowrapMode = (AutowrapMode)3;
		((Node)_preview).AddChild((Node)(object)_note, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)_preview, false, (InternalMode)0);
		_progress = new ProgressBar
		{
			MinValue = 0.0,
			MaxValue = 100.0,
			ShowPercentage = false,
			CustomMinimumSize = new Vector2(0f, 10f),
			Visible = false
		};
		((Node)val).AddChild((Node)(object)_progress, false, (InternalMode)0);
		_spacer = new Control
		{
			SizeFlagsVertical = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val).AddChild((Node)(object)_spacer, false, (InternalMode)0);
		HFlowContainer val9 = new HFlowContainer();
		((Control)val9).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 8);
		((Control)val9).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 8);
		_download = UiKit.Button(MlmLocManager.Get("collection.download"), "SecondaryButton", 0f, 34f);
		((BaseButton)_download).Pressed += DownloadMissing;
		((Node)val9).AddChild((Node)(object)_download, false, (InternalMode)0);
		_viewDownloads = UiKit.Button(MlmLocManager.Get("collection.viewDownloads"), "MiniButton", 0f, 34f);
		((Control)_viewDownloads).TooltipText = MlmLocManager.Get("collection.viewDownloads.tooltip");
		((CanvasItem)_viewDownloads).Visible = false;
		((BaseButton)_viewDownloads).Pressed += delegate
		{
			_host?.OpenDownloads();
		};
		((Node)val9).AddChild((Node)(object)_viewDownloads, false, (InternalMode)0);
		((Node)val9).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		_cancel = UiKit.Button(MlmLocManager.Get("common.action.cancel"), "SecondaryButton", 96f, 34f);
		((BaseButton)_cancel).Pressed += Close;
		_save = UiKit.Button(MlmLocManager.Get("collection.save"), "SecondaryButton", 0f, 34f);
		((BaseButton)_save).Pressed += delegate
		{
			Save(apply: false);
		};
		_saveApply = UiKit.Button(MlmLocManager.Get("collection.saveApply"), "PrimaryButton", 0f, 34f);
		((BaseButton)_saveApply).Pressed += delegate
		{
			Save(apply: true);
		};
		((Node)val9).AddChild((Node)(object)_cancel, false, (InternalMode)0);
		((Node)val9).AddChild((Node)(object)_save, false, (InternalMode)0);
		((Node)val9).AddChild((Node)(object)_saveApply, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val9, false, (InternalMode)0);
		return (Control)(object)obj;
	}

	private static void Read()
	{
		if (IsOpen && _plan != null && !Busy)
		{
			LineEdit? link = _link;
			if (!WorkshopCollectionLink.TryParse((link != null) ? link.Text : null, out var itemId))
			{
				_error = MlmLocManager.Get("collection.error.link");
				Render();
				return;
			}
			if (!WorkshopUpdates.IsAvailable)
			{
				_error = MlmLocManager.Get("collection.unavailable");
				Render();
				return;
			}
			_reader?.Cancel();
			_query?.Dispose();
			Rows.Clear();
			DownloadQueue.Clear();
			_snapshot = null;
			_error = null;
			_readElapsed = 0.0;
			_query = new WorkshopCollectionQuery(2868840u, WorkshopUpdates.SteamApiLanguage(MlmLocManager.EffectiveLocale));
			_reader = new WorkshopCollectionReader(_query.Fetch);
			GD.Print($"{"[ModLaunchManager]"} reading Workshop collection {itemId}.");
			_reader.Start(itemId, OnRead);
			Render();
		}
	}

	private static void OnRead(CollectionReadResult result)
	{
		if (IsOpen)
		{
			CollectionSnapshot snapshot = result.Snapshot;
			if ((object)snapshot == null)
			{
				GD.Print($"{"[ModLaunchManager]"} could not import a Workshop collection: {result.Error}.");
				_error = result.Error switch
				{
					CollectionReadError.SingleItem => MlmLocManager.Get("collection.error.singleItem"), 
					CollectionReadError.NotACollection => MlmLocManager.Get("collection.error.link"), 
					CollectionReadError.OtherGame => MlmLocManager.Get("collection.error.otherGame"), 
					CollectionReadError.Empty => MlmLocManager.Get("collection.error.empty"), 
					CollectionReadError.TooLarge => MlmLocManager.Get("collection.error.tooLarge", ("depth", 5), ("count", 500)), 
					_ => MlmLocManager.Get("collection.error.unreadable"), 
				};
				Render();
			}
			else
			{
				GD.Print($"{"[ModLaunchManager]"} read Workshop collection {snapshot.CollectionId} with {snapshot.Members.Count} item(s).");
				Preview(snapshot);
			}
		}
	}

	internal static void Preview(CollectionSnapshot snapshot)
	{
		if (!IsOpen || _host == null)
		{
			return;
		}
		_snapshot = snapshot;
		_error = null;
		Rows.Clear();
		DownloadQueue.Clear();
		foreach (CollectionMember member in snapshot.Members)
		{
			Rows.Add(new Row(member)
			{
				Selected = (member.Kind == WorkshopItemKind.Item)
			});
		}
		if (_name != null)
		{
			_name.Text = WorkshopCollectionPlanner.SuggestName(_host.Profiles(), snapshot.CollectionId, snapshot.Title, MlmLocManager.Get("collection.name.fallback", ("id", snapshot.CollectionId)));
		}
		Render();
	}

	internal static void PreviewMine(IReadOnlyList<CollectionChoice> choices)
	{
		if (IsOpen)
		{
			_mineQuery?.Dispose();
			_mineQuery = null;
			_mineChoices = choices;
			_mineState = MineState.Ready;
			RenderMine();
		}
	}

	private static void ListMine()
	{
		_mineQuery?.Dispose();
		_mineQuery = new WorkshopCollectionQuery(2868840u, WorkshopUpdates.SteamApiLanguage(MlmLocManager.EffectiveLocale));
		_mineState = MineState.Loading;
		_mineElapsed = 0.0;
		_mineChoices = null;
		_mineQuery.ListMine(OnMine);
	}

	private static void OnMine(IReadOnlyList<CollectionChoice>? choices)
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		GD.Print("[ModLaunchManager] the player's Workshop collections: " + ((choices == null) ? "Steam did not answer." : $"{choices.Count((CollectionChoice choice) => !choice.Favorited)} own, {choices.Count((CollectionChoice choice) => choice.Favorited)} favorited."));
		Callable val = Callable.From((Action)delegate
		{
			if (!IsOpen)
			{
				return;
			}
			_mineChoices = choices;
			_mineState = ((choices == null) ? MineState.Failed : MineState.Ready);
			try
			{
				RenderMine();
				Render();
			}
			catch (Exception value)
			{
				GD.PushWarning($"{"[ModLaunchManager]"} could not show the player's collections: {value}");
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static void RenderMine()
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		if (_mine == null || _mineCaption == null || _mineSpacer == null)
		{
			return;
		}
		bool visible = _mineState != MineState.Hidden;
		((CanvasItem)_mineCaption).Visible = visible;
		((CanvasItem)_mine).Visible = visible;
		((CanvasItem)_mineSpacer).Visible = visible;
		foreach (Node child in ((Node)_mine).GetChildren(false))
		{
			((Node)_mine).RemoveChild(child);
			child.QueueFree();
		}
		if (_mineState == MineState.Ready)
		{
			IReadOnlyList<CollectionChoice> mineChoices = _mineChoices;
			if (mineChoices != null && mineChoices.Count > 0)
			{
				ButtonGroup group = new ButtonGroup
				{
					AllowUnpress = true
				};
				foreach (CollectionChoice item2 in mineChoices)
				{
					string text = ((item2.Title.Trim().Length > 0) ? item2.Title.Trim() : MlmLocManager.Get("collection.name.fallback", ("id", item2.Id)));
					string item = ((text.Length > 40) ? (text.Substring(0, 39).TrimEnd() + "…") : text);
					Button val = UiKit.Chip(MlmLocManager.Get("collection.collectionTitle", ("title", item), ("count", item2.Items)), group, pressed: false);
					((Control)val).CustomMinimumSize = new Vector2(0f, 34f);
					((Control)val).TooltipText = MlmLocManager.Get(item2.Favorited ? "collection.mine.favorited.tooltip" : "collection.mine.own.tooltip", ("title", text));
					((GodotObject)val).SetMeta(MineMeta, Variant.op_Implicit(item2.Id.ToString(CultureInfo.InvariantCulture)));
					ulong id = item2.Id;
					((BaseButton)val).Pressed += delegate
					{
						ReadChoice(id);
					};
					((Node)_mine).AddChild((Node)(object)val, false, (InternalMode)0);
				}
				SyncMine();
				return;
			}
		}
		Label val2 = UiKit.Label(MlmLocManager.Get(_mineState switch
		{
			MineState.Loading => "collection.mine.loading", 
			MineState.Failed => "collection.mine.failed", 
			_ => "collection.mine.none", 
		}), "DimLabel");
		((Control)val2).CustomMinimumSize = new Vector2(0f, 34f);
		val2.VerticalAlignment = (VerticalAlignment)1;
		((Node)_mine).AddChild((Node)(object)val2, false, (InternalMode)0);
	}

	private static void SyncMine()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (_mine == null)
		{
			return;
		}
		string text = _snapshot?.CollectionId.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
		WorkshopCollectionReader? reader = _reader;
		bool disabled = (reader != null && reader.Running) || ActiveDownloads() > 0;
		foreach (Button item in ((IEnumerable)((Node)_mine).GetChildren(false)).OfType<Button>())
		{
			int num;
			if (text.Length > 0)
			{
				StringName mineMeta = MineMeta;
				Variant val = default(Variant);
				val = ((GodotObject)item).GetMeta(mineMeta, val);
				num = ((((Variant)(ref val)).AsString() == text) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			bool flag = (byte)num != 0;
			if (((BaseButton)item).ButtonPressed != flag)
			{
				((BaseButton)item).SetPressedNoSignal(flag);
			}
			((BaseButton)item).Disabled = disabled;
		}
	}

	private static void ReadChoice(ulong id)
	{
		if (_link != null && id != 0L)
		{
			_link.Text = $"https://steamcommunity.com/sharedfiles/filedetails/?id={id}";
			Read();
		}
	}

	private static void Poll()
	{
		if (!IsOpen)
		{
			return;
		}
		if (_mineState == MineState.Loading)
		{
			_mineElapsed += 0.5;
			if (_mineElapsed >= 30.0)
			{
				_mineQuery?.Dispose();
				OnMine(null);
			}
		}
		WorkshopCollectionReader? reader = _reader;
		if (reader != null && reader.Running)
		{
			_readElapsed += 0.5;
			if (_readElapsed >= 30.0)
			{
				_reader.Cancel();
				_query?.Dispose();
				_error = MlmLocManager.Get("collection.error.unreadable");
				GD.PushWarning("[ModLaunchManager] Workshop collection query timed out.");
				Render();
			}
			return;
		}
		bool flag = false;
		foreach (Row row in Rows)
		{
			(WorkshopDownloadState, float)? tuple = JobOf(row);
			if (tuple.HasValue && tuple.GetValueOrDefault().Item1 == WorkshopDownloadState.Complete && !IsInstalled(row))
			{
				row.WaitedForDiscovery += 0.5;
				flag = true;
			}
		}
		if (ActiveDownloads() != 0 || flag)
		{
			DownloadQueue.Poll(0.5);
			Render();
		}
	}

	private static void OnDownloadsChanged()
	{
		Render();
	}

	private static bool IsInstalled(Row row)
	{
		if (_plan != null)
		{
			return WorkshopCollectionPlanner.InstalledMods(_plan, row.Member.ItemId).Count > 0;
		}
		return false;
	}

	internal static void ItemRemoved(ulong itemId)
	{
		foreach (Row item in Rows.Where((Row row) => row.Member.ItemId == itemId))
		{
			item.Selected = false;
			item.WaitedForDiscovery = 0.0;
		}
		_listSignature = string.Empty;
		Render();
	}

	private static RowState StateOf(Row row)
	{
		if (WorkshopDownloadCenter.IsRemoved(row.Member.ItemId))
		{
			return RowState.NeedsDownload;
		}
		switch (row.Member.Kind)
		{
		case WorkshopItemKind.OtherGame:
			return RowState.OtherGame;
		case WorkshopItemKind.Collection:
		case WorkshopItemKind.NotAMod:
			return RowState.NotAMod;
		case WorkshopItemKind.Unavailable:
			return RowState.Unavailable;
		default:
			if (IsInstalled(row))
			{
				return RowState.Ready;
			}
			return JobOf(row)?.State switch
			{
				null => RowState.NeedsDownload, 
				WorkshopDownloadState.Queued => RowState.Queued, 
				WorkshopDownloadState.Downloading => RowState.Downloading, 
				WorkshopDownloadState.Failed => RowState.Failed, 
				_ => (row.WaitedForDiscovery >= 30.0) ? RowState.NoMods : RowState.Recognizing, 
			};
		}
	}

	private static bool CanSelect(RowState state)
	{
		bool flag = (uint)(state - 7) <= 2u;
		return !flag;
	}

	private static void Render()
	{
		if (!IsOpen || _status == null || _preview == null || _read == null || _link == null || _download == null || _save == null || _saveApply == null || _progress == null)
		{
			return;
		}
		bool flag = _reader?.Running ?? false;
		int num = ActiveDownloads();
		if (_viewDownloads != null)
		{
			((CanvasItem)_viewDownloads).Visible = WorkshopDownloadCenter.Downloads.Count > 0;
			_viewDownloads.Text = MlmLocManager.Get("collection.viewDownloads");
			((Control)_viewDownloads).TooltipText = MlmLocManager.Get("collection.viewDownloads.tooltip");
		}
		_read.Text = MlmLocManager.Get(flag ? "collection.reading" : "collection.read");
		((BaseButton)_read).Disabled = flag || num > 0;
		_link.Editable = !flag && num == 0;
		SyncMine();
		((CanvasItem)_preview).Visible = _snapshot != null;
		if (_spacer != null)
		{
			((CanvasItem)_spacer).Visible = _snapshot == null;
		}
		if (_snapshot == null || _plan == null)
		{
			((CanvasItem)_download).Visible = false;
			((CanvasItem)_save).Visible = false;
			((CanvasItem)_saveApply).Visible = false;
			((CanvasItem)_progress).Visible = false;
			SetStatus(_error ?? MlmLocManager.Get(flag ? "collection.reading" : (WorkshopUpdates.IsAvailable ? "collection.hint" : "collection.unavailable")), (_error != null || !WorkshopUpdates.IsAvailable) ? new bool?(false) : ((bool?)null));
			return;
		}
		Dictionary<Row, RowState> states = Rows.ToDictionary((Row row) => row, StateOf);
		List<Row> list = Rows.Where((Row row) => row.Selected && CanSelect(states[row])).ToList();
		IReadOnlyDictionary<ulong, ulong> clashes = WorkshopCollectionPlanner.Clashes(_plan, from row in list
			where states[row] == RowState.Ready
			select row.Member.ItemId);
		RenderSummary(states);
		RebuildList(states, clashes);
		List<(WorkshopDownloadState, float)> list2 = Rows.Select(JobOf).OfType<(WorkshopDownloadState, float)>().ToList();
		((CanvasItem)_progress).Visible = num > 0;
		((Range)_progress).Value = ((list2.Count == 0) ? 0f : (list2.Average<(WorkshopDownloadState, float)>(((WorkshopDownloadState State, float Progress) job) => job.Progress) * 100f));
		List<Row> list3 = list.Where(delegate(Row row)
		{
			RowState rowState = states[row];
			return (rowState == RowState.NeedsDownload || rowState == RowState.Failed) ? true : false;
		}).ToList();
		((CanvasItem)_download).Visible = list3.Count > 0;
		((BaseButton)_download).Disabled = !WorkshopUpdates.IsAvailable;
		_download.Text = (list3.All((Row row) => states[row] == RowState.Failed) ? MlmLocManager.Get("collection.retry") : MlmLocManager.Get("collection.download"));
		LineEdit? name = _name;
		LaunchProfile profile = WorkshopCollectionPlanner.Build(((name != null) ? name.Text.Trim() : null) ?? string.Empty, _snapshot.CollectionId, list.Select((Row row) => row.Member.ItemId), _plan);
		(string, bool)? tuple = Blocker(states, list, clashes, profile, num, list2);
		bool flag2 = _host?.Profiles().Any((LaunchProfile existing) => string.Equals(existing.Name, profile.Name, StringComparison.OrdinalIgnoreCase)) ?? false;
		((CanvasItem)_save).Visible = true;
		((CanvasItem)_saveApply).Visible = true;
		((BaseButton)_save).Disabled = tuple.HasValue;
		((BaseButton)_saveApply).Disabled = tuple.HasValue;
		_save.Text = MlmLocManager.Get(flag2 ? "collection.replace" : "collection.save");
		_saveApply.Text = MlmLocManager.Get(flag2 ? "collection.replaceApply" : "collection.saveApply");
		((Control)_save).TooltipText = (flag2 ? MlmLocManager.Get("collection.replace.tooltip", ("name", profile.Name)) : MlmLocManager.Get("collection.save.tooltip"));
		((Control)_saveApply).TooltipText = MlmLocManager.Get("collection.saveApply.tooltip");
		if (_error != null)
		{
			SetStatus(_error, false);
		}
		else if (tuple.HasValue)
		{
			(string, bool) valueOrDefault = tuple.GetValueOrDefault();
			SetStatus(valueOrDefault.Item1, valueOrDefault.Item2 ? "WarningLabel" : "DimLabel");
		}
		else
		{
			SetStatus(MlmLocManager.Get("collection.ready", ("count", profile.Mods.Count)), true);
		}
	}

	private static (string Text, bool Warning)? Blocker(IReadOnlyDictionary<Row, RowState> states, IReadOnlyList<Row> selected, IReadOnlyDictionary<ulong, ulong> clashes, LaunchProfile profile, int active, IReadOnlyList<(WorkshopDownloadState State, float Progress)> jobs)
	{
		if (active > 0)
		{
			return (MlmLocManager.Get("collection.downloading", ("done", jobs.Count(((WorkshopDownloadState State, float Progress) job) => job.State == WorkshopDownloadState.Complete)), ("total", jobs.Count)), false);
		}
		if (selected.Any((Row row) => states[row] == RowState.Recognizing))
		{
			return (MlmLocManager.Get("collection.recognizing"), false);
		}
		int num = selected.Count((Row row) => states[row] == RowState.Failed);
		if (num > 0)
		{
			return (MlmLocManager.Get("collection.downloadFailed", ("count", num)), true);
		}
		Row row2 = selected.FirstOrDefault((Row row) => states[row] == RowState.NoMods);
		if (row2 != null)
		{
			return (MlmLocManager.Get("collection.noMods", ("name", Title(row2.Member))), true);
		}
		int num2 = selected.Count((Row row) => states[row] == RowState.NeedsDownload);
		if (num2 > 0)
		{
			return (MlmLocManager.Get("collection.needsDownload", ("count", num2)), false);
		}
		if (clashes.Count > 0)
		{
			var (item, other) = (KeyValuePair<ulong, ulong>)(ref clashes.First());
			return (MlmLocManager.Get("collection.clash", ("first", Title(Rows.First((Row row) => row.Member.ItemId == other).Member)), ("second", Title(Rows.First((Row row) => row.Member.ItemId == item).Member))), true);
		}
		if (selected.Count == 0 || profile.Mods.Count == 0)
		{
			return (MlmLocManager.Get("collection.nothingSelected"), false);
		}
		if (profile.Name.Length != 0)
		{
			return null;
		}
		return (MlmLocManager.Get("profiles.error.nameRequired"), false);
	}

	private static void RenderSummary(IReadOnlyDictionary<Row, RowState> states)
	{
		if (!(_snapshot == null) && _collectionTitle != null && _summary != null)
		{
			int num = states.Values.Count((RowState state) => state == RowState.Ready);
			int num2 = states.Values.Count((RowState state) => !CanSelect(state) || state == RowState.NoMods);
			int num3 = states.Count - num - num2;
			string text = ((_snapshot.Title.Trim().Length > 0) ? _snapshot.Title.Trim() : MlmLocManager.Get("collection.name.fallback", ("id", _snapshot.CollectionId)));
			_collectionTitle.Text = MlmLocManager.Get("collection.collectionTitle", ("title", text), ("count", states.Count));
			((Control)_collectionTitle).TooltipText = text;
			_summary.Text = ((num2 > 0) ? MlmLocManager.Get("collection.summaryWithUnusable", ("installed", num), ("missing", num3), ("unusable", num2)) : MlmLocManager.Get("collection.summary", ("installed", num), ("missing", num3)));
		}
	}

	private static void RebuildList(IReadOnlyDictionary<Row, RowState> states, IReadOnlyDictionary<ulong, ulong> clashes)
	{
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		if (_list == null || _plan == null || !GodotObject.IsInstanceValid((GodotObject)(object)_list))
		{
			return;
		}
		string text = string.Join('\u001f', Rows.Select(delegate(Row row)
		{
			int value = (int)Math.Round((JobOf(row)?.Progress ?? 0f) * 100f);
			string value2 = string.Join(',', from entry in WorkshopCollectionPlanner.InstalledMods(_plan, row.Member.ItemId)
				select entry.Key);
			return $"{row.Member.ItemId}\u001e{row.Selected}\u001e{states[row]}\u001e{value}\u001e{clashes.GetValueOrDefault(row.Member.ItemId)}\u001e{value2}";
		}));
		if (string.Equals(_listSignature, text, StringComparison.Ordinal))
		{
			return;
		}
		_listSignature = text;
		int num = -1;
		Viewport viewport = ((Node)_list).GetViewport();
		Control val = ((viewport != null) ? viewport.GuiGetFocusOwner() : null);
		int num2 = 0;
		while (val != null && num2 < ((Node)_list).GetChildCount(false))
		{
			Node child = ((Node)_list).GetChild(num2, false);
			Control val2 = (Control)(object)((child is Control) ? child : null);
			if (val2 != null && ((Node)val2).IsAncestorOf((Node)(object)val))
			{
				num = num2;
			}
			num2++;
		}
		ScrollContainer? scroll = _scroll;
		int scrollPosition = ((scroll != null) ? scroll.ScrollVertical : 0);
		foreach (Node child2 in ((Node)_list).GetChildren(false))
		{
			((Node)_list).RemoveChild(child2);
			child2.QueueFree();
		}
		Button refocus = null;
		for (int i = 0; i < Rows.Count; i++)
		{
			Row row2 = Rows[i];
			RowState state = states[row2];
			PanelContainer card;
			Button val3 = RowControl(row2, state, clashes, out card);
			((Node)_list).AddChild((Node)(object)card, false, (InternalMode)0);
			if (i == num)
			{
				refocus = val3;
			}
		}
		ScrollContainer scroll2 = _scroll;
		Callable val4 = Callable.From((Action)delegate
		{
			if (scroll2 != null && GodotObject.IsInstanceValid((GodotObject)(object)scroll2))
			{
				scroll2.ScrollVertical = scrollPosition;
			}
			if (refocus != null && GodotObject.IsInstanceValid((GodotObject)(object)refocus) && ((CanvasItem)refocus).IsVisibleInTree())
			{
				((Control)refocus).GrabFocus();
			}
		});
		((Callable)(ref val4)).CallDeferred(Array.Empty<Variant>());
	}

	private static Button RowControl(Row row, RowState state, IReadOnlyDictionary<ulong, ulong> clashes, out PanelContainer card)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Expected O, but got Unknown
		card = UiKit.Card("InsetPanel");
		((Control)card).SizeFlagsVertical = (SizeFlags)0;
		HBoxContainer val = new HBoxContainer();
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		VBoxContainer val2 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)4
		};
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 1);
		Label val3 = UiKit.Label(Title(row.Member));
		val3.TextOverrunBehavior = (OverrunBehavior)3;
		val3.ClipText = true;
		((Control)val3).TooltipText = Title(row.Member);
		((Control)val3).MouseFilter = (MouseFilterEnum)1;
		Label val4 = UiKit.Label(Detail(row, state), "DimLabel");
		val4.TextOverrunBehavior = (OverrunBehavior)3;
		val4.ClipText = true;
		((Control)val4).TooltipText = val4.Text;
		((Control)val4).MouseFilter = (MouseFilterEnum)1;
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		int num;
		string text;
		if (row.Selected)
		{
			num = (clashes.ContainsKey(row.Member.ItemId) ? 1 : 0);
			if (num != 0)
			{
				text = MlmLocManager.Get("collection.row.clash");
				goto IL_014e;
			}
		}
		else
		{
			num = 0;
		}
		text = StateText(row, state);
		goto IL_014e;
		IL_014e:
		string text2 = text;
		bool flag = (byte)num != 0;
		if (!flag)
		{
			bool flag2 = (uint)(state - 5) <= 4u;
			flag = flag2;
		}
		Label val5 = UiKit.Label(text2, flag ? "WarningLabel" : ((state == RowState.Ready) ? "SuccessLabel" : "DimLabel"));
		((Control)val5).CustomMinimumSize = new Vector2(132f, 0f);
		((Control)val5).SizeFlagsVertical = (SizeFlags)4;
		val5.HorizontalAlignment = (HorizontalAlignment)2;
		val5.TextOverrunBehavior = (OverrunBehavior)3;
		val5.ClipText = true;
		((Control)val5).MouseFilter = (MouseFilterEnum)1;
		((Control)val5).TooltipText = StateTooltip(state);
		((Node)val).AddChild((Node)(object)val5, false, (InternalMode)0);
		bool flag3 = CanSelect(state);
		Button val6 = UiKit.Button(string.Empty, "SwitchButton", 62f, 30f);
		((BaseButton)val6).ToggleMode = true;
		((BaseButton)val6).ButtonPressed = row.Selected && flag3;
		((BaseButton)val6).Disabled = !flag3;
		((Control)val6).SizeFlagsVertical = (SizeFlags)4;
		((Control)val6).MouseDefaultCursorShape = (CursorShape)2;
		val6.Text = MlmLocManager.Get(((BaseButton)val6).ButtonPressed ? "options.boolean.on" : "options.boolean.off");
		((Control)val6).TooltipText = MlmLocManager.Get(flag3 ? "collection.row.toggle.tooltip" : "collection.row.unusable.tooltip");
		Row captured = row;
		((BaseButton)val6).Toggled += (ToggledEventHandler)delegate(bool pressed)
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			captured.Selected = pressed;
			_error = null;
			Callable val7 = Callable.From((Action)Render);
			((Callable)(ref val7)).CallDeferred(Array.Empty<Variant>());
		};
		((Node)val).AddChild((Node)(object)val6, false, (InternalMode)0);
		Button val8 = UiKit.Button(MlmLocManager.Get("workshop.remove.uninstall"), "SecondaryButton", 0f, 30f);
		((Control)val8).SizeFlagsVertical = (SizeFlags)4;
		int disabled;
		if (_host?.RemoveItem != null)
		{
			Func<ulong, bool>? canRemoveItem = _host.CanRemoveItem;
			if ((canRemoveItem == null || canRemoveItem(row.Member.ItemId)) && (state == RowState.Ready || JobOf(row).HasValue))
			{
				disabled = (WorkshopDownloadCenter.IsRemoved(row.Member.ItemId) ? 1 : 0);
				goto IL_0353;
			}
		}
		disabled = 1;
		goto IL_0353;
		IL_0353:
		((BaseButton)val8).Disabled = (byte)disabled != 0;
		((Control)val8).TooltipText = MlmLocManager.Get(((BaseButton)val8).Disabled ? "workshop.remove.locked" : "workshop.remove.tooltip");
		((BaseButton)val8).Pressed += delegate
		{
			_host?.RemoveItem?.Invoke(row.Member.ItemId, Title(row.Member), MlmLocManager.Get("workshop.remove.uninstall"));
		};
		((Node)val).AddChild((Node)(object)val8, false, (InternalMode)0);
		((Node)card).AddChild((Node)(object)UiKit.Margin((Control)(object)val, 12, 8), false, (InternalMode)0);
		return val6;
	}

	private static string CollectionTitle()
	{
		if (!(_snapshot == null))
		{
			if (_snapshot.Title.Trim().Length <= 0)
			{
				return MlmLocManager.Get("collection.name.fallback", ("id", _snapshot.CollectionId));
			}
			return _snapshot.Title.Trim();
		}
		return string.Empty;
	}

	private static string Title(CollectionMember member)
	{
		if (member.Title.Trim().Length <= 0)
		{
			return MlmLocManager.Get("collection.row.untitled", ("id", member.ItemId));
		}
		return member.Title.Trim();
	}

	private static string Detail(Row row, RowState state)
	{
		List<string> list = new List<string>();
		if (state == RowState.Ready && _plan != null)
		{
			IReadOnlyList<LaunchEntry> source = WorkshopCollectionPlanner.InstalledMods(_plan, row.Member.ItemId);
			list.Add(MlmLocManager.Get("collection.row.contains", ("mods", string.Join(MlmLocManager.Get("common.listSeparator"), source.Select((LaunchEntry entry) => entry.DisplayName)))));
		}
		string item = (string.IsNullOrWhiteSpace(row.Member.Via) ? MlmLocManager.Get("collection.row.untitled", ("id", row.Member.ViaId)) : row.Member.Via.Trim());
		if (row.Member.Role == CollectionMemberRole.Required)
		{
			list.Add(MlmLocManager.Get("collection.row.requiredBy", ("name", item)));
		}
		else if (row.Member.ViaId != 0)
		{
			list.Add(MlmLocManager.Get("collection.row.subCollection", ("name", item)));
		}
		if (list.Count == 0)
		{
			list.Add((row.Member.Title.Trim().Length == 0 && StateTooltip(state).Length > 0) ? StateTooltip(state) : MlmLocManager.Get("collection.row.workshop", ("id", row.Member.ItemId)));
		}
		return string.Join(" · ", list);
	}

	private static string StateText(Row row, RowState state)
	{
		return state switch
		{
			RowState.Ready => MlmLocManager.Get("collection.row.installed"), 
			RowState.NeedsDownload => MlmLocManager.Get("collection.row.needsDownload"), 
			RowState.Queued => MlmLocManager.Get("collection.row.queued"), 
			RowState.Downloading => MlmLocManager.Get("collection.row.downloading", ("progress", (int)Math.Round((JobOf(row)?.Progress ?? 0f) * 100f))), 
			RowState.Recognizing => MlmLocManager.Get("collection.row.recognizing"), 
			RowState.NoMods => MlmLocManager.Get("collection.row.noMods"), 
			RowState.Failed => MlmLocManager.Get("collection.row.failed"), 
			RowState.OtherGame => MlmLocManager.Get("collection.row.otherGame"), 
			RowState.NotAMod => MlmLocManager.Get("collection.row.notAMod"), 
			_ => MlmLocManager.Get("collection.row.unavailable"), 
		};
	}

	private static string StateTooltip(RowState state)
	{
		return state switch
		{
			RowState.NoMods => MlmLocManager.Get("collection.row.noMods.tooltip"), 
			RowState.Failed => MlmLocManager.Get("collection.row.failed.tooltip"), 
			RowState.Unavailable => MlmLocManager.Get("collection.row.unavailable.tooltip"), 
			_ => string.Empty, 
		};
	}

	private static void DownloadMissing()
	{
		if (!IsOpen || _plan == null || _snapshot == null || !WorkshopUpdates.IsAvailable)
		{
			return;
		}
		_error = null;
		foreach (Row item in Rows.Where((Row row) => row.Selected))
		{
			switch (StateOf(item))
			{
			case RowState.NeedsDownload:
				if (WorkshopDownloadCenter.IsRemoved(item.Member.ItemId))
				{
					DownloadQueue.Forget(item.Member.ItemId);
					WorkshopDownloadCenter.Allow(item.Member.ItemId);
				}
				WorkshopCollectionArrivals.Protect(item.Member.ItemId);
				DownloadQueue.Enqueue(item.Member.ItemId, Title(item.Member), subscribe: true);
				WorkshopDownloadCenter.Track(item.Member.ItemId, Title(item.Member), WorkshopDownloadSource.Collection, CollectionTitle());
				break;
			case RowState.Failed:
				item.WaitedForDiscovery = 0.0;
				if (!DownloadQueue.Retry(item.Member.ItemId, subscribe: true))
				{
					WorkshopCollectionArrivals.Protect(item.Member.ItemId);
					WorkshopDownloadCenter.Retry(item.Member.ItemId);
				}
				WorkshopDownloadCenter.Track(item.Member.ItemId, Title(item.Member), WorkshopDownloadSource.Collection, CollectionTitle());
				break;
			}
		}
		GD.Print($"{"[ModLaunchManager]"} downloading {ActiveDownloads()} item(s) of Workshop collection {_snapshot.CollectionId}.");
		Render();
	}

	private static void Save(bool apply)
	{
		if (IsOpen && _plan != null && !(_host == null) && !(_snapshot == null) && _save != null && !((BaseButton)_save).Disabled)
		{
			LineEdit? name = _name;
			LaunchProfile arg = WorkshopCollectionPlanner.Build(((name != null) ? name.Text.Trim() : null) ?? string.Empty, _snapshot.CollectionId, from row in Rows
				where row.Selected && StateOf(row) == RowState.Ready
				select row.Member.ItemId, _plan);
			PlanResult planResult = _host.Save(arg, apply);
			if (planResult.Success)
			{
				Close();
				return;
			}
			_error = planResult.Message;
			Render();
		}
	}

	private static void SetStatus(string message, bool? success)
	{
		string variation = ((!success.HasValue) ? "DimLabel" : ((success != true) ? "DangerLabel" : "SuccessLabel"));
		SetStatus(message, variation);
	}

	private static void SetStatus(string message, string variation)
	{
		if (_status != null && GodotObject.IsInstanceValid((GodotObject)(object)_status))
		{
			_status.Text = message;
			((Control)_status).ThemeTypeVariation = StringName.op_Implicit(variation);
		}
	}
}
