using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class PauseMenuEntryToggles : Control
{
	internal const string NodeName = "SkinChangerEntryToggles";

	private const float PanelWidth = 560f;

	private const float PanelHeight = 56f;

	private const float LeftMargin = 24f;

	private const float BottomMargin = 18f;

	private const float FontSize = 20f;

	private const int BoxSize = 26;

	private NPauseMenu _pauseMenu;

	private HBoxContainer _content;

	private Button _appearanceBox;

	private Button _backgroundBox;

	private Label _appearanceCheck;

	private Label _backgroundCheck;

	private readonly List<Control> _hoverTargets = new List<Control>();

	private bool _refreshQueued;

	private bool _shown;

	internal static void Attach(NPauseMenu pauseMenu)
	{
		PauseMenuEntryToggles nodeOrNull = ((Node)pauseMenu).GetNodeOrNull<PauseMenuEntryToggles>(NodePath.op_Implicit("SkinChangerEntryToggles"));
		if (nodeOrNull != null)
		{
			nodeOrNull.SyncFromConfig();
			return;
		}
		PauseMenuEntryToggles pauseMenuEntryToggles = new PauseMenuEntryToggles();
		((Node)pauseMenuEntryToggles).Name = StringName.op_Implicit("SkinChangerEntryToggles");
		pauseMenuEntryToggles._pauseMenu = pauseMenu;
		PauseMenuEntryToggles pauseMenuEntryToggles2 = pauseMenuEntryToggles;
		((Node)pauseMenu).AddChild((Node)(object)pauseMenuEntryToggles2, false, (InternalMode)0);
		pauseMenuEntryToggles2.Build();
	}

	private void Build()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Expected O, but got Unknown
		try
		{
			((Control)this).SetAnchorsPreset((LayoutPreset)2, false);
			((Control)this).AnchorLeft = 0f;
			((Control)this).AnchorTop = 1f;
			((Control)this).AnchorRight = 0f;
			((Control)this).AnchorBottom = 1f;
			((Control)this).OffsetLeft = 24f;
			((Control)this).OffsetTop = -74f;
			((Control)this).OffsetRight = 584f;
			((Control)this).OffsetBottom = -18f;
			((Control)this).MouseFilter = (MouseFilterEnum)0;
			_content = new HBoxContainer
			{
				MouseFilter = (MouseFilterEnum)2,
				Alignment = (AlignmentMode)0
			};
			((Control)_content).AddThemeConstantOverride(StringName.op_Implicit("separation"), 30);
			((Control)_content).SetAnchorsPreset((LayoutPreset)15, false);
			((Node)this).AddChild((Node)(object)_content, false, (InternalMode)0);
			BuildToggle(ModText.ShowAppearanceEntryToggle, out _appearanceBox, out _appearanceCheck, delegate(bool pressed)
			{
				ApplyEntryVisibility(appearance: true, pressed);
			});
			BuildToggle(ModText.ShowBackgroundEntryToggle, out _backgroundBox, out _backgroundCheck, delegate(bool pressed)
			{
				ApplyEntryVisibility(appearance: false, pressed);
			});
			SyncFromConfig();
			_shown = true;
			ApplyShown(shown: false);
			((Control)this).MouseEntered += QueueRefresh;
			((Control)this).MouseExited += QueueRefresh;
			((CanvasItem)this).VisibilityChanged += QueueRefresh;
			((Control)this).GuiInput += new GuiInputEventHandler(OnGuiInput);
			WatchHoverTargets();
		}
		catch (Exception ex)
		{
			ModLog.Error("建立暂停菜单入口开关失败：" + ex);
			((Node)this).QueueFree();
		}
	}

	private void BuildToggle(ModText text, out Button box, out Label check, Action<bool> onToggled)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Expected O, but got Unknown
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected O, but got Unknown
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Expected O, but got Unknown
		HBoxContainer val = new HBoxContainer
		{
			MouseFilter = (MouseFilterEnum)1,
			Alignment = (AlignmentMode)1
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		Label label = new Label
		{
			VerticalAlignment = (VerticalAlignment)1,
			MouseFilter = (MouseFilterEnum)1
		};
		((Control)label).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 20);
		((Control)label).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff6e2"));
		((Control)label).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color("332f27"));
		((Control)label).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 4);
		Font gameFont = ContextualSkinControls.GameFont;
		if (gameFont != null)
		{
			((Control)label).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
		}
		Button val2 = new Button
		{
			ToggleMode = true,
			CustomMinimumSize = new Vector2(26f, 26f),
			SizeFlagsVertical = (SizeFlags)4,
			FocusMode = (FocusModeEnum)0,
			MouseDefaultCursorShape = (CursorShape)2,
			ClipText = true
		};
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("normal"), (StyleBox)(object)BoxStyle(checkedState: false, hovered: false));
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("hover"), (StyleBox)(object)BoxStyle(checkedState: false, hovered: true));
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("pressed"), (StyleBox)(object)BoxStyle(checkedState: true, hovered: false));
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("hover_pressed"), (StyleBox)(object)BoxStyle(checkedState: true, hovered: true));
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("focus"), (StyleBox)new StyleBoxEmpty());
		Label tick = new Label
		{
			Text = "✓",
			Visible = false,
			MouseFilter = (MouseFilterEnum)2,
			HorizontalAlignment = (HorizontalAlignment)1,
			VerticalAlignment = (VerticalAlignment)1
		};
		((Control)tick).SetAnchorsPreset((LayoutPreset)15, false);
		((Control)tick).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 20);
		((Control)tick).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("332f27"));
		Font gameFont2 = ContextualSkinControls.GameFont;
		if (gameFont2 != null)
		{
			((Control)tick).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont2);
		}
		((Node)val2).AddChild((Node)(object)tick, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)label, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)_content).AddChild((Node)(object)val, false, (InternalMode)0);
		((BaseButton)val2).Toggled += (ToggledEventHandler)delegate(bool pressed)
		{
			((CanvasItem)tick).Visible = pressed;
			onToggled(pressed);
		};
		ModLocalization.Bind((Node)(object)label, delegate
		{
			label.Text = ModLocalization.Get(text);
		});
		box = val2;
		check = tick;
	}

	private void ApplyEntryVisibility(bool appearance, bool pressed)
	{
		try
		{
			ModLog.Info($"暂停菜单入口开关：{(appearance ? "外观" : "背景")} = {pressed}");
			if (appearance)
			{
				SkinService.SetShowInRunAppearanceEntry(pressed);
				CharacterAppearancePauseMenu.Attach(_pauseMenu);
			}
			else
			{
				SkinService.SetShowInRunBackgroundEntry(pressed);
				BackgroundAppearancePauseMenu.Attach(_pauseMenu);
			}
		}
		catch (Exception ex)
		{
			ModLog.Error("切换暂停菜单入口显示失败：" + ex);
		}
	}

	internal void SyncFromConfig()
	{
		if (_appearanceBox != null && _backgroundBox != null)
		{
			bool flag = SkinService.ShouldShowInRunAppearanceEntry();
			bool flag2 = SkinService.ShouldShowInRunBackgroundEntry();
			((BaseButton)_appearanceBox).SetPressedNoSignal(flag);
			((BaseButton)_backgroundBox).SetPressedNoSignal(flag2);
			((CanvasItem)_appearanceCheck).Visible = flag;
			((CanvasItem)_backgroundCheck).Visible = flag2;
		}
	}

	private void WatchHoverTargets()
	{
		Watch((Control)(object)this);
		foreach (Node item in ((Node)_content).FindChildren("*", "Control", true, false))
		{
			Control val = (Control)(object)((item is Control) ? item : null);
			if (val != null)
			{
				Watch(val);
			}
		}
		void Watch(Control control)
		{
			if (control != this && !_hoverTargets.Contains(control))
			{
				_hoverTargets.Add(control);
				control.MouseEntered += QueueRefresh;
				control.MouseExited += QueueRefresh;
			}
		}
	}

	private void OnGuiInput(InputEvent input)
	{
		if (input is InputEventMouse)
		{
			QueueRefresh();
		}
	}

	private void QueueRefresh()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (_refreshQueued || !((Node)this).IsInsideTree() || ((GodotObject)this).IsQueuedForDeletion())
		{
			return;
		}
		_refreshQueued = true;
		Callable val = Callable.From((Action)delegate
		{
			_refreshQueued = false;
			if (GodotObject.IsInstanceValid((GodotObject)(object)this) && ((Node)this).IsInsideTree() && !((GodotObject)this).IsQueuedForDeletion())
			{
				Refresh();
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private void Refresh()
	{
		Viewport viewport = ((Node)this).GetViewport();
		Control val = ((viewport != null) ? viewport.GuiGetHoveredControl() : null);
		bool flag = val != null && (val == this || ((Node)this).IsAncestorOf((Node)(object)val));
		bool flag2 = ((CanvasItem)this).IsVisibleInTree() && viewport != null && ((Node)viewport).GetWindow().HasFocus();
		ApplyShown(flag2 && flag);
	}

	private void ApplyShown(bool shown)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (_content != null && GodotObject.IsInstanceValid((GodotObject)(object)_content) && shown != _shown)
		{
			_shown = shown;
			((CanvasItem)_content).Modulate = new Color(1f, 1f, 1f, shown ? 1f : 0f);
			((Control)_content).MouseFilter = (MouseFilterEnum)(shown ? 1 : 2);
		}
	}

	private static StyleBoxFlat BoxStyle(bool checkedState, bool hovered)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		return new StyleBoxFlat
		{
			BgColor = (checkedState ? new Color("efc850") : new Color(0f, 0f, 0f, 0.45f)),
			BorderColor = (Color)(hovered ? Colors.White : new Color("fff6e2")),
			BorderWidthLeft = 2,
			BorderWidthRight = 2,
			BorderWidthTop = 2,
			BorderWidthBottom = 2,
			CornerRadiusTopLeft = 4,
			CornerRadiusTopRight = 4,
			CornerRadiusBottomLeft = 4,
			CornerRadiusBottomRight = 4
		};
	}
}
