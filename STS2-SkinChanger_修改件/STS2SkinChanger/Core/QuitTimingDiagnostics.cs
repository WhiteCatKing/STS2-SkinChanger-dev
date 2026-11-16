using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace STS2SkinChanger.Core;

// 退出耗时诊断（纯日志，不改行为）：区分「卡在我们自己的清理里」与「日志写完后卡在引擎侧收尾」。
[HarmonyPatch(typeof(NGame), nameof(NGame.Quit))]
internal static class QuitTimingDiagnostics
{
    private static readonly System.Diagnostics.Stopwatch Clock = new();

    [HarmonyPrefix]
    private static void Prefix()
    {
        Clock.Restart();
        ModLog.Info("[退出诊断] NGame.Quit 进入。");
    }

    [HarmonyPostfix]
    private static void Postfix(bool __runOriginal)
    {
        if (!__runOriginal) return;
        CrashDiagnostics.MarkNativeQuitCompleted();
        ModLog.Info($"[退出诊断] NGame.Quit 返回，用时 {Clock.ElapsedMilliseconds} ms；此后若卡住即引擎侧收尾。");
    }
}
