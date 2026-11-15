using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Core.Safety;

/// <summary>
/// 安全件共用的日志出口。安全件本身不引用 Godot / ModLog，便于在 LogicTests 里直接跑；
/// Entry 启动时把 <see cref="Sink"/> 接到 ModLog.Warn。
/// 同一个 key 在 <see cref="ThrottleMilliseconds"/> 内只输出一次，其余计数，下次输出时带上"另有 N 次"。
/// 这是"防刷屏"本身也属于熔断的一部分：报错风暴不能反过来把日志写爆。
/// </summary>
internal static class SafetyLog
{
    private static readonly object Gate = new();
    private static readonly Dictionary<string, (long LastMs, int Suppressed)> Recent = new(StringComparer.Ordinal);

    public static Action<string>? Sink { get; set; }

    public static Func<long> Clock { get; set; } = static () => Environment.TickCount64;

    public static long ThrottleMilliseconds { get; set; } = 30_000;

    /// <summary>不节流，直接输出（跳闸这种一会话一次的事件用它）。</summary>
    public static void Always(string message)
    {
        try
        {
            Sink?.Invoke(message);
        }
        catch
        {
            // 日志出口自身出错不能再往外抛。
        }
    }

    /// <summary>按 key 节流输出。返回 true 表示这次真的输出了。</summary>
    public static bool Throttled(string key, string message)
    {
        string? text = null;
        lock (Gate)
        {
            var now = Clock();
            if (Recent.TryGetValue(key, out var entry) && now - entry.LastMs < ThrottleMilliseconds)
            {
                Recent[key] = (entry.LastMs, entry.Suppressed + 1);
                return false;
            }

            var suppressed = Recent.TryGetValue(key, out entry) ? entry.Suppressed : 0;
            Recent[key] = (now, 0);
            text = suppressed > 0 ? $"{message}（此前 {suppressed} 次同类已省略）" : message;
        }

        Always(text);
        return true;
    }

    internal static void ResetForTests()
    {
        lock (Gate)
        {
            Recent.Clear();
        }
    }
}
