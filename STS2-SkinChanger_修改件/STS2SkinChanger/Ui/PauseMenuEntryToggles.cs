using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

/// <summary>
/// Bottom-left, hover-only checkboxes that decide whether the pause menu shows the Appearance
/// and Background entries. This replaces the old right-click gesture: hiding a button nobody
/// could bring back locked players out of in-run skin switching, so the switch to restore it
/// must always be discoverable when the pointer visits that corner.
/// </summary>
internal partial class PauseMenuEntryToggles : Control
{
    internal const string NodeName = "SkinChangerEntryToggles";

    private const float PanelWidth = 560f;
    private const float PanelHeight = 56f;
    private const float LeftMargin = 24f;
    private const float BottomMargin = 18f;
    private const float FontSize = 20f;
    private const int BoxSize = 26;

    private NPauseMenu _pauseMenu = null!;
    private HBoxContainer _content = null!;
    private Button _appearanceBox = null!;
    private Button _backgroundBox = null!;
    private Label _appearanceCheck = null!;
    private Label _backgroundCheck = null!;
    private readonly List<Control> _hoverTargets = [];
    private bool _refreshQueued;
    private bool _shown;

    internal static void Attach(NPauseMenu pauseMenu)
    {
        var existing = pauseMenu.GetNodeOrNull<PauseMenuEntryToggles>(NodeName);
        if (existing != null)
        {
            existing.SyncFromConfig();
            return;
        }

        var toggles = new PauseMenuEntryToggles { Name = NodeName, _pauseMenu = pauseMenu };
        pauseMenu.AddChild(toggles);
        toggles.Build();
    }

    private void Build()
    {
        try
        {
            SetAnchorsPreset(LayoutPreset.BottomLeft);
            AnchorLeft = 0f;
            AnchorTop = 1f;
            AnchorRight = 0f;
            AnchorBottom = 1f;
            OffsetLeft = LeftMargin;
            OffsetTop = -(PanelHeight + BottomMargin);
            OffsetRight = LeftMargin + PanelWidth;
            OffsetBottom = -BottomMargin;
            MouseFilter = MouseFilterEnum.Stop;

            _content = new HBoxContainer
            {
                MouseFilter = MouseFilterEnum.Ignore,
                Alignment = BoxContainer.AlignmentMode.Begin
            };
            _content.AddThemeConstantOverride("separation", 30);
            _content.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(_content);

            BuildToggle(ModText.ShowAppearanceEntryToggle,
                out _appearanceBox, out _appearanceCheck,
                pressed => ApplyEntryVisibility(appearance: true, pressed));
            BuildToggle(ModText.ShowBackgroundEntryToggle,
                out _backgroundBox, out _backgroundCheck,
                pressed => ApplyEntryVisibility(appearance: false, pressed));

            SyncFromConfig();
            // Start hidden: something in the corner before the pointer ever arrives is exactly the
            // kind of stray UI this panel must avoid. Make the first ApplyShown actually apply.
            _shown = true;
            ApplyShown(false);

            MouseEntered += QueueRefresh;
            MouseExited += QueueRefresh;
            VisibilityChanged += QueueRefresh;
            GuiInput += OnGuiInput;
            WatchHoverTargets();
        }
        catch (Exception exception)
        {
            ModLog.Error("建立暂停菜单入口开关失败：" + exception);
            QueueFree();
        }
    }

    private void BuildToggle(ModText text, out Button box, out Label check, Action<bool> onToggled)
    {
        var row = new HBoxContainer
        {
            MouseFilter = MouseFilterEnum.Pass,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        row.AddThemeConstantOverride("separation", 10);

        var label = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Pass
        };
        label.AddThemeFontSizeOverride("font_size", (int)FontSize);
        label.AddThemeColorOverride("font_color", new Color("fff6e2"));
        label.AddThemeColorOverride("font_outline_color", new Color("332f27"));
        label.AddThemeConstantOverride("outline_size", 4);
        if (ContextualSkinControls.GameFont is { } font)
        {
            label.AddThemeFontOverride("font", font);
        }

        var toggle = new Button
        {
            ToggleMode = true,
            CustomMinimumSize = new Vector2(BoxSize, BoxSize),
            // Without this the row stretches the box into a tall rectangle instead of a square.
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            FocusMode = FocusModeEnum.None,
            MouseDefaultCursorShape = CursorShape.PointingHand,
            ClipText = true
        };
        toggle.AddThemeStyleboxOverride("normal", BoxStyle(checkedState: false, hovered: false));
        toggle.AddThemeStyleboxOverride("hover", BoxStyle(checkedState: false, hovered: true));
        toggle.AddThemeStyleboxOverride("pressed", BoxStyle(checkedState: true, hovered: false));
        toggle.AddThemeStyleboxOverride("hover_pressed", BoxStyle(checkedState: true, hovered: true));
        toggle.AddThemeStyleboxOverride("focus", new StyleBoxEmpty());

        var tick = new Label
        {
            Text = "✓",
            Visible = false,
            MouseFilter = MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        tick.SetAnchorsPreset(LayoutPreset.FullRect);
        tick.AddThemeFontSizeOverride("font_size", BoxSize - 6);
        tick.AddThemeColorOverride("font_color", new Color("332f27"));
        if (ContextualSkinControls.GameFont is { } tickFont)
        {
            tick.AddThemeFontOverride("font", tickFont);
        }

        toggle.AddChild(tick);
        row.AddChild(label);
        row.AddChild(toggle);
        _content.AddChild(row);

        toggle.Toggled += pressed =>
        {
            tick.Visible = pressed;
            onToggled(pressed);
        };

        ModLocalization.Bind(label, () => label.Text = ModLocalization.Get(text));

        box = toggle;
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
        catch (Exception exception)
        {
            ModLog.Error("切换暂停菜单入口显示失败：" + exception);
        }
    }

    internal void SyncFromConfig()
    {
        if (_appearanceBox == null || _backgroundBox == null)
        {
            return;
        }

        var appearance = SkinService.ShouldShowInRunAppearanceEntry();
        var background = SkinService.ShouldShowInRunBackgroundEntry();
        _appearanceBox.SetPressedNoSignal(appearance);
        _backgroundBox.SetPressedNoSignal(background);
        _appearanceCheck.Visible = appearance;
        _backgroundCheck.Visible = background;
    }

    private void WatchHoverTargets()
    {
        Watch(this);
        foreach (var node in _content.FindChildren("*", "Control", true, false))
        {
            if (node is Control control)
            {
                Watch(control);
            }
        }

        void Watch(Control control)
        {
            if (control == this || _hoverTargets.Contains(control))
            {
                return;
            }

            _hoverTargets.Add(control);
            control.MouseEntered += QueueRefresh;
            control.MouseExited += QueueRefresh;
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
        if (_refreshQueued || !IsInsideTree() || IsQueuedForDeletion())
        {
            return;
        }

        _refreshQueued = true;
        Callable.From(() =>
        {
            _refreshQueued = false;
            if (!GodotObject.IsInstanceValid(this) || !IsInsideTree() || IsQueuedForDeletion())
            {
                return;
            }

            Refresh();
        }).CallDeferred();
    }

    private void Refresh()
    {
        var viewport = GetViewport();
        var hovered = viewport?.GuiGetHoveredControl();
        // The pointer can sit on the panel itself or on one of its labels and boxes; every one of
        // them must keep the panel revealed, otherwise moving onto the checkbox would flicker.
        var over = hovered != null && (hovered == this || IsAncestorOf(hovered));
        var visible = IsVisibleInTree() && viewport != null && viewport.GetWindow().HasFocus();
        ApplyShown(visible && over);
    }

    private void ApplyShown(bool shown)
    {
        if (_content == null || !GodotObject.IsInstanceValid(_content))
        {
            return;
        }

        if (shown == _shown)
        {
            return;
        }

        _shown = shown;
        _content.Modulate = new Color(1f, 1f, 1f, shown ? 1f : 0f);
        // While hidden the panel must not swallow clicks meant for the pause menu behind it.
        _content.MouseFilter = shown ? MouseFilterEnum.Pass : MouseFilterEnum.Ignore;
    }

    private static StyleBoxFlat BoxStyle(bool checkedState, bool hovered)
    {
        var style = new StyleBoxFlat
        {
            BgColor = checkedState ? new Color("efc850") : new Color(0f, 0f, 0f, 0.45f),
            BorderColor = hovered ? Colors.White : new Color("fff6e2"),
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4
        };
        return style;
    }
}

[HarmonyPatch]
internal static class PauseMenuEntryTogglesPatch
{
    private static IEnumerable<MethodBase> TargetMethods() =>
    [
        AccessTools.Method(typeof(NPauseMenu), nameof(NPauseMenu._Ready)),
        AccessTools.Method(typeof(NPauseMenu), nameof(NPauseMenu.OnSubmenuOpened))
    ];

    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NPauseMenu __instance) =>
        UiInjectionGuard.Run("PauseMenuEntryTogglesPatch.Postfix", () =>
            PauseMenuEntryToggles.Attach(__instance));
}
