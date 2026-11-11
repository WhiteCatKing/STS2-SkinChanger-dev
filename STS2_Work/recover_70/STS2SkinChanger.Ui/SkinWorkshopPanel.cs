using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class SkinWorkshopPanel : Control
{
	private sealed record HoverItem(ulong Id, Control Slot, PanelContainer Panel);

	private sealed record ItemVisual(WorkshopItemBinding Binding, WorkshopItemTicket Ticket, Label Title, TextureRect Cover, Panel Placeholder);

	private sealed class Marquee(Control row, Control clip, Label label)
	{
		public readonly Control Row = row;

		public readonly Control Clip = clip;

		public readonly Label Label = label;

		public ulong Started;
	}

	private sealed class RowView(WorkshopItemBinding binding, Control slot, PanelContainer panel)
	{
		public readonly WorkshopItemBinding Binding = binding;

		public readonly Control Slot = slot;

		public readonly PanelContainer Panel = panel;

		public Label Title;

		public Label Status;

		public Label Metric;

		public TextureRect Cover;

		public Panel Placeholder;

		public Button Action;

		public Button Cancel;

		public HFlowContainer Tags;

		public ulong WorkshopId;

		public Action? PrimaryAction;

		public Action? SecondaryAction;

		public readonly List<TagView> TagSlots = new List<TagView>();

		public ItemVisual Snapshot()
		{
			return new ItemVisual(binding, binding.Capture(), Title, Cover, Placeholder);
		}
	}

	private sealed class TagView
	{
		public Button Button;

		public Action? Select;
	}

	private bool _restartPending;

	private readonly Dictionary<ulong, string> _actionErrors = new Dictionary<ulong, string>();

	private const int PageSize = 8;

	private bool _suspended;

	private WorkshopSubmissionPanel? _submission;

	private GitHubScanPanel? _scan;

	private Button _communityRefresh;

	private Button _submissionButton;

	private int _communityRevision = -1;

	private static readonly string[] Kinds = new string[8] { "", "character", "cards", "monster", "ancient", "merchant", "companion", "event" };

	private string _kind = "";

	private string _target = "";

	private int _page;

	private int _generation;

	private Window? _window;

	private CancellationTokenSource? _pageToken;

	private Action? _refresh;

	private Node _origin;

	private bool _originConnected;

	private CanvasLayer _layer;

	private GridContainer _rows;

	private Label _pageLabel;

	private Button _previous;

	private Button _next;

	private ScrollContainer _listScroll;

	private readonly Dictionary<ulong, Label> _metrics = new Dictionary<ulong, Label>();

	private readonly List<(ulong Id, Label Status, Button Action, Button Cancel)> _actions = new List<(ulong, Label, Button, Button)>();

	private bool _closed;

	private WorkshopTargetNames _names;

	private string _region = "";

	private WorkshopPagedChoice _typePicker;

	private WorkshopPagedChoice _regionPicker;

	private WorkshopPagedChoice _targetPicker;

	private WorkshopPagedChoice _loadPicker;

	private WorkshopPagedChoice _subscriptionPicker;

	private readonly WorkshopSubscriptionFilter _subscriptions = new WorkshopSubscriptionFilter();

	private string _loadFilter = "";

	private ulong[] _filteredIds = Array.Empty<ulong>();

	private bool _filterRebuildPending;

	private Label? _emptyLabel;

	private readonly Dictionary<ulong, Button> _loadTags = new Dictionary<ulong, Button>();

	private WorkshopSort _sort;

	private OptionButton _sortPicker;

	private bool _reverseSort;

	private OptionButton _sortDirectionPicker;

	private readonly List<HoverItem> _hoverItems = new List<HoverItem>();

	private HoverItem? _lifted;

	private readonly WorkshopHoverDwell _introDwell = new WorkshopHoverDwell();

	private Control _intro;

	private Label _introTitle;

	private Label _introDescription;

	private Control _introDescriptionHost;

	private Label _introImageStatus;

	private Label _introPage;

	private TextureRect _introImage;

	private Control _introImageHost;

	private SceneTree? _hoverTree;

	private WorkshopCarousel<Texture2D>? _carousel;

	private readonly Dictionary<HoverItem, Tween> _returnTweens = new Dictionary<HoverItem, Tween>();

	private CancellationTokenSource? _introToken;

	private int _introGeneration;

	private int _hoverPopups;

	private bool _hoverKeyboard;

	private int _hoverChanges;

	private double _hoverTotalMs;

	private double _hoverMaxMs;

	private readonly List<Marquee> _marquees = new List<Marquee>();

	private static readonly Lazy<Shader> CoverShader = new Lazy<Shader>((Func<Shader>)(() => new Shader
	{
		Code = "shader_type canvas_item;\nrender_mode unshaded;\nuniform vec2 surface_size = vec2(1.0);\nuniform float corner_radius = 0.0;\nvoid fragment() {\n    vec2 half_size = surface_size * 0.5;\n    float r = min(corner_radius, min(half_size.x, half_size.y));\n    vec2 q = abs(UV * surface_size - half_size) - half_size + vec2(r);\n    float d = length(max(q, vec2(0.0))) + min(max(q.x, q.y), 0.0) - r;\n    COLOR.a *= 1.0 - smoothstep(-max(fwidth(d), 0.5), max(fwidth(d), 0.5), d);\n}"
	}));

	private WorkshopPagePool<RowView>? _pageRows;

	private int _pageBuilds;

	private double _pageBuildTotalMs;

	private double _pageBuildMaxMs;

	private string _source = "steam";

	private string _installFilter = "missing";

	private OptionButton _sourcePicker;

	private WorkshopPagedChoice _installPicker;

	private OptionButton _githubSortPicker;

	private Button _githubRefresh;

	private GitHubSort _githubSort = GitHubSort.Stars;

	private int _githubRevision = -1;

	private string EmptyText
	{
		get
		{
			if (!GitHubMode)
			{
				return WorkshopText.Get((_subscriptions.Value.Length > 0 && _subscriptions.Unavailable) ? WorkshopTextKey.Offline : WorkshopTextKey.Empty);
			}
			return GitHubText(GitHubSkinTextKey.Empty);
		}
	}

	private bool CodeErrors
	{
		get
		{
			if (!GitHubMode)
			{
				return _subscriptions.Value == "code_errors";
			}
			return false;
		}
	}

	private IEnumerable<ulong> SteamCatalogIds => from item in SkinWorkshopService.Catalog
		where SkinSourceRoutingPolicy.IsSteamItem(item.Id)
		select item.Id;

	private IReadOnlySet<string>? RegionMembers
	{
		get
		{
			object obj;
			if (_region.Length != 0)
			{
				obj = _names.Regions(_kind).GetValueOrDefault(_region);
				if (obj == null)
				{
					return new HashSet<string>();
				}
			}
			else
			{
				obj = null;
			}
			return (IReadOnlySet<string>?)obj;
		}
	}

	private static bool InstantHover
	{
		get
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Invalid comparison between Unknown and I4
			SaveManager instance = SaveManager.Instance;
			if (instance == null)
			{
				return false;
			}
			PrefsSave prefsSave = instance.PrefsSave;
			return (int)((prefsSave != null) ? new FastModeType?(prefsSave.FastMode) : ((FastModeType?)null)).GetValueOrDefault() == 3;
		}
	}

	private bool GitHubMode => _source == "github";

	private void PerformPrimary(ulong id)
	{
		if (_closed || _restartPending)
		{
			return;
		}
		try
		{
			_actionErrors.Remove(id);
			if (SkinSourceIdentity.IsGitHub(id))
			{
				if (!GitHubSkinService.IsInstalling(id) && !GitHubSkinService.IsScanning(id))
				{
					PerformGitHub(id);
				}
				return;
			}
			WorkshopDownload workshopDownload = SkinWorkshopService.DownloadState(id);
			if (workshopDownload == null || !workshopDownload.Busy)
			{
				WorkshopTextKey? workshopTextKey = WorkshopItemActions.Primary(SkinWorkshopService.IsSubscribed(id), SkinWorkshopService.IsActive(id), SkinWorkshopService.IsInstalled(id), SkinWorkshopService.Catalog.First((WorkshopCatalogItem i) => i.Id == id).RestartRequired, workshopDownload?.State, busy: false);
				if (workshopTextKey == WorkshopTextKey.Restart)
				{
					RestartGame(id, "工坊皮肤");
				}
				else if (workshopTextKey.HasValue)
				{
					Subscribe(id);
				}
			}
		}
		catch (Exception ex)
		{
			ShowActionError(id, ex);
		}
	}

	private void RestartGame(ulong id, string what)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (_restartPending)
		{
			return;
		}
		_restartPending = true;
		Poll();
		Callable val = Callable.From((Action)delegate
		{
			try
			{
				NGame instance = NGame.Instance;
				if (!GodotObject.IsInstanceValid((GodotObject)(object)instance))
				{
					throw new InvalidOperationException("Game is no longer available.");
				}
				OS.SetRestartOnExit(true, (string[])null);
				ModLog.Info($"玩家点击{what} {id} 的“需要重启”，按游戏正常退出流程重启。");
				instance.Quit();
			}
			catch (Exception ex)
			{
				OS.SetRestartOnExit(false, (string[])null);
				_restartPending = false;
				ShowActionError(id, ex);
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private void OpenItem(ulong id)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (_closed)
		{
			return;
		}
		if (SkinSourceIdentity.IsGitHub(id))
		{
			try
			{
				_actionErrors.Remove(id);
				string text = GitHubSkinService.RepoUrl(id);
				if (text.Length > 0)
				{
					OS.ShellOpen(text);
				}
				return;
			}
			catch (Exception ex)
			{
				ShowActionError(id, ex);
				return;
			}
		}
		if (!SkinWorkshopService.CanReadMetadata(id))
		{
			return;
		}
		try
		{
			_actionErrors.Remove(id);
			WorkshopCommunityLinks.OpenItem(id);
		}
		catch (Exception ex2)
		{
			ShowActionError(id, ex2);
		}
	}

	private static GitHubItemAction PrimaryAction(ulong id)
	{
		return GitHubItemActions.Primary(GitHubSkinService.IsRecognized(id), GitHubSkinService.InstallState(id), GitHubSkinService.IsPendingRestart(id));
	}

	private void PerformGitHub(ulong id)
	{
		switch (PrimaryAction(id))
		{
		case GitHubItemAction.Install:
		case GitHubItemAction.Update:
			InstallGitHub(id);
			break;
		case GitHubItemAction.Restart:
			RestartGame(id, "GitHub 皮肤");
			break;
		case GitHubItemAction.Uninstall:
			UninstallGitHub(id);
			break;
		default:
			ScanGitHub(id);
			break;
		}
	}

	private async Task ScanGitHub(ulong id)
	{
		Timer polling = BusyPolling();
		try
		{
			using CancellationTokenSource token = new CancellationTokenSource(TimeSpan.FromMinutes(30L));
			GitHubScanResult result = await GitHubSkinService.ScanAsync(id, token.Token);
			if (_closed || !GodotObject.IsInstanceValid((GodotObject)(object)this) || !((Node)this).IsInsideTree())
			{
				return;
			}
			OpenScanResult(id, result);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex2)
		{
			ShowActionError(id, ex2);
		}
		finally
		{
			((Node)polling).QueueFree();
			if (!_closed && GodotObject.IsInstanceValid((GodotObject)(object)this) && ((Node)this).IsInsideTree())
			{
				Poll();
			}
		}
	}

	private Timer BusyPolling()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		Timer timer = new Timer
		{
			WaitTime = 0.25,
			ProcessMode = (ProcessModeEnum)3
		};
		timer.Timeout += delegate
		{
			if (_closed || !GodotObject.IsInstanceValid((GodotObject)(object)this) || !((Node)this).IsInsideTree())
			{
				((Node)timer).QueueFree();
			}
			else
			{
				Poll();
			}
		};
		((Node)this).AddChild((Node)(object)timer, false, (InternalMode)0);
		timer.Start(-1.0);
		return timer;
	}

	private void OpenScanResult(ulong id, GitHubScanResult result)
	{
		try
		{
			if (_scan != null)
			{
				return;
			}
			ResetHover();
			GitHubSkinEntry gitHubSkinEntry = GitHubSkinService.Entry(id);
			WorkshopCatalogItem item = result.Item;
			string details = (((object)item != null) ? string.Join(" · ", item.Targets.Select((WorkshopTarget target) => _names.Name(target.Kind, target.Target)).Distinct()) : "");
			_scan = GitHubScanPanel.Show((Node)(object)this, gitHubSkinEntry?.Name ?? "", details, result, delegate
			{
				_scan = null;
				if (!_closed)
				{
					Poll();
				}
			});
			UpdateVisibleActions();
		}
		catch (Exception ex)
		{
			ShowActionError(id, ex);
		}
	}

	private void PerformGitHubSecondary(ulong id)
	{
		if (!_closed && !_restartPending && !GitHubSkinService.IsInstalling(id) && GitHubItemActions.ShowsUninstall(PrimaryAction(id)))
		{
			_actionErrors.Remove(id);
			UninstallGitHub(id);
		}
	}

	private async Task UninstallGitHub(ulong id)
	{
		try
		{
			await GitHubSkinService.UninstallAsync(id);
		}
		catch (Exception ex)
		{
			ShowActionError(id, ex);
		}
		finally
		{
			if (!_closed && GodotObject.IsInstanceValid((GodotObject)(object)this) && ((Node)this).IsInsideTree())
			{
				Poll();
			}
		}
	}

	private async Task InstallGitHub(ulong id)
	{
		Timer polling = BusyPolling();
		try
		{
			using CancellationTokenSource token = new CancellationTokenSource(TimeSpan.FromMinutes(30L));
			await GitHubSkinService.InstallAsync(id, token.Token);
		}
		catch (Exception ex)
		{
			ShowActionError(id, ex);
		}
		finally
		{
			((Node)polling).QueueFree();
			if (!_closed && GodotObject.IsInstanceValid((GodotObject)(object)this) && ((Node)this).IsInsideTree())
			{
				Poll();
			}
		}
	}

	private void OpenSubmission(Button button)
	{
		try
		{
			if (_submission != null)
			{
				return;
			}
			ResetHover();
			_submission = WorkshopSubmissionPanel.Show((Node)(object)this, delegate
			{
				_submission = null;
				if (!_closed)
				{
					Poll();
				}
			});
			UpdateVisibleActions();
			button.Text = WorkshopCommunityText.Get(WorkshopCommunityTextKey.SubmitMod);
			((Control)button).TooltipText = "";
		}
		catch (Exception ex)
		{
			button.Text = WorkshopCommunityText.Get(WorkshopCommunityTextKey.OpenFailed);
			((Control)button).TooltipText = ex.GetBaseException().Message;
			ModLog.Warn("打开模组投稿区失败：" + ex.GetBaseException().Message);
		}
	}

	private void ShowActionError(ulong id, Exception ex)
	{
		string text;
		if (SkinSourceIdentity.IsGitHub(id))
		{
			GitHubSkinEntry gitHubSkinEntry = GitHubSkinService.Entry(id);
			if ((object)gitHubSkinEntry != null)
			{
				text = gitHubSkinEntry.FullName;
				goto IL_003c;
			}
		}
		text = id.ToString();
		goto IL_003c;
		IL_003c:
		string text2 = text;
		ModLog.Warn("工坊皮肤 " + text2 + " 操作失败：" + ex.GetBaseException().Message);
		if (!_closed && GodotObject.IsInstanceValid((GodotObject)(object)this))
		{
			_actionErrors[id] = ex.GetBaseException().Message;
			(ulong, Label, Button, Button) tuple = _actions.FirstOrDefault<(ulong, Label, Button, Button)>(((ulong Id, Label Status, Button Action, Button Cancel) item) => item.Id == id);
			if (GodotObject.IsInstanceValid((GodotObject)(object)tuple.Item2))
			{
				tuple.Item2.Text = WorkshopText.Get(WorkshopTextKey.Failed);
				((Control)tuple.Item2).TooltipText = ex.GetBaseException().Message;
			}
		}
	}

	private void RebuildCodeErrors(int generation, CancellationToken token)
	{
		WorkshopCodeProblem[] array = WorkshopCodeDiagnostics.Group(SkinWorkshopService.CommunityIssues);
		int num = Math.Max(1, (array.Length + 8 - 1) / 8);
		_page = Math.Clamp(_page, 0, num - 1);
		_pageLabel.Text = $"{_page + 1} / {num}";
		((BaseButton)_previous).Disabled = _page == 0;
		((BaseButton)_next).Disabled = _page + 1 >= num;
		_filteredIds = Array.Empty<ulong>();
		_emptyLabel.Text = WorkshopCodeErrorText.Get(WorkshopCodeUi.Empty);
		((CanvasItem)_emptyLabel).Visible = array.Length == 0;
		WorkshopCodeProblem[] array2 = array.Skip(_page * 8).Take(8).ToArray();
		PrepareRows((from i in Enumerable.Range(1, array2.Length)
			select (ulong)i).ToArray());
		Dictionary<ulong, ItemVisual> dictionary = new Dictionary<ulong, ItemVisual>();
		for (int j = 0; j < array2.Length; j++)
		{
			RowView view = _pageRows.Slots[j].View;
			WorkshopCodeProblem problem = array2[j];
			view.WorkshopId = problem.Id;
			view.Title.Text = SkinWorkshopService.CachedDetails(problem.Id)?.Title ?? ((problem.Name.Length > 0) ? problem.Name : ((problem.Id != 0) ? "…" : WorkshopCodeErrorText.Get(WorkshopCodeUi.Unknown)));
			((CanvasItem)view.Tags).Hide();
			((CanvasItem)view.Metric).Hide();
			view.Status.Text = string.Join(" · ", problem.Issues.Select((WorkshopCodeIssue issue) => issue.Error).Distinct().Select(WorkshopCodeErrorText.Error));
			((Control)view.Status).TooltipText = view.Status.Text;
			view.Action.Text = WorkshopCodeErrorText.Get(WorkshopCodeUi.CopyDetails);
			view.Cancel.Text = WorkshopCodeErrorText.Get(WorkshopCodeUi.Locate);
			((CanvasItem)view.Action).Show();
			((CanvasItem)view.Cancel).Show();
			((BaseButton)view.Action).Disabled = false;
			WorkshopCodeSource source = problem.Sources.FirstOrDefault((WorkshopCodeSource s) => WorkshopCodeDiagnostics.SafeSource(s.Url));
			((BaseButton)view.Cancel).Disabled = source == null;
			view.PrimaryAction = delegate
			{
				try
				{
					DisplayServer.ClipboardSet(WorkshopCodeDiagnostics.Report(problem, ModLocalization.CurrentLanguage));
					view.Action.Text = WorkshopSubmissionText.Get(SubmissionText.Copied);
				}
				catch (Exception ex)
				{
					((Control)view.Status).TooltipText = ex.Message;
					ModLog.Warn("复制投稿错误失败：" + ex.Message);
				}
			};
			view.SecondaryAction = delegate
			{
				if (source == null)
				{
					return;
				}
				try
				{
					WorkshopCommunityLinks.OpenSubmissionSource(source.Url);
				}
				catch (Exception ex2)
				{
					((Control)view.Status).TooltipText = ex2.Message;
					ModLog.Warn("投稿来源打开失败：" + ex2.Message);
				}
			};
			if (problem.Id != 0L)
			{
				dictionary[problem.Id] = view.Snapshot();
				_hoverItems.Add(new HoverItem(problem.Id, view.slot, view.panel));
			}
		}
		LoadDetails(dictionary.Keys.ToArray(), dictionary, generation, token);
	}

	internal static void Show(Node origin, string kind, string target, Action refresh, string region = "")
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		SceneTree tree = origin.GetTree();
		Window val = ((tree != null) ? tree.Root : null);
		if (val == null || ((Node)val).GetNodeOrNull(NodePath.op_Implicit("SCSkinWorkshopLayer")) != null)
		{
			return;
		}
		CanvasLayer val2 = new CanvasLayer
		{
			Name = StringName.op_Implicit("SCSkinWorkshopLayer"),
			Layer = 100,
			ProcessMode = (ProcessModeEnum)3
		};
		SkinWorkshopPanel skinWorkshopPanel = new SkinWorkshopPanel
		{
			_kind = kind,
			_target = target,
			_region = region,
			_origin = origin,
			_refresh = refresh,
			_layer = val2
		};
		try
		{
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val2).AddChild((Node)(object)skinWorkshopPanel, false, (InternalMode)0);
			((Control)skinWorkshopPanel).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
			skinWorkshopPanel.Initialize();
			ModLog.Info($"已打开皮肤工坊：类型={kind}，对象={target}，地区={skinWorkshopPanel._region}。");
		}
		catch (Exception value)
		{
			skinWorkshopPanel.Cleanup();
			Node parent = ((Node)val2).GetParent();
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)val2);
			}
			((Node)val2).QueueFree();
			ModLog.Error($"打开皮肤工坊失败：{value}");
		}
	}

	private void Initialize()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Expected O, but got Unknown
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Expected O, but got Unknown
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Expected O, but got Unknown
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Expected O, but got Unknown
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Expected O, but got Unknown
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Expected O, but got Unknown
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Expected O, but got Unknown
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Expected O, but got Unknown
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_046a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Expected O, but got Unknown
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).MouseFilter = (MouseFilterEnum)0;
		((Node)this).ProcessMode = (ProcessModeEnum)3;
		((Node)this).TreeExiting += Cleanup;
		_window = ((Node)this).GetWindow();
		_window.WindowInput += new WindowInputEventHandler(HandleInput);
		Timer val = new Timer
		{
			WaitTime = 0.5,
			Autostart = true,
			ProcessMode = (ProcessModeEnum)3
		};
		val.Timeout += Poll;
		((Node)this).AddChild((Node)(object)val, false, (InternalMode)0);
		Timer val2 = new Timer
		{
			WaitTime = 0.05,
			Autostart = true,
			ProcessMode = (ProcessModeEnum)3
		};
		val2.Timeout += AnimateTitles;
		((Node)this).AddChild((Node)(object)val2, false, (InternalMode)0);
		ColorRect mask = new ColorRect
		{
			Color = new Color(0f, 0f, 0f, 0.48f),
			MouseFilter = (MouseFilterEnum)0
		};
		((Node)this).AddChild((Node)(object)mask, false, (InternalMode)0);
		((Control)mask).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Control)mask).GuiInput += (GuiInputEventHandler)delegate(InputEvent ev)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I8
			InputEventMouseButton val3 = (InputEventMouseButton)(object)((ev is InputEventMouseButton) ? ev : null);
			if (val3 != null && val3.Pressed && (long)val3.ButtonIndex == 1)
			{
				((Control)mask).AcceptEvent();
				Close();
			}
		};
		PanelContainer val4 = new PanelContainer
		{
			AnchorLeft = 0.06f,
			AnchorRight = 0.94f,
			AnchorTop = 0.015f,
			AnchorBottom = 0.985f,
			MouseFilter = (MouseFilterEnum)0
		};
		((Node)this).AddChild((Node)(object)val4, false, (InternalMode)0);
		ModThemeRuntime.Panel(val4);
		MarginContainer val5 = new MarginContainer();
		string[] array = new string[4] { "left", "right", "top", "bottom" };
		foreach (string text in array)
		{
			((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("margin_" + text), 16);
		}
		((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		VBoxContainer val6 = new VBoxContainer();
		((Control)val6).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)val5).AddChild((Node)(object)val6, false, (InternalMode)0);
		HBoxContainer val7 = new HBoxContainer();
		((Control)val7).AddThemeConstantOverride(StringName.op_Implicit("separation"), 18);
		((Node)val6).AddChild((Node)(object)val7, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)Text(WorkshopText.EntryLabel, 27, accent: true), false, (InternalMode)0);
		Button submit = Button(WorkshopCommunityText.Get(WorkshopCommunityTextKey.SubmitMod), delegate
		{
		});
		((BaseButton)submit).Pressed += delegate
		{
			OpenSubmission(submit);
		};
		((Node)val7).AddChild((Node)(object)submit, false, (InternalMode)0);
		_submissionButton = submit;
		_communityRefresh = Button(WorkshopSubmissionText.Get(SubmissionText.Refresh), delegate
		{
			SkinWorkshopService.RefreshCommunity();
		});
		((Node)val7).AddChild((Node)(object)_communityRefresh, false, (InternalMode)0);
		_githubRefresh = Button(GitHubText(GitHubSkinTextKey.Refresh), GitHubSkinService.StartRefresh);
		((CanvasItem)_githubRefresh).Visible = false;
		((Node)val7).AddChild((Node)(object)_githubRefresh, false, (InternalMode)0);
		BuildSort(val7);
		BuildFilters(val6);
		ScrollContainer scroll = new ScrollContainer
		{
			SizeFlagsVertical = (SizeFlags)3,
			HorizontalScrollMode = (ScrollMode)0
		};
		_listScroll = scroll;
		((Node)val6).AddChild((Node)(object)scroll, false, (InternalMode)0);
		_rows = new GridContainer
		{
			Columns = 2,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_rows).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 18);
		((Control)_rows).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 12);
		((Node)scroll).AddChild((Node)(object)_rows, false, (InternalMode)0);
		HBoxContainer val8 = new HBoxContainer();
		((Node)val6).AddChild((Node)(object)val8, false, (InternalMode)0);
		((Node)val8).AddChild((Node)(object)Button(ModLocalization.Get(ModText.Close), Close), false, (InternalMode)0);
		Label val9 = Text(PanelVersionText(), 16);
		((CanvasItem)val9).Modulate = new Color(1f, 1f, 1f, 0.6f);
		((Control)val9).SizeFlagsVertical = (SizeFlags)4;
		((Node)val8).AddChild((Node)(object)val9, false, (InternalMode)0);
		((Node)val8).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		_previous = Button(WorkshopText.Get(WorkshopTextKey.Previous), delegate
		{
			_page--;
			scroll.ScrollVertical = 0;
			Rebuild();
		});
		((Node)val8).AddChild((Node)(object)_previous, false, (InternalMode)0);
		_pageLabel = Text("");
		((Control)_pageLabel).CustomMinimumSize = new Vector2(85f, 0f);
		_pageLabel.HorizontalAlignment = (HorizontalAlignment)1;
		((Node)val8).AddChild((Node)(object)_pageLabel, false, (InternalMode)0);
		_next = Button(WorkshopText.Get(WorkshopTextKey.Next), delegate
		{
			_page++;
			scroll.ScrollVertical = 0;
			Rebuild();
		});
		((Node)val8).AddChild((Node)(object)_next, false, (InternalMode)0);
		InitializeHover();
		_origin.TreeExited += Close;
		_originConnected = true;
		_communityRevision = SkinWorkshopService.CommunityRevision;
		_githubRevision = GitHubSkinService.Revision;
		Rebuild();
	}

	private void Rebuild()
	{
		long timestamp = Stopwatch.GetTimestamp();
		ResetHover();
		_pageToken?.Cancel();
		_pageToken?.Dispose();
		_pageToken = new CancellationTokenSource();
		CancellationToken token = _pageToken.Token;
		int generation = ++_generation;
		if (_emptyLabel == null)
		{
			_emptyLabel = Text(EmptyText);
			((Node)_rows).AddChild((Node)(object)_emptyLabel, false, (InternalMode)0);
		}
		if (CodeErrors)
		{
			RebuildCodeErrors(generation, token);
			UpdateVisibleActions();
			RecordPageBuild(timestamp);
			return;
		}
		_subscriptions.Refresh(SteamCatalogIds, SkinWorkshopService.IsSubscribed);
		WorkshopCatalogItem[] array = FilteredItems();
		_filteredIds = array.Select((WorkshopCatalogItem item) => item.Id).ToArray();
		int num = Math.Max(1, (array.Length + 8 - 1) / 8);
		_page = Math.Clamp(_page, 0, num - 1);
		((BaseButton)_previous).Disabled = _page == 0;
		((BaseButton)_next).Disabled = _page + 1 >= num;
		_pageLabel.Text = $"{_page + 1} / {num}";
		_emptyLabel.Text = EmptyText;
		((CanvasItem)_emptyLabel).Visible = array.Length == 0;
		WorkshopCatalogItem[] array2 = array.Skip(_page * 8).Take(8).ToArray();
		Dictionary<ulong, ItemVisual> rows = BindRows(array2);
		UpdateVisibleActions();
		LoadDetails(array2.Select((WorkshopCatalogItem i) => i.Id).ToArray(), rows, generation, token);
		RecordPageBuild(timestamp);
	}

	private async Task LoadDetails(ulong[] ids, Dictionary<ulong, ItemVisual> rows, int generation, CancellationToken token)
	{
		_ = 2;
		try
		{
			List<Task> covers = new List<Task>();
			foreach (ulong item in ids.Where(SkinSourceIdentity.IsGitHub))
			{
				if (rows.TryGetValue(item, out ItemVisual value))
				{
					covers.Add(LoadCover(GitHubSkinService.CoverUrl(item), value, generation, token));
				}
			}
			ulong[] array = ids.Where(SkinSourceRoutingPolicy.IsSteamItem).ToArray();
			if (array.Length == 0)
			{
				await Task.WhenAll(covers);
				return;
			}
			Dictionary<ulong, WorkshopDetails> dictionary = await SkinWorkshopService.Query(array, token);
			if (token.IsCancellationRequested || generation != _generation || !GodotObject.IsInstanceValid((GodotObject)(object)this))
			{
				return;
			}
			foreach (var (num2, workshopDetails2) in dictionary)
			{
				if (rows.TryGetValue(num2, out ItemVisual value2) && value2.Binding.Matches(value2.Ticket))
				{
					value2.Title.Text = workshopDetails2.Title;
					if (_metrics.TryGetValue(num2, out Label value3))
					{
						value3.Text = ItemMetric(num2);
					}
					covers.Add(LoadCover(workshopDetails2.PreviewUrl, value2, generation, token));
				}
			}
			await Task.WhenAll(covers);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex2)
		{
			ModLog.Info("工坊列表详情加载失败：" + ex2.Message);
		}
	}

	private async Task LoadCover(string url, ItemVisual row, int generation, CancellationToken token)
	{
		Texture2D val = await WorkshopCoverCache.Get(url, token);
		if (val != null && !token.IsCancellationRequested && generation == _generation && row.Binding.Matches(row.Ticket) && GodotObject.IsInstanceValid((GodotObject)(object)row.Cover))
		{
			row.Cover.Texture = val;
			((CanvasItem)row.Placeholder).Hide();
			RoundCover(row.Cover);
		}
	}

	private async Task Subscribe(ulong id)
	{
		if (_closed || (SkinWorkshopService.DownloadState(id)?.Busy ?? false))
		{
			return;
		}
		_subscriptions.BeginAction(id);
		try
		{
			await SkinWorkshopService.Subscribe(id);
		}
		finally
		{
			_subscriptions.EndAction(id);
			if (GodotObject.IsInstanceValid((GodotObject)(object)this) && ((Node)this).IsInsideTree())
			{
				Poll();
			}
		}
	}

	private async Task Unsubscribe(ulong id)
	{
		if (_closed || (SkinWorkshopService.DownloadState(id)?.Busy ?? false))
		{
			return;
		}
		_actionErrors.Remove(id);
		_subscriptions.BeginAction(id);
		try
		{
			await SkinWorkshopService.Unsubscribe(id);
		}
		finally
		{
			_subscriptions.EndAction(id);
			if (GodotObject.IsInstanceValid((GodotObject)(object)this) && ((Node)this).IsInsideTree())
			{
				Poll();
			}
		}
	}

	private void Poll()
	{
		if (!_closed)
		{
			((BaseButton)_communityRefresh).Disabled = SkinWorkshopService.CommunityBusy;
			_communityRefresh.Text = WorkshopSubmissionText.Get(SkinWorkshopService.CommunityBusy ? SubmissionText.Refreshing : SubmissionText.Refresh);
			((Control)_communityRefresh).TooltipText = (SkinWorkshopService.CommunityFailed ? WorkshopSubmissionText.Get(SubmissionText.RefreshFailed) : SkinWorkshopService.CommunityProgress);
			if (_communityRevision != SkinWorkshopService.CommunityRevision)
			{
				_communityRevision = SkinWorkshopService.CommunityRevision;
				_names = WorkshopTargetNames.Build();
				RefreshFilters();
				Rebuild();
			}
			UpdateVisibleActions();
			PollFilters();
			PollGitHub();
		}
	}

	private void UpdateVisibleActions()
	{
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		if (_closed)
		{
			return;
		}
		int suspended;
		if (_submission == null && _scan == null)
		{
			NModalContainer instance = NModalContainer.Instance;
			suspended = ((((instance != null) ? instance.OpenModal : null) != null) ? 1 : 0);
		}
		else
		{
			suspended = 1;
		}
		_suspended = (byte)suspended != 0;
		if (_suspended)
		{
			ResetHover();
		}
		_layer.Visible = !_suspended;
		foreach (var action in _actions)
		{
			var (id, val, val2, val3) = action;
			try
			{
				bool flag3;
				bool flag4;
				string tooltipText;
				if (SkinSourceIdentity.IsGitHub(id))
				{
					GitHubSkinEntry gitHubSkinEntry = GitHubSkinService.Entry(id);
					GitHubInstallState state = GitHubSkinService.InstallState(id);
					bool flag = GitHubSkinService.IsInstalling(id);
					bool flag2 = GitHubSkinService.IsScanning(id);
					flag3 = GitHubSkinService.IsRecognized(id);
					GitHubItemAction gitHubItemAction = GitHubItemActions.Primary(flag3, state, GitHubSkinService.IsPendingRestart(id));
					flag4 = gitHubItemAction == GitHubItemAction.Restart;
					((CanvasItem)val2).Visible = true;
					((BaseButton)val2).Disabled = flag || flag2 || _restartPending;
					Button val4 = val2;
					string text;
					if (flag2)
					{
						text = GitHubText(GitHubSkinTextKey.Scanning);
					}
					else
					{
						string text2;
						if (flag)
						{
							text2 = GitHubText(GitHubSkinTextKey.Installing);
						}
						else
						{
							string text3 = ((!flag4) ? GitHubText(gitHubItemAction switch
							{
								GitHubItemAction.Install => GitHubSkinTextKey.Install, 
								GitHubItemAction.Update => GitHubSkinTextKey.Update, 
								GitHubItemAction.Scan => GitHubSkinTextKey.Scan, 
								_ => GitHubSkinTextKey.Uninstall, 
							}) : WorkshopText.Get(WorkshopTextKey.Restart));
							text2 = text3;
						}
						text = text2;
					}
					val4.Text = text;
					((Control)val2).TooltipText = ((flag2 || flag) ? val2.Text : ((gitHubItemAction == GitHubItemAction.Scan) ? GitHubText(GitHubSkinTextKey.ScanHint) : (flag4 ? WorkshopNoticeText.Get(WorkshopNoticeKey.KnownRestart) : (gitHubSkinEntry?.Error ?? ""))));
					((CanvasItem)val3).Visible = GitHubItemActions.ShowsUninstall(gitHubItemAction);
					((BaseButton)val3).Disabled = flag || _restartPending;
					val3.Text = GitHubText(GitHubSkinTextKey.Uninstall);
					((Control)val3).TooltipText = val3.Text;
					val.Text = (flag2 ? GitHubText(GitHubSkinTextKey.Scanning) : (flag ? GitHubText(GitHubSkinTextKey.Installing) : GitHubStatus(gitHubSkinEntry, state, flag4, GitHubSkinService.IsPendingRemoval(id))));
					if ((object)gitHubSkinEntry != null)
					{
						text = gitHubSkinEntry.Error;
						if (text != null && text.Length > 0)
						{
							tooltipText = gitHubSkinEntry.Error;
							goto IL_0283;
						}
					}
					tooltipText = val.Text;
					goto IL_0283;
				}
				WorkshopDownload workshopDownload = SkinWorkshopService.DownloadState(id);
				bool flag5 = SkinWorkshopService.IsActive(id);
				bool flag6 = SkinWorkshopService.IsSubscribed(id);
				bool restartRequired = SkinWorkshopService.Catalog.First((WorkshopCatalogItem i) => i.Id == id).RestartRequired;
				WorkshopTextKey? workshopTextKey = WorkshopItemActions.Primary(flag6, flag5, SkinWorkshopService.IsInstalled(id), restartRequired, workshopDownload?.State, workshopDownload?.Busy ?? false);
				((CanvasItem)val2).Visible = workshopTextKey.HasValue;
				((BaseButton)val2).Disabled = _restartPending || (workshopDownload?.Busy ?? false);
				object text4;
				if (workshopTextKey.HasValue)
				{
					WorkshopTextKey valueOrDefault = workshopTextKey.GetValueOrDefault();
					text4 = WorkshopText.Get(valueOrDefault);
				}
				else
				{
					text4 = "";
				}
				val2.Text = (string)text4;
				((Control)val2).TooltipText = ((workshopTextKey == WorkshopTextKey.Restart && workshopDownload != null && workshopDownload.Reason != 0) ? WorkshopNoticeText.Reason(workshopDownload.Reason) : ((!flag5 && restartRequired) ? WorkshopNoticeText.Get(WorkshopNoticeKey.KnownRestart) : ""));
				((CanvasItem)val3).Visible = flag6;
				((BaseButton)val3).Disabled = _restartPending || (workshopDownload?.Busy ?? false);
				((Control)val3).TooltipText = WorkshopBrowserText.Get(WorkshopBrowserTextKey.Retained);
				bool flag7 = workshopTextKey == WorkshopTextKey.Restart;
				WorkshopTextKey? workshopTextKey2 = WorkshopItemActions.Status(workshopDownload?.State);
				object text5;
				if (workshopTextKey2.HasValue)
				{
					WorkshopTextKey valueOrDefault2 = workshopTextKey2.GetValueOrDefault();
					text5 = WorkshopText.Get(valueOrDefault2);
				}
				else
				{
					text5 = "";
				}
				val.Text = (string)text5;
				Variant val5;
				if (((GodotObject)val2).HasMeta(StringName.op_Implicit("sc_restart_accent")))
				{
					StringName obj = StringName.op_Implicit("sc_restart_accent");
					val5 = default(Variant);
					val5 = ((GodotObject)val2).GetMeta(obj, val5);
					if (((Variant)(ref val5)).AsBool() == flag7)
					{
						goto IL_051e;
					}
				}
				ModThemeRuntime.TextControl((Control)(object)val2, 20, flag7);
				((GodotObject)val2).SetMeta(StringName.op_Implicit("sc_restart_accent"), Variant.op_Implicit(flag7));
				goto IL_051e;
				IL_051e:
				((Control)val).TooltipText = ((workshopDownload != null && workshopDownload.Reason != 0) ? WorkshopNoticeText.Reason(workshopDownload.Reason) : (workshopDownload?.Error ?? ""));
				if (workshopDownload != null && workshopDownload.Unsubscribed)
				{
					val.Text = WorkshopCommunityText.Get(WorkshopCommunityTextKey.Removed);
					((Control)val).TooltipText = val.Text;
				}
				if (workshopDownload != null && workshopDownload.Removing)
				{
					val.Text = WorkshopBrowserText.Get(WorkshopBrowserTextKey.Unsubscribing);
				}
				if (workshopDownload != null && !workshopDownload.Busy)
				{
					string text = workshopDownload.Error;
					if (text != null && text.Length > 0)
					{
						val.Text = WorkshopText.Get(WorkshopTextKey.Failed);
						((Control)val).TooltipText = workshopDownload.Error;
					}
				}
				if (workshopDownload != null && workshopDownload.Busy && !workshopDownload.Removing && workshopDownload.State == WorkshopTextKey.Waiting)
				{
					double? num = SkinWorkshopService.Progress(id);
					if (num.HasValue)
					{
						double valueOrDefault3 = num.GetValueOrDefault();
						val.Text = WorkshopText.Get(WorkshopTextKey.Download) + $" {valueOrDefault3:F0}%";
					}
				}
				if (_actionErrors.TryGetValue(id, out string value))
				{
					val.Text = WorkshopText.Get(WorkshopTextKey.Failed);
					((Control)val).TooltipText = value;
				}
				goto end_IL_009e;
				IL_0283:
				((Control)val).TooltipText = tooltipText;
				if (_actionErrors.TryGetValue(id, out string value2))
				{
					val.Text = GitHubText(flag3 ? GitHubSkinTextKey.InstallFailed : GitHubSkinTextKey.ScanFailed);
					((Control)val).TooltipText = value2;
				}
				if (!((GodotObject)val2).HasMeta(StringName.op_Implicit("sc_restart_accent")))
				{
					goto IL_02f3;
				}
				StringName obj2 = StringName.op_Implicit("sc_restart_accent");
				val5 = default(Variant);
				val5 = ((GodotObject)val2).GetMeta(obj2, val5);
				if (((Variant)(ref val5)).AsBool() != flag4)
				{
					goto IL_02f3;
				}
				goto end_IL_009e;
				IL_02f3:
				ModThemeRuntime.TextControl((Control)(object)val2, 20, flag4);
				((GodotObject)val2).SetMeta(StringName.op_Implicit("sc_restart_accent"), Variant.op_Implicit(flag4));
				end_IL_009e:;
			}
			catch
			{
				((BaseButton)val2).Disabled = true;
				((BaseButton)val3).Disabled = true;
				val.Text = WorkshopText.Get(WorkshopTextKey.Offline);
			}
		}
	}

	private void HandleInput(InputEvent ev)
	{
		if (_submission != null || _scan != null)
		{
			return;
		}
		if (ev is InputEventMouse)
		{
			_hoverKeyboard = false;
			goto IL_0058;
		}
		InputEventKey val = (InputEventKey)(object)((ev is InputEventKey) ? ev : null);
		if (val != null)
		{
			if (val.Pressed)
			{
				goto IL_0048;
			}
		}
		else
		{
			InputEventJoypadButton val2 = (InputEventJoypadButton)(object)((ev is InputEventJoypadButton) ? ev : null);
			if (val2 != null && val2.Pressed)
			{
				goto IL_0048;
			}
		}
		bool flag = false;
		goto IL_004e;
		IL_0058:
		if (ev is InputEventMouseMotion)
		{
			UpdateHover();
		}
		if (!_closed && !_suspended && ev.IsActionPressed(StringName.op_Implicit("ui_cancel"), false, false) && !ev.IsEcho())
		{
			((Node)this).GetViewport().SetInputAsHandled();
			Close();
		}
		return;
		IL_004e:
		if (flag)
		{
			_hoverKeyboard = true;
		}
		goto IL_0058;
		IL_0048:
		flag = true;
		goto IL_004e;
	}

	private void Close()
	{
		if (!_closed)
		{
			_closed = true;
			_submission?.Close();
			_submission = null;
			_scan?.Close();
			_scan = null;
			ResetHover();
			DisconnectOrigin();
			_pageToken?.Cancel();
			_layer.Hide();
			((Node)_layer).QueueFree();
			if (GodotObject.IsInstanceValid((GodotObject)(object)_origin) && _origin.IsInsideTree())
			{
				_refresh?.Invoke();
			}
		}
	}

	private void Cleanup()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		_closed = true;
		_submission?.Close();
		_submission = null;
		_scan?.Close();
		_scan = null;
		ResetHover();
		LogHoverTiming();
		LogPageBuildTiming();
		if (GodotObject.IsInstanceValid((GodotObject)(object)_hoverTree))
		{
			_hoverTree.ProcessFrame -= UpdateHover;
		}
		_hoverTree = null;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_window))
		{
			_window.WindowInput -= new WindowInputEventHandler(HandleInput);
		}
		_window = null;
		DisconnectOrigin();
		_pageToken?.Cancel();
		_pageToken?.Dispose();
		_pageToken = null;
		_marquees.Clear();
	}

	private void DisconnectOrigin()
	{
		if (_originConnected)
		{
			_originConnected = false;
			if (GodotObject.IsInstanceValid((GodotObject)(object)_origin))
			{
				_origin.TreeExited -= Close;
			}
		}
	}

	private static Label Text(string text, int size = 20, bool accent = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		Label val = new Label
		{
			Text = text,
			MouseFilter = (MouseFilterEnum)2
		};
		Font gameFont = ContextualSkinControls.GameFont;
		if (gameFont != null)
		{
			((Control)val).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
		}
		ModThemeRuntime.TextControl((Control)(object)val, size, accent);
		return val;
	}

	private static string PanelVersionText()
	{
		Assembly assembly = typeof(SkinWorkshopPanel).Assembly;
		string text = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? assembly.GetName().Version?.ToString() ?? "";
		int num = text.IndexOf('+');
		if (num >= 0)
		{
			return text.Substring(0, num);
		}
		return text;
	}

	private static Button Button(string text, Action pressed)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		Button val = new Button
		{
			Text = text,
			CustomMinimumSize = new Vector2(130f, 42f),
			MouseDefaultCursorShape = (CursorShape)2
		};
		ContextualSkinControls.ApplyGameTheme(val);
		((BaseButton)val).Pressed += pressed;
		return val;
	}

	private WorkshopCatalogItem[] FilteredItems()
	{
		string unknownKind = ((GitHubMode && GitHubSkinFilter.IgnoresKindFilter(_installFilter)) ? "" : _kind);
		WorkshopCatalogItem[] array = SkinWorkshopService.Catalog.Where((WorkshopCatalogItem item) => item.Targets.Length != 0 || GitHubSkinPolicy.KeepWithoutTargets(item.Id, item.Targets.Length, unknownKind)).ToArray();
		IEnumerable<WorkshopCatalogItem> enumerable = SourceFiltered(from item in WorkshopBrowserPolicy.Filter(array, _kind, _target, RegionMembers).Concat(array.Where((WorkshopCatalogItem item) => item.Targets.Length == 0))
			where WorkshopLoadTags.Matches(_loadFilter, SkinWorkshopService.LoadTag(item))
			select item);
		Dictionary<ulong, WorkshopCatalogItem> filtered = (GitHubMode ? enumerable : _subscriptions.Filter(enumerable)).ToDictionary((WorkshopCatalogItem item) => item.Id);
		if (!GitHubMode)
		{
			return (from id in WorkshopSortPolicy.OrderIds(filtered.Keys, SkinWorkshopService.CachedMetadata, _sort, _reverseSort, SkinWorkshopService.CommunityReplies)
				select filtered[id]).ToArray();
		}
		return (from id in GitHubSortPolicy.OrderIds(filtered.Keys, GitHubSkinService.EntriesById, _githubSort, _reverseSort)
			select filtered[id]).ToArray();
	}

	private string ItemMetric(ulong id)
	{
		return WorkshopSortPolicy.Metric(SkinWorkshopService.CachedDetails(id), _sort, ModLocalization.CurrentLanguage, SkinWorkshopService.CommunityReplies.GetValueOrDefault(id, -1));
	}

	private void BuildSort(HBoxContainer heading)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Expected O, but got Unknown
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Expected O, but got Unknown
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Expected O, but got Unknown
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Expected O, but got Unknown
		((Node)heading).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)2
		}, false, (InternalMode)0);
		OptionButton direction = new OptionButton
		{
			CustomMinimumSize = new Vector2(130f, 42f),
			FitToLongestItem = false,
			ClipText = true,
			TextOverrunBehavior = (OverrunBehavior)3
		};
		_sortDirectionPicker = direction;
		ContextualSkinControls.ApplyGameTheme(direction);
		direction.AddItem(WorkshopDetailsText.Get(WorkshopDetailsTextKey.Forward), 0);
		direction.AddItem(WorkshopDetailsText.Get(WorkshopDetailsTextKey.Reverse), 1);
		SelectByItemId(direction, _reverseSort ? 1 : 0);
		direction.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			_reverseSort = direction.GetItemId((int)index) == 1;
			_page = 0;
			_listScroll.ScrollVertical = 0;
			Rebuild();
		};
		((Node)heading).AddChild((Node)(object)direction, false, (InternalMode)0);
		KeepHoverBelow(direction);
		OptionButton picker = new OptionButton
		{
			CustomMinimumSize = new Vector2(245f, 42f),
			FitToLongestItem = false,
			ClipText = true,
			TextOverrunBehavior = (OverrunBehavior)3
		};
		_sortPicker = picker;
		ContextualSkinControls.ApplyGameTheme(picker);
		WorkshopSort[] values = Enum.GetValues<WorkshopSort>();
		foreach (WorkshopSort workshopSort in values)
		{
			picker.AddItem(WorkshopDetailsText.Get((WorkshopDetailsTextKey)((workshopSort == WorkshopSort.LatestReply) ? ((WorkshopSort)18) : workshopSort)), (int)workshopSort);
		}
		SelectByItemId(picker, (int)_sort);
		picker.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			_sort = (WorkshopSort)picker.GetItemId((int)index);
			_page = 0;
			_listScroll.ScrollVertical = 0;
			Rebuild();
		};
		((Node)heading).AddChild((Node)(object)picker, false, (InternalMode)0);
		KeepHoverBelow(picker);
		OptionButton githubSort = new OptionButton
		{
			CustomMinimumSize = new Vector2(245f, 42f),
			FitToLongestItem = false,
			ClipText = true,
			TextOverrunBehavior = (OverrunBehavior)3,
			Visible = false
		};
		_githubSortPicker = githubSort;
		ContextualSkinControls.ApplyGameTheme(githubSort);
		githubSort.AddItem(GitHubText(GitHubSkinTextKey.Stars), 1);
		githubSort.AddItem(GitHubText(GitHubSkinTextKey.Updated), 0);
		githubSort.AddItem(GitHubText(GitHubSkinTextKey.Name), 2);
		SelectByItemId(githubSort, (int)_githubSort);
		githubSort.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			_githubSort = (GitHubSort)githubSort.GetItemId((int)index);
			_page = 0;
			_listScroll.ScrollVertical = 0;
			Rebuild();
		};
		((Node)heading).AddChild((Node)(object)githubSort, false, (InternalMode)0);
		KeepHoverBelow(githubSort);
	}

	private static void SelectByItemId(OptionButton picker, int id)
	{
		int[] array = new int[picker.ItemCount];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = picker.GetItemId(i);
		}
		int num = WorkshopOptionSelectionPolicy.IndexOfId(array, id);
		if (num >= 0)
		{
			picker.Select(num);
		}
	}

	private void BuildFilters(VBoxContainer content)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		if (_kind == "companion" && _target == "osty")
		{
			_kind = "character";
			_target = "necrobinder";
		}
		_names = WorkshopTargetNames.Build();
		_region = ResolveEntryRegion(_region, _names.Regions(_kind).Keys);
		HFlowContainer val = new HFlowContainer();
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 12);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 8);
		((Node)content).AddChild((Node)(object)val, false, (InternalMode)0);
		_subscriptionPicker = new WorkshopPagedChoice(WorkshopSubscriptionFilter.Name, delegate(string value)
		{
			_subscriptions.Select(value);
			_page = 0;
			_listScroll.ScrollVertical = 0;
			RefreshFilters();
			Rebuild();
		});
		_typePicker = new WorkshopPagedChoice(WorkshopText.Kind, delegate(string id)
		{
			SetFilter(id, "");
		});
		_regionPicker = new WorkshopPagedChoice((string id) => _names.RegionNames.GetValueOrDefault(id, id), delegate(string id)
		{
			_region = id;
			_target = "";
			RefreshFilters();
			_page = 0;
			Rebuild();
		});
		_targetPicker = new WorkshopPagedChoice((string id) => _names.Name(_kind, id), delegate(string id)
		{
			_target = id;
			_page = 0;
			Rebuild();
		});
		_loadPicker = new WorkshopPagedChoice(WorkshopLoadTags.Name, SetLoadFilter);
		BuildSourceFilters(val);
		WorkshopPagedChoice[] array = new WorkshopPagedChoice[4] { _typePicker, _regionPicker, _targetPicker, _loadPicker };
		foreach (WorkshopPagedChoice workshopPagedChoice in array)
		{
			((Node)val).AddChild((Node)(object)workshopPagedChoice.Picker, false, (InternalMode)0);
			KeepHoverBelow(workshopPagedChoice.Picker);
		}
		InferRegion();
		RefreshFilters();
	}

	private void SetLoadFilter(string value)
	{
		_loadFilter = value;
		_page = 0;
		RefreshFilters();
		Rebuild();
	}

	private void SetFilter(string kind, string target)
	{
		_kind = kind;
		_target = target;
		_region = "";
		_page = 0;
		InferRegion();
		RefreshFilters();
		Rebuild();
	}

	private void InferRegion()
	{
		if (_target.Length > 0 && WorkshopBrowserPolicy.HasRegions(_kind))
		{
			_region = _names.Regions(_kind).FirstOrDefault<KeyValuePair<string, HashSet<string>>>((KeyValuePair<string, HashSet<string>> pair) => pair.Value.Contains(_target)).Key ?? "";
		}
	}

	internal static string ResolveEntryRegion(string region, IEnumerable<string> available)
	{
		if (region.StartsWith("act:", StringComparison.OrdinalIgnoreCase))
		{
			string text = region;
			region = text.Substring(4, text.Length - 4);
		}
		return available.FirstOrDefault((string id) => id.Equals(region, StringComparison.OrdinalIgnoreCase)) ?? "";
	}

	private void RefreshFilters()
	{
		_subscriptionPicker.SetOptions(WorkshopSubscriptionFilter.Options, _subscriptions.Value);
		((CanvasItem)_sourcePicker).Visible = !CodeErrors;
		SelectByItemId(_sourcePicker, SkinSources.IndexOf(_source));
		((Control)_sourcePicker).TooltipText = GitHubText(GitHubSkinTextKey.Source);
		_installPicker.SetOptions(GitHubSkinFilter.Options, _installFilter);
		((CanvasItem)_installPicker.Picker).Visible = !CodeErrors && GitHubMode;
		((CanvasItem)_githubSortPicker).Visible = !CodeErrors && GitHubMode;
		((CanvasItem)_subscriptionPicker.Picker).Visible = !CodeErrors && !GitHubMode;
		((CanvasItem)_sortPicker).Visible = !CodeErrors && !GitHubMode;
		((CanvasItem)_sortDirectionPicker).Visible = !CodeErrors;
		OptionButton picker = _typePicker.Picker;
		bool visible = (((CanvasItem)_loadPicker.Picker).Visible = !CodeErrors);
		((CanvasItem)picker).Visible = visible;
		_typePicker.SetOptions(Kinds, _kind);
		((CanvasItem)_regionPicker.Picker).Visible = !CodeErrors && WorkshopBrowserPolicy.HasRegions(_kind);
		_regionPicker.SetOptions(_names.Regions(_kind).Keys, _region);
		((CanvasItem)_targetPicker.Picker).Visible = !CodeErrors && _kind.Length > 0;
		IOrderedEnumerable<string> ids = (from t in SkinWorkshopService.Catalog.SelectMany((WorkshopCatalogItem i) => i.Targets)
			where t.Kind == _kind && WorkshopBrowserPolicy.VisibleTarget(t)
			select t.Target into id
			where RegionMembers == null || RegionMembers.Contains(id)
			select id).Distinct<string>(StringComparer.OrdinalIgnoreCase).OrderBy<string, string>((string id) => _names.Name(_kind, id), StringComparer.CurrentCulture);
		_targetPicker.SetOptions(ids, _target);
		_loadPicker.SetOptions(WorkshopLoadTags.FilterOptions, _loadFilter);
	}

	private void BindTags(RowView row, WorkshopCatalogItem item)
	{
		List<(string, Action)> list = new List<(string, Action)> { (WorkshopLoadTags.Name(SkinWorkshopService.LoadTag(item)), delegate
		{
			SetLoadFilter(SkinWorkshopService.LoadTag(item));
		}) };
		WorkshopTarget[] source = item.Targets.Where(WorkshopBrowserPolicy.VisibleTarget).ToArray();
		foreach (string kind in source.Select((WorkshopTarget t) => t.Kind).Distinct())
		{
			list.Add((WorkshopText.Kind(kind), delegate
			{
				SetFilter(kind, "");
			}));
		}
		foreach (WorkshopTarget target in source.Where((WorkshopTarget t) => t.Kind == "character" || (t.Kind == _kind && t.Target == _target)).Distinct())
		{
			list.Add((_names.Name(target.Kind, target.Target), delegate
			{
				SetFilter(target.Kind, target.Target);
			}));
		}
		while (row.TagSlots.Count < list.Count)
		{
			TagView tag = new TagView();
			tag.Button = BoundButton(row.binding, "", delegate
			{
				tag.Select?.Invoke();
			});
			tag.Button.ClipText = true;
			tag.Button.TextOverrunBehavior = (OverrunBehavior)3;
			((Control)tag.Button).SizeFlagsHorizontal = (SizeFlags)0;
			ModThemeRuntime.TextControl((Control)(object)tag.Button, 15);
			ModThemeRuntime.Bind((Node)(object)tag.Button, "tag_width", delegate(ModThemeSettings theme)
			{
				RefreshTagWidth(tag.Button, theme.FontScale);
			});
			((Node)row.Tags).AddChild((Node)(object)tag.Button, false, (InternalMode)0);
			row.TagSlots.Add(tag);
		}
		for (int i = 0; i < row.TagSlots.Count; i++)
		{
			TagView tagView = row.TagSlots[i];
			((CanvasItem)tagView.Button).Visible = i < list.Count;
			tagView.Select = ((i < list.Count) ? list[i].Item2 : null);
			if (i < list.Count)
			{
				tagView.Button.Text = list[i].Item1;
				((Control)tagView.Button).TooltipText = list[i].Item1;
				RefreshTagWidth(tagView.Button, ModThemeRuntime.Current.FontScale);
			}
		}
		_loadTags[item.Id] = row.TagSlots[0].Button;
		UpdateLoadTag(item, _loadTags[item.Id]);
	}

	private static void RefreshTagWidth(Button button, float scale)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		((Control)button).CustomMinimumSize = new Vector2(Math.Clamp(((Control)button).GetThemeFont(StringName.op_Implicit("font"), (StringName)null).GetStringSize(button.Text, (HorizontalAlignment)0, -1f, (int)(15f * scale), (JustificationFlag)3, (Direction)0, (Orientation)0).X + 24f, 48f, 190f * scale), 30f * scale);
	}

	private static void UpdateLoadTag(WorkshopCatalogItem item, Button button)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		string text = SkinWorkshopService.LoadTag(item);
		if (((GodotObject)button).HasMeta(StringName.op_Implicit("sc_load_tag")))
		{
			StringName obj = StringName.op_Implicit("sc_load_tag");
			Variant val = default(Variant);
			val = ((GodotObject)button).GetMeta(obj, val);
			if (((Variant)(ref val)).AsString() == text)
			{
				return;
			}
		}
		((GodotObject)button).SetMeta(StringName.op_Implicit("sc_load_tag"), Variant.op_Implicit(text));
		button.Text = WorkshopLoadTags.Name(text);
		((Control)button).TooltipText = button.Text;
		ModThemeRuntime.TextControl((Control)(object)button, 15, text == "restart");
		RefreshTagWidth(button, ModThemeRuntime.Current.FontScale);
	}

	private void PollFilters()
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (CodeErrors)
		{
			return;
		}
		_subscriptions.Refresh(SteamCatalogIds, SkinWorkshopService.IsSubscribed);
		foreach (WorkshopCatalogItem item in SkinWorkshopService.Catalog)
		{
			if (_loadTags.TryGetValue(item.Id, out Button value))
			{
				UpdateLoadTag(item, value);
			}
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_emptyLabel))
		{
			_emptyLabel.Text = EmptyText;
		}
		if (Input.IsMouseButtonPressed((MouseButton)1) || (from item in FilteredItems()
			select item.Id).SequenceEqual(_filteredIds) || _filterRebuildPending)
		{
			return;
		}
		_filterRebuildPending = true;
		Callable val = Callable.From((Action)delegate
		{
			_filterRebuildPending = false;
			if (!_closed && GodotObject.IsInstanceValid((GodotObject)(object)this) && ((Node)this).IsInsideTree())
			{
				Rebuild();
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private void InitializeHover()
	{
		CreateIntroduction();
		_hoverTree = ((Node)this).GetTree();
		_hoverTree.ProcessFrame += UpdateHover;
	}

	private void KeepHoverBelow(OptionButton picker)
	{
		((Window)picker.GetPopup()).AboutToPopup += delegate
		{
			_hoverPopups++;
			ResetHover();
		};
		((Popup)picker.GetPopup()).PopupHide += delegate
		{
			_hoverPopups = Math.Max(0, _hoverPopups - 1);
		};
	}

	private void CreateIntroduction()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Expected O, but got Unknown
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Expected O, but got Unknown
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Expected O, but got Unknown
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Expected O, but got Unknown
		_intro = new Control
		{
			Visible = false,
			MouseFilter = (MouseFilterEnum)2,
			ZIndex = 3
		};
		((Node)this).AddChild((Node)(object)_intro, false, (InternalMode)0);
		PanelContainer val = new PanelContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)_intro).AddChild((Node)(object)val, false, (InternalMode)0);
		((Control)val).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		ModThemeRuntime.Panel(val);
		MarginContainer val2 = new MarginContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		string[] array = new string[4] { "left", "right", "top", "bottom" };
		foreach (string text in array)
		{
			((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("margin_" + text), 14);
		}
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		VBoxContainer val3 = new VBoxContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		_introTitle = Text("", 22);
		_introTitle.AutowrapMode = (AutowrapMode)3;
		_introTitle.MaxLinesVisible = 2;
		_introTitle.TextOverrunBehavior = (OverrunBehavior)3;
		((Node)val3).AddChild((Node)(object)_introTitle, false, (InternalMode)0);
		_introImageHost = new Control
		{
			CustomMinimumSize = new Vector2(0f, 180f),
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val3).AddChild((Node)(object)_introImageHost, false, (InternalMode)0);
		Panel val4 = new Panel
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)_introImageHost).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Control)val4).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		StyleBoxFlat style = new StyleBoxFlat();
		((Control)val4).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)style);
		ModThemeRuntime.Bind((Node)(object)val4, "intro_placeholder", delegate(ModThemeSettings theme)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			style.BgColor = new Color(new Color(theme.AccentColor), 0.15f);
			StyleBoxFlat obj = style;
			StyleBoxFlat obj2 = style;
			StyleBoxFlat obj3 = style;
			int num = (style.CornerRadiusBottomRight = theme.CornerRadius);
			int num3 = (obj3.CornerRadiusBottomLeft = num);
			int cornerRadiusTopLeft = (obj2.CornerRadiusTopRight = num3);
			obj.CornerRadiusTopLeft = cornerRadiusTopLeft;
		});
		_introImage = new TextureRect
		{
			ExpandMode = (ExpandModeEnum)1,
			StretchMode = (StretchModeEnum)5,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)_introImageHost).AddChild((Node)(object)_introImage, false, (InternalMode)0);
		((Control)_introImage).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		_introImageStatus = Text("…", 18);
		_introImageStatus.HorizontalAlignment = (HorizontalAlignment)1;
		_introImageStatus.VerticalAlignment = (VerticalAlignment)1;
		_introImageStatus.AutowrapMode = (AutowrapMode)3;
		((Node)_introImageHost).AddChild((Node)(object)_introImageStatus, false, (InternalMode)0);
		((Control)_introImageStatus).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		_introPage = Text("", 16);
		_introPage.HorizontalAlignment = (HorizontalAlignment)1;
		((Node)val3).AddChild((Node)(object)_introPage, false, (InternalMode)0);
		_introDescriptionHost = new Control
		{
			SizeFlagsVertical = (SizeFlags)3,
			ClipContents = true,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val3).AddChild((Node)(object)_introDescriptionHost, false, (InternalMode)0);
		_introDescription = Text("…", 18);
		_introDescription.AutowrapMode = (AutowrapMode)3;
		_introDescription.MaxLinesVisible = 7;
		_introDescription.TextOverrunBehavior = (OverrunBehavior)3;
		_introDescription.ClipText = true;
		((Node)_introDescriptionHost).AddChild((Node)(object)_introDescription, false, (InternalMode)0);
		((Control)_introDescription).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
	}

	private void AttachItemClick(PanelContainer panel, WorkshopItemBinding binding, Func<ulong> itemId)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		((Control)panel).MouseFilter = (MouseFilterEnum)0;
		((Control)panel).MouseDefaultCursorShape = (CursorShape)2;
		((Control)panel).FocusMode = (FocusModeEnum)2;
		bool armed = false;
		WorkshopItemTicket ticket = default(WorkshopItemTicket);
		Vector2 pressedAt = Vector2.Zero;
		((Control)panel).MouseExited += delegate
		{
			armed = false;
		};
		((Control)panel).GuiInput += (GuiInputEventHandler)delegate(InputEvent ev)
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Invalid comparison between Unknown and I8
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Invalid comparison between Unknown and I8
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Invalid comparison between Unknown and I8
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			if (_closed || _suspended || _hoverPopups > 0)
			{
				armed = false;
				return;
			}
			InputEventMouseButton val = (InputEventMouseButton)(object)((ev is InputEventMouseButton) ? ev : null);
			bool flag;
			if (val != null && val.Pressed)
			{
				MouseButton buttonIndex = val.ButtonIndex;
				if ((long)(buttonIndex - 4) <= 1L)
				{
					flag = true;
					goto IL_0057;
				}
			}
			flag = false;
			goto IL_0057;
			IL_0057:
			if (flag && ((CanvasItem)panel).TopLevel)
			{
				ScrollContainer listScroll = _listScroll;
				listScroll.ScrollVertical += (int)(48f * Math.Max(1f, val.Factor)) * (((long)val.ButtonIndex != 4) ? 1 : (-1));
				((Control)panel).AcceptEvent();
				CancelIntroduction();
				return;
			}
			InputEventMouseButton val2 = (InputEventMouseButton)(object)((ev is InputEventMouseButton) ? ev : null);
			if (val2 == null || (long)val2.ButtonIndex != 1)
			{
				if (ev is InputEventMouseMotion && ((Vector2)(ref pressedAt)).DistanceTo(((CanvasItem)panel).GetGlobalMousePosition()) > 8f)
				{
					armed = false;
				}
				else if (((Control)panel).HasFocus() && ev.IsActionPressed(StringName.op_Implicit("ui_accept"), false, false) && !ev.IsEcho())
				{
					((Control)panel).AcceptEvent();
					OpenItem(itemId());
				}
				return;
			}
			((Control)panel).AcceptEvent();
			if (val2.Pressed)
			{
				armed = true;
				ticket = binding.Capture();
				pressedAt = ((CanvasItem)panel).GetGlobalMousePosition();
				((Control)panel).GrabFocus();
				return;
			}
			int num;
			if (armed && binding.Matches(ticket))
			{
				Rect2 globalRect = ((Control)panel).GetGlobalRect();
				if (((Rect2)(ref globalRect)).HasPoint(((CanvasItem)panel).GetGlobalMousePosition()))
				{
					num = ((((Vector2)(ref pressedAt)).DistanceTo(((CanvasItem)panel).GetGlobalMousePosition()) <= 8f) ? 1 : 0);
					goto IL_017c;
				}
			}
			num = 0;
			goto IL_017c;
			IL_017c:
			armed = false;
			if (num != 0)
			{
				OpenItem(itemId());
			}
		};
	}

	private HoverItem? HoveredItem()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Vector2 mouse = ((CanvasItem)this).GetGlobalMousePosition();
		Control focus = ((Node)this).GetViewport().GuiGetFocusOwner();
		Rect2 clip = ((Control)_listScroll).GetGlobalRect();
		return _hoverItems.FirstOrDefault(delegate(HoverItem item)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			Rect2 val = item.Slot.GetGlobalRect();
			if (((Rect2)(ref val)).Intersects(clip, false))
			{
				if (!_hoverKeyboard)
				{
					val = ((Rect2)(ref clip)).Grow(8f);
					if (((Rect2)(ref val)).HasPoint(mouse))
					{
						val = item.Slot.GetGlobalRect();
						if (!((Rect2)(ref val)).HasPoint(mouse))
						{
							if (item == _lifted)
							{
								val = ((Control)item.Panel).GetGlobalRect();
								return ((Rect2)(ref val)).HasPoint(mouse);
							}
							return false;
						}
						return true;
					}
					return false;
				}
				if (focus != null)
				{
					if (focus != item.Panel)
					{
						return ((Node)item.Panel).IsAncestorOf((Node)(object)focus);
					}
					return true;
				}
				return false;
			}
			return false;
		});
	}

	private void UpdateHover()
	{
		if (_hoverPopups > 0 || _closed || _suspended || !((CanvasItem)this).IsVisibleInTree())
		{
			return;
		}
		foreach (HoverItem key in _returnTweens.Keys)
		{
			PositionLift(key);
		}
		HoverItem hoverItem = HoveredItem();
		if (Input.IsMouseButtonPressed((MouseButton)1))
		{
			if (!WorkshopHoverPolicy.CanDisplay(hoverItem?.Id ?? 0, _lifted?.Id ?? 0, blocked: false))
			{
				CancelIntroduction();
			}
			return;
		}
		if (hoverItem != _lifted)
		{
			Lift(hoverItem);
		}
		HoverItem lifted = _lifted;
		if ((object)lifted != null)
		{
			PositionLift(lifted);
			if (_introDwell.Observe(lifted.Id, (double)Time.GetTicksMsec() / 1000.0) && _introToken == null)
			{
				ShowIntroduction(lifted);
			}
			if (((CanvasItem)_intro).Visible)
			{
				PositionIntroduction();
				_introTitle.Text = ItemTitle(lifted.Id);
				_carousel?.Tick(!InstantHover);
			}
		}
	}

	private static string ItemTitle(ulong id)
	{
		object obj;
		if (!SkinSourceIdentity.IsGitHub(id))
		{
			obj = SkinWorkshopService.CachedDetails(id)?.Title;
			if (obj == null)
			{
				return "…";
			}
		}
		else
		{
			obj = GitHubSkinService.Title(id);
		}
		return (string)obj;
	}

	private void Lift(HoverItem? item)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		long timestamp = Stopwatch.GetTimestamp();
		HoverItem lifted = _lifted;
		if ((object)lifted != null)
		{
			BeginReturn(lifted);
		}
		CancelIntroduction();
		_lifted = item;
		if (item == null)
		{
			RecordHoverTiming(timestamp);
			return;
		}
		if (_returnTweens.Remove(item, out Tween value))
		{
			value.Kill();
		}
		((CanvasItem)item.Panel).TopLevel = true;
		((CanvasItem)item.Panel).ZIndex = 2;
		((Control)item.Panel).SetAnchorsAndOffsetsPreset((LayoutPreset)0, (LayoutPresetMode)0, 0);
		((Control)item.Panel).Scale = Vector2.One * 1.025f;
		PositionLift(item);
		((Control)item.Panel).PivotOffset = ((Control)item.Panel).Size * 0.5f;
		RecordHoverTiming(timestamp);
	}

	private void ShowIntroduction(HoverItem item)
	{
		_introToken = new CancellationTokenSource();
		_introTitle.Text = ItemTitle(item.Id);
		_introDescription.Text = "…";
		_introImage.Texture = null;
		_introImageStatus.Text = "…";
		((CanvasItem)_introImageStatus).Show();
		((CanvasItem)_introPage).Hide();
		((CanvasItem)_intro).Show();
		PositionIntroduction();
		LoadIntroduction(item.Id, _introGeneration, _introToken.Token);
	}

	private void BeginReturn(HoverItem item)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (InstantHover)
		{
			RestoreItem(item);
			return;
		}
		if (_returnTweens.Remove(item, out Tween value))
		{
			value.Kill();
		}
		((CanvasItem)item.Panel).ZIndex = 1;
		Tween tween = ((Node)item.Panel).CreateTween().SetIgnoreTimeScale(true).SetEase((EaseType)1)
			.SetTrans((TransitionType)7);
		_returnTweens[item] = tween;
		tween.TweenProperty((GodotObject)(object)item.Panel, NodePath.op_Implicit("scale"), Variant.op_Implicit(Vector2.One), 0.12);
		tween.Finished += delegate
		{
			if (_returnTweens.TryGetValue(item, out Tween value2) && value2 == tween)
			{
				_returnTweens.Remove(item);
				if (item != _lifted)
				{
					RestoreItem(item);
				}
			}
		};
	}

	private void PositionLift(HoverItem item)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		((Control)item.Panel).Size = item.Slot.Size;
		((Control)item.Panel).PivotOffset = ((Control)item.Panel).Size * 0.5f;
		float num = Math.Clamp((((Control)item.Panel).Scale.X - 1f) / 0.025f, 0f, 1f);
		Vector2 position = item.Slot.GlobalPosition - new Vector2(0f, 3f * num);
		((Control)item.Panel).Position = position;
	}

	private void RestoreLift()
	{
		foreach (var (old, val2) in _returnTweens)
		{
			val2.Kill();
			RestoreItem(old);
		}
		_returnTweens.Clear();
		HoverItem lifted = _lifted;
		if ((object)lifted != null)
		{
			RestoreItem(lifted);
		}
		_lifted = null;
	}

	private void RestoreItem(HoverItem old)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)this).IsInsideTree() && !((GodotObject)this).IsQueuedForDeletion() && GodotObject.IsInstanceValid((GodotObject)(object)_layer) && !((GodotObject)_layer).IsQueuedForDeletion() && GodotObject.IsInstanceValid((GodotObject)(object)old.Panel) && GodotObject.IsInstanceValid((GodotObject)(object)old.Slot) && !((GodotObject)old.Panel).IsQueuedForDeletion() && !((GodotObject)old.Slot).IsQueuedForDeletion())
		{
			long timestamp = Stopwatch.GetTimestamp();
			((Control)old.Panel).Scale = Vector2.One;
			((CanvasItem)old.Panel).TopLevel = false;
			((CanvasItem)old.Panel).ZIndex = 0;
			((Control)old.Panel).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
			RecordHoverTiming(timestamp);
		}
	}

	private void RecordHoverTiming(long started)
	{
		double totalMilliseconds = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
		_hoverChanges++;
		_hoverTotalMs += totalMilliseconds;
		_hoverMaxMs = Math.Max(_hoverMaxMs, totalMilliseconds);
	}

	private void LogHoverTiming()
	{
		if (_hoverChanges != 0)
		{
			ModLog.Info($"工坊悬停节点切换：{_hoverChanges} 次，平均 {_hoverTotalMs / (double)_hoverChanges:F2}ms，最高 {_hoverMaxMs:F2}ms；简介延迟 1 秒。");
			_hoverChanges = 0;
			_hoverTotalMs = (_hoverMaxMs = 0.0);
		}
	}

	private void PositionIntroduction()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		if (!(_lifted == null))
		{
			float fontScale = ModThemeRuntime.Current.FontScale;
			Rect2 globalRect = ((Control)_lifted.Panel).GetGlobalRect();
			Rect2 val = WorkshopHoverPolicy.PlaceIntroduction(((Rect2)(ref globalRect)).Grow(6f), new Vector2(Math.Clamp(((Control)this).Size.X * 0.27f, 300f, 450f), Math.Min(((Control)this).Size.Y - 24f, 480f * fontScale)), new Rect2(Vector2.Zero, ((Control)this).Size));
			Control intro = _intro;
			Transform2D globalTransform = ((CanvasItem)this).GetGlobalTransform();
			intro.Position = ((Transform2D)(ref globalTransform)).AffineInverse() * ((Rect2)(ref val)).Position;
			_intro.Size = ((Rect2)(ref val)).Size;
			_introImageHost.CustomMinimumSize = new Vector2(0f, Math.Min(((Rect2)(ref val)).Size.X * 0.56f, ((Rect2)(ref val)).Size.Y * 0.4f));
			float val2 = ((Control)_introDescription).GetThemeFont(StringName.op_Implicit("font"), (StringName)null).GetHeight(((Control)_introDescription).GetThemeFontSize(StringName.op_Implicit("font_size"), (StringName)null)) + (float)((Control)_introDescription).GetThemeConstant(StringName.op_Implicit("line_spacing"), (StringName)null);
			_introDescription.MaxLinesVisible = Math.Clamp((int)(_introDescriptionHost.Size.Y / Math.Max(1f, val2)), 1, 7);
		}
	}

	private async Task LoadIntroduction(ulong id, int generation, CancellationToken token)
	{
		try
		{
			if (SkinSourceIdentity.IsGitHub(id))
			{
				await LoadGitHubIntroduction(id, generation, token);
				return;
			}
			WorkshopIntroduction workshopIntroduction = await SkinWorkshopService.Introduction(id, token);
			if (!IntroductionCurrent(generation, token))
			{
				return;
			}
			_introDescription.Text = ((!workshopIntroduction.Available) ? WorkshopDetailsText.Get(WorkshopDetailsTextKey.LoadFailed) : ((workshopIntroduction.Description.Length > 0) ? workshopIntroduction.Description : WorkshopDetailsText.Get(WorkshopDetailsTextKey.NoDescription)));
			((CanvasItem)_introPage).Visible = workshopIntroduction.Images.Length > 1;
			if (workshopIntroduction.Images.Length == 0)
			{
				_introImageStatus.Text = WorkshopDetailsText.Get(WorkshopDetailsTextKey.ImageUnavailable);
				return;
			}
			_carousel = new WorkshopCarousel<Texture2D>(WorkshopCoverCache.GetPreview, delegate(Texture2D? texture, int index, int count)
			{
				if (IntroductionCurrent(generation, token))
				{
					_introImage.Texture = texture;
					_introPage.Text = $"{index + 1} / {count}";
					if (texture != null)
					{
						((CanvasItem)_introImageStatus).Hide();
						RoundCover(_introImage);
					}
					else
					{
						_introImageStatus.Text = WorkshopDetailsText.Get(WorkshopDetailsTextKey.ImageUnavailable);
					}
				}
			}, () => (double)Time.GetTicksMsec() / 1000.0);
			_carousel.Start(workshopIntroduction.Images, token);
			_carousel.Tick(!InstantHover);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex2)
		{
			ModLog.Info("工坊简介显示失败：" + ex2.Message);
		}
	}

	private async Task LoadGitHubIntroduction(ulong id, int generation, CancellationToken token)
	{
		string text = await GitHubSkinService.ReadmeAsync(id, token);
		if (!IntroductionCurrent(generation, token))
		{
			return;
		}
		Label introDescription = _introDescription;
		object text3;
		if (text.Length <= 0)
		{
			string text2 = GitHubSkinService.Description(id);
			text3 = ((text2 != null && text2.Length > 0) ? text2 : WorkshopDetailsText.Get(WorkshopDetailsTextKey.NoDescription));
		}
		else
		{
			text3 = text;
		}
		introDescription.Text = (string)text3;
		((CanvasItem)_introPage).Hide();
		Texture2D val = await WorkshopCoverCache.GetPreview(GitHubSkinService.CoverUrl(id), token);
		if (IntroductionCurrent(generation, token))
		{
			_introImage.Texture = val;
			if (val != null)
			{
				((CanvasItem)_introImageStatus).Hide();
				RoundCover(_introImage);
			}
			else
			{
				_introImageStatus.Text = WorkshopDetailsText.Get(WorkshopDetailsTextKey.ImageUnavailable);
			}
		}
	}

	private bool IntroductionCurrent(int generation, CancellationToken token)
	{
		if (!token.IsCancellationRequested && !_closed && !_suspended && _hoverPopups == 0 && generation == _introGeneration && GodotObject.IsInstanceValid((GodotObject)(object)_intro))
		{
			return WorkshopHoverPolicy.CanDisplay(HoveredItem()?.Id ?? 0, _lifted?.Id ?? 0, blocked: false);
		}
		return false;
	}

	private void CancelIntroduction()
	{
		_introDwell.Clear();
		_introGeneration++;
		_carousel?.Clear();
		_carousel = null;
		_introToken?.Cancel();
		_introToken?.Dispose();
		_introToken = null;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_intro))
		{
			((CanvasItem)_intro).Hide();
			_introImage.Texture = null;
		}
	}

	private void ResetHover()
	{
		RestoreLift();
		CancelIntroduction();
	}

	private Label CreateMarquee(Control row, VBoxContainer labels, string text)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		Control clip = new Control
		{
			ClipContents = true,
			CustomMinimumSize = new Vector2(0f, 38f),
			SizeFlagsHorizontal = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)labels).AddChild((Node)(object)clip, false, (InternalMode)0);
		Label val = Text(text, 22);
		((Node)clip).AddChild((Node)(object)val, false, (InternalMode)0);
		_marquees.Add(new Marquee(row, clip, val));
		ModThemeRuntime.Bind((Node)(object)clip, "title_height", delegate(ModThemeSettings theme)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			clip.CustomMinimumSize = new Vector2(0f, 38f * theme.FontScale);
		});
		return val;
	}

	private TextureRect CreateCover(HBoxContainer row, out Panel placeholder)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		Control val = new Control
		{
			CustomMinimumSize = new Vector2(144f, 100f),
			SizeFlagsVertical = (SizeFlags)0,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)row).AddChild((Node)(object)val, false, (InternalMode)0);
		placeholder = new Panel
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val).AddChild((Node)(object)placeholder, false, (InternalMode)0);
		((Control)placeholder).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		StyleBoxFlat style = new StyleBoxFlat();
		((Control)placeholder).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)style);
		ModThemeRuntime.Bind((Node)(object)placeholder, "workshop_placeholder", delegate(ModThemeSettings theme)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			style.BgColor = new Color(theme.AccentColor);
			StyleBoxFlat obj = style;
			StyleBoxFlat obj2 = style;
			StyleBoxFlat obj3 = style;
			int num = (style.CornerRadiusBottomRight = theme.CornerRadius);
			int num3 = (obj3.CornerRadiusBottomLeft = num);
			int cornerRadiusTopLeft = (obj2.CornerRadiusTopRight = num3);
			obj.CornerRadiusTopLeft = cornerRadiusTopLeft;
		});
		TextureRect val2 = new TextureRect
		{
			ExpandMode = (ExpandModeEnum)1,
			StretchMode = (StretchModeEnum)5,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Control)val2).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		return val2;
	}

	private void AnimateTitles()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		if (_closed || _suspended || !((CanvasItem)this).IsVisibleInTree())
		{
			return;
		}
		Vector2 globalMousePosition = ((CanvasItem)this).GetGlobalMousePosition();
		Control val = ((Node)this).GetViewport().GuiGetFocusOwner();
		foreach (Marquee marquee in _marquees)
		{
			if (!((CanvasItem)marquee.row).IsVisibleInTree())
			{
				continue;
			}
			float num = Math.Max(0f, ((Control)marquee.label).GetMinimumSize().X - marquee.clip.Size.X + 6f);
			Rect2 globalRect = marquee.row.GetGlobalRect();
			if ((!((Rect2)(ref globalRect)).HasPoint(globalMousePosition) && (val == null || !((Node)marquee.row).IsAncestorOf((Node)(object)val))) || num <= 6f)
			{
				marquee.Started = 0uL;
				((Control)marquee.label).Position = Vector2.Zero;
				continue;
			}
			if (marquee.Started == 0L)
			{
				marquee.Started = Time.GetTicksMsec();
			}
			double elapsed = (double)(Time.GetTicksMsec() - marquee.Started) / 1000.0;
			((Control)marquee.label).Position = new Vector2(0f - WorkshopBrowserPolicy.MarqueeOffset(num, elapsed), 0f);
		}
	}

	private static void RoundCover(TextureRect cover)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		Material material = ((CanvasItem)cover).Material;
		ShaderMaterial val = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
		if (val != null)
		{
			RefreshCoverSize(cover, val);
			return;
		}
		ShaderMaterial material2 = new ShaderMaterial
		{
			Shader = CoverShader.Value
		};
		((CanvasItem)cover).Material = (Material)(object)material2;
		((Control)cover).Resized += Resize;
		ModThemeRuntime.Bind((Node)(object)cover, "workshop_cover", delegate(ModThemeSettings theme)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			material2.SetShaderParameter(StringName.op_Implicit("corner_radius"), Variant.op_Implicit(theme.CornerRadius));
			Resize();
		});
		void Resize()
		{
			RefreshCoverSize(cover, material2);
		}
	}

	private static void RefreshCoverSize(TextureRect cover, ShaderMaterial material)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)cover))
		{
			Texture2D texture = cover.Texture;
			if (texture != null)
			{
				Vector2 size = texture.GetSize();
				float num = Math.Min(((Control)cover).Size.X / Math.Max(1f, size.X), ((Control)cover).Size.Y / Math.Max(1f, size.Y));
				material.SetShaderParameter(StringName.op_Implicit("surface_size"), Variant.op_Implicit(size * num));
			}
		}
	}

	private Button BoundButton(WorkshopItemBinding binding, string title, Action select)
	{
		WorkshopItemTicket pressed = default(WorkshopItemTicket);
		Button obj = Button(title, delegate
		{
			if (binding.Matches(pressed))
			{
				select();
			}
		});
		((BaseButton)obj).ButtonDown += delegate
		{
			pressed = binding.Capture();
		};
		return obj;
	}

	private RowView CreateRow(WorkshopItemBinding binding)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Expected O, but got Unknown
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Expected O, but got Unknown
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Expected O, but got Unknown
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Expected O, but got Unknown
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Expected O, but got Unknown
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		Control slot = new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)2,
			CustomMinimumSize = new Vector2(0f, 182f)
		};
		((Node)_rows).AddChild((Node)(object)slot, false, (InternalMode)0);
		PanelContainer panel = new PanelContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Node)slot).AddChild((Node)(object)panel, false, (InternalMode)0);
		((Control)panel).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		ModThemeRuntime.Panel(panel);
		((Control)panel).MinimumSizeChanged += delegate
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			slot.CustomMinimumSize = new Vector2(0f, Math.Max(182f, ((Control)panel).GetCombinedMinimumSize().Y));
		};
		RowView view = new RowView(binding, slot, panel);
		AttachItemClick(panel, binding, () => view.WorkshopId);
		MarginContainer val = new MarginContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)panel).AddChild((Node)(object)val, false, (InternalMode)0);
		string[] array = new string[4] { "left", "right", "top", "bottom" };
		foreach (string text in array)
		{
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_" + text), 12);
		}
		VBoxContainer val2 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			CustomMinimumSize = new Vector2(0f, 158f),
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		HBoxContainer val3 = new HBoxContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 14);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		view.Cover = CreateCover(val3, out view.Placeholder);
		VBoxContainer val4 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		view.Title = CreateMarquee((Control)(object)panel, val4, "…");
		view.Tags = new HFlowContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val4).AddChild((Node)(object)view.Tags, false, (InternalMode)0);
		HBoxContainer val5 = new HBoxContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val2).AddChild((Node)(object)val5, false, (InternalMode)0);
		view.Status = Text("", 16);
		((Control)view.Status).SizeFlagsHorizontal = (SizeFlags)3;
		view.Status.ClipText = true;
		view.Status.TextOverrunBehavior = (OverrunBehavior)3;
		((Node)val5).AddChild((Node)(object)view.Status, false, (InternalMode)0);
		view.Metric = Text("", 16);
		view.Metric.ClipText = true;
		view.Metric.TextOverrunBehavior = (OverrunBehavior)3;
		((Control)view.Metric).SizeFlagsHorizontal = (SizeFlags)3;
		view.Metric.HorizontalAlignment = (HorizontalAlignment)2;
		((Node)val5).AddChild((Node)(object)view.Metric, false, (InternalMode)0);
		view.Action = BoundButton(binding, "", delegate
		{
			view.PrimaryAction?.Invoke();
		});
		((Control)view.Action).CustomMinimumSize = new Vector2(100f, 38f);
		((Node)val5).AddChild((Node)(object)view.Action, false, (InternalMode)0);
		view.Cancel = BoundButton(binding, "", delegate
		{
			view.SecondaryAction?.Invoke();
		});
		((Control)view.Cancel).CustomMinimumSize = new Vector2(120f, 38f);
		((Node)val5).AddChild((Node)(object)view.Cancel, false, (InternalMode)0);
		return view;
	}

	private void PrepareRows(IReadOnlyList<ulong> bindingIds)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		if (_pageRows == null)
		{
			_pageRows = new WorkshopPagePool<RowView>(8, CreateRow);
		}
		_pageRows.Bind(bindingIds);
		_actions.Clear();
		_loadTags.Clear();
		_metrics.Clear();
		_hoverItems.Clear();
		foreach (Marquee marquee in _marquees)
		{
			marquee.Started = 0uL;
			((Control)marquee.label).Position = Vector2.Zero;
		}
		for (int i = 0; i < _pageRows.Slots.Count; i++)
		{
			RowView item = _pageRows.Slots[i].View;
			((CanvasItem)item.slot).Visible = i < bindingIds.Count;
			item.WorkshopId = 0uL;
			item.PrimaryAction = (item.SecondaryAction = null);
			item.Cover.Texture = null;
			((CanvasItem)item.Placeholder).Show();
			((CanvasItem)item.Action).Hide();
			((CanvasItem)item.Cancel).Hide();
			Button action = item.Action;
			bool disabled = (((BaseButton)item.Cancel).Disabled = true);
			((BaseButton)action).Disabled = disabled;
			Button action2 = item.Action;
			string text2 = (item.Cancel.Text = "");
			action2.Text = text2;
			Button action3 = item.Action;
			text2 = (((Control)item.Cancel).TooltipText = "");
			((Control)action3).TooltipText = text2;
			Label status = item.Status;
			Label status2 = item.Status;
			string text5 = (((Control)item.panel).TooltipText = "");
			text2 = (((Control)status2).TooltipText = text5);
			status.Text = text2;
			((CanvasItem)item.Tags).Show();
			((CanvasItem)item.Metric).Show();
			foreach (TagView tagSlot in item.TagSlots)
			{
				tagSlot.Select = null;
				((CanvasItem)tagSlot.Button).Hide();
			}
			if (((GodotObject)item.Action).HasMeta(StringName.op_Implicit("sc_restart_accent")))
			{
				Button action4 = item.Action;
				StringName obj = StringName.op_Implicit("sc_restart_accent");
				Variant val = default(Variant);
				val = ((GodotObject)action4).GetMeta(obj, val);
				if (((Variant)(ref val)).AsBool())
				{
					ModThemeRuntime.TextControl((Control)(object)item.Action, 20);
					((GodotObject)item.Action).SetMeta(StringName.op_Implicit("sc_restart_accent"), Variant.op_Implicit(false));
				}
			}
		}
	}

	private Dictionary<ulong, ItemVisual> BindRows(WorkshopCatalogItem[] visible)
	{
		PrepareRows(visible.Select((WorkshopCatalogItem item) => item.Id).ToArray());
		Dictionary<ulong, ItemVisual> dictionary = new Dictionary<ulong, ItemVisual>();
		for (int i = 0; i < visible.Length; i++)
		{
			RowView item2 = _pageRows.Slots[i].View;
			WorkshopCatalogItem item3 = visible[i];
			item2.WorkshopId = item3.Id;
			item2.PrimaryAction = delegate
			{
				PerformPrimary(item3.Id);
			};
			item2.SecondaryAction = delegate
			{
				Unsubscribe(item3.Id);
			};
			item2.Cancel.Text = WorkshopBrowserText.Get(WorkshopBrowserTextKey.Unsubscribe);
			item2.Title.Text = SkinWorkshopService.CachedDetails(item3.Id)?.Title ?? "…";
			item2.Metric.Text = ItemMetric(item3.Id);
			if (SkinSourceIdentity.IsGitHub(item3.Id))
			{
				item2.SecondaryAction = delegate
				{
					PerformGitHubSecondary(item3.Id);
				};
				item2.Title.Text = GitHubSkinService.Title(item3.Id);
				item2.Metric.Text = GitHubMetric(item3.Id);
			}
			BindTags(item2, item3);
			_hoverItems.Add(new HoverItem(item3.Id, item2.slot, item2.panel));
			_actions.Add((item3.Id, item2.Status, item2.Action, item2.Cancel));
			_metrics[item3.Id] = item2.Metric;
			dictionary[item3.Id] = item2.Snapshot();
		}
		return dictionary;
	}

	private void RecordPageBuild(long started)
	{
		double totalMilliseconds = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
		_pageBuilds++;
		_pageBuildTotalMs += totalMilliseconds;
		_pageBuildMaxMs = Math.Max(_pageBuildMaxMs, totalMilliseconds);
	}

	private void LogPageBuildTiming()
	{
		if (_pageBuilds != 0)
		{
			ModLog.Info($"工坊翻页/筛选：{_pageBuilds} 次，平均 {_pageBuildTotalMs / (double)_pageBuilds:F2}ms，最高 {_pageBuildMaxMs:F2}ms；复用 {_pageRows?.Slots.Count ?? 0} 个物品槽位。");
			_pageBuilds = 0;
			_pageBuildTotalMs = (_pageBuildMaxMs = 0.0);
		}
	}

	private static string GitHubText(GitHubSkinTextKey key)
	{
		return GitHubSkinText.ForLanguage(ModLocalization.CurrentLanguage, key);
	}

	private static string InstallName(string value)
	{
		return GitHubText(value switch
		{
			"missing" => GitHubSkinTextKey.NotInstalled, 
			"installed" => GitHubSkinTextKey.Installed, 
			"updatable" => GitHubSkinTextKey.Updatable, 
			"unrecognized" => GitHubSkinTextKey.Unrecognized, 
			_ => GitHubSkinTextKey.All, 
		});
	}

	private void BuildSourceFilters(HFlowContainer filters)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		_sourcePicker = new OptionButton
		{
			CustomMinimumSize = new Vector2(215f, 42f),
			FitToLongestItem = false,
			ClipText = true,
			TextOverrunBehavior = (OverrunBehavior)3
		};
		ContextualSkinControls.ApplyGameTheme(_sourcePicker);
		for (int i = 0; i < SkinSources.Options.Length; i++)
		{
			_sourcePicker.AddItem(SkinSources.Name(ModLocalization.CurrentLanguage, SkinSources.Options[i]), i);
		}
		SelectByItemId(_sourcePicker, SkinSources.IndexOf(_source));
		_sourcePicker.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			SelectSource(SkinSources.OptionAt((int)index));
		};
		((Node)filters).AddChild((Node)(object)_sourcePicker, false, (InternalMode)0);
		KeepHoverBelow(_sourcePicker);
		((Node)filters).AddChild((Node)(object)_subscriptionPicker.Picker, false, (InternalMode)0);
		KeepHoverBelow(_subscriptionPicker.Picker);
		_installPicker = new WorkshopPagedChoice(InstallName, delegate(string value)
		{
			_installFilter = value;
			_page = 0;
			_listScroll.ScrollVertical = 0;
			RefreshFilters();
			Rebuild();
		}, () => GitHubText(GitHubSkinTextKey.All));
		((Node)filters).AddChild((Node)(object)_installPicker.Picker, false, (InternalMode)0);
		KeepHoverBelow(_installPicker.Picker);
	}

	private void SelectSource(string value)
	{
		_source = SkinSources.Normalize(value);
		_page = 0;
		_listScroll.ScrollVertical = 0;
		if (GitHubMode && GitHubSkinService.Revision == 0)
		{
			GitHubSkinService.StartRefresh();
		}
		RefreshFilters();
		Rebuild();
	}

	private IEnumerable<WorkshopCatalogItem> SourceFiltered(IEnumerable<WorkshopCatalogItem> items)
	{
		return from item in items
			where (!GitHubMode) ? SkinSourceRoutingPolicy.IsSteamItem(item.Id) : SkinSourceIdentity.IsGitHub(item.Id)
			where !GitHubMode || GitHubSkinFilter.Matches(_installFilter, GitHubSkinService.IsRecognized(item.Id), GitHubSkinService.InstallState(item.Id))
			select item;
	}

	private string GitHubMetric(ulong id)
	{
		return GitHubSortPolicy.Metric(GitHubSkinService.Entry(id), _githubSort);
	}

	private static string GitHubStatus(GitHubSkinEntry? entry, GitHubInstallState state, bool pendingRestart, bool pendingRemoval)
	{
		if ((object)entry == null)
		{
			return "";
		}
		GitHubRecognition recognition = entry.Recognition;
		return (recognition != 0) ? (pendingRemoval ? GitHubText(GitHubSkinTextKey.RemovePending) : ((recognition != GitHubRecognition.Recognized) ? GitHubText(GitHubSkinTextKey.Unrecognized) : ((!pendingRestart) ? GitHubText(state switch
		{
			GitHubInstallState.Updatable => GitHubSkinTextKey.Updatable, 
			GitHubInstallState.Installed => GitHubSkinTextKey.Installed, 
			_ => GitHubSkinTextKey.NotInstalled, 
		}) : ""))) : GitHubText(GitHubSkinTextKey.Refreshing);
	}

	private void PollGitHub()
	{
		((CanvasItem)_communityRefresh).Visible = !GitHubMode;
		((CanvasItem)_submissionButton).Visible = !GitHubMode;
		((CanvasItem)_githubRefresh).Visible = GitHubMode && !CodeErrors;
		((BaseButton)_githubRefresh).Disabled = GitHubSkinService.Busy;
		_githubRefresh.Text = GitHubText(GitHubSkinService.Busy ? GitHubSkinTextKey.Refreshing : GitHubSkinTextKey.Refresh);
		((Control)_githubRefresh).TooltipText = (GitHubSkinService.RateLimited ? GitHubText(GitHubSkinTextKey.RateLimited) : (GitHubSkinService.Failed ? GitHubText(GitHubSkinTextKey.RefreshFailed) : GitHubSkinService.Progress));
		if (_githubRevision != GitHubSkinService.Revision)
		{
			_githubRevision = GitHubSkinService.Revision;
			if (GitHubMode)
			{
				RefreshFilters();
				Rebuild();
			}
		}
	}
}
