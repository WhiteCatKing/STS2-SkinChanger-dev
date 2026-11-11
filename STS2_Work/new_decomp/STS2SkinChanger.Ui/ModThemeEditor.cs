using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class ModThemeEditor : CanvasLayer
{
	private const string NodeName = "SkinChangerThemeEditor";

	private Control _root;

	private PanelContainer _panel;

	private VBoxContainer _body;

	private ScrollContainer _scroll;

	private Button _collapse;

	private Button _save;

	private readonly ThemeSaveFeedback _saveFeedback = new ThemeSaveFeedback();

	private Label _status;

	private readonly List<Action<ModThemeSettings>> _readValues = new List<Action<ModThemeSettings>>();

	private bool _reading;

	private bool _collapsed;

	private bool _dragging;

	private Vector2 _dragOffset;

	private bool _initialized;

	private bool _connected;

	private Window? _window;

	private Button _presetCreate;

	private Button _copyPresetCode;

	private readonly ThemeSaveFeedback _copyFeedback = new ThemeSaveFeedback();

	private string _selectedPresetId = "default";

	private ModThemePresets? _presets;

	private PanelContainer? _presetPanel;

	private VBoxContainer _presetContent;

	private LineEdit _presetNewName;

	private Label _presetStatus;

	private readonly Dictionary<string, string> _presetNameDrafts = new Dictionary<string, string>();

	private readonly List<Action> _presetStates = new List<Action>();

	private string _activePresetId = "default";

	private string? _deletePresetId;

	private bool _positioningPresets;

	public ModThemeEditor()
	{
		((Node)this).TreeEntered += Connect;
		((Node)this).TreeExiting += Disconnect;
	}

	public static ModThemeEditor Ensure(Node context)
	{
		Window root = context.GetTree().Root;
		ModThemeEditor modThemeEditor = ((Node)root).GetNodeOrNull<ModThemeEditor>(NodePath.op_Implicit("SkinChangerThemeEditor"));
		if (modThemeEditor == null)
		{
			ModThemeEditor modThemeEditor2 = new ModThemeEditor();
			((Node)modThemeEditor2).Name = StringName.op_Implicit("SkinChangerThemeEditor");
			((CanvasLayer)modThemeEditor2).Layer = 110;
			((Node)modThemeEditor2).ProcessMode = (ProcessModeEnum)3;
			modThemeEditor = modThemeEditor2;
			((Node)root).AddChild((Node)(object)modThemeEditor, false, (InternalMode)0);
		}
		try
		{
			modThemeEditor.Initialize();
			return modThemeEditor;
		}
		catch
		{
			((Node)root).RemoveChild((Node)(object)modThemeEditor);
			((Node)modThemeEditor).QueueFree();
			throw;
		}
	}

	private void Initialize()
	{
		if (!_initialized)
		{
			BuildUi();
			_initialized = true;
		}
		Connect();
	}

	public static void Toggle(Node context)
	{
		Ensure(context).Toggle();
	}

	private void Toggle()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		((CanvasItem)_panel).Visible = !((CanvasItem)_panel).Visible;
		_dragging = false;
		CancelSaveFeedback();
		CancelPresetDelete();
		if (!((CanvasItem)_panel).Visible)
		{
			HidePresetPanel();
		}
		if (((CanvasItem)_panel).Visible)
		{
			ReadValues();
			ClampPanel();
		}
		ModLog.Info($"主题调节窗口：visible={((CanvasItem)_panel).Visible}, size={((Control)_panel).Size}, position={((Control)_panel).Position}");
	}

	private void BuildUi()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected O, but got Unknown
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Expected O, but got Unknown
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Expected O, but got Unknown
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Expected O, but got Unknown
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Expected O, but got Unknown
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Expected O, but got Unknown
		//IL_0e6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e79: Expected O, but got Unknown
		//IL_0ecc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ed6: Expected O, but got Unknown
		//IL_10e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_10ea: Expected O, but got Unknown
		_root = new Control
		{
			MouseFilter = (MouseFilterEnum)2
		};
		_root.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Node)this).AddChild((Node)(object)_root, false, (InternalMode)0);
		_panel = new PanelContainer
		{
			Position = new Vector2(36f, 100f),
			Size = new Vector2(540f, 750f),
			Visible = false,
			MouseFilter = (MouseFilterEnum)0
		};
		ModThemeRuntime.Panel(_panel);
		((Node)_root).AddChild((Node)(object)_panel, false, (InternalMode)0);
		MarginContainer val = new MarginContainer();
		string[] array = new string[4] { "left", "top", "right", "bottom" };
		foreach (string text in array)
		{
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_" + text), 16);
		}
		((Node)_panel).AddChild((Node)(object)val, false, (InternalMode)0);
		VBoxContainer val2 = new VBoxContainer();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		HBoxContainer val3 = new HBoxContainer();
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		Button grip = MakeButton(() => ModThemeLocalization.Get(ThemeText.Theme));
		grip.Flat = true;
		((Control)grip).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)grip).MouseDefaultCursorShape = (CursorShape)13;
		((Control)grip).GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I8
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			InputEventMouseButton val4 = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
			if (val4 != null && (long)val4.ButtonIndex == 1)
			{
				_dragging = val4.Pressed;
				_dragOffset = ((CanvasItem)_root).GetLocalMousePosition() - ((Control)_panel).Position;
				((Control)grip).AcceptEvent();
			}
		};
		((Node)val3).AddChild((Node)(object)grip, false, (InternalMode)0);
		_collapse = MakeButton(() => ModThemeLocalization.Get(_collapsed ? ThemeText.Expand : ThemeText.Collapse));
		((BaseButton)_collapse).Pressed += delegate
		{
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			_collapsed = !_collapsed;
			((CanvasItem)_body).Visible = !_collapsed;
			if (_collapsed)
			{
				HidePresetPanel();
			}
			_collapse.Text = ModThemeLocalization.Get(_collapsed ? ThemeText.Expand : ThemeText.Collapse);
			((Control)_panel).Size = new Vector2(540f, _collapsed ? 72f : Math.Min(750f, _root.Size.Y - 40f));
			ClampPanel();
			Callable val5 = Callable.From((Action)ClampPanel);
			((Callable)(ref val5)).CallDeferred(Array.Empty<Variant>());
		};
		((Node)val3).AddChild((Node)(object)_collapse, false, (InternalMode)0);
		Button val6 = MakeButton(() => ModLocalization.Get(ModText.Close));
		((BaseButton)val6).Pressed += Toggle;
		((Node)val3).AddChild((Node)(object)val6, false, (InternalMode)0);
		_body = new VBoxContainer
		{
			SizeFlagsVertical = (SizeFlags)3
		};
		((Control)_body).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		((Node)val2).AddChild((Node)(object)_body, false, (InternalMode)0);
		_scroll = new ScrollContainer
		{
			SizeFlagsVertical = (SizeFlags)3,
			HorizontalScrollMode = (ScrollMode)0
		};
		((Node)_body).AddChild((Node)(object)_scroll, false, (InternalMode)0);
		VBoxContainer val7 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val7).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		((Node)_scroll).AddChild((Node)(object)val7, false, (InternalMode)0);
		VBoxContainer rows = Section(val7, ThemeText.Panel);
		ColorRow(rows, ThemeText.Color, (ModThemeSettings s) => s.PanelColor, (ModThemeSettings s, string v) => s with
		{
			PanelColor = v
		});
		NumberRow(rows, ThemeText.Opacity, 0.0, 100.0, 1.0, (ModThemeSettings s) => s.PanelOpacity * 100f, (ModThemeSettings s, double v) => s with
		{
			PanelOpacity = (float)v / 100f
		}, "%");
		NumberRow(rows, ThemeText.Blur, 0.0, 5.0, 0.1, (ModThemeSettings s) => s.PanelBlur, (ModThemeSettings s, double v) => s with
		{
			PanelBlur = (float)v
		});
		rows = Section(val7, ThemeText.Selection);
		ColorRow(rows, ThemeText.Color, (ModThemeSettings s) => s.SelectionColor, (ModThemeSettings s, string v) => s with
		{
			SelectionColor = v
		});
		NumberRow(rows, ThemeText.Opacity, 0.0, 100.0, 1.0, (ModThemeSettings s) => s.SelectionOpacity * 100f, (ModThemeSettings s, double v) => s with
		{
			SelectionOpacity = (float)v / 100f
		}, "%");
		NumberRow(rows, ThemeText.Blur, 0.0, 5.0, 0.1, (ModThemeSettings s) => s.SelectionBlur, (ModThemeSettings s, double v) => s with
		{
			SelectionBlur = (float)v
		});
		ColorRow(rows, ThemeText.HoverColor, (ModThemeSettings s) => s.SelectionHoverColor, (ModThemeSettings s, string v) => s with
		{
			SelectionHoverColor = v
		});
		NumberRow(rows, ThemeText.HoverOpacity, 0.0, 100.0, 1.0, (ModThemeSettings s) => s.SelectionHoverOpacity * 100f, (ModThemeSettings s, double v) => s with
		{
			SelectionHoverOpacity = (float)v / 100f
		}, "%");
		NumberRow(rows, ThemeText.HoverBlur, 0.0, 5.0, 0.1, (ModThemeSettings s) => s.SelectionHoverBlur, (ModThemeSettings s, double v) => s with
		{
			SelectionHoverBlur = (float)v
		});
		rows = Section(val7, ThemeText.Buttons);
		ColorRow(rows, ThemeText.Color, (ModThemeSettings s) => s.ButtonColor, (ModThemeSettings s, string v) => s with
		{
			ButtonColor = v
		});
		NumberRow(rows, ThemeText.Opacity, 0.0, 100.0, 1.0, (ModThemeSettings s) => s.ButtonOpacity * 100f, (ModThemeSettings s, double v) => s with
		{
			ButtonOpacity = (float)v / 100f
		}, "%");
		NumberRow(rows, ThemeText.Blur, 0.0, 5.0, 0.1, (ModThemeSettings s) => s.ButtonBlur, (ModThemeSettings s, double v) => s with
		{
			ButtonBlur = (float)v
		});
		ColorRow(rows, ThemeText.HoverColor, (ModThemeSettings s) => s.HoverColor, (ModThemeSettings s, string v) => s with
		{
			HoverColor = v
		});
		rows = Section(val7, ThemeText.Dropdown);
		ColorRow(rows, ThemeText.Color, (ModThemeSettings s) => s.DropdownColor, (ModThemeSettings s, string v) => s with
		{
			DropdownColor = v
		});
		NumberRow(rows, ThemeText.Opacity, 0.0, 100.0, 1.0, (ModThemeSettings s) => s.DropdownOpacity * 100f, (ModThemeSettings s, double v) => s with
		{
			DropdownOpacity = (float)v / 100f
		}, "%");
		NumberRow(rows, ThemeText.Blur, 0.0, 5.0, 0.1, (ModThemeSettings s) => s.DropdownBlur, (ModThemeSettings s, double v) => s with
		{
			DropdownBlur = (float)v
		});
		ColorRow(rows, ThemeText.HoverColor, (ModThemeSettings s) => s.DropdownHoverColor, (ModThemeSettings s, string v) => s with
		{
			DropdownHoverColor = v
		});
		NumberRow(rows, ThemeText.HoverOpacity, 0.0, 100.0, 1.0, (ModThemeSettings s) => s.DropdownHoverOpacity * 100f, (ModThemeSettings s, double v) => s with
		{
			DropdownHoverOpacity = (float)v / 100f
		}, "%");
		ColorRow(rows, ThemeText.SelectionColor, (ModThemeSettings s) => s.DropdownSelectionColor, (ModThemeSettings s, string v) => s with
		{
			DropdownSelectionColor = v
		});
		NumberRow(rows, ThemeText.SelectionOpacity, 0.0, 100.0, 1.0, (ModThemeSettings s) => s.DropdownSelectionOpacity * 100f, (ModThemeSettings s, double v) => s with
		{
			DropdownSelectionOpacity = (float)v / 100f
		}, "%");
		ColorRow(rows, ThemeText.SelectedHoverColor, (ModThemeSettings s) => s.DropdownSelectionHoverColor, (ModThemeSettings s, string v) => s with
		{
			DropdownSelectionHoverColor = v
		});
		NumberRow(rows, ThemeText.SelectedHoverOpacity, 0.0, 100.0, 1.0, (ModThemeSettings s) => s.DropdownSelectionHoverOpacity * 100f, (ModThemeSettings s, double v) => s with
		{
			DropdownSelectionHoverOpacity = (float)v / 100f
		}, "%");
		ColorRow(rows, ThemeText.BorderColor, (ModThemeSettings s) => s.DropdownBorderColor, (ModThemeSettings s, string v) => s with
		{
			DropdownBorderColor = v
		});
		NumberRow(rows, ThemeText.BorderWidth, 0.0, 5.0, 1.0, (ModThemeSettings s) => s.DropdownBorderWidth, (ModThemeSettings s, double v) => s with
		{
			DropdownBorderWidth = (int)v
		});
		NumberRow(rows, ThemeText.Radius, 0.0, 24.0, 1.0, (ModThemeSettings s) => s.DropdownCornerRadius, (ModThemeSettings s, double v) => s with
		{
			DropdownCornerRadius = (int)v
		});
		rows = Section(val7, ThemeText.TextBorder);
		ColorRow(rows, ThemeText.TextColor, (ModThemeSettings s) => s.TextColor, (ModThemeSettings s, string v) => s with
		{
			TextColor = v
		});
		ColorRow(rows, ThemeText.AccentColor, (ModThemeSettings s) => s.AccentColor, (ModThemeSettings s, string v) => s with
		{
			AccentColor = v
		});
		ColorRow(rows, ThemeText.BorderColor, (ModThemeSettings s) => s.BorderColor, (ModThemeSettings s, string v) => s with
		{
			BorderColor = v
		});
		NumberRow(rows, ThemeText.BorderWidth, 0.0, 5.0, 1.0, (ModThemeSettings s) => s.BorderWidth, (ModThemeSettings s, double v) => s with
		{
			BorderWidth = (int)v
		});
		NumberRow(rows, ThemeText.Radius, 0.0, 24.0, 1.0, (ModThemeSettings s) => s.CornerRadius, (ModThemeSettings s, double v) => s with
		{
			CornerRadius = (int)v
		});
		NumberRow(rows, ThemeText.FontScale, 75.0, 150.0, 1.0, (ModThemeSettings s) => s.FontScale * 100f, (ModThemeSettings s, double v) => s with
		{
			FontScale = (float)v / 100f
		}, "%");
		NumberRow(rows, ThemeText.Outline, 0.0, 8.0, 1.0, (ModThemeSettings s) => s.TextOutline, (ModThemeSettings s, double v) => s with
		{
			TextOutline = (int)v
		});
		rows = Section(val7, ThemeText.Shadow);
		CheckButton shadow = new CheckButton();
		ModLocalization.Bind((Node)(object)shadow, delegate
		{
			((Button)shadow).Text = ModThemeLocalization.Get(ThemeText.EnableShadow);
		});
		ModThemeRuntime.Button((Button)(object)shadow, 18);
		Font gameFont = ContextualSkinControls.GameFont;
		if (gameFont != null)
		{
			((Control)shadow).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
		}
		((BaseButton)shadow).Toggled += (ToggledEventHandler)delegate(bool enabled)
		{
			if (!_reading)
			{
				ModThemeRuntime.Session.Preview(ModThemeRuntime.Current with
				{
					TextShadowEnabled = enabled
				});
			}
		};
		_readValues.Add(delegate(ModThemeSettings theme)
		{
			((BaseButton)shadow).SetPressedNoSignal(theme.TextShadowEnabled);
		});
		((Node)rows).AddChild((Node)(object)shadow, false, (InternalMode)0);
		ColorRow(rows, ThemeText.Color, (ModThemeSettings s) => s.TextShadowColor, (ModThemeSettings s, string v) => s with
		{
			TextShadowColor = v
		});
		NumberRow(rows, ThemeText.Opacity, 0.0, 100.0, 1.0, (ModThemeSettings s) => s.TextShadowOpacity * 100f, (ModThemeSettings s, double v) => s with
		{
			TextShadowOpacity = (float)v / 100f
		}, "%");
		NumberRow(rows, ThemeText.OffsetX, -12.0, 12.0, 1.0, (ModThemeSettings s) => s.TextShadowOffsetX, (ModThemeSettings s, double v) => s with
		{
			TextShadowOffsetX = (int)v
		});
		NumberRow(rows, ThemeText.OffsetY, -12.0, 12.0, 1.0, (ModThemeSettings s) => s.TextShadowOffsetY, (ModThemeSettings s, double v) => s with
		{
			TextShadowOffsetY = (int)v
		});
		NumberRow(rows, ThemeText.ShadowSize, 0.0, 8.0, 1.0, (ModThemeSettings s) => s.TextShadowSize, (ModThemeSettings s, double v) => s with
		{
			TextShadowSize = (int)v
		});
		HBoxContainer val8 = new HBoxContainer();
		((Node)_body).AddChild((Node)(object)val8, false, (InternalMode)0);
		Button val9 = MakeButton(() => ModThemeLocalization.Get(ThemeText.Presets));
		((BaseButton)val9).Pressed += TogglePresetPanel;
		((Node)val8).AddChild((Node)(object)val9, false, (InternalMode)0);
		Button val10 = MakeButton(() => ModThemeLocalization.Get(ThemeText.Revert));
		((BaseButton)val10).Pressed += ModThemeRuntime.Session.Revert;
		((Node)val8).AddChild((Node)(object)val10, false, (InternalMode)0);
		_save = MakeButton(SaveCaption);
		((Control)_save).SizeFlagsHorizontal = (SizeFlags)3;
		((BaseButton)_save).Pressed += SaveTheme;
		((Node)val8).AddChild((Node)(object)_save, false, (InternalMode)0);
		_status = MakeLabel(() => "");
		_status.AutowrapMode = (AutowrapMode)3;
		((CanvasItem)_status).Hide();
		((Node)_body).AddChild((Node)(object)_status, false, (InternalMode)0);
		_root.Resized += ClampPanel;
		ReadValues();
	}

	private void Connect()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		if (_initialized && !_connected && ((Node)this).IsInsideTree())
		{
			_window = ((Node)this).GetWindow();
			_window.WindowInput += new WindowInputEventHandler(HandleInput);
			ModThemeRuntime.Session.Changed += ReadValues;
			_connected = true;
			ReadValues();
		}
	}

	private void Disconnect()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		if (_connected)
		{
			_connected = false;
			_dragging = false;
			_saveFeedback.Cancel();
			HidePresetPanel();
			ModThemeRuntime.Session.Changed -= ReadValues;
			if (GodotObject.IsInstanceValid((GodotObject)(object)_window))
			{
				_window.WindowInput -= new WindowInputEventHandler(HandleInput);
			}
			_window = null;
		}
	}

	private void HandleInput(InputEvent input)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Invalid comparison between Unknown and I8
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!((CanvasItem)_panel).Visible || !_dragging)
		{
			return;
		}
		if (input is InputEventMouseMotion)
		{
			if (!Input.IsMouseButtonPressed((MouseButton)1) || !((Node)_root).GetWindow().HasFocus())
			{
				_dragging = false;
				return;
			}
			((Control)_panel).Position = ((CanvasItem)_root).GetLocalMousePosition() - _dragOffset;
			ClampPanel();
			((Node)this).GetViewport().SetInputAsHandled();
		}
		else
		{
			InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
			if (val != null && (long)val.ButtonIndex == 1 && !val.Pressed)
			{
				_dragging = false;
			}
		}
	}

	private void ClampPanel()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		if (!(_root.Size.X <= 0f) && !(_root.Size.Y <= 0f))
		{
			((Control)_panel).Size = new Vector2(Math.Min(540f, _root.Size.X - 24f), _collapsed ? 72f : Math.Min(750f, _root.Size.Y - 24f));
			((Control)_panel).Position = new Vector2(Math.Clamp(((Control)_panel).Position.X, 12f, Math.Max(12f, _root.Size.X - ((Control)_panel).Size.X - 12f)), Math.Clamp(((Control)_panel).Position.Y, 12f, Math.Max(12f, _root.Size.Y - ((Control)_panel).Size.Y - 12f)));
			PositionPresetPanel();
		}
	}

	private void ReadValues()
	{
		_reading = true;
		try
		{
			CancelSaveFeedback();
			foreach (Action<ModThemeSettings> readValue in _readValues)
			{
				readValue(ModThemeRuntime.Current);
			}
			RefreshPresetStates();
			PositionPresetPanel();
			if (_status != null)
			{
				_status.Text = "";
				((CanvasItem)_status).Hide();
			}
		}
		finally
		{
			_reading = false;
		}
	}

	private string SaveCaption()
	{
		if (!_saveFeedback.Active)
		{
			return ModLocalization.Get(ModText.SaveCharacterSkinMerge);
		}
		return ModThemeLocalization.Get(ThemeText.Saved) + "✓";
	}

	private void RefreshSaveButton()
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)_save))
		{
			_save.Text = SaveCaption();
			ModThemeRuntime.TextControl((Control)(object)_save, 18, _saveFeedback.Active);
		}
	}

	private void CancelSaveFeedback()
	{
		_saveFeedback.Cancel();
		RefreshSaveButton();
	}

	private void SaveTheme()
	{
		CancelSaveFeedback();
		try
		{
			ModThemeRuntime.Session.Save(ModThemeRuntime.Path);
			((CanvasItem)_status).Hide();
			int revision = _saveFeedback.Begin();
			RefreshSaveButton();
			((Node)this).GetTree().CreateTimer(1.0, true, false, true).Timeout += delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)this) && ((Node)this).IsInsideTree() && !((GodotObject)this).IsQueuedForDeletion() && _saveFeedback.Expire(revision))
				{
					RefreshSaveButton();
				}
			};
		}
		catch (Exception error)
		{
			ShowThemeError(error, null, null);
		}
	}

	private void ShowThemeError(Exception error, Label? target = null, ThemeText? text = null)
	{
		CancelSaveFeedback();
		Label? obj = target ?? _status;
		obj.Text = ModThemeLocalization.Get((ThemeText)(((int?)text) ?? ((error is FormatException) ? 39 : ((error is ArgumentException) ? 35 : 21))));
		((CanvasItem)obj).Show();
		ModLog.Error("主题操作失败：" + error);
	}

	private VBoxContainer Section(VBoxContainer rows, ThemeText title)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		((Node)rows).AddChild((Node)(object)val, false, (InternalMode)0);
		bool expanded = false;
		Button header = MakeButton(() => (expanded ? "▼ " : "▶ ") + ModThemeLocalization.Get(title));
		((BaseButton)header).ToggleMode = true;
		header.Alignment = (HorizontalAlignment)0;
		header.ClipText = true;
		header.TextOverrunBehavior = (OverrunBehavior)3;
		((Control)header).SizeFlagsHorizontal = (SizeFlags)3;
		((Node)val).AddChild((Node)(object)header, false, (InternalMode)0);
		VBoxContainer body = new VBoxContainer();
		((Control)body).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		((CanvasItem)body).Hide();
		((Node)val).AddChild((Node)(object)body, false, (InternalMode)0);
		((BaseButton)header).Toggled += (ToggledEventHandler)delegate(bool value)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			expanded = value;
			((CanvasItem)body).Visible = value;
			header.Text = (expanded ? "▼ " : "▶ ") + ModThemeLocalization.Get(title);
			Callable val2 = Callable.From((Action)ClampPanel);
			((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
		};
		return body;
	}

	private HBoxContainer Row(VBoxContainer rows, ThemeText title)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		HBoxContainer val = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(0f, 38f)
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)rows).AddChild((Node)(object)val, false, (InternalMode)0);
		Label val2 = MakeLabel(() => ModThemeLocalization.Get(title));
		((Control)val2).CustomMinimumSize = new Vector2(168f, 0f);
		val2.ClipText = true;
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		return val;
	}

	private void ColorRow(VBoxContainer rows, ThemeText title, Func<ModThemeSettings, string> get, Func<ModThemeSettings, string, ModThemeSettings> set)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		HBoxContainer obj = Row(rows, title);
		ColorPickerButton picker = new ColorPickerButton
		{
			EditAlpha = false,
			CustomMinimumSize = new Vector2(64f, 32f)
		};
		((Node)obj).AddChild((Node)(object)picker, false, (InternalMode)0);
		ModThemeRuntime.Button((Button)(object)picker, 18);
		Label hex = MakeLabel(() => get(ModThemeRuntime.Current));
		((Node)obj).AddChild((Node)(object)hex, false, (InternalMode)0);
		picker.PopupClosed += delegate
		{
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			if (!_reading && _connected && ((Node)this).IsInsideTree() && !((GodotObject)this).IsQueuedForDeletion())
			{
				ModThemeSession session = ModThemeRuntime.Session;
				Func<ModThemeSettings, string, ModThemeSettings> func = set;
				ModThemeSettings current = ModThemeRuntime.Current;
				Color color = picker.Color;
				session.Preview(func(current, "#" + ((Color)(ref color)).ToHtml(false)));
			}
		};
		_readValues.Add(delegate(ModThemeSettings theme)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			picker.Color = new Color(get(theme));
			hex.Text = get(theme);
		});
	}

	private void NumberRow(VBoxContainer rows, ThemeText title, double min, double max, double step, Func<ModThemeSettings, double> get, Func<ModThemeSettings, double, ModThemeSettings> set, string suffix = "")
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		HBoxContainer obj = Row(rows, title);
		HSlider slider = new HSlider
		{
			MinValue = min,
			MaxValue = max,
			Step = step,
			SizeFlagsHorizontal = (SizeFlags)3,
			CustomMinimumSize = new Vector2(120f, 24f),
			SizeFlagsVertical = (SizeFlags)4
		};
		((Node)obj).AddChild((Node)(object)slider, false, (InternalMode)0);
		SpinBox number = new SpinBox
		{
			MinValue = min,
			MaxValue = max,
			Step = step,
			Suffix = suffix,
			CustomMinimumSize = new Vector2(102f, 32f)
		};
		((Node)obj).AddChild((Node)(object)number, false, (InternalMode)0);
		ModThemeRuntime.Input(number.GetLineEdit(), 18);
		ThemeSlider(slider);
		((Range)slider).ValueChanged += new ValueChangedEventHandler(Change);
		((Range)number).ValueChanged += new ValueChangedEventHandler(Change);
		_readValues.Add(delegate(ModThemeSettings theme)
		{
			((Range)slider).SetValueNoSignal(get(theme));
			((Range)number).SetValueNoSignal(get(theme));
		});
		void Change(double value)
		{
			if (!_reading)
			{
				ModThemeRuntime.Session.Preview(set(ModThemeRuntime.Current, value));
			}
		}
	}

	private static Label MakeLabel(Func<string> text)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		Label label = new Label
		{
			VerticalAlignment = (VerticalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		ModThemeRuntime.TextControl((Control)(object)label, 18);
		Font gameFont = ContextualSkinControls.GameFont;
		if (gameFont != null)
		{
			((Control)label).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
		}
		ModLocalization.Bind((Node)(object)label, delegate
		{
			label.Text = text();
		});
		return label;
	}

	private static Button MakeButton(Func<string> text)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		Button button = new Button
		{
			CustomMinimumSize = new Vector2(62f, 36f),
			FocusMode = (FocusModeEnum)0
		};
		ModThemeRuntime.Button(button, 18);
		Font gameFont = ContextualSkinControls.GameFont;
		if (gameFont != null)
		{
			((Control)button).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
		}
		ModLocalization.Bind((Node)(object)button, delegate
		{
			button.Text = text();
		});
		return button;
	}

	private static void ThemeSlider(HSlider slider)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		StyleBoxFlat track = new StyleBoxFlat
		{
			ContentMarginTop = 2f,
			ContentMarginBottom = 2f
		};
		StyleBoxFlat fill = new StyleBoxFlat
		{
			ContentMarginTop = 2f,
			ContentMarginBottom = 2f
		};
		StyleBoxFlat hover = new StyleBoxFlat
		{
			ContentMarginTop = 2f,
			ContentMarginBottom = 2f
		};
		((Control)slider).AddThemeStyleboxOverride(StringName.op_Implicit("slider"), (StyleBox)(object)track);
		((Control)slider).AddThemeStyleboxOverride(StringName.op_Implicit("grabber_area"), (StyleBox)(object)fill);
		((Control)slider).AddThemeStyleboxOverride(StringName.op_Implicit("grabber_area_highlight"), (StyleBox)(object)hover);
		ModThemeRuntime.Bind((Node)(object)slider, "slider", delegate(ModThemeSettings theme)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			track.BgColor = ModThemeRuntime.Tint(theme.ButtonColor, theme.ButtonOpacity);
			fill.BgColor = new Color(theme.AccentColor);
			hover.BgColor = new Color(theme.HoverColor);
		});
	}

	private string PresetCreateCaption()
	{
		if (!ThemePresetCode.LooksLikeCode(_presetNewName.Text))
		{
			return ModLocalization.Get(ModText.SaveCurrentPreset);
		}
		return ModThemeLocalization.Get(ThemeText.ImportPreset);
	}

	private void SaveOrImportThemePreset()
	{
		string input = _presetNewName.Text;
		ModThemeSettings current = ModThemeRuntime.Current;
		QueueThemePresetAction(delegate
		{
			if (ThemePresetCode.LooksLikeCode(input))
			{
				ImportThemePreset(input);
			}
			else
			{
				_activePresetId = (_selectedPresetId = _presets.Create(input, current));
				ModThemeRuntime.Session.ApplyPreset(current);
			}
			if (_presetNewName.Text == input)
			{
				_presetNewName.Text = "";
			}
		});
	}

	private void ImportThemePreset(string code)
	{
		ModThemePreset modThemePreset = _presets.ImportCode(code, ModThemeLocalization.Get(ThemeText.ImportPreset));
		_activePresetId = (_selectedPresetId = modThemePreset.Id);
		ModThemeRuntime.Session.ApplyPreset(modThemePreset.Settings);
	}

	private string CopyPresetCaption()
	{
		if (!_copyFeedback.Active)
		{
			return ModThemeLocalization.Get(ThemeText.CopyPresetCode);
		}
		return ModThemeLocalization.Get(ThemeText.Copied) + "✓";
	}

	private void CopyThemePreset()
	{
		CancelCopyFeedback();
		try
		{
			DisplayServer.ClipboardSet(_presets.ExportCode(_selectedPresetId));
			((CanvasItem)_presetStatus).Hide();
			int revision = _copyFeedback.Begin();
			RefreshCopyButton();
			((Node)this).GetTree().CreateTimer(1.0, true, false, true).Timeout += delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)this) && ((Node)this).IsInsideTree() && !((GodotObject)this).IsQueuedForDeletion() && _copyFeedback.Expire(revision))
				{
					RefreshCopyButton();
				}
			};
		}
		catch (Exception error)
		{
			ShowThemeError(error, _presetStatus, ThemeText.CopyFailed);
		}
	}

	private void CancelCopyFeedback()
	{
		_copyFeedback.Cancel();
		RefreshCopyButton();
	}

	private void RefreshCopyButton()
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)_copyPresetCode))
		{
			_copyPresetCode.Text = CopyPresetCaption();
			ModThemeRuntime.TextControl((Control)(object)_copyPresetCode, 18, _copyFeedback.Active);
		}
	}

	private void TogglePresetPanel()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		PanelContainer? presetPanel = _presetPanel;
		if (presetPanel != null && ((CanvasItem)presetPanel).Visible)
		{
			HidePresetPanel();
			return;
		}
		try
		{
			if (_presetPanel == null)
			{
				BuildPresetPanel();
			}
			BuildPresetRows();
			((CanvasItem)_presetPanel).Show();
			((CanvasItem)_presetPanel).MoveToFront();
			PositionPresetPanel();
			Callable val = Callable.From((Action)PositionPresetPanel);
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
		catch (Exception error)
		{
			((CanvasItem)_panel).ItemRectChanged -= PositionPresetPanel;
			_presetStates.Clear();
			if (GodotObject.IsInstanceValid((GodotObject)(object)_presetPanel))
			{
				Node parent = ((Node)_presetPanel).GetParent();
				if (parent != null)
				{
					parent.RemoveChild((Node)(object)_presetPanel);
				}
				((Node)_presetPanel).QueueFree();
			}
			_presetPanel = null;
			_presetNewName = null;
			ShowThemeError(error, null, null);
		}
	}

	private void BuildPresetPanel()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		_presets = new ModThemePresets(SkinChangerPaths.Configuration("skin_changer_theme_presets.json"));
		_selectedPresetId = _presets.FindMatchingId(ModThemeRuntime.Current, _selectedPresetId) ?? "default";
		_presetPanel = new PanelContainer
		{
			Name = StringName.op_Implicit("ThemePresets"),
			Visible = false,
			MouseFilter = (MouseFilterEnum)0
		};
		ModThemeRuntime.Panel(_presetPanel);
		((Node)_root).AddChild((Node)(object)_presetPanel, false, (InternalMode)0);
		MarginContainer val = new MarginContainer();
		string[] array = new string[2] { "left", "right" };
		foreach (string text in array)
		{
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_" + text), 20);
		}
		array = new string[2] { "top", "bottom" };
		foreach (string text2 in array)
		{
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_" + text2), 18);
		}
		((Node)_presetPanel).AddChild((Node)(object)val, false, (InternalMode)0);
		_presetContent = new VBoxContainer
		{
			SizeFlagsVertical = (SizeFlags)3
		};
		((Control)_presetContent).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)val).AddChild((Node)(object)_presetContent, false, (InternalMode)0);
		((CanvasItem)_panel).ItemRectChanged += PositionPresetPanel;
		((Control)_presetPanel).Resized += PositionPresetPanel;
	}

	private void HidePresetPanel()
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)_presetPanel))
		{
			((CanvasItem)_presetPanel).Hide();
		}
		CancelCopyFeedback();
		CancelPresetDelete();
	}

	private void PositionPresetPanel()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (_positioningPresets || !GodotObject.IsInstanceValid((GodotObject)(object)_presetPanel) || !((CanvasItem)_presetPanel).Visible || !GodotObject.IsInstanceValid((GodotObject)(object)_root) || _root.Size.X <= 0f || _root.Size.Y <= 0f)
		{
			return;
		}
		_positioningPresets = true;
		try
		{
			var (position, val) = ThemePresetPanelLayout.Place(_root.Size, new Rect2(((Control)_panel).Position, ((Control)_panel).Size), new Vector2(820f, Math.Max(480f, ((Control)_presetPanel).GetCombinedMinimumSize().Y)));
			((Control)_panel).Position = position;
			((Control)_presetPanel).Size = ((Rect2)(ref val)).Size;
			((Control)_presetPanel).Position = ((Rect2)(ref val)).Position;
		}
		finally
		{
			_positioningPresets = false;
		}
	}

	private void BuildPresetRows()
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Expected O, but got Unknown
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Expected O, but got Unknown
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Expected O, but got Unknown
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		string text = (GodotObject.IsInstanceValid((GodotObject)(object)_presetNewName) ? _presetNewName.Text : "");
		CancelCopyFeedback();
		_presetStates.Clear();
		_deletePresetId = null;
		ScrollContainer obj = ScrollListRebuild.Begin(_presetContent, "theme-presets");
		Label val = MakeLabel(() => ModThemeLocalization.Get(ThemeText.Presets));
		val.HorizontalAlignment = (HorizontalAlignment)1;
		ModThemeRuntime.TextControl((Control)(object)val, 25, accent: true);
		((Node)_presetContent).AddChild((Node)(object)val, false, (InternalMode)0);
		HBoxContainer val2 = new HBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)_presetContent).AddChild((Node)(object)val2, false, (InternalMode)0);
		_presetNewName = new LineEdit
		{
			Text = text,
			MaxLength = 2048,
			CustomMinimumSize = new Vector2(0f, 40f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		ModThemeRuntime.Input(_presetNewName, 18);
		ModLocalization.Bind((Node)(object)_presetNewName, delegate
		{
			_presetNewName.PlaceholderText = ModThemeLocalization.Get(ThemeText.PresetNameOrCode);
		});
		((Node)val2).AddChild((Node)(object)_presetNewName, false, (InternalMode)0);
		_presetCreate = PresetButton(PresetCreateCaption, 138f);
		((Node)val2).AddChild((Node)(object)_presetCreate, false, (InternalMode)0);
		_presetNewName.TextChanged += (TextChangedEventHandler)delegate
		{
			_presetCreate.Text = PresetCreateCaption();
		};
		((BaseButton)_presetCreate).Pressed += SaveOrImportThemePreset;
		((Control)obj).CustomMinimumSize = new Vector2(0f, 96f);
		((Control)obj).SizeFlagsVertical = (SizeFlags)3;
		((Control)obj).SizeFlagsHorizontal = (SizeFlags)3;
		obj.HorizontalScrollMode = (ScrollMode)1;
		ScrollListRebuild.PlaceAfterHeader(obj);
		VBoxContainer val3 = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(744f, 0f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 7);
		((Node)obj).AddChild((Node)(object)val3, false, (InternalMode)0);
		foreach (ModThemePreset preset in _presets.Presets)
		{
			BuildPresetRow(val3, preset);
		}
		_presetStatus = MakeLabel(() => "");
		_presetStatus.AutowrapMode = (AutowrapMode)3;
		((CanvasItem)_presetStatus).Hide();
		((Node)_presetContent).AddChild((Node)(object)_presetStatus, false, (InternalMode)0);
		HBoxContainer val4 = new HBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)4
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)_presetContent).AddChild((Node)(object)val4, false, (InternalMode)0);
		Button val5 = PresetButton(() => ModLocalization.Get(ModText.Close), 90f);
		((BaseButton)val5).Pressed += HidePresetPanel;
		((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		_copyPresetCode = PresetButton(CopyPresetCaption, 180f);
		((BaseButton)_copyPresetCode).Pressed += CopyThemePreset;
		((Node)val4).AddChild((Node)(object)_copyPresetCode, false, (InternalMode)0);
		Button val6 = PresetButton(() => WorkshopCommunityText.Get(WorkshopCommunityTextKey.SubmitPreset), 140f);
		((BaseButton)val6).Pressed += delegate
		{
			try
			{
				WorkshopCommunityLinks.OpenDiscussion(presets: true);
			}
			catch (Exception error)
			{
				ShowThemeError(error, _presetStatus, null);
			}
		};
		((Node)val4).AddChild((Node)(object)val6, false, (InternalMode)0);
		RefreshPresetStates();
		Callable val7 = Callable.From((Action)PositionPresetPanel);
		((Callable)(ref val7)).CallDeferred(Array.Empty<Variant>());
	}

	private void BuildPresetRow(VBoxContainer rows, ModThemePreset preset)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Expected O, but got Unknown
		bool readOnly = preset.Id == "default";
		HBoxContainer val = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(0f, 46f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		((Node)rows).AddChild((Node)(object)val, false, (InternalMode)0);
		Label active = MakeLabel(() => (!(_activePresetId == preset.Id)) ? "" : "●");
		((Control)active).CustomMinimumSize = new Vector2(24f, 38f);
		active.HorizontalAlignment = (HorizontalAlignment)1;
		ModThemeRuntime.AccentText((Control)(object)active);
		((Node)val).AddChild((Node)(object)active, false, (InternalMode)0);
		LineEdit name = new LineEdit
		{
			Text = (readOnly ? ModThemeLocalization.Get(ThemeText.CoolJade) : _presetNameDrafts.GetValueOrDefault(preset.Id, preset.Name)),
			Editable = !readOnly,
			MaxLength = 100,
			CustomMinimumSize = new Vector2(0f, 38f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		ModThemeRuntime.Input(name, 18, readOnly);
		if (readOnly)
		{
			ModLocalization.Bind((Node)(object)name, delegate
			{
				name.Text = ModThemeLocalization.Get(ThemeText.CoolJade);
			});
		}
		name.TextChanged += (TextChangedEventHandler)delegate(string value)
		{
			_presetNameDrafts[preset.Id] = value;
			CancelPresetDelete();
		};
		((Node)val).AddChild((Node)(object)name, false, (InternalMode)0);
		Button apply = PresetButton(() => ModLocalization.Get((_activePresetId == preset.Id) ? ModText.ActiveCardPreset : ModText.ApplyCardPreset), 108f);
		((BaseButton)apply).Pressed += delegate
		{
			QueueThemePresetAction(delegate
			{
				_activePresetId = (_selectedPresetId = preset.Id);
				CancelCopyFeedback();
				ModThemeRuntime.Session.ApplyPreset(preset.Settings);
			}, rebuild: false);
		};
		((Node)val).AddChild((Node)(object)apply, false, (InternalMode)0);
		Button val2 = PresetButton(() => ModLocalization.Get(ModText.OverwriteCardPreset), 100f);
		((BaseButton)val2).Disabled = readOnly;
		((BaseButton)val2).Pressed += delegate
		{
			QueueThemePresetAction(delegate
			{
				_presets.Overwrite(preset.Id, ModThemeRuntime.Current);
				_activePresetId = (_selectedPresetId = preset.Id);
				ModThemeRuntime.Session.ApplyPreset(ModThemeRuntime.Current);
			});
		};
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		Button val3 = PresetButton(() => ModLocalization.Get(ModText.RenameCardPreset), 100f);
		((BaseButton)val3).Disabled = readOnly;
		((BaseButton)val3).Pressed += delegate
		{
			string requestedName = name.Text;
			QueueThemePresetAction(delegate
			{
				_presets.Rename(preset.Id, requestedName);
				_presetNameDrafts.Remove(preset.Id);
			});
		};
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		Button delete = PresetButton(() => ModLocalization.Get((_deletePresetId == preset.Id) ? ModText.ConfirmDeleteCardPreset : ModText.DeleteCardPreset), 108f);
		((BaseButton)delete).Disabled = readOnly;
		((BaseButton)delete).Pressed += delegate
		{
			if (!readOnly)
			{
				if (_deletePresetId != preset.Id)
				{
					_deletePresetId = preset.Id;
					RefreshPresetStates();
				}
				else
				{
					QueueThemePresetAction(delegate
					{
						_presets.Delete(preset.Id);
						_presetNameDrafts.Remove(preset.Id);
					});
				}
			}
		};
		((Node)val).AddChild((Node)(object)delete, false, (InternalMode)0);
		_presetStates.Add(delegate
		{
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			bool flag = _activePresetId == preset.Id;
			active.Text = (flag ? "●" : "");
			apply.Text = ModLocalization.Get(flag ? ModText.ActiveCardPreset : ModText.ApplyCardPreset);
			((BaseButton)apply).Disabled = flag;
			bool flag2 = _deletePresetId == preset.Id;
			delete.Text = ModLocalization.Get(flag2 ? ModText.ConfirmDeleteCardPreset : ModText.DeleteCardPreset);
			ModThemeRuntime.TextControl((Control)(object)delete, 18);
			if (flag2)
			{
				string[] array = new string[4] { "font_color", "font_hover_color", "font_pressed_color", "font_focus_color" };
				foreach (string text in array)
				{
					((Control)delete).AddThemeColorOverride(StringName.op_Implicit(text), new Color("ff7272"));
				}
			}
		});
	}

	private static Button PresetButton(Func<string> text, float width)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Button obj = MakeButton(text);
		((Control)obj).CustomMinimumSize = new Vector2(width, 38f);
		obj.ClipText = true;
		obj.TextOverrunBehavior = (OverrunBehavior)3;
		return obj;
	}

	private void RefreshPresetStates()
	{
		if (_presets == null || !GodotObject.IsInstanceValid((GodotObject)(object)_presetPanel))
		{
			return;
		}
		_activePresetId = _presets.FindMatchingId(ModThemeRuntime.Current, _activePresetId) ?? "";
		if (!_presets.Presets.Any((ModThemePreset p) => p.Id == _selectedPresetId))
		{
			_selectedPresetId = ((_activePresetId.Length > 0) ? _activePresetId : "default");
		}
		foreach (Action presetState in _presetStates)
		{
			presetState();
		}
	}

	private void CancelPresetDelete()
	{
		_deletePresetId = null;
		RefreshPresetStates();
	}

	private void QueueThemePresetAction(Action action, bool rebuild = true)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From((Action)delegate
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)this) || !((Node)this).IsInsideTree() || ((GodotObject)this).IsQueuedForDeletion())
			{
				return;
			}
			CancelPresetDelete();
			try
			{
				action();
				((CanvasItem)_presetStatus).Hide();
				if (rebuild)
				{
					BuildPresetRows();
				}
				else
				{
					RefreshPresetStates();
				}
			}
			catch (Exception error)
			{
				ShowThemeError(error, _presetStatus, null);
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}
}
