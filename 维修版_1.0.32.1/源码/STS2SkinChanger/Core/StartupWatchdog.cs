using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace STS2SkinChanger.Core;

/// <summary>
/// 启动卡死检测。游戏启动时约四成概率卡在加载阶段（进程无响应但 CPU 空转），
/// 光看 godot.log 只知道「最后一行是什么」，不知道卡在哪一段、停了多久。
/// 这里用后台线程按阶段打点，某阶段停滞超过阈值就写一份现场报告，
/// 便于收集「卡住时到底走到哪一步」的证据。只在启动期工作，完成后自行退出。
/// </summary>
internal static class StartupWatchdog
{
    private const int StallSeconds = 45;
    private static readonly Stopwatch Clock = new();
    private static readonly List<string> Timeline = new();
    private static readonly object Gate = new();
    private static string _stage = "begin";
    private static long _stageStartedMs;
    private static bool _completed;
    private static bool _reported;

    public static void Begin()
    {
        if (Clock.IsRunning)
        {
            return;
        }

        Clock.Start();
        _stageStartedMs = 0;
        Timeline.Clear();
        AddTimeline("begin");
        var thread = new Thread(Watch)
        {
            IsBackground = true,
            Name = "SkinChanger.StartupWatchdog",
        };
        thread.Start();
        ModLog.Info("[启动看门] 已开始监控启动阶段。");
    }

    public static void Stage(string stage)
    {
        if (_completed)
        {
            return;
        }

        lock (Gate)
        {
            var now = Clock.ElapsedMilliseconds;
            var spent = now - _stageStartedMs;
            Timeline.Add($"{DateTime.Now:HH:mm:ss.fff} {stage}（上一阶段 {_stage} 用时 {spent} ms）");
            if (Timeline.Count > 64)
            {
                Timeline.RemoveAt(0);
            }

            ModLog.Info($"[启动看门] 阶段={stage}，上一阶段 {_stage} 用时 {spent} ms，累计 {now} ms。");
            _stage = stage;
            _stageStartedMs = now;
            _reported = false;
        }
    }

    public static void Complete()
    {
        if (_completed)
        {
            return;
        }

        _completed = true;
        var total = Clock.ElapsedMilliseconds;
        Clock.Stop();
        ModLog.Info($"[启动看门] 启动阶段结束，累计 {total} ms。");
    }

    private static void AddTimeline(string stage)
    {
        Timeline.Add($"{DateTime.Now:HH:mm:ss.fff} {stage}");
    }

    private static void Watch()
    {
        while (true)
        {
            Thread.Sleep(2000);
            if (_completed)
            {
                return;
            }

            long stageMs;
            long totalMs;
            string stage;
            lock (Gate)
            {
                stage = _stage;
                stageMs = Clock.ElapsedMilliseconds - _stageStartedMs;
                totalMs = Clock.ElapsedMilliseconds;
                if (_reported || stageMs < StallSeconds * 1000L)
                {
                    continue;
                }

                _reported = true;
            }

            try
            {
                WriteReport(stage, stageMs, totalMs);
                ModLog.Warn($"[启动看门] 阶段「{stage}」已停滞 {stageMs / 1000} 秒（累计 {totalMs / 1000} 秒），已写出启动现场报告。");
            }
            catch (Exception exception)
            {
                ModLog.Warn($"[启动看门] 写现场报告失败：{exception.GetBaseException().Message}");
            }
        }
    }

    private static void WriteReport(string stage, long stageMs, long totalMs)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"时间 / Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"卡住阶段 / Stalled stage: {stage}");
        builder.AppendLine($"该阶段已持续 / In stage: {stageMs / 1000.0:0.0} s");
        builder.AppendLine($"启动累计 / Total: {totalMs / 1000.0:0.0} s");
        builder.AppendLine($"进程 / Process: {Environment.ProcessPath}");
        builder.AppendLine($"工作集 / WorkingSet: {Environment.WorkingSet / (1024 * 1024)} MB");
        builder.AppendLine($"处理器数 / Cores: {Environment.ProcessorCount}");
        builder.AppendLine();
        builder.AppendLine("阶段时间线 / Timeline:");
        lock (Gate)
        {
            foreach (var line in Timeline)
            {
                builder.AppendLine("  " + line);
            }
        }

        var root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SlayTheSpire2", "Gurio.SkinChanger");
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "startup_hang.txt");
        File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
        ModLog.Warn($"[启动看门] 现场报告已写入 {path}");
    }
}
