using System.Diagnostics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class CacheWriteNotice
{
    private static readonly CacheWriteNoticeState State = new();
    private static SceneTree? _tree;
    private static PanelContainer? _banner;
    private static Label? _label;
    private static long _hideAt;
    private static string _language = "eng";
    private static string? _lastMessage;

    // Writers may run off the main thread: enqueue data without touching Godot.
    internal static string Report(CacheWriteRejectionReason reason)
    {
        State.Report(reason);
        return _lastMessage = ModLocalization.CacheWriteMessage(TextKey(reason), _language);
    }

    internal static void ShowInteractiveFailure(string? message)
    {
        if (string.IsNullOrEmpty(message) || message != _lastMessage) return;
        try { ShowBanner(message); }
        catch (Exception error) { ModLog.Warn("显示皮肤缓存提示失败：" + error.GetBaseException().Message); }
    }

    internal static ModText TextKey(CacheWriteRejectionReason reason) => reason switch
    {
        CacheWriteRejectionReason.DiskSpace => ModText.CacheDiskSpaceRejected,
        CacheWriteRejectionReason.ProtectedArchive => ModText.CacheWriteBlocked,
        _ => ModText.CacheBudgetRejected
    };

    internal static void Install(SceneTree tree)
    {
        if (ReferenceEquals(_tree, tree)) return;
        if (_tree != null && GodotObject.IsInstanceValid(_tree)) _tree.ProcessFrame -= OnFrame;
        _tree = tree;
        _language = ModLocalization.CurrentLanguage;
        // Runtime-loaded Node virtual callbacks are unreliable; use the native frame signal.
        tree.ProcessFrame += OnFrame;
    }

    private static void OnFrame()
    {
        try
        {
            State.SetRun(NRun.Instance);
            if (_banner != null && GodotObject.IsInstanceValid(_banner) && Stopwatch.GetTimestamp() >= _hideAt)
                _banner.Hide();
            if (!State.TryTake(out var reason, out var first)) return;
            _language = ModLocalization.CurrentLanguage;
            var message = ModLocalization.CacheWriteMessage(TextKey(reason), _language);
            var inAppearance = CharacterAppearanceScreen.ShowCacheWriteWarning(message);
            if (!first || inAppearance) return;
            ShowBanner(message);
        }
        catch (Exception error)
        {
            ModLog.Warn("显示皮肤缓存提示失败：" + error.GetBaseException().Message);
        }
    }

    private static void ShowBanner(string message)
    {
        if (_tree == null) return;
        if (_banner == null || !GodotObject.IsInstanceValid(_banner))
        {
            var layer = new CanvasLayer { Layer = 1000 };
            _tree.Root.AddChild(layer);
            _banner = new PanelContainer { MouseFilter = Control.MouseFilterEnum.Ignore, FocusMode = Control.FocusModeEnum.None };
            layer.AddChild(_banner);
            _banner.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopWide);
            _banner.OffsetLeft = 48; _banner.OffsetRight = -48;
            _banner.OffsetTop = 24; _banner.OffsetBottom = 134;
            _banner.AddThemeStyleboxOverride("panel", new StyleBoxFlat
            {
                BgColor = new Color(0.12f, 0.09f, 0.04f, 0.94f),
                ContentMarginLeft = 20, ContentMarginRight = 20,
                ContentMarginTop = 12, ContentMarginBottom = 12
            });
            _label = new Label
            {
                MouseFilter = Control.MouseFilterEnum.Ignore, FocusMode = Control.FocusModeEnum.None,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
            };
            _label.AddThemeFontSizeOverride("font_size", 24);
            if (ContextualSkinControls.GameFont is { } font) _label.AddThemeFontOverride("font", font);
            _label.AddThemeColorOverride("font_color", new Color(1f, 0.88f, 0.58f));
            _banner.AddChild(_label);
        }
        _label!.Text = message;
        _hideAt = Stopwatch.GetTimestamp() + 12 * Stopwatch.Frequency;
        _banner.Show();
    }
}

[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
internal static class CacheWriteNoticeReadyPatch
{
    private static void Postfix(NMainMenu __instance) =>
        UiInjectionGuard.Run("CacheWriteNoticeReadyPatch.Postfix", () =>
            CacheWriteNotice.Install(__instance.GetTree()));
}
