using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using STS2SkinChanger.Core;
using STS2SkinChanger.Ui.LoadOrderWarning;

namespace STS2SkinChanger.Ui;

internal static class MixedCardArtDirectoryNotice
{
    private static readonly MixedCardArtDirectoryNoticeState State = new();

    internal static void Report(IReadOnlyList<MixedCardArtDirectoryConflict> conflicts) =>
        State.Report(conflicts);

    internal static void ResetForTests() =>
        State.Reset();

    internal static string FormatLogWarning(MixedCardArtDirectoryConflict conflict) =>
        $"检测到混放卡图目录「{conflict.DirectoryPath}」：同时包含已启用 Mod（{string.Join("、", conflict.ActiveModNames)}）" +
        $"与未加载/已禁用 Mod（{string.Join("、", conflict.UnloadedModNames)}）且存在 *.cardartpack.json；" +
        "其中的卡图无法按 Mod 区分，禁用不会让这些卡图失效，请把这些 Mod 分别放进各自的文件夹。";

    internal static string FormatPopupTitle(string language) =>
        ModLocalization.MixedCardArtDirectoryTitle(language);

    internal static string FormatPopupMessage(
        IReadOnlyList<MixedCardArtDirectoryConflict> conflicts,
        string language)
    {
        var separator = language.Equals("zhs", StringComparison.OrdinalIgnoreCase) ||
                        language.Equals("zht", StringComparison.OrdinalIgnoreCase) ||
                        language.Equals("jpn", StringComparison.OrdinalIgnoreCase)
            ? "、"
            : ", ";
        var folders = conflicts
            .Select(conflict => EscapeBbCode(conflict.FolderName))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return ModLocalization.MixedCardArtDirectoryMessage(string.Join(separator, folders), language);
    }

    internal static void OnMainMenuReady()
    {
        State.MarkMainMenuReached();
        TryShow();
    }

    internal static void ScheduleTryShowAfterClose()
    {
        // [延迟调度] 原触发时机：NModalContainer.Clear 后置补丁与 LoadOrderWarningController.ShowWarning 结束时触发；
        // 最早可执行帧：本帧末 idle/下一帧（当前关闭回调与确认 continuation 结束之后）；
        // 顺序依赖：需等 LoadOrderWarning 确认回调（含 RestartGame 置位 IsRestartRequested）及 NModalContainer.Clear 全部完成；
        // 能否合并：可合并（TryShow 内部经 MixedCardArtDirectoryNoticeState 幂等防重，同次启动仅弹出一次）；
        // 重试上限：无自动重试循环（仅由事件驱动触发，单次执行即止）；
        // 取消条件：已请求重启（IsRestartRequested）、本次启动已展示或无待提示混放目录、容器不可用/已有弹窗。
        DeferredDispatch.CallDeferred(
            "MixedCardArtDirectoryNotice.TryShowAfterClose",
            (object?)NModalContainer.Instance ?? "main_menu",
            static () => UiInjectionGuard.Run("MixedCardArtDirectoryNotice.TryShow.AfterClose", TryShow));
    }

    internal static void TryShow()
    {
        var container = NModalContainer.Instance;
        var restartRequested = LoadOrderWarningController.IsRestartRequested;
        var deferDisplay = container == null ||
                           container.OpenModal != null ||
                           LoadOrderWarningController.IsShowingOrPending;
        if (!State.TryTakeForMainMenu(out var conflicts, deferDisplay, restartRequested))
        {
            return;
        }

        TaskHelper.RunSafely(ShowPopup(container!, conflicts));
    }

    private static async Task ShowPopup(
        NModalContainer container,
        IReadOnlyList<MixedCardArtDirectoryConflict> conflicts)
    {
        var shown = false;
        try
        {
            var popup = NGenericPopup.Create();
            if (popup == null)
            {
                ModLog.Warn("游戏未能创建混放卡图目录提示框。");
                State.RestoreForRetry(conflicts);
                return;
            }

            container.Add(popup);
            var confirmation = popup.WaitForConfirmation(
                new LocString("main_menu_ui", "MOD_NOT_LOADED_POPUP.description"),
                new LocString("main_menu_ui", "MOD_NOT_LOADED_POPUP.title"),
                new LocString("main_menu_ui", "GENERIC_POPUP.cancel"),
                new LocString("main_menu_ui", "GENERIC_POPUP.confirm"));
            var verticalPopup = popup.GetNodeOrNull<NVerticalPopup>("VerticalPopup");
            if (verticalPopup == null)
            {
                ModLog.Error("混放卡图目录提示框缺少 VerticalPopup 节点，无法显示。");
                popup.QueueFree();
                State.RestoreForRetry(conflicts);
                return;
            }

            var language = ModLocalization.CurrentLanguage;
            verticalPopup.SetText(
                FormatPopupTitle(language),
                FormatPopupMessage(conflicts, language));
            verticalPopup.YesButton.SetText(ModLocalization.Get(ModText.Acknowledge));
            verticalPopup.NoButton.Hide();

            shown = true;
            ModLog.Info("已显示混放卡图目录提示框。");

            await confirmation;
            ModLog.Info("玩家确认已知晓混放卡图目录提示。");
        }
        catch (Exception exception)
        {
            ModLog.Error("显示混放卡图目录提示框失败：" + exception.GetBaseException().Message);
            if (!shown)
            {
                State.RestoreForRetry(conflicts);
            }
        }
    }

    private static string EscapeBbCode(string value) => value
        .Replace('[', '(')
        .Replace(']', ')')
        .Replace('\r', ' ')
        .Replace('\n', ' ');
}
