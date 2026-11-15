using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Core.Safety;

/// <summary>
/// 按功能 key 的熔断器（需求 1、2）。两种跳闸条件，都按滑动时间窗计：
/// <list type="bullet">
/// <item>失败过多：窗口内 <see cref="BreakerPolicy.MaxFailures"/> 次失败（异常）。</item>
/// <item>调用过频：窗口内超过 <see cref="BreakerPolicy.MaxCalls"/> 次进入——用来挡住
/// "重建 → 刷新 → 补丁回放 → 再重建"这类互相触发的无限循环。</item>
/// </list>
/// 跳闸后该功能本会话停用（不做半开自动恢复：外观功能宁可停，也不要反复试把游戏拖崩），
/// 只打一行 <c>[熔断]</c>。调用方拿到 false 就走"保持原样 / 游戏原版"的退路。
/// </summary>
internal static class CircuitBreaker
{
    private sealed class State
    {
        public BreakerPolicy Policy = BreakerPolicy.Default;
        public readonly Queue<long> Calls = new();
        public readonly Queue<long> Failures = new();
        public string? TripReason;
    }

    private static readonly object Gate = new();
    private static readonly Dictionary<string, State> States = new(StringComparer.Ordinal);

    public static Func<long> Clock { get; set; } = static () => Environment.TickCount64;

    /// <summary>某个功能跳闸时触发（参数：key、原因）。用于让模块自己 Deactivate。</summary>
    public static event Action<string, string>? Tripped;

    public static void Configure(string key, BreakerPolicy policy)
    {
        lock (Gate)
        {
            Get(key).Policy = policy;
        }
    }

    public static bool IsOpen(string key)
    {
        lock (Gate)
        {
            return States.TryGetValue(key, out var state) && state.TripReason != null;
        }
    }

    public static string? TripReason(string key)
    {
        lock (Gate)
        {
            return States.TryGetValue(key, out var state) ? state.TripReason : null;
        }
    }

    /// <summary>
    /// 进入一次受保护的操作。返回 false 表示已跳闸（或这次进入刚好使它因过频跳闸），调用方应直接放弃。
    /// </summary>
    public static bool TryEnter(string key)
    {
        string? tripped = null;
        lock (Gate)
        {
            var state = Get(key);
            if (state.TripReason != null)
            {
                return false;
            }

            if (state.Policy.MaxCalls is { } maxCalls)
            {
                var now = Clock();
                Prune(state.Calls, now, state.Policy.WindowMilliseconds);
                state.Calls.Enqueue(now);
                if (state.Calls.Count > maxCalls)
                {
                    tripped = state.TripReason =
                        $"{state.Policy.WindowMilliseconds / 1000.0:0.#} 秒内调用 {state.Calls.Count} 次，超过上限 {maxCalls}（疑似循环触发）";
                }
            }
        }

        if (tripped != null)
        {
            OnTripped(key, tripped);
            return false;
        }

        return true;
    }

    public static void RecordFailure(string key, Exception? exception = null)
    {
        string? tripped = null;
        lock (Gate)
        {
            var state = Get(key);
            if (state.TripReason != null)
            {
                return;
            }

            var now = Clock();
            Prune(state.Failures, now, state.Policy.WindowMilliseconds);
            state.Failures.Enqueue(now);
            if (state.Failures.Count >= state.Policy.MaxFailures)
            {
                var last = exception == null
                    ? string.Empty
                    : $"；最后一次：{exception.GetBaseException().GetType().Name}: {exception.GetBaseException().Message}";
                tripped = state.TripReason =
                    $"{state.Policy.WindowMilliseconds / 1000.0:0.#} 秒内失败 {state.Failures.Count} 次{last}";
            }
        }

        if (tripped != null)
        {
            OnTripped(key, tripped);
        }
    }

    /// <summary>人为跳闸（例如磁盘守卫判定写满时连带停用某功能）。</summary>
    public static void Trip(string key, string reason)
    {
        lock (Gate)
        {
            var state = Get(key);
            if (state.TripReason != null)
            {
                return;
            }

            state.TripReason = reason;
        }

        OnTripped(key, reason);
    }

    public static IReadOnlyList<(string Key, string Reason)> OpenBreakers()
    {
        lock (Gate)
        {
            var result = new List<(string, string)>();
            foreach (var (key, state) in States)
            {
                if (state.TripReason != null)
                {
                    result.Add((key, state.TripReason));
                }
            }

            return result;
        }
    }

    internal static void ResetForTests()
    {
        lock (Gate)
        {
            States.Clear();
        }

        Tripped = null;
    }

    private static State Get(string key)
    {
        if (!States.TryGetValue(key, out var state))
        {
            state = new State();
            States[key] = state;
        }

        return state;
    }

    private static void Prune(Queue<long> queue, long now, long window)
    {
        while (queue.Count > 0 && now - queue.Peek() > window)
        {
            queue.Dequeue();
        }
    }

    private static void OnTripped(string key, string reason)
    {
        SafetyLog.Always($"[熔断] 功能「{key}」本会话已停用：{reason}。外观保持当前状态，重启游戏后恢复。");
        // 逐个订阅者调用：一个回调出错不能让后面的模块收不到停用通知。
        foreach (var handler in Tripped?.GetInvocationList() ?? [])
        {
            try
            {
                ((Action<string, string>)handler)(key, reason);
            }
            catch (Exception exception)
            {
                SafetyLog.Always($"[熔断] 功能「{key}」的停用回调出错（已忽略）：{exception.GetBaseException().Message}");
            }
        }
    }
}

/// <param name="MaxFailures">窗口内失败多少次跳闸。</param>
/// <param name="WindowMilliseconds">滑动窗口长度。</param>
/// <param name="MaxCalls">窗口内最多进入多少次；null 表示不限（只按失败跳闸）。</param>
internal readonly record struct BreakerPolicy(int MaxFailures, long WindowMilliseconds, int? MaxCalls = null)
{
    /// <summary>默认：60 秒内失败 5 次跳闸，不限调用频率。</summary>
    public static BreakerPolicy Default { get; } = new(5, 60_000);
}
