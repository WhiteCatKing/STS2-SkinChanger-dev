using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;

namespace ModLaunchManager;

internal static class FriendModSyncDialog
{
	private sealed class DrawnRow
	{
		private readonly PanelContainer _badge;

		private readonly Label _marker;

		private readonly Label _title;

		private readonly ProgressBar _download;

		private readonly Label _trailing;

		private BadgeTone _tone;

		private string _markerText;

		private string _titleText;

		private float? _downloadShown;

		private string _trailingText;

		internal string Id { get; }

		internal DrawnRow(string id, PanelContainer badge, Label marker, Label title, ProgressBar download, float? progress, Label trailing, BadgeTone tone)
		{
			Id = id;
			_badge = badge;
			_marker = marker;
			_title = title;
			_download = download;
			_downloadShown = progress;
			_trailing = trailing;
			_tone = tone;
			_markerText = marker.Text;
			_titleText = title.Text;
			_trailingText = trailing.Text;
		}

		internal bool Shows(string id)
		{
			if (string.Equals(Id, id, StringComparison.Ordinal))
			{
				return GodotObject.IsInstanceValid((GodotObject)(object)_badge);
			}
			return false;
		}

		internal void Show(string marker, BadgeTone tone, string title, string trailing, float? progress)
		{
			if (_downloadShown != progress)
			{
				((CanvasItem)_download).Visible = progress.HasValue;
				((Range)_download).Value = progress.GetValueOrDefault() * 100f;
				_downloadShown = progress;
			}
			if (_tone != tone)
			{
				UiKit.RestyleBadge(_badge, tone);
				_tone = tone;
			}
			if (_markerText != marker)
			{
				_marker.Text = marker;
				_markerText = marker;
			}
			if (_titleText != title)
			{
				_title.Text = title;
				_titleText = title;
			}
			if (_trailingText != trailing)
			{
				_trailing.Text = trailing;
				_trailingText = trailing;
			}
		}
	}

	private const int MarkerWidth = 112;

	private const double CheckPollSeconds = 0.2;

	private const ulong CheckRedrawMilliseconds = 500uL;

	private const int DownloadBarWidth = 120;

	private static readonly IReadOnlyDictionary<ulong, WorkshopDownloadJob> NoDownloads = new Dictionary<ulong, WorkshopDownloadJob>();

	private static readonly List<DrawnRow> DrawnRows = new List<DrawnRow>();

	private static readonly List<DrawnRow> DrawnExtras = new List<DrawnRow>();

	private static readonly List<(ulong SteamId, Button Row)> FriendRows = new List<(ulong, Button)>();

	private static ulong? _drawnFriend;

	private static string _drawnExtras = string.Empty;

	private static string? _drawnFriends;

	private static string? _drawnChips;

	private static CanvasLayer? _layer;

	private static Control? _root;

	private static VBoxContainer? _friendList;

	private static VBoxContainer? _remoteList;

	private static ScrollContainer? _remoteScroll;

	private static Label? _remoteEmpty;

	private static HFlowContainer? _summaryChips;

	private static Label? _selectedTitle;

	private static Label? _summary;

	private static Label? _downloadStatus;

	private static Label? _dialogTitle;

	private static Label? _dialogSubtitle;

	private static Label? _publishStatus;

	private static Label? _friendsCaption;

	private static Button? _import;

	private static Button? _importGameplay;

	private static Button? _apply;

	private static Button? _refresh;

	private static Button? _retry;

	private static Button? _cancel;

	private static Button? _close;

	private static LaunchPlan? _plan;

	private static Func<LaunchProfile, PlanResult>? _applyProfile;

	private static ulong _selectedSteamId;

	private static string? _operationError;

	private static int _drawnChecks;

	private static ulong _drawnAt;

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

	internal static Control? TourFriends => (Control?)(object)_friendList;

	internal static IEnumerable<Control?> TourComparison => new global::_003C_003Ez__ReadOnlyArray<Control>((Control[])(object)new Control[3]
	{
		(Control)_selectedTitle,
		(Control)_summaryChips,
		(Control)_remoteScroll
	});

	internal static IEnumerable<Control?> TourImport => new global::_003C_003Ez__ReadOnlyArray<Control>((Control[])(object)new Control[3]
	{
		(Control)_import,
		(Control)_importGameplay,
		(Control)_apply
	});

	internal static void Show(LaunchPlan plan, Func<LaunchProfile, PlanResult> applyProfile)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Expected O, but got Unknown
		_plan = plan;
		_applyProfile = applyProfile;
		_operationError = null;
		if (IsOpen)
		{
			Refresh();
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			FriendFileChecks.Recheck();
			_layer = new CanvasLayer
			{
				Name = StringName.op_Implicit("FriendModSyncDialog"),
				Layer = 1100
			};
			_root = UiKit.FullRect<Control>(new Control
			{
				Name = StringName.op_Implicit("FriendModSyncRoot"),
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
			((Node)val2).Name = StringName.op_Implicit("FriendModSyncCard");
			((Control)val2).AnchorLeft = 0.08f;
			((Control)val2).AnchorTop = 0.08f;
			((Control)val2).AnchorRight = 0.92f;
			((Control)val2).AnchorBottom = 0.92f;
			((Control)val2).OffsetLeft = 0f;
			((Control)val2).OffsetTop = 0f;
			((Control)val2).OffsetRight = 0f;
			((Control)val2).OffsetBottom = 0f;
			((Control)val2).SizeFlagsHorizontal = (SizeFlags)3;
			((Control)val2).SizeFlagsVertical = (SizeFlags)3;
			((Node)val2).AddChild((Node)(object)BuildContent(), false, (InternalMode)0);
			((Node)_root).AddChild((Node)(object)val2, false, (InternalMode)0);
			Timer val3 = new Timer
			{
				Name = StringName.op_Implicit("FileChecks"),
				WaitTime = 0.2,
				Autostart = true
			};
			val3.Timeout += DrawCheckedFiles;
			((Node)_root).AddChild((Node)(object)val3, false, (InternalMode)0);
			((Node)_layer).AddChild((Node)(object)_root, false, (InternalMode)0);
			((Node)val.Root).AddChild((Node)(object)_layer, false, (InternalMode)0);
			LauncherController.Active?.AddScope(new LauncherController.Scope(_root, Close, () => (Control?)(object)_close));
			Refresh();
		}
	}

	internal static void Refresh()
	{
		if (IsOpen && _root != null && GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			_root.Theme = UiKit.CreateTheme();
			if (_dialogTitle != null)
			{
				_dialogTitle.Text = MlmLocManager.Get("friendSync.title");
			}
			if (_dialogSubtitle != null)
			{
				_dialogSubtitle.Text = MlmLocManager.Get("friendSync.subtitle");
			}
			UpdatePublishStatus();
			if (_friendsCaption != null)
			{
				_friendsCaption.Text = MlmLocManager.Get("friendSync.friends.caption");
			}
			if (_close != null)
			{
				((Control)_close).TooltipText = MlmLocManager.Get("friendSync.action.close.tooltip");
			}
			if (_refresh != null)
			{
				_refresh.Text = MlmLocManager.Get("friendSync.action.refresh");
				((Control)_refresh).TooltipText = MlmLocManager.Get("friendSync.action.refresh.tooltip");
			}
			if (_import != null)
			{
				_import.Text = MlmLocManager.Get("friendSync.action.import");
				((Control)_import).TooltipText = MlmLocManager.Get("friendSync.action.import.tooltip");
			}
			if (_importGameplay != null)
			{
				_importGameplay.Text = MlmLocManager.Get("friendSync.action.importGameplay");
				((Control)_importGameplay).TooltipText = MlmLocManager.Get("friendSync.action.importGameplay.tooltip");
			}
			if (_apply != null)
			{
				_apply.Text = MlmLocManager.Get("friendSync.action.apply");
				((Control)_apply).TooltipText = MlmLocManager.Get("friendSync.action.apply.tooltip");
			}
			if (_retry != null)
			{
				_retry.Text = MlmLocManager.Get("friendSync.action.retry");
				((Control)_retry).TooltipText = MlmLocManager.Get("friendSync.action.retry.tooltip");
			}
			if (_cancel != null)
			{
				_cancel.Text = MlmLocManager.Get("friendSync.action.cancel");
				((Control)_cancel).TooltipText = MlmLocManager.Get("friendSync.action.cancel.tooltip");
			}
			Render();
		}
	}

	internal static void Update()
	{
		if (IsOpen && _root != null && GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			UpdatePublishStatus();
			Draw(rebuild: false);
		}
	}

	internal static void Close()
	{
		FriendFileChecks.Cancel();
		if (_layer != null && GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			((Node)_layer).QueueFree();
		}
		_layer = null;
		_root = null;
		_friendList = null;
		_remoteList = null;
		_remoteScroll = null;
		_remoteEmpty = null;
		_summaryChips = null;
		_selectedTitle = null;
		_summary = null;
		_downloadStatus = null;
		_dialogTitle = null;
		_dialogSubtitle = null;
		_publishStatus = null;
		_friendsCaption = null;
		_import = null;
		_importGameplay = null;
		_apply = null;
		_refresh = null;
		_retry = null;
		_cancel = null;
		_close = null;
		_plan = null;
		_applyProfile = null;
		_selectedSteamId = 0uL;
		_operationError = null;
		ForgetRows();
		FriendRows.Clear();
		_drawnFriends = null;
		_drawnChips = null;
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
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Expected O, but got Unknown
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Expected O, but got Unknown
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Expected O, but got Unknown
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Expected O, but got Unknown
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Expected O, but got Unknown
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Expected O, but got Unknown
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Expected O, but got Unknown
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Expected O, but got Unknown
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Expected O, but got Unknown
		//IL_04dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Expected O, but got Unknown
		//IL_076b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0770: Unknown result type (might be due to invalid IL or missing references)
		//IL_0778: Unknown result type (might be due to invalid IL or missing references)
		//IL_0782: Expected O, but got Unknown
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
		_dialogTitle = UiKit.Label(MlmLocManager.Get("friendSync.title"), "SectionLabel");
		_dialogSubtitle = UiKit.Label(MlmLocManager.Get("friendSync.subtitle"), "DimLabel");
		_dialogSubtitle.AutowrapMode = (AutowrapMode)3;
		_publishStatus = UiKit.Label(string.Empty, "WarningLabel");
		_publishStatus.AutowrapMode = (AutowrapMode)3;
		((CanvasItem)_publishStatus).Visible = false;
		((Node)val3).AddChild((Node)(object)_dialogTitle, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)_dialogSubtitle, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)_publishStatus, false, (InternalMode)0);
		_close = UiKit.Button("✕", "AccentIconButton", 34f, 32f);
		((Control)_close).SizeFlagsVertical = (SizeFlags)0;
		((Control)_close).MouseDefaultCursorShape = (CursorShape)2;
		((Control)_close).TooltipText = MlmLocManager.Get("friendSync.action.close.tooltip");
		((BaseButton)_close).Pressed += Close;
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_close, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)UiKit.Divider(), false, (InternalMode)0);
		HBoxContainer val4 = new HBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 14);
		VBoxContainer val5 = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(250f, 0f),
			SizeFlagsVertical = (SizeFlags)3
		};
		((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_friendsCaption = UiKit.Label(MlmLocManager.Get("friendSync.friends.caption"), "CaptionLabel");
		((Node)val5).AddChild((Node)(object)_friendsCaption, false, (InternalMode)0);
		ScrollContainer val6 = new ScrollContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3,
			HorizontalScrollMode = (ScrollMode)0
		};
		_friendList = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_friendList).AddThemeConstantOverride(StringName.op_Implicit("separation"), 2);
		((Node)val6).AddChild((Node)(object)_friendList, false, (InternalMode)0);
		((Node)val5).AddChild((Node)(object)val6, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val4).AddChild((Node)new VSeparator
		{
			CustomMinimumSize = new Vector2(1f, 0f)
		}, false, (InternalMode)0);
		VBoxContainer val7 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3
		};
		((Control)val7).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_selectedTitle = UiKit.Label(MlmLocManager.Get("friendSync.selection.none"), "SectionLabel");
		_selectedTitle.AutowrapMode = (AutowrapMode)3;
		_summary = UiKit.Label(string.Empty, "DimLabel");
		_summary.AutowrapMode = (AutowrapMode)3;
		_summaryChips = new HFlowContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_summaryChips).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 7);
		((Control)_summaryChips).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 6);
		((Control)_summaryChips).Resized += delegate
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			Callable val8 = Callable.From((Action)FloorChipRowHeight);
			((Callable)(ref val8)).CallDeferred(Array.Empty<Variant>());
		};
		((Node)val7).AddChild((Node)(object)_selectedTitle, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)_summary, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)_summaryChips, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)UiKit.Divider(), false, (InternalMode)0);
		_remoteScroll = new ScrollContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3,
			HorizontalScrollMode = (ScrollMode)0
		};
		_remoteList = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_remoteList).AddThemeConstantOverride(StringName.op_Implicit("separation"), 4);
		((Node)_remoteScroll).AddChild((Node)(object)_remoteList, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)_remoteScroll, false, (InternalMode)0);
		_remoteEmpty = UiKit.Label(string.Empty, "DimLabel");
		((Control)_remoteEmpty).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)_remoteEmpty).SizeFlagsVertical = (SizeFlags)3;
		_remoteEmpty.HorizontalAlignment = (HorizontalAlignment)1;
		_remoteEmpty.VerticalAlignment = (VerticalAlignment)1;
		_remoteEmpty.AutowrapMode = (AutowrapMode)3;
		((CanvasItem)_remoteEmpty).Visible = false;
		((Node)val7).AddChild((Node)(object)_remoteEmpty, false, (InternalMode)0);
		_downloadStatus = UiKit.Label(string.Empty, "DimLabel");
		_downloadStatus.AutowrapMode = (AutowrapMode)3;
		((Node)val7).AddChild((Node)(object)_downloadStatus, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)val7, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val4, false, (InternalMode)0);
		HBoxContainer val9 = new HBoxContainer();
		((Control)val9).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_refresh = UiKit.Button(MlmLocManager.Get("friendSync.action.refresh"), "SecondaryButton", 0f, 34f);
		((Control)_refresh).TooltipText = MlmLocManager.Get("friendSync.action.refresh.tooltip");
		((BaseButton)_refresh).Pressed += delegate
		{
			_operationError = null;
			FriendFileChecks.Recheck();
			FriendModSync.RefreshNow();
			Update();
		};
		_cancel = UiKit.Button(MlmLocManager.Get("friendSync.action.cancel"), "SecondaryButton", 0f, 34f);
		((Control)_cancel).TooltipText = MlmLocManager.Get("friendSync.action.cancel.tooltip");
		((BaseButton)_cancel).Pressed += CancelPending;
		_retry = UiKit.Button(MlmLocManager.Get("friendSync.action.retry"), "SecondaryButton", 0f, 34f);
		((Control)_retry).TooltipText = MlmLocManager.Get("friendSync.action.retry.tooltip");
		((BaseButton)_retry).Pressed += RetryFailed;
		_import = UiKit.Button(MlmLocManager.Get("friendSync.action.import"), "PrimaryButton", 0f, 34f);
		((Control)_import).TooltipText = MlmLocManager.Get("friendSync.action.import.tooltip");
		((BaseButton)_import).Pressed += ImportSelected;
		_importGameplay = UiKit.Button(MlmLocManager.Get("friendSync.action.importGameplay"), "SecondaryButton", 0f, 34f);
		((Control)_importGameplay).TooltipText = MlmLocManager.Get("friendSync.action.importGameplay.tooltip");
		((BaseButton)_importGameplay).Pressed += delegate
		{
			ImportSelectedCore(gameplayOnly: true);
		};
		_apply = UiKit.Button(MlmLocManager.Get("friendSync.action.apply"), "PrimaryButton", 0f, 34f);
		((Control)_apply).TooltipText = MlmLocManager.Get("friendSync.action.apply.tooltip");
		((BaseButton)_apply).Pressed += ApplyPending;
		Control val10 = new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val9).AddChild((Node)(object)_refresh, false, (InternalMode)0);
		((Node)val9).AddChild((Node)(object)val10, false, (InternalMode)0);
		((Node)val9).AddChild((Node)(object)_cancel, false, (InternalMode)0);
		((Node)val9).AddChild((Node)(object)_retry, false, (InternalMode)0);
		((Node)val9).AddChild((Node)(object)_importGameplay, false, (InternalMode)0);
		((Node)val9).AddChild((Node)(object)_import, false, (InternalMode)0);
		((Node)val9).AddChild((Node)(object)_apply, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val9, false, (InternalMode)0);
		return (Control)(object)obj;
	}

	private static void Render()
	{
		Draw(rebuild: true);
	}

	private static void Draw(bool rebuild)
	{
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		if (_friendList == null || _remoteList == null || _plan == null)
		{
			return;
		}
		VBoxContainer friendList = _friendList;
		VBoxContainer remoteList = _remoteList;
		HFlowContainer summaryChips = _summaryChips;
		Label? selectedTitle = _selectedTitle;
		Label summary = _summary;
		Label downloadStatus = _downloadStatus;
		Button import = _import;
		Button apply = _apply;
		Button retry = _retry;
		Button cancel = _cancel;
		if (rebuild)
		{
			_drawnFriends = null;
			_drawnChips = null;
			ForgetRows();
		}
		_drawnChecks = FriendFileChecks.Changes;
		_drawnAt = Time.GetTicksMsec();
		PendingFriendImport pending = FriendModSync.Pending;
		List<OnlineFriendMods> list = FriendModSync.Friends.ToList();
		if (pending != null)
		{
			OnlineFriendMods onlineFriendMods = new OnlineFriendMods(pending.SteamId, pending.FriendName, FriendPresenceState.PendingImport, pending.List, string.Empty, null);
			int num = list.FindIndex((OnlineFriendMods friend) => friend.SteamId == pending.SteamId);
			if (num >= 0)
			{
				list[num] = onlineFriendMods;
			}
			else
			{
				list.Insert(0, onlineFriendMods);
			}
		}
		OnlineFriendMods onlineFriendMods2 = list.FirstOrDefault((OnlineFriendMods friend) => friend.SteamId == _selectedSteamId);
		if (onlineFriendMods2 == null && list.Count > 0)
		{
			onlineFriendMods2 = list.FirstOrDefault((OnlineFriendMods friend) => friend.State == FriendPresenceState.PendingImport) ?? list.FirstOrDefault((OnlineFriendMods friend) => friend.State == FriendPresenceState.Ready) ?? list[0];
			_selectedSteamId = onlineFriendMods2.SteamId;
		}
		DrawFriends(friendList, list);
		selectedTitle.Text = ((onlineFriendMods2 == null) ? MlmLocManager.Get("friendSync.selection.none") : MlmLocManager.Get("friendSync.selection.friend", ("name", onlineFriendMods2.PersonaName)));
		string text = _operationError ?? string.Empty;
		string style = ((_operationError == null) ? "DimLabel" : "DangerLabel");
		string tooltip = string.Empty;
		((CanvasItem)import).Visible = (object)onlineFriendMods2 != null && onlineFriendMods2.State == FriendPresenceState.Ready;
		((BaseButton)import).Disabled = FriendModSync.Pending != null;
		if (_importGameplay != null)
		{
			((CanvasItem)_importGameplay).Visible = ((CanvasItem)import).Visible;
			((BaseButton)_importGameplay).Disabled = ((BaseButton)import).Disabled || (object)onlineFriendMods2 == null || onlineFriendMods2.List?.Mods.Any((FriendModEntry m) => m.AffectsGameplay) != true;
		}
		Callable val;
		if (onlineFriendMods2?.List == null)
		{
			summary.Text = string.Empty;
			((CanvasItem)summary).Visible = false;
			((CanvasItem)apply).Visible = false;
			((CanvasItem)retry).Visible = false;
			((CanvasItem)cancel).Visible = false;
			ForgetRows();
			Clear((Node)(object)remoteList);
			DrawChips(summaryChips, Array.Empty<(string, BadgeTone)>());
			if (onlineFriendMods2 == null)
			{
				ShowEmptyState(MlmLocManager.Get("friendSync.selection.hint"), warning: false);
			}
			else
			{
				string text2 = FriendModSync.DescribeError(onlineFriendMods2.Error);
				if (text2 != null && text2.Length > 0)
				{
					ShowEmptyState(text2, warning: true);
				}
				else
				{
					ShowEmptyState(MlmLocManager.Get("friendSync.list.unavailable"), warning: false);
				}
			}
			ShowStatus(downloadStatus, text, style, tooltip);
			val = Callable.From((Action)FloorChipRowHeight);
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			return;
		}
		ShowList();
		FriendModEvaluation evaluation = new FriendModEvaluation();
		FriendModComparison friendModComparison = FriendModSync.Compare(onlineFriendMods2.List, _plan, evaluation);
		summary.Text = MlmLocManager.Get("friendSync.remoteVersions", ("game", string.IsNullOrWhiteSpace(onlineFriendMods2.List.GameVersion) ? MlmLocManager.Get("common.value.unknown") : onlineFriendMods2.List.GameVersion), ("manager", string.IsNullOrWhiteSpace(onlineFriendMods2.List.ManagerVersion) ? MlmLocManager.Get("common.value.unknown") : onlineFriendMods2.List.ManagerVersion));
		((CanvasItem)summary).Visible = true;
		DrawChips(summaryChips, SummaryChips(onlineFriendMods2.List, friendModComparison));
		val = Callable.From((Action)FloorChipRowHeight);
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		PendingFriendImport pendingFriendImport = ((pending?.SteamId == onlineFriendMods2.SteamId) ? pending : null);
		IReadOnlyList<WorkshopDownloadJob> readOnlyList2;
		if (pendingFriendImport == null)
		{
			IReadOnlyList<WorkshopDownloadJob> readOnlyList = Array.Empty<WorkshopDownloadJob>();
			readOnlyList2 = readOnlyList;
		}
		else
		{
			readOnlyList2 = FriendModSync.Downloads;
		}
		IReadOnlyList<WorkshopDownloadJob> readOnlyList3 = readOnlyList2;
		IReadOnlyDictionary<ulong, WorkshopDownloadJob> readOnlyDictionary2;
		if (readOnlyList3.Count != 0)
		{
			IReadOnlyDictionary<ulong, WorkshopDownloadJob> readOnlyDictionary = readOnlyList3.ToDictionary((WorkshopDownloadJob job) => job.ItemId);
			readOnlyDictionary2 = readOnlyDictionary;
		}
		else
		{
			readOnlyDictionary2 = NoDownloads;
		}
		IReadOnlyDictionary<ulong, WorkshopDownloadJob> downloads = readOnlyDictionary2;
		string text3 = string.Join('\n', friendModComparison.ExtraEnabled.Select((LaunchEntry entry) => entry.Key));
		if (onlineFriendMods2.SteamId != _drawnFriend || text3 != _drawnExtras || !UpdateRows(friendModComparison, downloads))
		{
			RebuildRows(remoteList, onlineFriendMods2.SteamId, friendModComparison, downloads, text3);
		}
		bool visible = false;
		bool visible2 = false;
		bool flag = false;
		if (pendingFriendImport != null)
		{
			visible = true;
			FriendModEvaluation evaluation2 = new FriendModEvaluation(current: true);
			int num2 = readOnlyList3.Count(delegate(WorkshopDownloadJob job)
			{
				WorkshopDownloadState state = job.State;
				return (uint)state <= 1u;
			});
			int num3 = readOnlyList3.Count((WorkshopDownloadJob job) => job.State == WorkshopDownloadState.Failed);
			if (num2 > 0)
			{
				int num4 = (int)Math.Round(readOnlyList3.Where(delegate(WorkshopDownloadJob job)
				{
					WorkshopDownloadState state2 = job.State;
					return (uint)state2 <= 1u;
				}).Average((WorkshopDownloadJob job) => job.Progress) * 100f);
				text = MlmLocManager.Get("friendSync.downloads.active", ("count", num2), ("progress", num4));
				style = "WarningLabel";
			}
			else if (num3 > 0)
			{
				text = MlmLocManager.Get("friendSync.downloads.failed", ("count", num3));
				style = "DangerLabel";
				tooltip = string.Join('\n', from job in readOnlyList3
					where job.State == WorkshopDownloadState.Failed
					select job.DisplayName + ": " + FriendModSync.DescribeDownloadError(job));
				visible2 = true;
				flag = FriendModSync.PendingCanApply(_plan, evaluation2);
			}
			else if (FriendModSync.DownloadsWaitForChecks)
			{
				text = MlmLocManager.Get("friendSync.downloads.checking");
				style = "DimLabel";
			}
			else if (!FriendModSync.PendingHasInstalledMod(_plan))
			{
				text = MlmLocManager.Get("friendSync.downloads.missingLocal", ("count", pendingFriendImport.List.Mods.Count));
				style = "WarningLabel";
			}
			else if (!FriendModSync.PendingCanApply(_plan, evaluation2))
			{
				text = MlmLocManager.Get("friendSync.apply.checking");
				style = "DimLabel";
			}
			else
			{
				int num5 = FriendModSync.PendingUnresolvedCount(_plan, evaluation2);
				if (num5 > 0)
				{
					text = MlmLocManager.Get("friendSync.downloads.partial", ("count", num5));
					style = "WarningLabel";
					flag = true;
				}
				else
				{
					text = MlmLocManager.Get(pendingFriendImport.GameplayOnly ? "friendSync.downloads.gameplayReady" : "friendSync.downloads.ready");
					style = "SuccessLabel";
					flag = true;
				}
			}
		}
		if (!string.IsNullOrWhiteSpace(_operationError))
		{
			text = _operationError;
			style = "DangerLabel";
			tooltip = string.Empty;
		}
		((CanvasItem)cancel).Visible = visible;
		((CanvasItem)retry).Visible = visible2;
		((CanvasItem)apply).Visible = flag;
		if (flag)
		{
			((BaseButton)apply).Disabled = false;
		}
		ShowStatus(downloadStatus, text, style, tooltip);
	}

	private static void ShowStatus(Label label, string text, string style, string tooltip)
	{
		label.Text = text;
		((Control)label).ThemeTypeVariation = StringName.op_Implicit(style);
		((Control)label).TooltipText = tooltip;
		((CanvasItem)label).Visible = text.Length > 0;
	}

	private static void DrawCheckedFiles()
	{
		if (FriendFileChecks.Changes != _drawnChecks && (!FriendFileChecks.Busy || Time.GetTicksMsec() - _drawnAt >= 500))
		{
			Draw(rebuild: false);
		}
	}

	private static void DrawFriends(VBoxContainer friendList, IReadOnlyList<OnlineFriendMods> friends)
	{
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Expected O, but got Unknown
		string text = string.Join('\n', friends.Select((OnlineFriendMods friend) => $"{friend.SteamId}\t{friend.PersonaName}\t{friend.State}\t{FriendModSync.DescribeError(friend.Error)}"));
		if (text == _drawnFriends && FriendRows.All<(ulong, Button)>(((ulong SteamId, Button Row) row) => GodotObject.IsInstanceValid((GodotObject)(object)row.Row)))
		{
			foreach (var friendRow in FriendRows)
			{
				var (num, _) = friendRow;
				((BaseButton)friendRow.Row).SetPressedNoSignal(num == _selectedSteamId);
			}
			return;
		}
		Viewport viewport = ((Node)friendList).GetViewport();
		Control focused = ((viewport != null) ? viewport.GuiGetFocusOwner() : null);
		int num2 = ((focused == null) ? (-1) : FriendRows.FindIndex(((ulong SteamId, Button Row) row) => row.Row == focused));
		ulong focusedFriend = ((num2 >= 0) ? FriendRows[num2].SteamId : 0);
		Clear((Node)(object)friendList);
		FriendRows.Clear();
		_drawnFriends = text;
		if (friends.Count == 0)
		{
			Label val = UiKit.Label(MlmLocManager.Get("friendSync.friends.empty"), "DimLabel");
			val.AutowrapMode = (AutowrapMode)3;
			((Node)friendList).AddChild((Node)(object)val, false, (InternalMode)0);
			return;
		}
		ButtonGroup group = new ButtonGroup();
		foreach (OnlineFriendMods friend in friends)
		{
			Button val2 = FriendRow(friend, group);
			((Node)friendList).AddChild((Node)(object)val2, false, (InternalMode)0);
			FriendRows.Add((friend.SteamId, val2));
		}
		if (num2 >= 0)
		{
			int num3 = FriendRows.FindIndex(((ulong SteamId, Button Row) row) => row.SteamId == focusedFriend);
			((Control)FriendRows[(num3 >= 0) ? num3 : Math.Min(num2, FriendRows.Count - 1)].Row).GrabFocus();
		}
	}

	private static void RebuildRows(VBoxContainer remoteList, ulong friend, FriendModComparison comparison, IReadOnlyDictionary<ulong, WorkshopDownloadJob> downloads, string extras)
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		Clear((Node)(object)remoteList);
		ForgetRows();
		_drawnFriend = friend;
		_drawnExtras = extras;
		foreach (FriendModComparisonRow row in comparison.Rows)
		{
			var (marker, tone, detail, progress) = Describe(row, downloads);
			((Node)remoteList).AddChild((Node)(object)ComparisonRow(marker, tone, row.Remote.DisplayName, row.Remote.Version, detail, progress, row.Remote.Id, out DrawnRow drawn), false, (InternalMode)0);
			DrawnRows.Add(drawn);
		}
		if (comparison.ExtraEnabled.Count <= 0)
		{
			return;
		}
		((Node)remoteList).AddChild((Node)(object)UiKit.Divider(), false, (InternalMode)0);
		((Node)remoteList).AddChild((Node)(object)ColoredLabel(MlmLocManager.Get("friendSync.extra", ("count", comparison.ExtraEnabled.Count)), UiKit.TextDim), false, (InternalMode)0);
		foreach (LaunchEntry item in comparison.ExtraEnabled)
		{
			((Node)remoteList).AddChild((Node)(object)ComparisonRow(MlmLocManager.Get("friendSync.marker.extra"), BadgeTone.Quiet, item.DisplayName, item.Mod.manifest?.version ?? string.Empty, string.Empty, null, item.Id, out DrawnRow drawn2), false, (InternalMode)0);
			DrawnExtras.Add(drawn2);
		}
	}

	private static bool UpdateRows(FriendModComparison comparison, IReadOnlyDictionary<ulong, WorkshopDownloadJob> downloads)
	{
		if (DrawnRows.Count != comparison.Rows.Count || DrawnExtras.Count != comparison.ExtraEnabled.Count)
		{
			return false;
		}
		for (int i = 0; i < DrawnRows.Count; i++)
		{
			if (!DrawnRows[i].Shows(comparison.Rows[i].Remote.Id))
			{
				return false;
			}
		}
		for (int j = 0; j < DrawnExtras.Count; j++)
		{
			if (!DrawnExtras[j].Shows(comparison.ExtraEnabled[j].Id))
			{
				return false;
			}
		}
		for (int k = 0; k < DrawnRows.Count; k++)
		{
			FriendModComparisonRow friendModComparisonRow = comparison.Rows[k];
			var (marker, tone, detail, progress) = Describe(friendModComparisonRow, downloads);
			DrawnRows[k].Show(marker, tone, friendModComparisonRow.Remote.DisplayName, Trailing(friendModComparisonRow.Remote.Version, detail), progress);
		}
		string marker2 = MlmLocManager.Get("friendSync.marker.extra");
		for (int l = 0; l < DrawnExtras.Count; l++)
		{
			LaunchEntry launchEntry = comparison.ExtraEnabled[l];
			DrawnExtras[l].Show(marker2, BadgeTone.Quiet, launchEntry.DisplayName, Trailing(launchEntry.Mod.manifest?.version ?? string.Empty, string.Empty), null);
		}
		return true;
	}

	private static void ForgetRows()
	{
		DrawnRows.Clear();
		DrawnExtras.Clear();
		_drawnFriend = null;
		_drawnExtras = string.Empty;
	}

	private static void ShowEmptyState(string text, bool warning)
	{
		if (_remoteEmpty != null && _remoteScroll != null)
		{
			_remoteEmpty.Text = text;
			((Control)_remoteEmpty).ThemeTypeVariation = StringName.op_Implicit(warning ? "WarningLabel" : "DimLabel");
			((CanvasItem)_remoteEmpty).Visible = true;
			((CanvasItem)_remoteScroll).Visible = false;
		}
	}

	private static void ShowList()
	{
		if (_remoteEmpty != null && _remoteScroll != null)
		{
			((CanvasItem)_remoteEmpty).Visible = false;
			((CanvasItem)_remoteScroll).Visible = true;
		}
	}

	private static Button FriendRow(OnlineFriendMods friend, ButtonGroup group)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		OnlineFriendMods captured = friend;
		string text = FriendStateText(friend.State);
		Button obj = UiKit.Button(string.Empty, "RowButton", 0f, 56f);
		((BaseButton)obj).ToggleMode = true;
		((BaseButton)obj).ButtonGroup = group;
		((BaseButton)obj).ButtonPressed = friend.SteamId == _selectedSteamId;
		((Control)obj).MouseDefaultCursorShape = (CursorShape)2;
		((Control)obj).TooltipText = FriendModSync.DescribeError(friend.Error) ?? text;
		((BaseButton)obj).Toggled += (ToggledEventHandler)delegate(bool pressed)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			if (pressed)
			{
				_operationError = null;
				_selectedSteamId = captured.SteamId;
				Callable val = Callable.From((Action)Update);
				((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			}
		};
		Label val2 = UiKit.Label(friend.PersonaName);
		((Control)val2).AddThemeColorOverride(StringName.op_Implicit("font_color"), UiKit.Text);
		val2.TextOverrunBehavior = (OverrunBehavior)3;
		val2.ClipText = true;
		Label val3 = UiKit.Label(text, "CaptionLabel");
		((Control)val3).AddThemeColorOverride(StringName.op_Implicit("font_color"), UiKit.BadgeInk(FriendStateTone(friend.State)));
		val3.TextOverrunBehavior = (OverrunBehavior)3;
		val3.ClipText = true;
		VBoxContainer val4 = new VBoxContainer();
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 1);
		((Node)val4).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)val3, false, (InternalMode)0);
		MarginContainer val5 = UiKit.Margin((Control)val4, 12, 7);
		IgnoreMouse((Node)(object)val5);
		((Node)obj).AddChild((Node)(object)UiKit.FullRect<MarginContainer>(val5), false, (InternalMode)0);
		return obj;
	}

	private static Control ComparisonRow(string marker, BadgeTone tone, string name, string version, string detail, float? progress, string tooltip, out DrawnRow drawn)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		HBoxContainer val = new HBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			TooltipText = tooltip
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		Label label;
		PanelContainer val2 = UiKit.Badge(marker, tone, out label);
		((Control)val2).CustomMinimumSize = new Vector2(112f, 0f);
		IgnoreMouse((Node)(object)val2);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		Label val3 = UiKit.Label(name);
		((Control)val3).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val3).SizeFlagsStretchRatio = 3f;
		((Control)val3).SizeFlagsVertical = (SizeFlags)4;
		val3.TextOverrunBehavior = (OverrunBehavior)3;
		val3.ClipText = true;
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		ProgressBar val4 = new ProgressBar
		{
			MinValue = 0.0,
			MaxValue = 100.0,
			Value = progress.GetValueOrDefault() * 100f,
			ShowPercentage = false,
			CustomMinimumSize = new Vector2(120f, 8f),
			SizeFlagsVertical = (SizeFlags)4,
			MouseFilter = (MouseFilterEnum)2,
			Visible = progress.HasValue
		};
		((Node)val).AddChild((Node)(object)val4, false, (InternalMode)0);
		Label val5 = UiKit.Label(Trailing(version, detail), "DimLabel");
		((Control)val5).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val5).SizeFlagsStretchRatio = 2f;
		((Control)val5).SizeFlagsVertical = (SizeFlags)4;
		val5.HorizontalAlignment = (HorizontalAlignment)2;
		val5.TextOverrunBehavior = (OverrunBehavior)3;
		val5.ClipText = true;
		((Node)val).AddChild((Node)(object)val5, false, (InternalMode)0);
		drawn = new DrawnRow(tooltip, val2, label, val3, val4, progress, val5, tone);
		return (Control)val;
	}

	private static string Trailing(string version, string detail)
	{
		return string.Join(" · ", new string[2] { version, detail }.Where((string value) => !string.IsNullOrWhiteSpace(value)));
	}

	private static List<(string Text, BadgeTone Tone)> SummaryChips(FriendModList list, FriendModComparison comparison)
	{
		int num = 1;
		List<(string, BadgeTone)> list2 = new List<(string, BadgeTone)>(num);
		CollectionsMarshal.SetCount(list2, num);
		CollectionsMarshal.AsSpan(list2)[0] = (MlmLocManager.Get("friendSync.chip.mods", ("count", list.Mods.Count)), BadgeTone.Neutral);
		List<(string, BadgeTone)> list3 = list2;
		if (comparison.ExactMatch)
		{
			list3.Add((MlmLocManager.Get("friendSync.chip.exact"), BadgeTone.Positive));
			return list3;
		}
		AddCountChip(list3, "friendSync.chip.checking", comparison.CheckingCount, BadgeTone.Neutral);
		AddCountChip(list3, "friendSync.chip.missing", comparison.MissingCount, BadgeTone.Caution);
		AddCountChip(list3, "friendSync.chip.disabled", comparison.DisabledCount, BadgeTone.Caution);
		AddCountChip(list3, "friendSync.chip.version", comparison.VersionMismatchCount, BadgeTone.Caution);
		AddCountChip(list3, "friendSync.chip.content", comparison.ContentMismatchCount, BadgeTone.Critical);
		AddCountChip(list3, "friendSync.chip.order", comparison.OrderMismatchCount, BadgeTone.Caution);
		AddCountChip(list3, "friendSync.chip.extra", comparison.ExtraEnabled.Count, BadgeTone.Quiet);
		return list3;
	}

	private static void DrawChips(HFlowContainer chips, IReadOnlyList<(string Text, BadgeTone Tone)> wanted)
	{
		string text = string.Join('\n', wanted.Select<(string, BadgeTone), string>(((string Text, BadgeTone Tone) chip) => $"{chip.Tone}\t{chip.Text}"));
		if (text == _drawnChips)
		{
			return;
		}
		_drawnChips = text;
		Clear((Node)(object)chips);
		foreach (var (text2, tone) in wanted)
		{
			((Node)chips).AddChild((Node)(object)UiKit.Badge(text2, tone), false, (InternalMode)0);
		}
	}

	private static void FloorChipRowHeight()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (_summaryChips == null || !GodotObject.IsInstanceValid((GodotObject)(object)_summaryChips))
		{
			return;
		}
		float num = 0f;
		foreach (Node child in ((Node)_summaryChips).GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null && ((CanvasItem)val).Visible)
			{
				num = Math.Max(num, val.Position.Y + val.Size.Y);
			}
		}
		((Control)_summaryChips).CustomMinimumSize = new Vector2(0f, num);
	}

	private static void AddCountChip(List<(string Text, BadgeTone Tone)> chips, string key, int count, BadgeTone tone)
	{
		if (count > 0)
		{
			chips.Add((MlmLocManager.Get(key, ("count", count)), tone));
		}
	}

	private static (string Marker, BadgeTone Tone, string Detail, float? Progress) Describe(FriendModComparisonRow row, IReadOnlyDictionary<ulong, WorkshopDownloadJob> downloads)
	{
		if (row.Remote.WorkshopId != 0 && downloads.TryGetValue(row.Remote.WorkshopId, out WorkshopDownloadJob value))
		{
			if (value.Failure == WorkshopDownloadFailure.Cancelled)
			{
				return (Marker: MlmLocManager.Get("workshop.remove.cancelled"), Tone: BadgeTone.Quiet, Detail: string.Empty, Progress: null);
			}
			switch (value.State)
			{
			case WorkshopDownloadState.Queued:
				return (Marker: MlmLocManager.Get("friendSync.marker.queued"), Tone: BadgeTone.Quiet, Detail: string.Empty, Progress: value.Progress);
			case WorkshopDownloadState.Downloading:
				return (Marker: MlmLocManager.Get("friendSync.marker.downloading"), Tone: BadgeTone.Neutral, Detail: MlmLocManager.Get("friendSync.row.downloading", ("progress", (int)Math.Round(value.Progress * 100f))), Progress: value.Progress);
			case WorkshopDownloadState.Failed:
				return (Marker: MlmLocManager.Get("friendSync.marker.downloadFailed"), Tone: BadgeTone.Critical, Detail: MlmLocManager.Get("friendSync.row.downloadFailed"), Progress: null);
			case WorkshopDownloadState.Complete:
			{
				FriendModMatchState state = row.State;
				if ((state == FriendModMatchState.Missing || state == FriendModMatchState.Checking) ? true : false)
				{
					return (Marker: MlmLocManager.Get("friendSync.marker.checking"), Tone: BadgeTone.Neutral, Detail: MlmLocManager.Get("friendSync.row.downloaded"), Progress: null);
				}
				break;
			}
			}
		}
		var (item, item2, item3) = Describe(row);
		return (Marker: item, Tone: item2, Detail: item3, Progress: null);
	}

	private static (string Marker, BadgeTone Tone, string Detail) Describe(FriendModComparisonRow row)
	{
		return row.State switch
		{
			FriendModMatchState.Match => (Marker: MlmLocManager.Get("friendSync.marker.match"), Tone: BadgeTone.Positive, Detail: (row.LocalPosition > 0) ? MlmLocManager.Get("friendSync.row.position", ("position", row.LocalPosition)) : string.Empty), 
			FriendModMatchState.Missing => (Marker: MlmLocManager.Get("friendSync.marker.missing"), Tone: BadgeTone.Caution, Detail: (row.Remote.WorkshopId != 0) ? MlmLocManager.Get("friendSync.row.workshopAvailable") : MlmLocManager.Get("friendSync.row.localOnly")), 
			FriendModMatchState.Disabled => (Marker: MlmLocManager.Get("friendSync.marker.disabled"), Tone: BadgeTone.Caution, Detail: MlmLocManager.Get("friendSync.row.disabled")), 
			FriendModMatchState.VersionMismatch => (Marker: MlmLocManager.Get("friendSync.marker.version"), Tone: BadgeTone.Caution, Detail: MlmLocManager.Get("friendSync.row.versionMismatch", ("version", row.Local?.Mod.manifest?.version ?? MlmLocManager.Get("common.value.unknown")))), 
			FriendModMatchState.ContentMismatch => (Marker: MlmLocManager.Get("friendSync.marker.content"), Tone: BadgeTone.Critical, Detail: MlmLocManager.Get("friendSync.row.contentMismatch")), 
			FriendModMatchState.Checking => (Marker: MlmLocManager.Get("friendSync.marker.checking"), Tone: BadgeTone.Neutral, Detail: MlmLocManager.Get("friendSync.row.checking")), 
			_ => (Marker: MlmLocManager.Get("friendSync.marker.order"), Tone: BadgeTone.Caution, Detail: MlmLocManager.Get("friendSync.row.position", ("position", row.LocalPosition))), 
		};
	}

	private static string FriendStateText(FriendPresenceState state)
	{
		return state switch
		{
			FriendPresenceState.Ready => MlmLocManager.Get("friendSync.friend.ready"), 
			FriendPresenceState.PendingImport => MlmLocManager.Get("friendSync.friend.pending"), 
			FriendPresenceState.NotSharing => MlmLocManager.Get("friendSync.friend.notSharing"), 
			_ => MlmLocManager.Get("friendSync.friend.invalid"), 
		};
	}

	private static BadgeTone FriendStateTone(FriendPresenceState state)
	{
		return state switch
		{
			FriendPresenceState.Ready => BadgeTone.Positive, 
			FriendPresenceState.PendingImport => BadgeTone.Neutral, 
			FriendPresenceState.NotSharing => BadgeTone.Quiet, 
			_ => BadgeTone.Caution, 
		};
	}

	private static void ImportSelected()
	{
		ImportSelectedCore(gameplayOnly: false);
	}

	private static void ImportSelectedCore(bool gameplayOnly)
	{
		OnlineFriendMods onlineFriendMods = FriendModSync.Friends.FirstOrDefault((OnlineFriendMods item) => item.SteamId == _selectedSteamId);
		if (!(onlineFriendMods == null) && _plan != null)
		{
			_operationError = null;
			string text = FriendModSync.DescribeError(FriendModSync.BeginImport(onlineFriendMods, _plan, gameplayOnly).Error);
			if (text != null && text.Length > 0)
			{
				_operationError = text;
				Update();
			}
			else
			{
				Update();
			}
		}
	}

	private static void RetryFailed()
	{
		if (_plan != null)
		{
			_operationError = null;
			FriendModSync.RetryFailedDownloads(_plan);
			Update();
		}
	}

	private static void CancelPending()
	{
		_operationError = null;
		FriendModSync.CancelPending();
		Update();
	}

	private static void UpdatePublishStatus()
	{
		if (_publishStatus != null)
		{
			if (FriendModSync.ShowingSample)
			{
				((CanvasItem)_publishStatus).Visible = false;
			}
			else if (!FriendModSync.IsAvailable)
			{
				((CanvasItem)_publishStatus).Visible = true;
				_publishStatus.Text = MlmLocManager.Get("friendSync.unavailable", ("error", FriendModSync.AvailabilityError ?? MlmLocManager.Get("common.value.unknown")));
			}
			else
			{
				string publishError = FriendModSync.PublishError;
				((CanvasItem)_publishStatus).Visible = !string.IsNullOrWhiteSpace(publishError);
				_publishStatus.Text = (((CanvasItem)_publishStatus).Visible ? MlmLocManager.Get("friendSync.publish.failed", ("error", publishError)) : string.Empty);
			}
		}
	}

	private static void ApplyPending()
	{
		if (_plan == null || _applyProfile == null)
		{
			return;
		}
		LaunchProfile launchProfile = FriendModSync.PendingProfile(_plan);
		if (launchProfile == null || !FriendModSync.PendingCanApply(_plan))
		{
			Update();
			return;
		}
		PlanResult planResult = _applyProfile(launchProfile);
		if (planResult.Success)
		{
			FriendModSync.CompletePending();
			Close();
		}
		else
		{
			_operationError = planResult.Message;
			Update();
		}
	}

	private static void Clear(Node parent)
	{
		foreach (Node child in parent.GetChildren(false))
		{
			parent.RemoveChild(child);
			child.QueueFree();
		}
	}

	private static void IgnoreMouse(Node node)
	{
		Control val = (Control)(object)((node is Control) ? node : null);
		if (val != null)
		{
			val.MouseFilter = (MouseFilterEnum)2;
		}
		foreach (Node child in node.GetChildren(false))
		{
			IgnoreMouse(child);
		}
	}

	private static Label ColoredLabel(string text, Color color)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Label obj = UiKit.Label(text, "DimLabel");
		((Control)obj).AddThemeColorOverride(StringName.op_Implicit("font_color"), color);
		obj.AutowrapMode = (AutowrapMode)3;
		return obj;
	}
}
