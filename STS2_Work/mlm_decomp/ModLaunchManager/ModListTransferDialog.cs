using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;

namespace ModLaunchManager;

internal static class ModListTransferDialog
{
	private sealed record PendingMissingMod(string Id, string DisplayName, ulong WorkshopId);

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<Control?> _003C_003E9__41_0;

		public static TextSubmittedEventHandler _003C_003E9__44_0;

		public static Action<bool, string> _003C_003E9__47_0;

		public static Action<bool, string> _003C_003E9__52_0;

		public static Func<ProfileMod, bool> _003C_003E9__55_0;

		public static Func<ModListItem, bool> _003C_003E9__55_1;

		public static Func<ModListItem, string> _003C_003E9__55_2;

		public static Func<IGrouping<string, ModListItem>, string> _003C_003E9__55_3;

		public static Func<IGrouping<string, ModListItem>, string> _003C_003E9__55_4;

		public static Func<PendingMissingMod, bool> _003C_003E9__55_5;

		public static Func<PendingMissingMod, bool> _003C_003E9__56_0;

		public static Func<PendingMissingMod, bool> _003C_003E9__58_0;

		public static Func<WorkshopDownloadJob, bool> _003C_003E9__58_1;

		public static Func<WorkshopDownloadJob, bool> _003C_003E9__58_2;

		public static Func<WorkshopDownloadJob, bool> _003C_003E9__58_3;

		public static Func<PendingMissingMod, bool> _003C_003E9__58_4;

		public static Func<PendingMissingMod, bool> _003C_003E9__58_5;

		public static Func<PendingMissingMod, bool> _003C_003E9__58_6;

		public static Func<WorkshopDownloadJob, float> _003C_003E9__58_7;

		public static Func<WorkshopDownloadJob, float> _003C_003E9__58_10;

		public static Func<WorkshopDownloadJob, bool> _003C_003E9__58_8;

		public static Func<WorkshopDownloadJob, string> _003C_003E9__58_9;

		public static Func<PendingMissingMod, string> _003C_003E9__62_0;

		public static Func<PendingMissingMod, bool> _003C_003E9__67_1;

		public static Func<WorkshopDownloadJob, bool> _003C_003E9__67_0;

		internal Control? _003CShow_003Eb__41_0()
		{
			return (Control?)(object)_close;
		}

		internal void _003CBuildContent_003Eb__44_0(string _)
		{
			ImportFromSource();
		}

		internal void _003CExportToLink_003Eb__47_0(bool success, string message)
		{
			SetBusy(busy: false);
			if (!success)
			{
				SetStatus(MlmLocManager.Get("transfer.export.linkFailed", ("error", message)), false);
				return;
			}
			if (_link != null && GodotObject.IsInstanceValid((GodotObject)(object)_link))
			{
				_link.Text = message;
			}
			CopyLink(announce: false);
			SetStatus(MlmLocManager.Get("transfer.export.linkReady", ("url", message)), true);
		}

		internal void _003CImportLink_003Eb__52_0(bool success, string message)
		{
			SetBusy(busy: false);
			if (!success)
			{
				SetStatus(MlmLocManager.Get("transfer.import.downloadFailed", ("error", message)), false);
			}
			else
			{
				Apply(message);
			}
		}

		internal bool _003CBeginImport_003Eb__55_0(ProfileMod mod)
		{
			_003C_003Ec__DisplayClass55_0 CS_0024_003C_003E8__locals1 = new _003C_003Ec__DisplayClass55_0
			{
				mod = mod
			};
			return !_plan.Entries.Any((LaunchEntry entry) => string.Equals(entry.Id, CS_0024_003C_003E8__locals1.mod.Id, StringComparison.OrdinalIgnoreCase));
		}

		internal bool _003CBeginImport_003Eb__55_1(ModListItem item)
		{
			return !string.IsNullOrWhiteSpace(item.Id);
		}

		internal string _003CBeginImport_003Eb__55_2(ModListItem item)
		{
			return item.Id.Trim();
		}

		internal string _003CBeginImport_003Eb__55_3(IGrouping<string, ModListItem> group)
		{
			return group.Key;
		}

		internal string _003CBeginImport_003Eb__55_4(IGrouping<string, ModListItem> group)
		{
			if (!string.IsNullOrWhiteSpace(group.First().Name))
			{
				return group.First().Name.Trim();
			}
			return group.Key;
		}

		internal bool _003CBeginImport_003Eb__55_5(PendingMissingMod mod)
		{
			return mod.WorkshopId != 0;
		}

		internal bool _003CPollDownloads_003Eb__56_0(PendingMissingMod mod)
		{
			if (mod.WorkshopId != 0)
			{
				WorkshopDownloadJob? workshopDownloadJob = DownloadQueue.Find(mod.WorkshopId);
				if (workshopDownloadJob != null && workshopDownloadJob.State == WorkshopDownloadState.Complete)
				{
					return !IsRecognized(mod);
				}
			}
			return false;
		}

		internal bool _003CRenderPendingImport_003Eb__58_0(PendingMissingMod mod)
		{
			return mod.WorkshopId != 0;
		}

		internal bool _003CRenderPendingImport_003Eb__58_1(WorkshopDownloadJob job)
		{
			WorkshopDownloadState state = job.State;
			if ((uint)state <= 1u)
			{
				return true;
			}
			return false;
		}

		internal bool _003CRenderPendingImport_003Eb__58_2(WorkshopDownloadJob job)
		{
			return job.State == WorkshopDownloadState.Failed;
		}

		internal bool _003CRenderPendingImport_003Eb__58_3(WorkshopDownloadJob job)
		{
			return job.State == WorkshopDownloadState.Complete;
		}

		internal bool _003CRenderPendingImport_003Eb__58_4(PendingMissingMod mod)
		{
			WorkshopDownloadJob? workshopDownloadJob = DownloadQueue.Find(mod.WorkshopId);
			if (workshopDownloadJob != null && workshopDownloadJob.State == WorkshopDownloadState.Complete)
			{
				return !IsRecognized(mod);
			}
			return false;
		}

		internal bool _003CRenderPendingImport_003Eb__58_5(PendingMissingMod mod)
		{
			return DownloadQueue.Find(mod.WorkshopId) == null;
		}

		internal bool _003CRenderPendingImport_003Eb__58_6(PendingMissingMod mod)
		{
			WorkshopDownloadJob workshopDownloadJob = DownloadQueue.Find(mod.WorkshopId);
			if (workshopDownloadJob != null)
			{
				return workshopDownloadJob.State == WorkshopDownloadState.Failed;
			}
			return true;
		}

		internal float _003CRenderPendingImport_003Eb__58_7(WorkshopDownloadJob job)
		{
			return job.Progress;
		}

		internal float _003CRenderPendingImport_003Eb__58_10(WorkshopDownloadJob job)
		{
			return job.Progress;
		}

		internal bool _003CRenderPendingImport_003Eb__58_8(WorkshopDownloadJob job)
		{
			return job.State == WorkshopDownloadState.Failed;
		}

		internal string _003CRenderPendingImport_003Eb__58_9(WorkshopDownloadJob job)
		{
			return job.DisplayName + ": " + DownloadError(job);
		}

		internal string _003CRebuildMissingList_003Eb__62_0(PendingMissingMod mod)
		{
			WorkshopDownloadJob workshopDownloadJob = ((mod.WorkshopId != 0) ? DownloadQueue.Find(mod.WorkshopId) : null);
			int value = ((workshopDownloadJob != null) ? ((int)Math.Round(workshopDownloadJob.Progress * 100f)) : 0);
			return $"{mod.Id}\u001e{mod.WorkshopId}\u001e{workshopDownloadJob?.State}\u001e{value}\u001e{IsRecognized(mod)}\u001e{WorkshopUpdates.IsAvailable}";
		}

		internal bool _003CDownloadAll_003Eb__67_1(PendingMissingMod mod)
		{
			return mod.WorkshopId != 0;
		}

		internal bool _003CDownloadAll_003Eb__67_0(WorkshopDownloadJob job)
		{
			return !HasWorkshopCopy(job.ItemId);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass55_0
	{
		public ProfileMod mod;

		internal bool _003CBeginImport_003Eb__6(LaunchEntry entry)
		{
			return string.Equals(entry.Id, mod.Id, StringComparison.OrdinalIgnoreCase);
		}
	}

	private static readonly WorkshopDownloadQueue DownloadQueue = new WorkshopDownloadQueue(OnDownloadsChanged, WorkshopDownloadSource.ModList);

	private static readonly List<PendingMissingMod> PendingMissingMods = new List<PendingMissingMod>();

	private static CanvasLayer? _layer;

	private static Control? _root;

	private static Label? _title;

	private static Label? _subtitle;

	private static Label? _exportCaption;

	private static Label? _importCaption;

	private static Label? _linkCaption;

	private static Label? _status;

	private static LineEdit? _link;

	private static LineEdit? _source;

	private static Button? _exportFile;

	private static Button? _exportLink;

	private static Button? _copyLink;

	private static Button? _importFile;

	private static Button? _importSource;

	private static Button? _applyPending;

	private static Button? _cancelPending;

	private static Button? _close;

	private static Button? _done;

	private static Label? _missingCaption;

	private static ScrollContainer? _missingScroll;

	private static VBoxContainer? _missingList;

	private static Control? _contentSpacer;

	private static HBoxContainer? _pendingActions;

	private static Button? _downloadAll;

	private static ProgressBar? _downloadProgress;

	private static Timer? _downloadTimer;

	private static LaunchPlan? _plan;

	private static string _gameVersion = string.Empty;

	private static Func<LaunchProfile, PlanResult>? _apply;

	private static LaunchProfile? _pendingProfile;

	private static string? _pendingApplyError;

	private static string _missingListSignature = string.Empty;

	private static int _pendingLocalOnly;

	private static int _pendingWorkshopUnavailable;

	private static bool _busy;

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

	internal static void Show(LaunchPlan plan, string gameVersion, Func<LaunchProfile, PlanResult> apply)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Expected O, but got Unknown
		_plan = plan;
		_gameVersion = gameVersion;
		_apply = apply;
		if (IsOpen)
		{
			Refresh();
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			_busy = false;
			_layer = new CanvasLayer
			{
				Name = StringName.op_Implicit("ModListTransferDialog"),
				Layer = 1100
			};
			_root = UiKit.FullRect<Control>(new Control
			{
				Name = StringName.op_Implicit("ModListTransferRoot"),
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
			((Node)val2).Name = StringName.op_Implicit("ModListTransferCard");
			((Control)val2).AnchorLeft = 0.24f;
			((Control)val2).AnchorTop = 0.17f;
			((Control)val2).AnchorRight = 0.76f;
			((Control)val2).AnchorBottom = 0.83f;
			((Control)val2).OffsetLeft = 0f;
			((Control)val2).OffsetTop = 0f;
			((Control)val2).OffsetRight = 0f;
			((Control)val2).OffsetBottom = 0f;
			((Node)val2).AddChild((Node)(object)BuildContent(), false, (InternalMode)0);
			((Node)_root).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)_layer).AddChild((Node)(object)_root, false, (InternalMode)0);
			((Node)val.Root).AddChild((Node)(object)_layer, false, (InternalMode)0);
			LauncherController.Active?.AddScope(new LauncherController.Scope(_root, Close, () => (Control?)(object)_close));
			_downloadTimer = new Timer
			{
				WaitTime = 0.5,
				Autostart = true
			};
			_downloadTimer.Timeout += PollDownloads;
			((Node)_root).AddChild((Node)(object)_downloadTimer, false, (InternalMode)0);
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
		_exportCaption = null;
		_importCaption = null;
		_linkCaption = null;
		_status = null;
		_link = null;
		_source = null;
		_exportFile = null;
		_exportLink = null;
		_copyLink = null;
		_importFile = null;
		_importSource = null;
		_applyPending = null;
		_cancelPending = null;
		_close = null;
		_done = null;
		_missingCaption = null;
		_missingScroll = null;
		_missingList = null;
		_contentSpacer = null;
		_pendingActions = null;
		_plan = null;
		_apply = null;
		_pendingProfile = null;
		_pendingApplyError = null;
		_missingListSignature = string.Empty;
		_pendingLocalOnly = 0;
		PendingMissingMods.Clear();
		_downloadAll = null;
		_downloadProgress = null;
		_downloadTimer = null;
		_pendingWorkshopUnavailable = 0;
		DownloadQueue.Clear();
		_gameVersion = string.Empty;
		_busy = false;
	}

	internal static void Refresh()
	{
		if (IsOpen && _root != null && GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			_root.Theme = UiKit.CreateTheme();
			_missingListSignature = string.Empty;
			if (_title != null)
			{
				_title.Text = MlmLocManager.Get("transfer.title");
			}
			if (_subtitle != null)
			{
				_subtitle.Text = MlmLocManager.Get("transfer.subtitle", ("service", ModListPasteService.DisplayName));
			}
			if (_exportCaption != null)
			{
				_exportCaption.Text = MlmLocManager.Get("transfer.export.caption");
			}
			if (_importCaption != null)
			{
				_importCaption.Text = MlmLocManager.Get("transfer.import.caption");
			}
			if (_linkCaption != null)
			{
				_linkCaption.Text = MlmLocManager.Get("transfer.export.link.label");
			}
			if (_exportFile != null)
			{
				_exportFile.Text = MlmLocManager.Get("transfer.export.toFile");
			}
			if (_exportLink != null)
			{
				_exportLink.Text = MlmLocManager.Get("transfer.export.toLink");
			}
			if (_copyLink != null)
			{
				_copyLink.Text = MlmLocManager.Get("transfer.export.copy");
			}
			if (_importFile != null)
			{
				_importFile.Text = MlmLocManager.Get("transfer.import.fromFile");
			}
			if (_importSource != null)
			{
				_importSource.Text = MlmLocManager.Get("transfer.import.action");
			}
			if (_applyPending != null)
			{
				_applyPending.Text = MlmLocManager.Get("transfer.import.apply");
			}
			if (_downloadAll != null)
			{
				_downloadAll.Text = MlmLocManager.Get("transfer.import.downloadAll");
			}
			if (_cancelPending != null)
			{
				_cancelPending.Text = MlmLocManager.Get("transfer.import.cancel");
			}
			if (_source != null)
			{
				_source.PlaceholderText = MlmLocManager.Get("transfer.import.source.placeholder");
			}
			if (_close != null)
			{
				((Control)_close).TooltipText = MlmLocManager.Get("transfer.action.close.tooltip");
			}
			if (_done != null)
			{
				_done.Text = MlmLocManager.Get("common.action.close");
			}
			UpdateBusyState();
			RenderPendingImport();
		}
	}

	private static Control BuildContent()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Expected O, but got Unknown
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Expected O, but got Unknown
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Expected O, but got Unknown
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Expected O, but got Unknown
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Expected O, but got Unknown
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Expected O, but got Unknown
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Expected O, but got Unknown
		//IL_0574: Unknown result type (might be due to invalid IL or missing references)
		//IL_0579: Unknown result type (might be due to invalid IL or missing references)
		//IL_0586: Expected O, but got Unknown
		//IL_059b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d9: Expected O, but got Unknown
		//IL_05f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_060d: Unknown result type (might be due to invalid IL or missing references)
		//IL_061c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0623: Unknown result type (might be due to invalid IL or missing references)
		//IL_062e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0638: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Expected O, but got Unknown
		//IL_0652: Unknown result type (might be due to invalid IL or missing references)
		//IL_0657: Unknown result type (might be due to invalid IL or missing references)
		//IL_0663: Expected O, but got Unknown
		//IL_079a: Unknown result type (might be due to invalid IL or missing references)
		//IL_079f: Unknown result type (might be due to invalid IL or missing references)
		//IL_07af: Expected O, but got Unknown
		//IL_07bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d7: Expected O, but got Unknown
		//IL_07e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ec: Expected O, but got Unknown
		//IL_07ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0803: Expected O, but got Unknown
		MarginContainer val = UiKit.Margin((Control)new VBoxContainer(), 20, 18);
		VBoxContainer val2 = (VBoxContainer)((Node)val).GetChild(0, false);
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		HBoxContainer val3 = new HBoxContainer();
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		VBoxContainer val4 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 2);
		_title = UiKit.Label(MlmLocManager.Get("transfer.title"), "SectionLabel");
		_subtitle = UiKit.Label(MlmLocManager.Get("transfer.subtitle", ("service", ModListPasteService.DisplayName)), "DimLabel");
		_subtitle.AutowrapMode = (AutowrapMode)3;
		((Node)val4).AddChild((Node)(object)_title, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_subtitle, false, (InternalMode)0);
		_close = UiKit.Button("✕", "AccentIconButton", 34f, 32f);
		((Control)_close).SizeFlagsVertical = (SizeFlags)0;
		((Control)_close).MouseDefaultCursorShape = (CursorShape)2;
		((BaseButton)_close).Pressed += Close;
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)_close, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)UiKit.Divider(), false, (InternalMode)0);
		_exportCaption = UiKit.Label(MlmLocManager.Get("transfer.export.caption"), "CaptionLabel");
		((Node)val2).AddChild((Node)(object)_exportCaption, false, (InternalMode)0);
		HBoxContainer val5 = new HBoxContainer();
		((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_exportFile = UiKit.Button(MlmLocManager.Get("transfer.export.toFile"), "SecondaryButton", 170f, 34f);
		((BaseButton)_exportFile).Pressed += ExportToFile;
		_exportLink = UiKit.Button(MlmLocManager.Get("transfer.export.toLink"), "SecondaryButton", 170f, 34f);
		((BaseButton)_exportLink).Pressed += ExportToLink;
		((Node)val5).AddChild((Node)(object)_exportFile, false, (InternalMode)0);
		((Node)val5).AddChild((Node)(object)_exportLink, false, (InternalMode)0);
		((Node)val5).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val5, false, (InternalMode)0);
		HBoxContainer val6 = new HBoxContainer();
		((Control)val6).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_linkCaption = UiKit.Label(MlmLocManager.Get("transfer.export.link.label"), "DimLabel");
		((Control)_linkCaption).SizeFlagsVertical = (SizeFlags)4;
		_link = new LineEdit
		{
			Editable = false,
			SelectingEnabled = true,
			SizeFlagsHorizontal = (SizeFlags)3,
			CustomMinimumSize = new Vector2(0f, 34f)
		};
		_copyLink = UiKit.Button(MlmLocManager.Get("transfer.export.copy"), "MiniButton", 70f, 34f);
		((BaseButton)_copyLink).Pressed += CopyLink;
		((Node)val6).AddChild((Node)(object)_linkCaption, false, (InternalMode)0);
		((Node)val6).AddChild((Node)(object)_link, false, (InternalMode)0);
		((Node)val6).AddChild((Node)(object)_copyLink, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val6, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)UiKit.Divider(), false, (InternalMode)0);
		_importCaption = UiKit.Label(MlmLocManager.Get("transfer.import.caption"), "CaptionLabel");
		((Node)val2).AddChild((Node)(object)_importCaption, false, (InternalMode)0);
		HBoxContainer val7 = new HBoxContainer();
		((Control)val7).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_source = new LineEdit
		{
			ClearButtonEnabled = true,
			PlaceholderText = MlmLocManager.Get("transfer.import.source.placeholder"),
			SizeFlagsHorizontal = (SizeFlags)3,
			CustomMinimumSize = new Vector2(0f, 34f)
		};
		LineEdit? source = _source;
		object obj = _003C_003Ec._003C_003E9__44_0;
		if (obj == null)
		{
			TextSubmittedEventHandler val8 = delegate
			{
				ImportFromSource();
			};
			_003C_003Ec._003C_003E9__44_0 = val8;
			obj = (object)val8;
		}
		source.TextSubmitted += (TextSubmittedEventHandler)obj;
		_importSource = UiKit.Button(MlmLocManager.Get("transfer.import.action"), "SecondaryButton", 90f, 34f);
		((BaseButton)_importSource).Pressed += ImportFromSource;
		_importFile = UiKit.Button(MlmLocManager.Get("transfer.import.fromFile"), "SecondaryButton", 170f, 34f);
		((BaseButton)_importFile).Pressed += ImportFromFile;
		((Node)val7).AddChild((Node)(object)_source, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)_importSource, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)_importFile, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val7, false, (InternalMode)0);
		_status = UiKit.Label(string.Empty, "DimLabel");
		_status.AutowrapMode = (AutowrapMode)3;
		((Node)val2).AddChild((Node)(object)_status, false, (InternalMode)0);
		_missingCaption = UiKit.Label(string.Empty, "CaptionLabel");
		((CanvasItem)_missingCaption).Visible = false;
		((Node)val2).AddChild((Node)(object)_missingCaption, false, (InternalMode)0);
		_missingList = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_missingList).AddThemeConstantOverride(StringName.op_Implicit("separation"), 6);
		_missingScroll = new ScrollContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3,
			CustomMinimumSize = new Vector2(0f, 180f),
			HorizontalScrollMode = (ScrollMode)0,
			Visible = false
		};
		((Node)_missingScroll).AddChild((Node)(object)_missingList, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_missingScroll, false, (InternalMode)0);
		_downloadProgress = new ProgressBar
		{
			MinValue = 0.0,
			MaxValue = 100.0,
			ShowPercentage = false,
			CustomMinimumSize = new Vector2(0f, 10f),
			Visible = false
		};
		((Node)val2).AddChild((Node)(object)_downloadProgress, false, (InternalMode)0);
		_pendingActions = new HBoxContainer
		{
			Visible = false
		};
		((Control)_pendingActions).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_applyPending = UiKit.Button(MlmLocManager.Get("transfer.import.apply"), "PrimaryButton", 150f, 34f);
		((BaseButton)_applyPending).Pressed += ApplyPending;
		_cancelPending = UiKit.Button(MlmLocManager.Get("transfer.import.cancel"), "SecondaryButton", 120f, 34f);
		((BaseButton)_cancelPending).Pressed += CancelPending;
		((Node)_pendingActions).AddChild((Node)(object)_applyPending, false, (InternalMode)0);
		_downloadAll = UiKit.Button(MlmLocManager.Get("transfer.import.downloadAll"), "SecondaryButton", 120f, 34f);
		((BaseButton)_downloadAll).Pressed += DownloadAll;
		((Node)_pendingActions).AddChild((Node)(object)_downloadAll, false, (InternalMode)0);
		((Node)_pendingActions).AddChild((Node)(object)_cancelPending, false, (InternalMode)0);
		((Node)_pendingActions).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_pendingActions, false, (InternalMode)0);
		_contentSpacer = new Control
		{
			SizeFlagsVertical = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val2).AddChild((Node)(object)_contentSpacer, false, (InternalMode)0);
		HBoxContainer val9 = new HBoxContainer();
		((Node)val9).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		_done = UiKit.Button(MlmLocManager.Get("common.action.close"), "PrimaryButton", 96f, 34f);
		((BaseButton)_done).Pressed += Close;
		((Node)val9).AddChild((Node)(object)_done, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val9, false, (InternalMode)0);
		return (Control)(object)val;
	}

	private static void ExportToFile()
	{
		if (_plan != null && !_busy)
		{
			string payload = ModListTransfer.Serialize(ModListTransfer.Capture(_plan, _gameVersion));
			int count = _plan.Entries.Count;
			string defaultFileName = ModListTransfer.DefaultFileName;
			if (NativeFileDialogs.TryShow(save: true, MlmLocManager.Get("transfer.export.dialogTitle"), ModListTransfer.FallbackExportFolder, defaultFileName, delegate(string path)
			{
				WriteExport(EnsureJsonExtension(path), payload, count);
			}, out string error))
			{
				SetStatus(MlmLocManager.Get("transfer.export.pickFile"), null);
				return;
			}
			GD.PushWarning("[ModLaunchManager] " + MlmLocManager.Get("transfer.file.fallback", ("error", error)));
			WriteExport(Path.Combine(ModListTransfer.FallbackExportFolder, defaultFileName), payload, count);
		}
	}

	private static void WriteExport(string path, string payload, int count)
	{
		if (ModListTransfer.TryWrite(path, payload, out string error))
		{
			SetStatus(MlmLocManager.Get("transfer.export.fileSaved", ("count", count), ("path", path)), true);
		}
		else
		{
			SetStatus(MlmLocManager.Get("transfer.export.fileFailed", ("error", error)), false);
		}
	}

	private static void ExportToLink()
	{
		if (_plan == null || _root == null || _busy)
		{
			return;
		}
		string payload = ModListTransfer.Serialize(ModListTransfer.Capture(_plan, _gameVersion));
		SetBusy(busy: true);
		SetStatus(MlmLocManager.Get("transfer.export.uploading", ("service", ModListPasteService.DisplayName)), null);
		ModListPasteService.Upload((Node)(object)_root, payload, delegate(bool success, string message)
		{
			SetBusy(busy: false);
			if (!success)
			{
				SetStatus(MlmLocManager.Get("transfer.export.linkFailed", ("error", message)), false);
			}
			else
			{
				if (_link != null && GodotObject.IsInstanceValid((GodotObject)(object)_link))
				{
					_link.Text = message;
				}
				CopyLink(announce: false);
				SetStatus(MlmLocManager.Get("transfer.export.linkReady", ("url", message)), true);
			}
		});
	}

	private static void CopyLink()
	{
		CopyLink(announce: true);
	}

	private static void CopyLink(bool announce)
	{
		LineEdit? link = _link;
		string text = ((link != null) ? link.Text.Trim() : null) ?? string.Empty;
		if (text.Length == 0)
		{
			if (announce)
			{
				SetStatus(MlmLocManager.Get("transfer.export.noLinkYet"), false);
			}
			return;
		}
		try
		{
			DisplayServer.ClipboardSet(text);
			if (announce)
			{
				SetStatus(MlmLocManager.Get("transfer.export.copied"), true);
			}
		}
		catch (Exception ex)
		{
			SetStatus(MlmLocManager.Get("transfer.export.copyFailed", ("error", ex.Message)), false);
		}
	}

	private static void ImportFromFile()
	{
		if (_busy)
		{
			return;
		}
		if (NativeFileDialogs.TryShow(save: false, MlmLocManager.Get("transfer.import.dialogTitle"), ModListTransfer.FallbackExportFolder, string.Empty, ImportFile, out string error))
		{
			SetStatus(MlmLocManager.Get("transfer.import.pickFile"), null);
			return;
		}
		SetStatus(MlmLocManager.Get("transfer.file.fallbackImport", ("error", error)), false);
		LineEdit? source = _source;
		if (source != null)
		{
			((Control)source).GrabFocus();
		}
	}

	private static void ImportFromSource()
	{
		if (!_busy)
		{
			LineEdit? source = _source;
			string text = ((source != null) ? source.Text.Trim() : null) ?? string.Empty;
			string url;
			if (text.Length == 0)
			{
				SetStatus(MlmLocManager.Get("transfer.import.empty"), false);
			}
			else if (ModListPasteService.TryResolveUrl(text, out url))
			{
				ImportLink(url);
			}
			else
			{
				ImportFile(text);
			}
		}
	}

	private static void ImportLink(string url)
	{
		if (_root == null || !GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			return;
		}
		SetBusy(busy: true);
		SetStatus(MlmLocManager.Get("transfer.import.downloading", ("url", url)), null);
		ModListPasteService.Download((Node)(object)_root, url, delegate(bool success, string message)
		{
			SetBusy(busy: false);
			if (!success)
			{
				SetStatus(MlmLocManager.Get("transfer.import.downloadFailed", ("error", message)), false);
			}
			else
			{
				Apply(message);
			}
		});
	}

	private static void ImportFile(string path)
	{
		if (!ModListTransfer.TryRead(path, out string payload, out string error))
		{
			SetStatus(MlmLocManager.Get("transfer.import.readFailed", ("path", path), ("error", error)), false);
		}
		else
		{
			Apply(payload);
		}
	}

	private static void Apply(string payload)
	{
		if (_apply != null)
		{
			if (!ModListTransfer.TryParse(payload, out ModListDocument document, out LaunchProfile profile, out string error))
			{
				SetStatus(MlmLocManager.Get("transfer.import.parseFailed", ("error", error)), false);
			}
			else
			{
				BeginImport(document, profile);
			}
		}
	}

	private static void BeginImport(ModListDocument document, LaunchProfile profile)
	{
		if (_plan == null || _apply == null)
		{
			return;
		}
		List<ProfileMod> list = profile.Mods.Where((ProfileMod mod) => !_plan.Entries.Any((LaunchEntry entry) => string.Equals(entry.Id, mod.Id, StringComparison.OrdinalIgnoreCase))).ToList();
		if (list.Count == 0)
		{
			PlanResult planResult = _apply(profile);
			SetStatus(planResult.Message, planResult.Success);
			return;
		}
		Dictionary<string, string> dictionary = (from item in (document.Mods ?? new List<ModListItem>()).OfType<ModListItem>()
			where !string.IsNullOrWhiteSpace(item.Id)
			select item).GroupBy<ModListItem, string>((ModListItem item) => item.Id.Trim(), StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, ModListItem>, string, string>((IGrouping<string, ModListItem> group) => group.Key, (IGrouping<string, ModListItem> group) => (!string.IsNullOrWhiteSpace(group.First().Name)) ? group.First().Name.Trim() : group.Key, StringComparer.OrdinalIgnoreCase);
		_pendingProfile = profile;
		_pendingApplyError = null;
		_missingListSignature = string.Empty;
		_pendingLocalOnly = 0;
		PendingMissingMods.Clear();
		_pendingWorkshopUnavailable = 0;
		DownloadQueue.Clear();
		foreach (ProfileMod item in list)
		{
			ulong itemId;
			bool flag = ModListTransfer.TryWorkshopItemId(item.Source, out itemId);
			PendingMissingMods.Add(new PendingMissingMod(item.Id, dictionary.GetValueOrDefault(item.Id, item.Id), flag ? itemId : 0));
			if (!flag)
			{
				_pendingLocalOnly++;
			}
		}
		if (!WorkshopUpdates.IsAvailable)
		{
			_pendingWorkshopUnavailable = PendingMissingMods.Count((PendingMissingMod mod) => mod.WorkshopId != 0);
		}
		UpdateBusyState();
		RenderPendingImport();
	}

	private static void PollDownloads()
	{
		if (_pendingProfile == null)
		{
			return;
		}
		bool flag = PendingMissingMods.Any(delegate(PendingMissingMod mod)
		{
			if (mod.WorkshopId != 0)
			{
				WorkshopDownloadJob? workshopDownloadJob = DownloadQueue.Find(mod.WorkshopId);
				if (workshopDownloadJob != null && workshopDownloadJob.State == WorkshopDownloadState.Complete)
				{
					return !IsRecognized(mod);
				}
			}
			return false;
		});
		if (DownloadQueue.ActiveCount != 0 || flag)
		{
			DownloadQueue.Poll(0.5);
			RenderPendingImport();
		}
	}

	private static void OnDownloadsChanged()
	{
		RenderPendingImport();
	}

	private static void RenderPendingImport()
	{
		if (_pendingActions == null || _downloadProgress == null || _applyPending == null || _downloadAll == null || _cancelPending == null || _missingCaption == null || _missingScroll == null || _missingList == null || _pendingProfile == null)
		{
			if (_pendingActions != null)
			{
				((CanvasItem)_pendingActions).Visible = false;
			}
			if (_downloadProgress != null)
			{
				((CanvasItem)_downloadProgress).Visible = false;
			}
			if (_missingCaption != null)
			{
				((CanvasItem)_missingCaption).Visible = false;
			}
			if (_missingScroll != null)
			{
				((CanvasItem)_missingScroll).Visible = false;
			}
			if (_contentSpacer != null)
			{
				((CanvasItem)_contentSpacer).Visible = true;
			}
			return;
		}
		IReadOnlyList<WorkshopDownloadJob> jobs = DownloadQueue.Jobs;
		List<PendingMissingMod> list = PendingMissingMods.Where((PendingMissingMod mod) => mod.WorkshopId != 0).ToList();
		int num = jobs.Count(delegate(WorkshopDownloadJob job)
		{
			WorkshopDownloadState state = job.State;
			return (uint)state <= 1u;
		});
		int num2 = jobs.Count((WorkshopDownloadJob job) => job.State == WorkshopDownloadState.Failed);
		int num3 = jobs.Count((WorkshopDownloadJob job) => job.State == WorkshopDownloadState.Complete);
		int num4 = list.Count(delegate(PendingMissingMod mod)
		{
			WorkshopDownloadJob? workshopDownloadJob = DownloadQueue.Find(mod.WorkshopId);
			return workshopDownloadJob != null && workshopDownloadJob.State == WorkshopDownloadState.Complete && !IsRecognized(mod);
		});
		int num5 = list.Count((PendingMissingMod mod) => DownloadQueue.Find(mod.WorkshopId) == null);
		bool visible = WorkshopUpdates.IsAvailable && list.Any(delegate(PendingMissingMod mod)
		{
			WorkshopDownloadJob workshopDownloadJob2 = DownloadQueue.Find(mod.WorkshopId);
			return workshopDownloadJob2 == null || workshopDownloadJob2.State == WorkshopDownloadState.Failed;
		});
		RebuildMissingList();
		_missingCaption.Text = MlmLocManager.Get("transfer.import.missingCaption", ("count", PendingMissingMods.Count));
		((CanvasItem)_missingCaption).Visible = true;
		((CanvasItem)_missingScroll).Visible = true;
		if (_contentSpacer != null)
		{
			((CanvasItem)_contentSpacer).Visible = false;
		}
		((CanvasItem)_pendingActions).Visible = true;
		((CanvasItem)_applyPending).Visible = num == 0;
		_applyPending.Text = ((list.All(IsRecognized) && _pendingLocalOnly == 0) ? MlmLocManager.Get("transfer.import.apply") : MlmLocManager.Get("transfer.import.applyAvailable"));
		((CanvasItem)_downloadAll).Visible = visible;
		((CanvasItem)_cancelPending).Visible = true;
		((CanvasItem)_downloadProgress).Visible = jobs.Count > 0 && num > 0;
		((Range)_downloadProgress).Value = ((jobs.Count == 0) ? 0f : (jobs.Average((WorkshopDownloadJob job) => job.Progress) * 100f));
		if (!string.IsNullOrWhiteSpace(_pendingApplyError))
		{
			((CanvasItem)_applyPending).Visible = true;
			SetStatus(_pendingApplyError, false);
		}
		else if (_pendingWorkshopUnavailable > 0)
		{
			SetStatus(MlmLocManager.Get("transfer.import.workshopUnavailable", ("count", _pendingWorkshopUnavailable), ("localNote", LocalOnlyNote())), false);
		}
		else if (num > 0)
		{
			int num6 = (int)Math.Round(jobs.Average((WorkshopDownloadJob job) => job.Progress) * 100f);
			SetStatus(MlmLocManager.Get("transfer.import.downloadsActive", ("active", num), ("total", jobs.Count), ("progress", num6), ("localNote", LocalOnlyNote())), null);
		}
		else if (num2 > 0)
		{
			SetStatus(MlmLocManager.Get("transfer.import.downloadsFailed", ("count", num2), ("localNote", LocalOnlyNote())), false);
			((Control)_status).TooltipText = string.Join('\n', from job in jobs
				where job.State == WorkshopDownloadState.Failed
				select job.DisplayName + ": " + DownloadError(job));
		}
		else if (num4 > 0)
		{
			SetStatus(MlmLocManager.Get("transfer.import.waitingForDiscovery", ("downloaded", num3), ("remaining", num4)), null);
		}
		else if (num5 > 0)
		{
			SetStatus(MlmLocManager.Get("transfer.import.chooseDownloads", ("workshop", num5), ("local", _pendingLocalOnly)), null);
		}
		else if (list.Count == 0)
		{
			SetStatus(MlmLocManager.Get("transfer.import.localOnlyReady", ("count", _pendingLocalOnly)), null);
		}
		else
		{
			SetStatus((_pendingLocalOnly > 0) ? MlmLocManager.Get("transfer.import.downloadsReadyWithLocal", ("count", _pendingLocalOnly)) : MlmLocManager.Get("transfer.import.downloadsReady"), true);
		}
	}

	private static bool IsRecognized(PendingMissingMod mod)
	{
		if (mod.WorkshopId != 0)
		{
			return _plan?.Entries.Any((LaunchEntry entry) => string.Equals(entry.Id, mod.Id, StringComparison.OrdinalIgnoreCase) && WorkshopUpdates.ItemIdFor(entry.Mod) == mod.WorkshopId) ?? false;
		}
		return false;
	}

	private static string DownloadError(WorkshopDownloadJob job)
	{
		return MlmLocManager.Get((job.Failure == WorkshopDownloadFailure.Cancelled) ? "workshop.remove.cancelled" : ((job.Failure == WorkshopDownloadFailure.Timeout) ? "transfer.import.downloadTimeout" : "transfer.import.downloadUnexpected"));
	}

	private static string LocalOnlyNote()
	{
		if (_pendingLocalOnly <= 0)
		{
			return string.Empty;
		}
		return " " + MlmLocManager.Get("transfer.import.localOnlyNote", ("count", _pendingLocalOnly));
	}

	private static void RebuildMissingList()
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Expected O, but got Unknown
		//IL_048b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		if (_missingList == null || !GodotObject.IsInstanceValid((GodotObject)(object)_missingList))
		{
			return;
		}
		string text = string.Join('\u001f', PendingMissingMods.Select(delegate(PendingMissingMod mod)
		{
			WorkshopDownloadJob workshopDownloadJob = ((mod.WorkshopId != 0) ? DownloadQueue.Find(mod.WorkshopId) : null);
			int value = ((workshopDownloadJob != null) ? ((int)Math.Round(workshopDownloadJob.Progress * 100f)) : 0);
			return $"{mod.Id}\u001e{mod.WorkshopId}\u001e{workshopDownloadJob?.State}\u001e{value}\u001e{IsRecognized(mod)}\u001e{WorkshopUpdates.IsAvailable}";
		}));
		if (string.Equals(_missingListSignature, text, StringComparison.Ordinal))
		{
			return;
		}
		_missingListSignature = text;
		ScrollContainer? missingScroll = _missingScroll;
		int scrollPosition = ((missingScroll != null) ? missingScroll.ScrollVertical : 0);
		foreach (Node child in ((Node)_missingList).GetChildren(false))
		{
			((Node)_missingList).RemoveChild(child);
			child.QueueFree();
		}
		using (List<PendingMissingMod>.Enumerator enumerator2 = PendingMissingMods.GetEnumerator())
		{
			PanelContainer val;
			HBoxContainer val2;
			for (; enumerator2.MoveNext(); ((Node)val).AddChild((Node)(object)UiKit.Margin((Control)(object)val2, 12, 8), false, (InternalMode)0), ((Node)_missingList).AddChild((Node)(object)val, false, (InternalMode)0))
			{
				PendingMissingMod current2 = enumerator2.Current;
				PendingMissingMod captured = current2;
				WorkshopDownloadJob workshopDownloadJob2 = ((current2.WorkshopId != 0) ? DownloadQueue.Find(current2.WorkshopId) : null);
				val = UiKit.Card("InsetPanel");
				((Control)val).SizeFlagsVertical = (SizeFlags)0;
				val2 = new HBoxContainer();
				((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
				VBoxContainer val3 = new VBoxContainer
				{
					SizeFlagsHorizontal = (SizeFlags)3,
					SizeFlagsVertical = (SizeFlags)4
				};
				((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 1);
				Label val4 = UiKit.Label(current2.DisplayName);
				val4.TextOverrunBehavior = (OverrunBehavior)3;
				val4.ClipText = true;
				Label val5 = UiKit.Label((current2.WorkshopId != 0) ? MlmLocManager.Get("transfer.import.row.workshop", ("itemId", current2.WorkshopId)) : MlmLocManager.Get("transfer.import.row.localOnly"), "DimLabel");
				val5.TextOverrunBehavior = (OverrunBehavior)3;
				val5.ClipText = true;
				((Control)val5).TooltipText = current2.Id;
				((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
				((Node)val3).AddChild((Node)(object)val5, false, (InternalMode)0);
				((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
				Label val6 = UiKit.Label(MissingModState(current2, workshopDownloadJob2), MissingModStateStyle(current2, workshopDownloadJob2));
				((Control)val6).CustomMinimumSize = new Vector2(150f, 0f);
				((Control)val6).SizeFlagsVertical = (SizeFlags)4;
				val6.HorizontalAlignment = (HorizontalAlignment)2;
				val6.TextOverrunBehavior = (OverrunBehavior)3;
				val6.ClipText = true;
				if (workshopDownloadJob2 != null && workshopDownloadJob2.State == WorkshopDownloadState.Failed)
				{
					((Control)val6).TooltipText = DownloadError(workshopDownloadJob2);
				}
				((Node)val2).AddChild((Node)(object)val6, false, (InternalMode)0);
				if (current2.WorkshopId == 0)
				{
					continue;
				}
				Button val7 = UiKit.Button(MlmLocManager.Get("transfer.import.openWorkshop"), "MiniButton", 126f, 30f);
				((BaseButton)val7).Disabled = !WorkshopUpdates.IsAvailable;
				((Control)val7).TooltipText = MlmLocManager.Get("transfer.import.openWorkshop.tooltip");
				((Control)val7).MouseDefaultCursorShape = (CursorShape)2;
				((BaseButton)val7).Pressed += delegate
				{
					OpenWorkshop(captured);
				};
				((Node)val2).AddChild((Node)(object)val7, false, (InternalMode)0);
				Button val8 = UiKit.Button((workshopDownloadJob2 != null && workshopDownloadJob2.State == WorkshopDownloadState.Failed) ? MlmLocManager.Get("transfer.import.retryOne") : ((workshopDownloadJob2 != null && workshopDownloadJob2.State == WorkshopDownloadState.Complete) ? MlmLocManager.Get("transfer.import.downloadedOne") : MlmLocManager.Get("transfer.import.downloadOne")), (workshopDownloadJob2 != null && workshopDownloadJob2.State == WorkshopDownloadState.Failed) ? "SecondaryButton" : "MiniButton", 80f, 30f);
				Button val9 = val8;
				bool flag = !WorkshopUpdates.IsAvailable;
				bool flag2;
				if (!flag)
				{
					WorkshopDownloadState? workshopDownloadState = workshopDownloadJob2?.State;
					if (workshopDownloadState.HasValue)
					{
						WorkshopDownloadState valueOrDefault = workshopDownloadState.GetValueOrDefault();
						if ((uint)valueOrDefault <= 2u)
						{
							flag2 = true;
							goto IL_03d6;
						}
					}
					flag2 = false;
					goto IL_03d6;
				}
				goto IL_03da;
				IL_03d6:
				flag = flag2;
				goto IL_03da;
				IL_03da:
				((BaseButton)val9).Disabled = flag;
				((Control)val8).TooltipText = MlmLocManager.Get("transfer.import.downloadOne.tooltip");
				((Control)val8).MouseDefaultCursorShape = (CursorShape)2;
				((BaseButton)val8).Pressed += delegate
				{
					DownloadOne(captured);
				};
				((Node)val2).AddChild((Node)(object)val8, false, (InternalMode)0);
			}
		}
		if (_missingScroll == null || !GodotObject.IsInstanceValid((GodotObject)(object)_missingScroll))
		{
			return;
		}
		ScrollContainer scroll = _missingScroll;
		Callable val10 = Callable.From((Action)delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)scroll))
			{
				scroll.ScrollVertical = scrollPosition;
			}
		});
		((Callable)(ref val10)).CallDeferred(Array.Empty<Variant>());
	}

	private static string MissingModState(PendingMissingMod mod, WorkshopDownloadJob? job)
	{
		if (job != null && job.Failure == WorkshopDownloadFailure.Cancelled)
		{
			return MlmLocManager.Get("workshop.remove.cancelled");
		}
		if (mod.WorkshopId == 0L)
		{
			return MlmLocManager.Get("transfer.import.row.unavailable");
		}
		if (IsRecognized(mod))
		{
			return MlmLocManager.Get("transfer.import.row.installed");
		}
		return job?.State switch
		{
			WorkshopDownloadState.Queued => MlmLocManager.Get("transfer.import.row.queued"), 
			WorkshopDownloadState.Downloading => MlmLocManager.Get("transfer.import.row.downloading", ("progress", (int)Math.Round(job.Progress * 100f))), 
			WorkshopDownloadState.Complete => MlmLocManager.Get("transfer.import.row.waiting"), 
			WorkshopDownloadState.Failed => MlmLocManager.Get("transfer.import.row.failed"), 
			_ => MlmLocManager.Get("transfer.import.row.notDownloaded"), 
		};
	}

	private static string MissingModStateStyle(PendingMissingMod mod, WorkshopDownloadJob? job)
	{
		if (job != null && job.Failure == WorkshopDownloadFailure.Cancelled)
		{
			return "DimLabel";
		}
		if (mod.WorkshopId == 0L || (job != null && job.State == WorkshopDownloadState.Failed))
		{
			return "WarningLabel";
		}
		if (!IsRecognized(mod))
		{
			return "DimLabel";
		}
		return "SuccessLabel";
	}

	private static void OpenWorkshop(PendingMissingMod mod)
	{
		if (mod.WorkshopId == 0L)
		{
			return;
		}
		string text = $"https://steamcommunity.com/sharedfiles/filedetails/?id={mod.WorkshopId}";
		try
		{
			PageOpenResult pageOpenResult = SteamOverlay.OpenPage(text);
			SetStatus(pageOpenResult switch
			{
				PageOpenResult.InGame => MlmLocManager.Get("workshop.feedback.pageOpened", ("name", mod.DisplayName)), 
				PageOpenResult.InSteam => MlmLocManager.Get("workshop.feedback.pageOpenedInSteam", ("name", mod.DisplayName)), 
				PageOpenResult.InBrowser => MlmLocManager.Get("workshop.feedback.pageOpenedInBrowser", ("name", mod.DisplayName)), 
				_ => MlmLocManager.Get("workshop.feedback.browserOpenFailed", ("url", text)), 
			}, pageOpenResult != PageOpenResult.Failed);
		}
		catch (Exception ex)
		{
			SetStatus(MlmLocManager.Get("workshop.feedback.pageOpenFailed", ("error", ex.Message)), false);
		}
	}

	private static void DownloadOne(PendingMissingMod mod)
	{
		if (_pendingProfile != null && mod.WorkshopId != 0L && WorkshopUpdates.IsAvailable)
		{
			WorkshopDownloadJob workshopDownloadJob = DownloadQueue.Find(mod.WorkshopId);
			bool subscribe = !HasWorkshopCopy(mod.WorkshopId);
			if (workshopDownloadJob == null)
			{
				DownloadQueue.Enqueue(mod.WorkshopId, mod.DisplayName, subscribe);
			}
			else if (workshopDownloadJob.State == WorkshopDownloadState.Failed)
			{
				DownloadQueue.Retry(mod.WorkshopId, subscribe);
			}
			_pendingApplyError = null;
			RenderPendingImport();
		}
	}

	private static void DownloadAll()
	{
		if (_pendingProfile == null || !WorkshopUpdates.IsAvailable)
		{
			return;
		}
		foreach (PendingMissingMod item in PendingMissingMods.Where((PendingMissingMod mod) => mod.WorkshopId != 0))
		{
			if (DownloadQueue.Find(item.WorkshopId) == null)
			{
				DownloadQueue.Enqueue(item.WorkshopId, item.DisplayName, !HasWorkshopCopy(item.WorkshopId));
			}
		}
		DownloadQueue.RetryFailed((WorkshopDownloadJob job) => !HasWorkshopCopy(job.ItemId));
		_pendingApplyError = null;
		RenderPendingImport();
	}

	private static bool HasWorkshopCopy(ulong itemId)
	{
		return _plan?.Entries.Any((LaunchEntry entry) => WorkshopUpdates.ItemIdFor(entry.Mod) == itemId) ?? false;
	}

	private static void ApplyPending()
	{
		if (_pendingProfile != null && _apply != null && DownloadQueue.ActiveCount <= 0)
		{
			PlanResult planResult = _apply(_pendingProfile);
			if (planResult.Success)
			{
				ClearPending();
				SetStatus(planResult.Message, true);
			}
			else
			{
				_pendingApplyError = planResult.Message;
				RenderPendingImport();
			}
		}
	}

	private static void CancelPending()
	{
		ClearPending();
		SetStatus(MlmLocManager.Get("transfer.import.cancelled"), null);
	}

	private static void ClearPending()
	{
		_pendingProfile = null;
		_pendingApplyError = null;
		_missingListSignature = string.Empty;
		_pendingLocalOnly = 0;
		PendingMissingMods.Clear();
		_pendingWorkshopUnavailable = 0;
		DownloadQueue.Clear();
		if (_status != null)
		{
			((Control)_status).TooltipText = string.Empty;
		}
		UpdateBusyState();
		RenderPendingImport();
	}

	private static void SetBusy(bool busy)
	{
		_busy = busy;
		UpdateBusyState();
	}

	private static void UpdateBusyState()
	{
		bool flag = _busy || _pendingProfile != null;
		if (_exportFile != null)
		{
			((BaseButton)_exportFile).Disabled = flag;
		}
		if (_exportLink != null)
		{
			((BaseButton)_exportLink).Disabled = flag;
		}
		if (_importFile != null)
		{
			((BaseButton)_importFile).Disabled = flag;
		}
		if (_importSource != null)
		{
			((BaseButton)_importSource).Disabled = flag;
		}
		if (_source != null)
		{
			_source.Editable = !flag;
		}
	}

	private static void SetStatus(string message, bool? success)
	{
		if (_status != null && GodotObject.IsInstanceValid((GodotObject)(object)_status))
		{
			_status.Text = message;
			((Control)_status).TooltipText = string.Empty;
			Label? status = _status;
			string text = ((!success.HasValue) ? "DimLabel" : ((success != true) ? "DangerLabel" : "SuccessLabel"));
			((Control)status).ThemeTypeVariation = StringName.op_Implicit(text);
		}
	}

	private static string EnsureJsonExtension(string path)
	{
		if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
		{
			return path + ".json";
		}
		return path;
	}
}
