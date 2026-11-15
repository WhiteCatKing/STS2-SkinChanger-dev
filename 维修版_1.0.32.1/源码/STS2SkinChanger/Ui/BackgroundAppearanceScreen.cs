using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Core.Safety;
using STS2SkinChanger.Core.GameApi;

namespace STS2SkinChanger.Ui;

/// <summary>
/// In-run background switcher opened from the pause menu. It deliberately mirrors the
/// appearance panel: same hint line, same panel, a single "背景" section whose list contains
/// follow-category, game default, the detected provider background and the workshop entry.
/// The act is fixed to the act the player is currently in.
/// </summary>
internal partial class BackgroundAppearanceScreen : NSubmenu
{
    private const string BackButtonScenePath = "res://scenes/ui/back_button.tscn";
    private static WeakReference<BackgroundAppearanceScreen>? _openScreen;

    internal static bool IsOpen =>
        _openScreen != null && _openScreen.TryGetTarget(out var screen) &&
        NodeGuard.IsAliveInTree(screen) && screen.IsVisibleInTree();

    private PanelContainer _panel = null!;
    private VBoxContainer _content = null!;
    private Label _title = null!;
    private Label _sectionLabel = null!;
    private Label _hint = null!;
    private OptionButton _backgroundDropdown = null!;
    private NBackButton _backButton = null!;
    private BackgroundSkinService.BackgroundActEntry? _act;
    private bool _updating;

    protected override Control? InitialFocusedControl => _backgroundDropdown;

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        BuildInterface();
        ConnectSignals();
        ModLocalization.Bind(this, RefreshLocalizedText);
        RefreshLocalizedText();
    }

    public override void OnSubmenuOpened()
    {
        _openScreen = new WeakReference<BackgroundAppearanceScreen>(this);
        // The appearance panel keeps the paused combat visible; without this the game's
        // submenu backstop darkens the whole scene.
        NCapstoneContainer.Instance?.DisableBackstopInstantly();
        RefreshLocalizedText();
    }

    public override void OnSubmenuClosed()
    {
        if (_openScreen != null && _openScreen.TryGetTarget(out var screen) && ReferenceEquals(screen, this))
        {
            _openScreen = null;
        }

        NCapstoneContainer.Instance?.EnableBackstopInstantly();
        base.OnSubmenuClosed();
    }

    protected override void ConnectSignals()
    {
        _backButton.Connect(
            NClickableControl.SignalName.Released,
            Callable.From<NButton>(_ => _stack.Pop()));
        _backButton.Disable();
        VisibilityChanged += OnScreenVisibilityChanged;
        _backgroundDropdown.ItemSelected += OnBackgroundSelected;
    }

    private void OnScreenVisibilityChanged()
    {
        if (Visible)
        {
            _backButton.MoveToHidePosition();
            _backButton.Enable();
            RefreshLocalizedText();
            _backgroundDropdown.GrabFocus();
            return;
        }

        _backButton.Disable();
    }

    private void BuildInterface()
    {
        _hint = new Label
        {
            Name = "BackgroundHint",
            Position = new Vector2(34f, 28f),
            Size = new Vector2(900f, 64f),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore
        };
        ApplyTextTheme(_hint, 22);
        ModThemeRuntime.AccentText(_hint);
        AddChild(_hint);

        _panel = new PanelContainer
        {
            Name = "BackgroundPanel",
            AnchorLeft = 0.015f,
            AnchorTop = 0.07f,
            AnchorRight = 0.205f,
            AnchorBottom = 0.07f,
            GrowVertical = GrowDirection.End,
            MouseFilter = MouseFilterEnum.Stop,
            ZIndex = 10
        };
        _panel.AddThemeStyleboxOverride(
            "panel",
            ContextualSkinControls.CreateStyleBox(
                new Color(0.10f, 0.16f, 0.22f, 0.94f),
                new Color("7394ad"),
                2));
        ModThemeRuntime.Panel(_panel);
        AddChild(_panel);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 28);
        margin.AddThemeConstantOverride("margin_top", 24);
        margin.AddThemeConstantOverride("margin_right", 28);
        margin.AddThemeConstantOverride("margin_bottom", 24);
        _panel.AddChild(margin);

        _content = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(470f, 0f)
        };
        _content.AddThemeConstantOverride("separation", 7);
        margin.AddChild(_content);

        _title = BuildLabel(string.Empty, 31, HorizontalAlignment.Center);
        _title.CustomMinimumSize = new Vector2(0f, 46f);
        _content.AddChild(_title);
        _content.AddChild(BuildVerticalSpacer(12f));

        _sectionLabel = BuildLabel(string.Empty, 23);
        _sectionLabel.CustomMinimumSize = new Vector2(0f, 38f);
        ModThemeRuntime.AccentText(_sectionLabel);
        _content.AddChild(_sectionLabel);

        _backgroundDropdown = new OptionButton
        {
            CustomMinimumSize = new Vector2(0f, 50f),
            FitToLongestItem = false,
            ClipText = true,
            Alignment = HorizontalAlignment.Center,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        ContextualSkinControls.ApplyGameTheme(_backgroundDropdown);
        _content.AddChild(_backgroundDropdown);

        var backScene = ResourceLoader.Load<PackedScene>(BackButtonScenePath) ??
                        throw new InvalidOperationException("无法加载游戏返回按钮场景。");
        _backButton = backScene.Instantiate<NBackButton>(PackedScene.GenEditState.Disabled);
        _backButton.Name = "BackButton";
        _backButton.ZIndex = 20;
        AddChild(_backButton);
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
        _updating = true;
        _backgroundDropdown.Clear();
        if (_act == null)
        {
            _updating = false;
            return;
        }

        var selection = BackgroundSkinService.GetSelection(_act.ActId);
        var options = _act.Options;
        var defaultOption = BackgroundSkinService.DefaultOptionFor(_act.ActId);
        if (defaultOption != null &&
            options.All(option => !option.Id.Equals(defaultOption.Id, StringComparison.OrdinalIgnoreCase)))
        {
            options = options.Concat([defaultOption]).ToList();
        }

        foreach (var option in options)
        {
            var index = _backgroundDropdown.ItemCount;
            _backgroundDropdown.AddItem(option.DisplayName);
            _backgroundDropdown.SetItemMetadata(index, option.Id);
            if (option.Id.Equals(selection, StringComparison.OrdinalIgnoreCase))
            {
                _backgroundDropdown.Select(index);
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
        if (_updating || _act == null)
        {
            return;
        }

        var optionId = _backgroundDropdown.GetItemMetadata(checked((int)index)).AsString();
        if (SkinWorkshopEntry.Open(
                optionId,
                this,
                _act.ActId,
                PopulateBackgrounds,
                kind: "background"))
        {
            return;
        }

        if (!BackgroundSkinService.SetSelection(_act.ActId, optionId))
        {
            ModLog.Error("背景切换失败：" + (SkinService.LastError ?? string.Empty));
        }

        PopulateBackgrounds();
    }

    private static Control BuildVerticalSpacer(float height) => new()
    {
        CustomMinimumSize = new Vector2(0f, height),
        MouseFilter = MouseFilterEnum.Ignore
    };

    private static Label BuildLabel(
        string text,
        int fontSize,
        HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        var label = new Label
        {
            Text = text,
            HorizontalAlignment = alignment,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        ApplyTextTheme(label, fontSize);
        return label;
    }

    private static void ApplyTextTheme(Control control, int fontSize)
    {
        control.AddThemeColorOverride("font_color", new Color("fff6e2"));
        control.AddThemeColorOverride("font_hover_color", Colors.White);
        control.AddThemeColorOverride("font_pressed_color", new Color("efc850"));
        control.AddThemeColorOverride("font_outline_color", new Color("332f27"));
        control.AddThemeConstantOverride("outline_size", 4);
        control.AddThemeFontSizeOverride("font_size", fontSize);
        ModThemeRuntime.TextControl(control, fontSize);
        if (ContextualSkinControls.GameFont != null)
        {
            control.AddThemeFontOverride("font", ContextualSkinControls.GameFont);
        }
    }
}

internal static class BackgroundAppearancePauseMenu
{
    private const string ButtonName = "SkinChangerBackgroundAppearance";
    private const string ScreenName = "SkinChangerBackgroundAppearanceScreen";
    private const string PauseButtonScenePath = "res://scenes/pause_menu/pause_menu_button.tscn";

    internal static void Attach(NPauseMenu pauseMenu)
    {
        try
        {
            var container = pauseMenu.GetNode<Control>("%ButtonContainer");
            var existing = container.GetNodeOrNull<NPauseMenuButton>(ButtonName);
            var visibility = PauseMenuAppearanceEntryPolicy.Resolve(
                SkinService.ShouldShowInRunBackgroundEntry(),
                existing != null);
            if (existing != null)
            {
                existing.Visible = visibility.ShowButton;
                RebuildFocusNeighbors(container);
                return;
            }

            if (!visibility.CreateButton)
            {
                return;
            }

            var scene = ResourceLoader.Load<PackedScene>(PauseButtonScenePath) ??
                        throw new InvalidOperationException("无法加载暂停菜单按钮场景。");
            var button = scene.Instantiate<NPauseMenuButton>(PackedScene.GenEditState.Disabled);
            button.Name = ButtonName;
            container.AddChild(button);
            var anchor = container.GetNodeOrNull<NPauseMenuButton>(
                             "SkinChangerCharacterAppearance") ??
                         container.GetNodeOrNull<NPauseMenuButton>("Compendium");
            container.MoveChild(button, (anchor?.GetIndex() ?? (container.GetChildCount() - 2)) + 1);
            button.Connect(
                NClickableControl.SignalName.Released,
                Callable.From<NButton>(_ => Open(pauseMenu)));
            ModLocalization.Bind(button, () =>
                button.GetNode<MegaCrit.Sts2.addons.mega_text.MegaLabel>("Label")
                    .SetTextAutoSize(ModLocalization.Get(ModText.Background)));
            RebuildFocusNeighbors(container);
        }
        catch (Exception exception)
        {
            ModLog.Error("添加游戏内背景入口失败：" + exception);
        }
    }

    private static void Open(NPauseMenu pauseMenu)
    {
        try
        {
            if (pauseMenu.GetParent() is not NSubmenuStack stack)
            {
                throw new InvalidOperationException("找不到游戏内子菜单容器。");
            }

            var screen = stack.GetNodeOrNull<BackgroundAppearanceScreen>(ScreenName);
            if (screen == null)
            {
                screen = new BackgroundAppearanceScreen
                {
                    Name = ScreenName,
                    Visible = false,
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };
                stack.AddChild(screen);
            }

            stack.Push(screen);
        }
        catch (Exception exception)
        {
            ModLog.Error("打开游戏内背景界面失败：" + exception);
        }
    }

    private static void RebuildFocusNeighbors(Control container)
    {
        var buttons = container.GetChildren()
            .OfType<NPauseMenuButton>()
            .Where(button => button.Visible)
            .ToArray();
        for (var index = 0; index < buttons.Length; index++)
        {
            var button = buttons[index];
            button.FocusNeighborLeft = button.GetPath();
            button.FocusNeighborRight = button.GetPath();
            button.FocusNeighborTop = buttons[(index - 1 + buttons.Length) % buttons.Length].GetPath();
            button.FocusNeighborBottom = buttons[(index + 1) % buttons.Length].GetPath();
        }
    }
}

[HarmonyPatch]
internal static class BackgroundAppearancePauseMenuPatch
{
    private static IEnumerable<MethodBase> TargetMethods() => CombatBackgroundApi.PauseMenuTargetMethods();

    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NPauseMenu __instance) => Safe.Run("patch:background", () => BackgroundAppearancePauseMenu.Attach(__instance));
}
