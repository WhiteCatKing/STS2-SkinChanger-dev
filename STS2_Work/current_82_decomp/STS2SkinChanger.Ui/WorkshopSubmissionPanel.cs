using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class WorkshopSubmissionPanel : Control
{
	private CanvasLayer _layer;

	private Window _window;

	private Node _origin;

	private Action _closedCallback;

	private readonly CancellationTokenSource _cancel = new CancellationTokenSource();

	private readonly HashSet<ulong> _selected = new HashSet<ulong>();

	private readonly Dictionary<ulong, Button> _choices = new Dictionary<ulong, Button>();

	private WorkshopLocalSubmission[] _candidates = Array.Empty<WorkshopLocalSubmission>();

	private VBoxContainer _list;

	private HBoxContainer _tools;

	private Button _scan;

	private Button _back;

	private Label _status;

	private LineEdit _search;

	private bool _busy;

	private bool _closed;

	private int _candidateRevision = -1;

	private bool Alive
	{
		get
		{
			if (!_closed && GodotObject.IsInstanceValid((GodotObject)(object)this))
			{
				return ((Node)this).IsInsideTree();
			}
			return false;
		}
	}

	internal static WorkshopSubmissionPanel Show(Node origin, Action closed)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		CanvasLayer val = new CanvasLayer
		{
			Name = StringName.op_Implicit("SCWorkshopSubmissionLayer"),
			Layer = 101,
			ProcessMode = (ProcessModeEnum)3
		};
		WorkshopSubmissionPanel workshopSubmissionPanel = new WorkshopSubmissionPanel
		{
			_layer = val,
			_origin = origin,
			_closedCallback = closed
		};
		try
		{
			((Node)origin.GetTree().Root).AddChild((Node)(object)val, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)workshopSubmissionPanel, false, (InternalMode)0);
			((Control)workshopSubmissionPanel).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
			workshopSubmissionPanel.Build();
			return workshopSubmissionPanel;
		}
		catch
		{
			workshopSubmissionPanel.Cleanup();
			((Node)val).QueueFree();
			throw;
		}
	}

	private void Build()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Expected O, but got Unknown
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Expected O, but got Unknown
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Expected O, but got Unknown
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Expected O, but got Unknown
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Expected O, but got Unknown
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Expected O, but got Unknown
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Expected O, but got Unknown
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Expected O, but got Unknown
		((Control)this).MouseFilter = (MouseFilterEnum)0;
		((Node)this).ProcessMode = (ProcessModeEnum)3;
		_window = ((Node)this).GetWindow();
		_window.WindowInput += new WindowInputEventHandler(OnInput);
		_origin.TreeExited += Close;
		((Node)this).TreeExiting += Cleanup;
		Timer val = new Timer
		{
			WaitTime = 0.1,
			Autostart = true,
			ProcessMode = (ProcessModeEnum)3
		};
		val.Timeout += delegate
		{
			if (Alive)
			{
				CanvasLayer layer = _layer;
				NModalContainer instance = NModalContainer.Instance;
				layer.Visible = ((instance != null) ? instance.OpenModal : null) == null;
				if (!_busy && _candidateRevision >= 0 && !((CanvasItem)_back).Visible && _candidateRevision != SkinWorkshopService.CommunityRevision)
				{
					ShowChoices();
				}
			}
		};
		((Node)this).AddChild((Node)(object)val, false, (InternalMode)0);
		ColorRect val2 = new ColorRect
		{
			Color = new Color(0f, 0f, 0f, 0.5f),
			MouseFilter = (MouseFilterEnum)0
		};
		((Node)this).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Control)val2).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		PanelContainer val3 = new PanelContainer
		{
			AnchorLeft = 0.12f,
			AnchorRight = 0.88f,
			AnchorTop = 0.07f,
			AnchorBottom = 0.93f,
			MouseFilter = (MouseFilterEnum)0
		};
		((Node)this).AddChild((Node)(object)val3, false, (InternalMode)0);
		ModThemeRuntime.Panel(val3);
		MarginContainer val4 = new MarginContainer();
		string[] array = new string[4] { "left", "right", "top", "bottom" };
		foreach (string text in array)
		{
			((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("margin_" + text), 16);
		}
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		VBoxContainer val5 = new VBoxContainer();
		((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val5).AddChild((Node)(object)Text(WorkshopCommunityText.Get(WorkshopCommunityTextKey.SubmitMod), 27, accent: true), false, (InternalMode)0);
		_tools = new HBoxContainer();
		((Node)val5).AddChild((Node)(object)_tools, false, (InternalMode)0);
		_search = new LineEdit
		{
			PlaceholderText = T(SubmissionText.Search),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		ModThemeRuntime.Input(_search, 20);
		((Node)_tools).AddChild((Node)(object)_search, false, (InternalMode)0);
		_search.TextChanged += (TextChangedEventHandler)delegate
		{
			Filter();
		};
		((Node)_tools).AddChild((Node)(object)Button(T(SubmissionText.All), delegate
		{
			SelectVisible(selected: true);
		}), false, (InternalMode)0);
		((Node)_tools).AddChild((Node)(object)Button(T(SubmissionText.Clear), delegate
		{
			SelectVisible(selected: false);
		}), false, (InternalMode)0);
		_status = Text(T(SubmissionText.Reading));
		_status.ClipText = true;
		_status.TextOverrunBehavior = (OverrunBehavior)3;
		((Node)val5).AddChild((Node)(object)_status, false, (InternalMode)0);
		ScrollContainer val6 = new ScrollContainer
		{
			SizeFlagsVertical = (SizeFlags)3,
			HorizontalScrollMode = (ScrollMode)0
		};
		((Node)val5).AddChild((Node)(object)val6, false, (InternalMode)0);
		_list = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_list).AddThemeConstantOverride(StringName.op_Implicit("separation"), 6);
		((Node)val6).AddChild((Node)(object)_list, false, (InternalMode)0);
		HBoxContainer val7 = new HBoxContainer();
		((Control)val7).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)val5).AddChild((Node)(object)val7, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)Button(ModLocalization.Get(ModText.Close), Close), false, (InternalMode)0);
		_back = Button(T(SubmissionText.Back), ShowChoices);
		((CanvasItem)_back).Visible = false;
		((Node)val7).AddChild((Node)(object)_back, false, (InternalMode)0);
		((Node)val7).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)2
		}, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)Button(T(SubmissionText.Open), delegate
		{
			try
			{
				WorkshopCommunityLinks.OpenDiscussion(presets: false);
			}
			catch (Exception ex)
			{
				Error(ex);
			}
		}), false, (InternalMode)0);
		_scan = Button(T(SubmissionText.Scan), delegate
		{
			Scan();
		});
		((BaseButton)_scan).Disabled = true;
		((Node)val7).AddChild((Node)(object)_scan, false, (InternalMode)0);
		LoadCandidates();
	}

	private async Task LoadCandidates()
	{
		try
		{
			_candidates = await SkinWorkshopService.SubmissionCandidates(_cancel.Token);
			if (Alive)
			{
				ShowChoices();
			}
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex2)
		{
			if (Alive)
			{
				Error(ex2);
			}
		}
	}

	private void ClearRows()
	{
		foreach (Node child in ((Node)_list).GetChildren(false))
		{
			((Node)_list).RemoveChild(child);
			child.QueueFree();
		}
		_choices.Clear();
	}

	private void ShowChoices()
	{
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Expected O, but got Unknown
		if (!Alive || _busy)
		{
			return;
		}
		_candidateRevision = SkinWorkshopService.CommunityRevision;
		_candidates = SkinWorkshopService.FilterSubmissionCandidates(_candidates);
		_selected.IntersectWith(_candidates.Select((WorkshopLocalSubmission item) => item.Id));
		ClearRows();
		((CanvasItem)_tools).Show();
		((CanvasItem)_back).Hide();
		((CanvasItem)_scan).Show();
		WorkshopLocalSubmission[] candidates = _candidates;
		foreach (WorkshopLocalSubmission local in candidates)
		{
			Button choice = Button(local.Name, delegate
			{
			});
			((BaseButton)choice).ToggleMode = true;
			choice.Alignment = (HorizontalAlignment)0;
			choice.ClipText = true;
			choice.TextOverrunBehavior = (OverrunBehavior)3;
			((Control)choice).TooltipText = local.Name;
			((BaseButton)choice).ButtonPressed = _selected.Contains(local.Id);
			choice.Text = (((BaseButton)choice).ButtonPressed ? "☑ " : "☐ ") + local.Name;
			((BaseButton)choice).Toggled += (ToggledEventHandler)delegate(bool pressed)
			{
				if (pressed)
				{
					_selected.Add(local.Id);
				}
				else
				{
					_selected.Remove(local.Id);
				}
				choice.Text = (pressed ? "☑ " : "☐ ") + local.Name;
				UpdateSelection();
			};
			_choices[local.Id] = choice;
			((Node)_list).AddChild((Node)(object)choice, false, (InternalMode)0);
		}
		Filter();
		UpdateSelection();
	}

	private void Filter()
	{
		WorkshopLocalSubmission[] candidates = _candidates;
		foreach (WorkshopLocalSubmission workshopLocalSubmission in candidates)
		{
			if (_choices.TryGetValue(workshopLocalSubmission.Id, out Button value))
			{
				((CanvasItem)value).Visible = _search.Text.Length == 0 || workshopLocalSubmission.Name.Contains(_search.Text, StringComparison.CurrentCultureIgnoreCase) || workshopLocalSubmission.Id.ToString().Contains(_search.Text);
			}
		}
	}

	private void SelectVisible(bool selected)
	{
		if (_busy)
		{
			return;
		}
		foreach (Button item in _choices.Values.Where((Button b) => ((CanvasItem)b).Visible))
		{
			((BaseButton)item).ButtonPressed = selected;
		}
	}

	private void UpdateSelection()
	{
		if (!_busy)
		{
			_status.Text = ((_candidates.Length == 0) ? T(SubmissionText.Empty) : (T(SubmissionText.Select) + $"  {_selected.Count}/{_candidates.Length}"));
			((BaseButton)_scan).Disabled = _selected.Count == 0;
		}
	}

	private async Task Scan()
	{
		if (_busy || _selected.Count == 0)
		{
			return;
		}
		_busy = true;
		((BaseButton)_scan).Disabled = true;
		foreach (Button value in _choices.Values)
		{
			((BaseButton)value).Disabled = true;
		}
		Progress<(int, int, string)> progress = new Progress<(int, int, string)>(delegate((int Current, int Total, string Name) p)
		{
			if (Alive && _busy)
			{
				_status.Text = T(SubmissionText.Progress, p.Current, p.Total, p.Name);
			}
		});
		try
		{
			(string[], int, int) tuple = await SkinWorkshopService.ScanSubmissions(_candidates.Where((WorkshopLocalSubmission c) => _selected.Contains(c.Id)).ToArray(), progress, _cancel.Token);
			if (!Alive)
			{
				return;
			}
			ClearRows();
			((CanvasItem)_tools).Hide();
			((CanvasItem)_scan).Hide();
			((CanvasItem)_back).Show();
			_status.Text = T(SubmissionText.Result, tuple.Item2, tuple.Item3, tuple.Item1.Length);
			for (int i = 0; i < tuple.Item1.Length; i++)
			{
				string code = tuple.Item1[i];
				HBoxContainer val = new HBoxContainer();
				((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
				((Node)_list).AddChild((Node)(object)val, false, (InternalMode)0);
				((Node)val).AddChild((Node)(object)Text((i + 1).ToString(), 20, accent: true), false, (InternalMode)0);
				LineEdit val2 = new LineEdit
				{
					Text = code,
					Editable = false,
					SizeFlagsHorizontal = (SizeFlags)3
				};
				ModThemeRuntime.Input(val2, 20);
				((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
				Button copy = null;
				copy = Button(T(SubmissionText.Copy), delegate
				{
					Copy(copy, code);
				});
				((Node)val).AddChild((Node)(object)copy, false, (InternalMode)0);
			}
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception ex2)
		{
			if (Alive)
			{
				Error(ex2);
			}
		}
		finally
		{
			_busy = false;
			if (Alive)
			{
				((BaseButton)_scan).Disabled = _selected.Count == 0;
				foreach (Button value2 in _choices.Values)
				{
					((BaseButton)value2).Disabled = false;
				}
			}
		}
	}

	private void Copy(Button button, string code)
	{
		try
		{
			DisplayServer.ClipboardSet(code);
			button.Text = T(SubmissionText.Copied);
			ModThemeRuntime.TextControl((Control)(object)button, 20, accent: true);
			((Node)this).GetTree().CreateTimer(1.0, true, false, true).Timeout += delegate
			{
				if (Alive && GodotObject.IsInstanceValid((GodotObject)(object)button) && !((GodotObject)button).IsQueuedForDeletion())
				{
					button.Text = T(SubmissionText.Copy);
					ModThemeRuntime.TextControl((Control)(object)button, 20);
				}
			};
		}
		catch (Exception ex)
		{
			Error(ex);
		}
	}

	private void OnInput(InputEvent ev)
	{
		if (Alive)
		{
			NModalContainer instance = NModalContainer.Instance;
			if (((instance != null) ? instance.OpenModal : null) == null && ev.IsActionPressed(StringName.op_Implicit("ui_cancel"), false, false) && !ev.IsEcho())
			{
				((Node)this).GetViewport().SetInputAsHandled();
				Close();
			}
		}
	}

	internal void Close()
	{
		if (!_closed)
		{
			_closed = true;
			_cancel.Cancel();
			_layer.Hide();
			((Node)_layer).QueueFree();
			_closedCallback();
		}
	}

	private void Cleanup()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		_closed = true;
		_cancel.Cancel();
		if (GodotObject.IsInstanceValid((GodotObject)(object)_window))
		{
			_window.WindowInput -= new WindowInputEventHandler(OnInput);
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_origin))
		{
			_origin.TreeExited -= Close;
		}
	}

	private void Error(Exception ex)
	{
		_status.Text = WorkshopText.Get(WorkshopTextKey.Failed);
		((Control)_status).TooltipText = ex.GetBaseException().Message;
		ModLog.Warn("工坊投稿：" + ex);
	}

	private static string T(SubmissionText key, params object[] values)
	{
		return WorkshopSubmissionText.Get(key, values);
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
			CustomMinimumSize = new Vector2(100f, 42f),
			MouseDefaultCursorShape = (CursorShape)2
		};
		ContextualSkinControls.ApplyGameTheme(val);
		((BaseButton)val).Pressed += pressed;
		return val;
	}
}
