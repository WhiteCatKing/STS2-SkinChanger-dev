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

    // 仅在原版 Quit 正常返回（其保存调用已经执行）后监视引擎收尾。
    // 缓存清理和原版 Quit/保存方法的执行时长均不计入这 12 秒。
    private static void ArmForceExitWatchdog()
    {
        if (Interlocked.Exchange(ref _forceExitArmed, 1) != 0)
        {
            return;
        }

        var watchdog = new Thread(() =>
        {
            Thread.Sleep(TimeSpan.FromSeconds(12));
            ModLog.Warn("[退出诊断] 原版 Quit 返回后 12 秒进程仍未结束，强制结束引擎收尾。");
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
                // 引擎关停时会等后台加载任务收尾；先把皮肤预加载队列清空，
                // 避免「点退出后卡在 Steamworks shutdown 之后」的那种死等。
                SkinPreloader.CancelAll();
                return Task.Run(() => QuitExitCleanup.Clean(SkinChangerPaths.Storage));
            },
            () => { if (GodotObject.IsInstanceValid(__instance)) __instance.Quit(); },
            ModLog.Info);

    private static void Postfix(bool __runOriginal)
    {
        // Harmony also calls Postfix when Prefix suppressed Quit for asynchronous cleanup.
        if (__runOriginal && Coordinator.AllowNativeQuit) ArmForceExitWatchdog();
    }
}
