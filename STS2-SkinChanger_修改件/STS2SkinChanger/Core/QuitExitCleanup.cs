using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Modding;

namespace STS2SkinChanger.Core;

// The filesystem half of the exit path, separated from the engine boundary so the same code the
// game runs can be exercised against a real temporary storage. Stops accepting cache writes, then
// deletes what is not held open by this session.
internal static class QuitExitCleanup
{
    public static string Clean(SkinChangerStorage storage)
    {
        storage.StopCacheWrites();
        // Cache files the running game still has mounted cannot be deleted here; they are reported
        // and picked up by the cleanup the next start runs. Nothing outlives this process any more:
        // an earlier version started a helper executable for that job, which no platform without it
        // could run and which antivirus software read as a dropper.
        var report = storage.CleanCaches();
        var pending = report.Files.Count + report.Directories.Count;
        return $"退出缓存清理：已删除 {report.RemovedFiles} 个文件；" +
               (pending + report.Errors.Count == 0 ? "清理完成。" : $"未完成的 {pending} 项缓存清理将在下次启动重试。") +
               (report.Errors.Count == 0 ? "" : "部分目录无法检查：" + string.Join("；", report.Errors));
    }
}

// 退出耗时诊断（纯日志，不改行为）：定位「日志已写完但进程不退出」卡在谁身上。
[HarmonyLib.HarmonyPatch(typeof(NGame), nameof(NGame.Quit))]
internal static class QuitTimingDiagnostics
{
    private static readonly System.Diagnostics.Stopwatch Clock = new();

    [HarmonyLib.HarmonyPrefix]
    private static void Prefix()
    {
        Clock.Restart();
        ModLog.Info("[退出诊断] NGame.Quit 进入。");
    }

    [HarmonyLib.HarmonyPostfix]
    private static void Postfix()
    {
        ModLog.Info($"[退出诊断] NGame.Quit 返回，用时 {Clock.ElapsedMilliseconds} ms；此后若卡住即引擎侧收尾。");
    }
}

