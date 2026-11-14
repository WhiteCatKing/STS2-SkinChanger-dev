using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace STS2SkinChanger.Core;

// Intercept the game's final Quit entry, not the confirmation popup. Cancelling the
// popup must do nothing, and the native settings/progress saves still run unchanged.
// A Quit that arrives before this mod has finished initializing is not an exit and is rejected
// without a side effect; see QuitInterceptionPolicy for the reported session behind that.
[HarmonyPatch(typeof(NGame), nameof(NGame.Quit))]
internal static class QuitCacheCleanup
{
    private static readonly QuitCleanupCoordinator Coordinator = new();

    // Called by the entry patch once the whole start-up chain has finished, which is the moment a
    // Quit call starts to mean the player is leaving.
    internal static void MarkInitializationCompleted() => Coordinator.MarkInitializationCompleted();

    // Whether a Quit call currently counts as the player leaving.
    internal static bool AcceptsQuitAsExit => Coordinator.InitializationCompleted;

    private static int _forceExitArmed;

    // 引擎关停偶尔会挂在后台资源加载上（日志停在 "Steamworks shutdown succeeded!"，
    // 存档早已写完）。这种时候进程永远不会自己退出，只能兜底强杀；12 秒足够正常运行走完关闭。
    private static void ArmForceExitWatchdog()
    {
        if (Interlocked.Exchange(ref _forceExitArmed, 1) != 0)
        {
            return;
        }

        var watchdog = new Thread(() =>
        {
            Thread.Sleep(TimeSpan.FromSeconds(12));
            ModLog.Warn("[退出诊断] 退出 12 秒仍未结束（存档已写完），强制结束进程。");
            System.Environment.Exit(0);
        })
        {
            IsBackground = true,
            Name = "SkinChanger.QuitWatchdog",
        };
        watchdog.Start();
    }

    private static bool Prefix(NGame __instance) =>
        Coordinator.HandleQuit(
            () =>
            {
                ArmForceExitWatchdog();
                // 引擎关停时会等后台加载任务收尾；先把皮肤预加载队列清空，
                // 避免「点退出后卡在 Steamworks shutdown 之后」的那种死等。
                SkinPreloader.CancelAll();
                return Task.Run(() => QuitExitCleanup.Clean(SkinChangerPaths.Storage));
            },
            () => { if (GodotObject.IsInstanceValid(__instance)) __instance.Quit(); },
            ModLog.Info);
}
