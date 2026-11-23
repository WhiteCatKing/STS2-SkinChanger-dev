using Godot;

namespace STS2SkinChanger.Core;

/// <summary>
/// 启动阶段"延后初始化"的皮肤 DLL 队列（见 <see cref="ProviderActivationSchedulePolicy"/>）。
///
/// 主菜单一出来，就每 0.12 s 取一个提供者做作者初始化器 + Harmony 补丁安装；单次 tick 最多
/// 占 45 ms（大包本身就会超，这时这个 tick 只做它一个），剩下的交给下一 tick。这样启动时被
/// 阻塞的时间只剩"角色/随从/商人"那一批，其余工作分散在菜单的空闲帧里完成。
///
/// 三个约束：
///   * 进入真实场景（选角预览、对局、房间）时立刻 <see cref="Cancel"/>：从那一刻起由场景范围
///     决定谁该激活，后台再激活"看不见的提供者"会把刚收窄的补丁范围又放开。
///   * 队列只由最近一次重建决定（<see cref="Schedule"/> 整体替换），所以不存在"取消选择后
///     还在后台激活"的旧任务。
///   * 任何异常只记一条警告并放弃这一项，绝不影响换肤本身。
/// </summary>
internal static class DeferredProviderActivator
{
    private const double TickSeconds = 0.12;
    private const double MaxTickMilliseconds = 12;

    private static readonly Queue<string> Pending = new();
    private static readonly object Sync = new();
    private static bool _timerRunning;
    private static bool _reportedFailure;
    private static int _completed;
    private static int _skipped;
    private static int _failed;
    private static double _totalMs;
    private static double _maxMs;
    private static string? _slowestProvider;

    internal static bool HasPending
    {
        get
        {
            lock (Sync)
            {
                return Pending.Count > 0;
            }
        }
    }

    /// <summary>用这次重建得到的"延后"列表整体替换队列。传空列表等于取消。</summary>
    internal static void Schedule(IEnumerable<string> providerIds)
    {
        var next = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var providerId in providerIds)
        {
            if (!string.IsNullOrWhiteSpace(providerId) && seen.Add(providerId))
            {
                next.Add(providerId);
            }
        }

        lock (Sync)
        {
            Pending.Clear();
            foreach (var providerId in next)
            {
                Pending.Enqueue(providerId);
            }

            if (Pending.Count == 0)
            {
                return;
            }

            ModLog.Info(
                $"[性能] 启动阶段延后初始化 {Pending.Count} 个皮肤提供者的 DLL：" +
                "先让主菜单出来，再在后台逐个完成。");
        }

        EnsureTimer();
    }

    /// <summary>目录重建/重载时清空，不写"完成"汇总。</summary>
    internal static void Reset()
    {
        lock (Sync)
        {
            Pending.Clear();
            _completed = 0;
            _skipped = 0;
            _failed = 0;
            _totalMs = 0;
            _maxMs = 0;
            _slowestProvider = null;
            _reportedFailure = false;
        }
    }

    /// <summary>进入真实场景时调用：剩下的交给场景范围按需激活。</summary>
    internal static void Cancel(string reason)
    {
        int remaining;
        lock (Sync)
        {
            remaining = Pending.Count;
            Pending.Clear();
            WriteSummaryLocked("已停止");
        }

        if (remaining > 0)
        {
            ModLog.Info(
                $"[性能] 停止启动阶段的延后初始化（还有 {remaining} 个没轮到）：{reason}；" +
                "这些提供者会在进入它们所属的场景时按场景范围激活。");
        }
    }

    private static void EnsureTimer()
    {
        lock (Sync)
        {
            if (_timerRunning)
            {
                return;
            }

            _timerRunning = true;
        }

        try
        {
            if (Engine.GetMainLoop() is not SceneTree tree)
            {
                lock (Sync)
                {
                    _timerRunning = false;
                    Pending.Clear();
                }

                ModLog.Warn("[性能] 当前不是场景树环境，跳过皮肤提供者的延后初始化。");
                return;
            }

            // [延迟调度/Timer] 原触发时机：启动重建将非首屏提供者入队后由 SceneTreeTimer(0.12s) 周期唤醒；
            // 最早可执行帧：入队后 0.12 秒到期帧；顺序依赖：主线程单定时器串行执行，单 tick 超 12 ms 让出剩余项；
            // 能否合并：_timerRunning 保证单定时器；重试上限：按待初始化队列数收敛（单项失败不重试）；
            // 取消条件：队列清空、进入真实场景 Cancel() 或 Reset()。
            var enqueueFrame = DeferredDispatch.CurrentFrame();
            var timer = tree.CreateTimer(TickSeconds);
            timer.Timeout += DeferredDispatch.WrapCallback(
                "DeferredProviderActivator.Tick",
                "global",
                Tick,
                DeferredDispatchRisk.Normal,
                enqueueFrame);
        }
        catch (Exception exception)
        {
            lock (Sync)
            {
                _timerRunning = false;
                Pending.Clear();
            }

            if (!_reportedFailure)
            {
                _reportedFailure = true;
                ModLog.Warn("[性能] 延后初始化的定时器创建失败（不影响换肤）：" +
                            exception.GetBaseException().Message);
            }
        }
    }

    private static void Tick()
    {
        lock (Sync)
        {
            _timerRunning = false;
        }

        while (TryDequeue(out var providerId))
        {
            var started = System.Diagnostics.Stopwatch.GetTimestamp();
            var activated = false;
            var failure = false;
            try
            {
                // [延迟调度/HighRisk] 激活第三方提供者 DLL 初始化器与 Harmony 补丁，具原生闪退风险，执行前先落盘 ENTER 面包屑。
                DeferredDispatch.RunTracked(
                    "DeferredProviderActivator.Activate",
                    providerId,
                    () => activated = SkinService.TryActivateDeferredProvider(providerId),
                    DeferredDispatchRisk.HighRisk);
            }
            catch (Exception exception)
            {
                failure = true;
                ModLog.Warn($"[性能] 延后初始化 {providerId} 失败（不影响其它皮肤）：" +
                            exception.GetBaseException().Message);
            }

            var elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            var keepGoing = Record(providerId, activated, failure, elapsed);
            if (keepGoing)
            {
                EnsureTimer();
            }

            if (elapsed >= MaxTickMilliseconds)
            {
                // 大包一次就超过了预算：这个 tick 只做它一个，剩下的等下一 tick。
                break;
            }
        }
    }

    private static bool TryDequeue(out string providerId)
    {
        lock (Sync)
        {
            if (Pending.Count > 0)
            {
                providerId = Pending.Dequeue();
                return true;
            }
        }

        providerId = string.Empty;
        return false;
    }

    /// <summary>记录一项的结果；返回是否还有排队项（没有时顺手写一条汇总）。</summary>
    private static bool Record(string providerId, bool activated, bool failure, double elapsed)
    {
        lock (Sync)
        {
            _totalMs += elapsed;
            if (elapsed > _maxMs)
            {
                _maxMs = elapsed;
                _slowestProvider = providerId;
            }

            if (failure)
            {
                _failed++;
            }
            else if (activated)
            {
                _completed++;
            }
            else
            {
                _skipped++;
            }

            if (Pending.Count > 0)
            {
                return true;
            }

            WriteSummaryLocked("延后初始化完成");
            return false;
        }
    }

    private static void WriteSummaryLocked(string title)
    {
        if (_completed == 0 && _skipped == 0 && _failed == 0)
        {
            return;
        }

        var detail = string.IsNullOrWhiteSpace(_slowestProvider)
            ? string.Empty
            : $"，最慢 {_maxMs:F0} ms（{_slowestProvider}）";
        ModLog.Info(
            $"[性能] {title}：成功 {_completed}、跳过 {_skipped}、失败 {_failed}，" +
            $"累计 {_totalMs:F0} ms{detail}。");
        _completed = 0;
        _skipped = 0;
        _failed = 0;
        _totalMs = 0;
        _maxMs = 0;
        _slowestProvider = null;
    }
}

