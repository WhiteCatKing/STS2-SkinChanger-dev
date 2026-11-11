namespace STS2SkinChanger.Core;

/// <summary>
/// 临时性能探针（玩家反馈"切皮肤要等很久"时用来定位，稳定后可整体删掉）。
///
/// 用法：<c>PerfTrace.Report("阶段名", 毫秒, 处理项数)</c>，
/// 超过 <see cref="WarnMilliseconds"/> 的阶段会额外打一条 WARN，方便直接在日志里 grep `[性能]`。
/// </summary>
internal static class PerfTrace
{
    /// <summary>总开关。玩家不需要这些日志时置 false 即可完全不产生输出。</summary>
    internal static bool Enabled { get; set; } = true;

    /// <summary>超过这个耗时就额外打 WARN（默认 200 ms，明显能感觉到的卡顿）。</summary>
    internal static double WarnMilliseconds { get; set; } = 200;

    internal static void Report(string name, double milliseconds, int count = -1)
    {
        if (!Enabled)
        {
            return;
        }

        var suffix = count >= 0 ? $"（{count} 项）" : string.Empty;
        var line = $"[性能] {name} = {milliseconds:F1} ms{suffix}";
        if (milliseconds >= WarnMilliseconds)
        {
            ModLog.Warn(line);
        }
        else
        {
            ModLog.Info(line);
        }
    }
}
