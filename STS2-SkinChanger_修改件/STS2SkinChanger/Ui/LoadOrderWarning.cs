using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui.LoadOrderWarning;

internal static class LoadOrderWarningController
{
    private static bool _pending;
    private static bool _showingPopup;
    private static bool _restartRequested;
    private static bool _shownThisSession;
    private static bool _autoReorderedThisSession;
    private static LoadOrderPromptPlan? _pendingPlan;
    private static Mod[] _reorderTargets = [];

    internal static bool IsPending => _pending;
    internal static bool IsShowingOrPending => _pending || _showingPopup;
    internal static bool IsRestartRequested => _restartRequested;

    public static void Schedule()
    {
        if (_shownThisSession)
        {
            return;
        }

        var currentOrder = BuildOrderSnapshot();
        var previousOrder = SkinService.GetLastKnownSkinProviderOrder();
        var providersAboveSelf = ManagedSkinModLoader.SkinProvidersBeforeSelf;
        var hasExternalManager = LoadOrderRiskDetector.IsExternalLoadOrderManagerLoaded(ModManager.Mods);
        var dangerousProviders = LoadOrderRiskDetector.DetectDangerousProviders(
            providersAboveSelf,
            SkinService.Catalog);
        if (dangerousProviders.Count > 0)
        {
            ModLog.Warn(LoadOrderRiskDetector.FormatRiskLogLine(dangerousProviders));
        }

        var isFirstInstall = previousOrder == null && !SkinService.HasLoadOrderHistory();
        var isUpgradeBaseline = previousOrder == null && !isFirstInstall;
        var isLegacySnapshot = previousOrder != null && previousOrder.Any(Entry.IsSelfModId);
        var orderChanged = previousOrder != null &&
                           !isLegacySnapshot &&
                           !currentOrder.SequenceEqual(previousOrder, StringComparer.OrdinalIgnoreCase);

        if (dangerousProviders.Count == 0)
        {
            if (isUpgradeBaseline)
            {
                // Upgraded from a build that did not store an order snapshot. There is nothing to
                // compare against, so treat this launch as the baseline and stay silent; the next
                // launch can then honour the AND condition (order changed AND provider before us).
                SkinService.SetLastKnownSkinProviderOrder(currentOrder);
                ModLog.Info("已记录当前 Mod 顺序快照（没有上一次快照，不弹出加载顺序询问）。");
                return;
            }

            if (isLegacySnapshot)
            {
                // Snapshot written by the previous build stored every skin provider including self.
                // Migrate it to the new "providers above self only" format without prompting.
                SkinService.SetLastKnownSkinProviderOrder(currentOrder);
                ModLog.Info("已迁移旧版加载顺序快照为“本 Mod 之前的提供者”格式，不弹出询问。");
                return;
            }
        }

        var shouldShowWarning = SkinService.ShouldShowLoadOrderWarning(
            ManagedSkinModLoader.IsBeforeAllSkinProviders);
        var plan = LoadOrderRiskDetector.ResolvePromptPlan(
            hasExternalManager,
            isFirstInstall,
            isUpgradeBaseline || isLegacySnapshot,
            orderChanged,
            shouldShowWarning,
            providersAboveSelf,
            dangerousProviders);

        if (plan.Kind == LoadOrderPromptKind.None)
        {
            if (plan.SaveSnapshotOnSchedule)
            {
                SkinService.SetLastKnownSkinProviderOrder(currentOrder);
            }

            return;
        }

        if (plan.ShouldAutoMove)
        {
            if (!TryAutoMoveFirstInstall(providersAboveSelf))
            {
                return;
            }

            SkinService.SetLastKnownSkinProviderOrder([]);
        }
        else if (plan.SaveSnapshotOnSchedule)
        {
            SkinService.SetLastKnownSkinProviderOrder(currentOrder);
        }

        _pendingPlan = plan;
        _reorderTargets = providersAboveSelf.ToArray();
        _pending = true;
        ModLog.Info(DescribeScheduledPlan(plan, hasExternalManager));
        TryShow();
    }

    private static bool TryAutoMoveFirstInstall(IReadOnlyList<Mod> providersAboveSelf)
    {
        if (_autoReorderedThisSession)
        {
            return true;
        }

        try
        {
            MoveSelfBeforeSkinProviders(providersAboveSelf
                .Select(mod => mod.manifest?.id ?? string.Empty)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToArray());
            _autoReorderedThisSession = true;
            ModLog.Info(
                "已自动将皮肤切换器-Skin Changer 移到未手动上移的皮肤 Mod 之前；需要重启游戏后才能开始接管。");
            return true;
        }
        catch (Exception exception)
        {
            ModLog.Error("自动调整皮肤 Mod 加载顺序失败：" + exception.GetBaseException().Message);
            return false;
        }
    }

    private static string DescribeScheduledPlan(LoadOrderPromptPlan plan, bool hasExternalManager)
    {
        var names = string.Join("、", plan.ListedProviders.Select(LoadOrderRiskDetector.GetDisplayName));
        return plan.Kind switch
        {
            LoadOrderPromptKind.FirstInstallAutoMoved =>
                "已准备显示首次安装的加载顺序提示。",
            LoadOrderPromptKind.FirstInstallRiskAutoMoved =>
                "已自动调整首次安装顺序，准备显示提前加载危险提供者的重启提示：" + names,
            LoadOrderPromptKind.RiskChoice =>
                "检测到提前加载的危险皮肤提供者，准备弹出风险提示：" + names,
            LoadOrderPromptKind.RiskWithExternalManager =>
                "检测到 LoadOrderManager 且存在提前加载的危险皮肤提供者，准备显示手动调整提示（不自动移动）：" + names,
            LoadOrderPromptKind.OrderWithExternalManager =>
                "检测到 LoadOrderManager 且有皮肤提供者排在本 Mod 前面，准备显示手动调整提示（不自动移动）：" + names,
            _ =>
                $"检测到皮肤 Mod 排序变更且位于本 Mod 之前（LoadOrderManager={hasExternalManager}），准备询问是否移动本 Mod：{names}"
        };
    }

    // Only the providers currently above SkinChanger matter. Ordering changes after SkinChanger
    // (or among unrelated mods) must not trigger the prompt; comparing the full mod list was
    // unstable because other mod managers also re-sort the tail on every launch.
    private static string[] BuildOrderSnapshot() =>
        ManagedSkinModLoader.SkinProvidersBeforeSelf
            .Select(mod => mod.manifest?.id ?? string.Empty)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public static void TryShow()
    {
        var container = NModalContainer.Instance;
        if (!_pending || _shownThisSession || _pendingPlan == null ||
            container == null || container.OpenModal != null)
        {
            return;
        }

        _pending = false;
        _showingPopup = true;
        try
        {
            TaskHelper.RunSafely(ShowWarning(container, _pendingPlan));
        }
        catch
        {
            _showingPopup = false;
            throw;
        }
    }

    private static async Task ShowWarning(NModalContainer container, LoadOrderPromptPlan plan)
    {
        try
        {
            var popup = NGenericPopup.Create();
            if (popup == null)
            {
                ModLog.Warn("游戏未能创建加载顺序提示框。");
                _pending = true; // 在下一个钩子重试
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
                ModLog.Error("加载顺序提示框缺少 VerticalPopup 节点，无法显示。");
                popup.QueueFree();
                _pending = true;
                return;
            }

            verticalPopup.SetText(
                ModLocalization.Get(ModText.LoadOrderTitle),
                BuildPromptMessage(plan));
            if (plan.ShowRestartButton)
            {
                verticalPopup.YesButton.SetText(ModLocalization.Get(ModText.PrioritizeAndRestart));
                verticalPopup.NoButton.SetText(ModLocalization.Get(
                    plan.ShouldAutoMove ? ModText.Acknowledge : ModText.LoadOrderDecline));
            }
            else
            {
                verticalPopup.YesButton.SetText(ModLocalization.Get(ModText.Acknowledge));
                verticalPopup.NoButton.Hide();
            }

            _pendingPlan = null;
            _shownThisSession = true;
            ModLog.Info(plan.Kind switch
            {
                LoadOrderPromptKind.RiskChoice or LoadOrderPromptKind.RiskWithExternalManager =>
                    "已显示提前加载危险皮肤提供者的风险提示框。",
                LoadOrderPromptKind.OrderWithExternalManager =>
                    "已显示 LoadOrderManager 加载顺序手动调整提示框。",
                LoadOrderPromptKind.OrderChangedChoice =>
                    "已显示“是否将本 Mod 移到前面”的加载顺序询问框。",
                _ =>
                    "已显示需要重启才能生效的加载顺序提示框。"
            });

            var confirmed = await confirmation;
            if (!plan.ShowRestartButton)
            {
                ModLog.Info("玩家确认已知晓加载顺序提示（未执行自动移动或重启）。");
                return;
            }

            if (!plan.ShouldAutoMove)
            {
                if (!confirmed)
                {
                    if (plan.SaveSnapshotOnDecline)
                    {
                        // Save the current order so the same unchanged non-risky order does not prompt again.
                        SkinService.SetLastKnownSkinProviderOrder(BuildOrderSnapshot());
                    }

                    ModLog.Info("玩家选择“先不了”，保留当前 Mod 顺序。");
                    return;
                }

                try
                {
                    MoveSelfBeforeSkinProviders(_reorderTargets
                        .Select(mod => mod.manifest?.id ?? string.Empty)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .ToArray());
                    SkinService.SetLastKnownSkinProviderOrder([]);
                }
                catch (Exception exception)
                {
                    ModLog.Error("按玩家请求调整皮肤 Mod 加载顺序失败：" + exception.GetBaseException().Message);
                    verticalPopup.SetText(
                        ModLocalization.Get(ModText.LoadOrderTitle),
                        ModLocalization.Get(ModText.LoadOrderFailure) + "\n\n" +
                        exception.GetBaseException().Message);
                    return;
                }

                RestartGame(verticalPopup);
                return;
            }

            if (confirmed)
            {
                RestartGame(verticalPopup);
            }
            else
            {
                ModLog.Info("玩家选择“知道了”，已自动调整加载顺序；等待玩家手动重启后生效。");
            }
        }
        catch (Exception exception)
        {
            ModLog.Error("显示加载顺序提示框失败：" + exception.GetBaseException().Message);
            if (!_shownThisSession)
            {
                _pendingPlan = plan;
                _pending = true;
            }
        }
        finally
        {
            _showingPopup = false;
            if (!_restartRequested)
            {
                UiInjectionGuard.Run(
                    "MixedCardArtDirectoryNotice.ScheduleTryShowAfterClose.AfterLoadOrder",
                    MixedCardArtDirectoryNotice.ScheduleTryShowAfterClose);
            }
        }
    }

    internal static string BuildPromptMessage(LoadOrderPromptPlan plan)
    {
        if (plan.MessageKey is not { } messageKey)
        {
            return string.Empty;
        }

        var providers = plan.ListedProviders
            .Select(LoadOrderRiskDetector.GetDisplayName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .Select(name => "• " + EscapeBbCode(name))
            .ToArray();
        return ModLocalization.Get(messageKey)
            .Replace("{0}", string.Join("\n", providers), StringComparison.Ordinal);
    }

    private static void RestartGame(NVerticalPopup popup)
    {
        try
        {
            // Let Godot relaunch the same exported project after the game's normal shutdown
            // path completes. This is supported by both game versions and avoids depending on
            // PowerShell (which also made the button unusable on macOS).
            OS.SetRestartOnExit(true);
            _restartRequested = true;
            ModLog.Info("玩家选择“重启”，正在重启游戏以应用新的 Mod 加载顺序。");
            popup.QueueFree();
            Callable.From(() =>
            {
                if (NGame.Instance != null)
                {
                    NGame.Instance.Quit();
                }
                else
                {
                    (Engine.GetMainLoop() as SceneTree)?.Quit();
                }
            }).CallDeferred();
        }
        catch (Exception exception)
        {
            ModLog.Error("按玩家请求重启游戏失败：" + exception.GetBaseException().Message);
            popup.SetText(
                ModLocalization.Get(ModText.LoadOrderTitle),
                ModLocalization.Get(ModText.LoadOrderFailure) + "\n\n" +
                exception.GetBaseException().Message);
        }
    }

    private static string EscapeBbCode(string value) => value
        .Replace('[', '(')
        .Replace(']', ')')
        .Replace('\r', ' ')
        .Replace('\n', ' ');

    internal static bool ShouldPromptForOrderChange(bool orderChanged, int providersAboveCount) =>
        orderChanged && providersAboveCount > 0;

    private static void MoveSelfBeforeSkinProviders(string[] actionableProviderIds)
    {
        var self = ModManager.Mods.FirstOrDefault(mod => Entry.IsSelfModId(mod.manifest?.id)) ??
                   throw new InvalidOperationException("当前 Mod 列表中找不到皮肤切换器-Skin Changer。");
        var actionableIds = actionableProviderIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var skinProviders = ManagedSkinModLoader.SkinProvidersInLoadOrder
            .Where(provider => actionableIds.Contains(provider.manifest?.id ?? string.Empty))
            .ToArray();
        if (skinProviders.Length == 0)
        {
            throw new InvalidOperationException("当前 Mod 列表中找不到需要调整顺序的皮肤 Mod。");
        }

        var saveManager = SaveManager.Instance;
        var settings = saveManager.SettingsSave;
        var previousModSettings = settings.ModSettings;
        var modSettings = previousModSettings ?? new ModSettings();
        var previousOrder = modSettings.ModList;
        // Build the complete candidate before touching live settings. A stale/missing source
        // must not remove our entry from a list that other settings screens can later save.
        var modList = previousOrder.ToList();
        var wasEnabled = modList
            .FirstOrDefault(entry => Entry.IsSelfModId(entry.Id) && entry.Source == self.modSource)?
            .IsEnabled ?? true;
        modList.RemoveAll(entry => Entry.IsSelfModId(entry.Id));
        // MLM pins itself to the front of the saved list. Putting Skin Changer immediately
        // after it keeps both requirements true: MLM first, then Skin Changer, then the skin
        // providers. Without MLM fall back to inserting before the first provider.
        var managerIndex = modList.FindIndex(entry =>
            string.Equals(entry.Id, "ModLaunchManager", StringComparison.OrdinalIgnoreCase));
        var insertionIndex = managerIndex >= 0 ? managerIndex + 1 : -1;
        if (insertionIndex < 0)
        {
            insertionIndex = modList.FindIndex(entry => skinProviders.Any(provider =>
                string.Equals(entry.Id, provider.manifest?.id, StringComparison.OrdinalIgnoreCase) &&
                entry.Source == provider.modSource));
        }
        if (insertionIndex < 0)
        {
            // Older settings can omit the source on a retained entry. Fall back to the manifest
            // id while preserving every unrelated mod's relative position.
            insertionIndex = modList.FindIndex(entry => skinProviders.Any(provider =>
                string.Equals(entry.Id, provider.manifest?.id, StringComparison.OrdinalIgnoreCase)));
        }

        if (insertionIndex < 0)
        {
            throw new InvalidOperationException("无法在已保存的 Mod 顺序中定位皮肤 Mod。");
        }

        modList.Insert(insertionIndex, new SettingsSaveMod(self) { IsEnabled = wasEnabled });
        var persistence = AccessTools.Field(typeof(SaveManager), "_settingsSaveManager")?
            .GetValue(saveManager) as SettingsSaveManager ??
            throw new InvalidOperationException("无法取得游戏设置保存接口，保留原 Mod 加载顺序。");
        settings.ModSettings = modSettings;
        modSettings.ModList = modList;
        try
        {
            // SaveManager.SaveSettings catches IO failures and returns void. Use the same
            // native settings writer directly so Schedule only reports success after saving.
            persistence.SaveSettings();
            // MLM reads its own canonical file before the game loads mods. Keep that file in
            // sync with the order just saved, otherwise the next launch shows the old order,
            // writes it back, and the load-order prompt loops forever.
            MlmLaunchPlanSync.SyncFromSettings("重启前重排");
        }
        catch
        {
            modSettings.ModList = previousOrder;
            settings.ModSettings = previousModSettings;
            throw;
        }
    }

}

[HarmonyPatch(typeof(NModalContainer), nameof(NModalContainer._Ready))]
internal static class LoadOrderWarningModalReadyPatch
{
    private static void Postfix()
    {
        UiInjectionGuard.Run("LoadOrderWarningModalReadyPatch.Postfix", LoadOrderWarningController.TryShow);
        UiInjectionGuard.Run("MixedCardArtDirectoryNotice.TryShow.Ready", MixedCardArtDirectoryNotice.TryShow);
    }
}

[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
internal static class MainMenuLoadOrderWarningPatch
{
    private static void Postfix()
    {
        UiInjectionGuard.Run("MainMenuLoadOrderWarningPatch.Postfix", LoadOrderWarningController.Schedule);
        UiInjectionGuard.Run("MixedCardArtDirectoryNotice.OnMainMenuReady", MixedCardArtDirectoryNotice.OnMainMenuReady);
    }
}

[HarmonyPatch(typeof(NModalContainer), nameof(NModalContainer.Clear))]
internal static class LoadOrderWarningModalClosedPatch
{
    private static void Postfix()
    {
        UiInjectionGuard.Run("LoadOrderWarningModalClosedPatch.Postfix", LoadOrderWarningController.TryShow);
        UiInjectionGuard.Run("MixedCardArtDirectoryNotice.ScheduleTryShowAfterClose", MixedCardArtDirectoryNotice.ScheduleTryShowAfterClose);
    }
}
