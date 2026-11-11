using System;
using Godot;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class GitHubScanPanel : Control
{
	private CanvasLayer _layer;

	private Window _window;

	private Node _origin;

	private Action _closedCallback;

	private bool _closed;

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

	internal static GitHubScanPanel Show(Node origin, string title, string details, GitHubScanResult result, Action closed)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		CanvasLayer val = new CanvasLayer
		{
			Name = StringName.op_Implicit("SCGitHubScanLayer"),
			Layer = 101,
			ProcessMode = (ProcessModeEnum)3
		};
		GitHubScanPanel gitHubScanPanel = new GitHubScanPanel
		{
			_layer = val,
			_origin = origin,
			_closedCallback = closed
		};
		try
		{
			((Node)origin.GetTree().Root).AddChild((Node)(object)val, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)gitHubScanPanel, false, (InternalMode)0);
			((Control)gitHubScanPanel).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
			gitHubScanPanel.Build(title, details, result);
			return gitHubScanPanel;
		}
		catch
		{
			gitHubScanPanel.Cleanup();
			((Node)val).QueueFree();
			throw;
		}
	}

	private void Build(string title, string details, GitHubScanResult result)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Expected O, but got Unknown
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Expected O, but got Unknown
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Expected O, but got Unknown
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Expected O, but got Unknown
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Expected O, but got Unknown
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Expected O, but got Unknown
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Expected O, but got Unknown
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Expected O, but got Unknown
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Expected O, but got Unknown
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Expected O, but got Unknown
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Expected O, but got Unknown
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
		Label val6 = new Label
		{
			Text = title,
			MouseFilter = (MouseFilterEnum)2
		};
		Font gameFont = ContextualSkinControls.GameFont;
		if (gameFont != null)
		{
			((Control)val6).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
		}
		ModThemeRuntime.TextControl((Control)(object)val6, 27, accent: true);
		((Node)val5).AddChild((Node)(object)val6, false, (InternalMode)0);
		Label val7 = new Label
		{
			MouseFilter = (MouseFilterEnum)2,
			ClipText = true
		};
		val7.TextOverrunBehavior = (OverrunBehavior)3;
		Font gameFont2 = ContextualSkinControls.GameFont;
		if (gameFont2 != null)
		{
			((Control)val7).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont2);
		}
		ModThemeRuntime.TextControl((Control)(object)val7, 20);
		((Node)val5).AddChild((Node)(object)val7, false, (InternalMode)0);
		Label val8 = new Label
		{
			MouseFilter = (MouseFilterEnum)2,
			AutowrapMode = (AutowrapMode)3
		};
		Font gameFont3 = ContextualSkinControls.GameFont;
		if (gameFont3 != null)
		{
			((Control)val8).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont3);
		}
		ModThemeRuntime.TextControl((Control)(object)val8, 18);
		((Node)val5).AddChild((Node)(object)val8, false, (InternalMode)0);
		ScrollContainer val9 = new ScrollContainer
		{
			SizeFlagsVertical = (SizeFlags)3,
			HorizontalScrollMode = (ScrollMode)0
		};
		((Node)val5).AddChild((Node)(object)val9, false, (InternalMode)0);
		VBoxContainer val10 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val10).AddThemeConstantOverride(StringName.op_Implicit("separation"), 6);
		((Node)val9).AddChild((Node)(object)val10, false, (InternalMode)0);
		HBoxContainer val11 = new HBoxContainer();
		((Control)val11).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)val5).AddChild((Node)(object)val11, false, (InternalMode)0);
		((Node)val11).AddChild((Node)(object)Button(ModLocalization.Get(ModText.Close), Close), false, (InternalMode)0);
		((Node)val11).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)2
		}, false, (InternalMode)0);
		((Node)val11).AddChild((Node)(object)Button(T(GitHubSkinTextKey.OpenRepo), delegate
		{
			OpenRepo(title);
		}), false, (InternalMode)0);
		if (result.Error.Length > 0)
		{
			val7.Text = T(GitHubSkinTextKey.ScanFailed);
			((Control)val7).TooltipText = result.Error;
			((CanvasItem)val8).Hide();
			return;
		}
		val7.Text = GitHubSkinText.Format(ModLocalization.CurrentLanguage, GitHubSkinTextKey.ScanResult, details, result.Codes.Length);
		((Control)val7).TooltipText = details;
		val8.Text = T(GitHubSkinTextKey.ScanHint);
		for (int j = 0; j < result.Codes.Length; j++)
		{
			string code = result.Codes[j];
			HBoxContainer val12 = new HBoxContainer();
			((Control)val12).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
			((Node)val10).AddChild((Node)(object)val12, false, (InternalMode)0);
			Label val13 = new Label
			{
				Text = (j + 1).ToString(),
				VerticalAlignment = (VerticalAlignment)1,
				MouseFilter = (MouseFilterEnum)2
			};
			Font gameFont4 = ContextualSkinControls.GameFont;
			if (gameFont4 != null)
			{
				((Control)val13).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont4);
			}
			ModThemeRuntime.TextControl((Control)(object)val13, 20, accent: true);
			((Node)val12).AddChild((Node)(object)val13, false, (InternalMode)0);
			LineEdit val14 = new LineEdit
			{
				Text = code,
				Editable = false,
				SizeFlagsHorizontal = (SizeFlags)3
			};
			ModThemeRuntime.Input(val14, 20);
			((Node)val12).AddChild((Node)(object)val14, false, (InternalMode)0);
			Button copy = null;
			copy = Button(WorkshopSubmissionText.Get(SubmissionText.Copy), delegate
			{
				Copy(copy, code);
			});
			((Node)val12).AddChild((Node)(object)copy, false, (InternalMode)0);
		}
	}

	private static void OpenRepo(string fullName)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (GitHubSkinPolicy.TryParseFullName(fullName, out GitHubRepoRef repo))
		{
			OS.ShellOpen(repo.Url);
		}
	}

	private void Copy(Button button, string code)
	{
		try
		{
			DisplayServer.ClipboardSet(code);
			button.Text = WorkshopSubmissionText.Get(SubmissionText.Copied);
			ModThemeRuntime.TextControl((Control)(object)button, 20, accent: true);
			((Node)this).GetTree().CreateTimer(1.0, true, false, true).Timeout += delegate
			{
				if (Alive && GodotObject.IsInstanceValid((GodotObject)(object)button) && !((GodotObject)button).IsQueuedForDeletion())
				{
					button.Text = WorkshopSubmissionText.Get(SubmissionText.Copy);
					ModThemeRuntime.TextControl((Control)(object)button, 20);
				}
			};
		}
		catch (Exception ex)
		{
			ModLog.Warn("复制信息码失败：" + ex.Message);
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
			_layer.Hide();
			((Node)_layer).QueueFree();
			_closedCallback();
		}
	}

	private void Cleanup()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		_closed = true;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_window))
		{
			_window.WindowInput -= new WindowInputEventHandler(OnInput);
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_origin))
		{
			_origin.TreeExited -= Close;
		}
	}

	private static string T(GitHubSkinTextKey key)
	{
		return GitHubSkinText.ForLanguage(ModLocalization.CurrentLanguage, key);
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
