using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class BackgroundAppearanceScreen : NSubmenu
{
	private const string BackButtonScenePath = "res://scenes/ui/back_button.tscn";

	private PanelContainer _panel;

	private VBoxContainer _content;

	private Label _title;

	private Label _sectionLabel;

	private Label _hint;

	private OptionButton _backgroundDropdown;

	private NBackButton _backButton;

	private BackgroundSkinService.BackgroundActEntry? _act;

	private bool _updating;

	protected override Control? InitialFocusedControl => (Control?)(object)_backgroundDropdown;

	public override void _Ready()
	{
		((Control)this).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Control)this).MouseFilter = (MouseFilterEnum)0;
		BuildInterface();
		((NSubmenu)this).ConnectSignals();
		ModLocalization.Bind((Node)(object)this, RefreshLocalizedText);
		RefreshLocalizedText();
	}

	public override void OnSubmenuOpened()
	{
		NCapstoneContainer instance = NCapstoneContainer.Instance;
		if (instance != null)
		{
			instance.DisableBackstopInstantly();
		}
		RefreshLocalizedText();
	}

	public override void OnSubmenuClosed()
	{
		NCapstoneContainer instance = NCapstoneContainer.Instance;
		if (instance != null)
		{
			instance.EnableBackstopInstantly();
		}
		((NSubmenu)this).OnSubmenuClosed();
	}

	protected override void ConnectSignals()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		((GodotObject)_backButton).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			base._stack.Pop();
		}), 0u);
		((NClickableControl)_backButton).Disable();
		((CanvasItem)this).VisibilityChanged += OnScreenVisibilityChanged;
		_backgroundDropdown.ItemSelected += new ItemSelectedEventHandler(OnBackgroundSelected);
	}

	private void OnScreenVisibilityChanged()
	{
		if (((CanvasItem)this).Visible)
		{
			_backButton.MoveToHidePosition();
			((NClickableControl)_backButton).Enable();
			RefreshLocalizedText();
			((Control)_backgroundDropdown).GrabFocus();
		}
		else
		{
			((NClickableControl)_backButton).Disable();
		}
	}

	private void BuildInterface()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Expected O, but got Unknown
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Expected O, but got Unknown
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Expected O, but got Unknown
		_hint = new Label
		{
			Name = StringName.op_Implicit("BackgroundHint"),
			Position = new Vector2(34f, 28f),
			Size = new Vector2(900f, 64f),
			HorizontalAlignment = (HorizontalAlignment)0,
			VerticalAlignment = (VerticalAlignment)1,
			AutowrapMode = (AutowrapMode)3,
			MouseFilter = (MouseFilterEnum)2
		};
		ApplyTextTheme((Control)(object)_hint, 22);
		ModThemeRuntime.AccentText((Control)(object)_hint);
		((Node)this).AddChild((Node)(object)_hint, false, (InternalMode)0);
		_panel = new PanelContainer
		{
			Name = StringName.op_Implicit("BackgroundPanel"),
			AnchorLeft = 0.015f,
			AnchorTop = 0.07f,
			AnchorRight = 0.205f,
			AnchorBottom = 0.07f,
			GrowVertical = (GrowDirection)1,
			MouseFilter = (MouseFilterEnum)0,
			ZIndex = 10
		};
		((Control)_panel).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color(0.1f, 0.16f, 0.22f, 0.94f), new Color("7394ad"), 2));
		ModThemeRuntime.Panel(_panel);
		((Node)this).AddChild((Node)(object)_panel, false, (InternalMode)0);
		MarginContainer val = new MarginContainer();
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_left"), 28);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_top"), 24);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), 28);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_bottom"), 24);
		((Node)_panel).AddChild((Node)(object)val, false, (InternalMode)0);
		_content = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			CustomMinimumSize = new Vector2(470f, 0f)
		};
		((Control)_content).AddThemeConstantOverride(StringName.op_Implicit("separation"), 7);
		((Node)val).AddChild((Node)(object)_content, false, (InternalMode)0);
		_title = BuildLabel(string.Empty, 31, (HorizontalAlignment)1);
		((Control)_title).CustomMinimumSize = new Vector2(0f, 46f);
		((Node)_content).AddChild((Node)(object)_title, false, (InternalMode)0);
		((Node)_content).AddChild((Node)(object)BuildVerticalSpacer(12f), false, (InternalMode)0);
		_sectionLabel = BuildLabel(string.Empty, 23, (HorizontalAlignment)0);
		((Control)_sectionLabel).CustomMinimumSize = new Vector2(0f, 38f);
		ModThemeRuntime.AccentText((Control)(object)_sectionLabel);
		((Node)_content).AddChild((Node)(object)_sectionLabel, false, (InternalMode)0);
		_backgroundDropdown = new OptionButton
		{
			CustomMinimumSize = new Vector2(0f, 50f),
			FitToLongestItem = false,
			ClipText = true,
			Alignment = (HorizontalAlignment)1,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		ContextualSkinControls.ApplyGameTheme(_backgroundDropdown);
		((Node)_content).AddChild((Node)(object)_backgroundDropdown, false, (InternalMode)0);
		PackedScene val2 = ResourceLoader.Load<PackedScene>("res://scenes/ui/back_button.tscn", (string)null, (CacheMode)1) ?? throw new InvalidOperationException("无法加载游戏返回按钮场景。");
		_backButton = val2.Instantiate<NBackButton>((GenEditState)0);
		((Node)_backButton).Name = StringName.op_Implicit("BackButton");
		((CanvasItem)_backButton).ZIndex = 20;
		((Node)this).AddChild((Node)(object)_backButton, false, (InternalMode)0);
	}

	private void RefreshLocalizedText()
	{
		_hint.Text = ModLocalization.Get(ModText.BackgroundHint);
		_sectionLabel.Text = ModLocalization.Get(ModText.Background);
		_act = BackgroundSkinService.CurrentAct;
		if (_act != null)
		{
			BackgroundSkinService.EnsureActDefaultOption(_act.ActId);
			_act = BackgroundSkinService.CurrentAct;
		}
		_title.Text = _act?.Title ?? ModLocalization.Get(ModText.Background);
		PopulateBackgrounds();
	}

	private void PopulateBackgrounds()
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		_updating = true;
		_backgroundDropdown.Clear();
		if (_act == null)
		{
			_updating = false;
			return;
		}
		string selection = BackgroundSkinService.GetSelection(_act.ActId);
		List<BackgroundOption> list = _act.Options;
		BackgroundOption defaultOption = BackgroundSkinService.DefaultOptionFor(_act.ActId);
		if (defaultOption != null && list.All((BackgroundOption option) => !option.Id.Equals(defaultOption.Id, StringComparison.OrdinalIgnoreCase)))
		{
			list = list.Concat(new _003C_003Ez__ReadOnlySingleElementList<BackgroundOption>(defaultOption)).ToList();
		}
		foreach (BackgroundOption item in list)
		{
			int itemCount = _backgroundDropdown.ItemCount;
			_backgroundDropdown.AddItem(item.DisplayName, -1);
			_backgroundDropdown.SetItemMetadata(itemCount, Variant.op_Implicit(item.Id));
			if (item.Id.Equals(selection, StringComparison.OrdinalIgnoreCase))
			{
				_backgroundDropdown.Select(itemCount);
			}
		}
		SkinWorkshopEntry.Append(_backgroundDropdown);
		if (_backgroundDropdown.Selected < 0 && _backgroundDropdown.ItemCount > 0)
		{
			_backgroundDropdown.Select(0);
		}
		_updating = false;
	}

	private void OnBackgroundSelected(long index)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (_updating || _act == null)
		{
			return;
		}
		Variant itemMetadata = _backgroundDropdown.GetItemMetadata(checked((int)index));
		string optionId = ((Variant)(ref itemMetadata)).AsString();
		if (!SkinWorkshopEntry.Open(optionId, (Node)(object)this, _act.ActId, PopulateBackgrounds, cards: false, "background"))
		{
			if (!BackgroundSkinService.SetSelection(_act.ActId, optionId))
			{
				ModLog.Error("背景切换失败：" + (SkinService.LastError ?? string.Empty));
			}
			PopulateBackgrounds();
		}
	}

	private static Control BuildVerticalSpacer(float height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		return new Control
		{
			CustomMinimumSize = new Vector2(0f, height),
			MouseFilter = (MouseFilterEnum)2
		};
	}

	private static Label BuildLabel(string text, int fontSize, HorizontalAlignment alignment = 0L)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_002b: Expected O, but got Unknown
		Label val = new Label
		{
			Text = text,
			HorizontalAlignment = alignment,
			VerticalAlignment = (VerticalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		ApplyTextTheme((Control)val, fontSize);
		return val;
	}

	private static void ApplyTextTheme(Control control, int fontSize)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		control.AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff6e2"));
		control.AddThemeColorOverride(StringName.op_Implicit("font_hover_color"), Colors.White);
		control.AddThemeColorOverride(StringName.op_Implicit("font_pressed_color"), new Color("efc850"));
		control.AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color("332f27"));
		control.AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 4);
		control.AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), fontSize);
		ModThemeRuntime.TextControl(control, fontSize);
		if (ContextualSkinControls.GameFont != null)
		{
			control.AddThemeFontOverride(StringName.op_Implicit("font"), ContextualSkinControls.GameFont);
		}
	}
}
