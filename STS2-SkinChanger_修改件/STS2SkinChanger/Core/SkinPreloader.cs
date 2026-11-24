using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

/// <summary>
/// 皮肤资源预加载：把"已经选好的皮肤"用到的场景在后台线程提前读进 Godot 的资源缓存，
/// 这样第一次进战斗/火堆/选人预览时不用再同步读盘，减少黑屏和卡顿。
///
/// 为什么不用 <c>ResourceLoader.Load</c>：同步加载会把开销压在当前帧上，等于把卡顿从
/// A 点搬到 B 点。这里用每 0.15 秒一次的定时器，每次只发起 1-2 个
/// <c>LoadThreadedRequest</c>（真正解码在引擎的子线程里），读完再 <c>LoadThreadedGet</c>
/// 落进缓存。因此主线程每帧最多只付一次"发起请求"的微秒级开销。
///
/// 设计上刻意保守：
///   * 只预加载**当前选择**的场景/资源表（最多 <see cref="MaxQueuedPaths"/> 个），不遍历整个皮肤目录；
///   * 路径按"覆盖包里真实存在"过滤，避免对不存在的路径刷错误日志；
///   * 任何异常只记一次警告并停止本次预加载，绝不影响换肤本身。
///
/// 防死锁约束（2026-11-15 启动黑屏排查结论）：
///   * Godot 的后台加载以"低优先级任务"投进 WorkerThreadPool，同时最多只跑
///     <c>线程数 × low_priority_thread_ratio(默认 0.3)</c> 个——8 线程 CPU 上只有 2 个。
///     Spine 骨骼→图集→贴图的嵌套加载在某些情况下会以非协作方式长时间占住名额，名额被占满后
///     游戏自己的 IntroLogo/MainMenuEssentials/Common 预加载只能排队，表现为黑屏或后台加载永不完成。
///   * 因此：同一时间只在途 <see cref="MaxInFlight"/> 个请求、不使用 sub-threads
///     （与游戏 <c>AssetLoadingSession</c> 一致，官方 VFX 也是逐个加载），
///     并且等游戏 <c>Common</c> 预加载完成后才开始派发（带超时兜底）。
/// </summary>
internal static class SkinPreloader
{
    private const int MaxQueuedPaths = 48;
    private const double TickSeconds = 0.15;

    // 同一时间最多只有 1 个后台加载在途（原实现每 tick 发 2 个且不限在途，最多 48 个并发）。
    private const int MaxInFlight = 1;

    // 游戏 Common 预加载迟迟不完成时的兜底：从首次排队算起，最多等这么久就放行派发。
    private const double StartupGateTimeoutSeconds = 60;

    private static readonly Queue<string> Pending = new();
    private static readonly HashSet<string> Known = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> InFlight = new(StringComparer.OrdinalIgnoreCase);
    private static readonly System.Diagnostics.Stopwatch StartupGateWatch = new();
    private static bool _timerRunning;
    private static bool _reportedFailure;
    private static bool _gamePreloadSettled;
    private static bool _cancelled;
    private static int _loaded;
    private static int _failed;
    private static double _scheduledTotalMs;

    /// <summary>游戏自己的 Common 预加载是否已经完成（或超时兜底放行）。</summary>
    internal static bool GamePreloadSettled => _gamePreloadSettled;

    /// <summary>放行皮肤预加载派发。由 <see cref="GameCommonPreloadCompletePatch"/> 或超时兜底调用。</summary>
    internal static void MarkGamePreloadSettled(string reason)
    {
        if (_gamePreloadSettled)
        {
            return;
        }

        _gamePreloadSettled = true;
        ModLog.Info($"[性能] 游戏预加载已结束（{reason}），开始派发皮肤预加载（队列 {Pending.Count} 个）。");
        try
        {
            SkinLoadingInputObserver.Install();
            SkinService.PrefetchCharacterChoices("ironclad");
        }
        catch (Exception error) { ModLog.Info("空闲预读稍后重试：" + error.GetBaseException().Message); }
    }

    /// <summary>
    /// 暂停派发。选人界面重建外观时会同步 <c>ResourceLoader.Load</c>（还会反射触发皮肤包自己的
    /// 同步加载），此时若后台仍有线程加载在跑，Godot 的加载器会互相等待，表现为切皮肤瞬间死锁。
    /// 重建期间把派发停掉，重建结束再继续。
    /// </summary>
    internal static bool Paused { get; set; }

    /// <summary>
    /// 退出前调用：停止派发并丢掉还没派发的任务。
    /// 注意：Godot 没有取消后台加载的接口，已发出的请求无法撤回；这里刻意不清 <c>InFlight</c>，
    /// 让定时器继续把它们 <c>LoadThreadedGet</c> 取回，而不是丢掉 token 让引擎在关停时干等。
    /// 因为在途最多 <see cref="MaxInFlight"/> 个，关停等待时间是有界的。
    /// </summary>
    internal static void CancelAll()
    {
        CharacterPackPrefetch.Cancel();
        _cancelled = true;
        if (Pending.Count == 0 && InFlight.Count == 0)
        {
            return;
        }

        ModLog.Info(
            $"[性能] 退出前停止派发：丢弃 {Pending.Count} 个未派发任务；" +
            $"{InFlight.Count} 个在途任务无法取消，交给引擎收尾。");
        Pending.Clear();
        Known.Clear();
    }

    /// <summary>
    /// 为这些分组当前选中的皮肤排队预加载。重复调用是安全的：已在队列/已加载的路径会被跳过。
    /// </summary>
    internal static void Schedule(
        SkinCatalog catalog,
        IReadOnlyDictionary<string, string> selections,
        IEnumerable<string> groupIds)
    {
        if (_cancelled)
        {
            return;
        }

        try
        {
            var added = 0;
            foreach (var groupId in groupIds)
            {
                var group = catalog.Groups.FirstOrDefault(candidate =>
                    candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
                if (group == null)
                {
                    continue;
                }

                var selectedId = selections.GetValueOrDefault(groupId, SkinCatalog.BaseOptionId);
                var option = group.Options.FirstOrDefault(candidate =>
                    candidate.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase));
                if (option == null)
                {
                    continue;
                }

                foreach (var asset in option.Assets.Values)
                {
                    if (Pending.Count + InFlight.Count >= MaxQueuedPaths)
                    {
                        break;
                    }

                    foreach (var file in asset.Files)
                    {
                        // 只预加载"场景"和"资源表"：贴图会被场景带出来，单独排队只会挤占名额。
                        if (!file.Path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) &&
                            !file.Path.EndsWith(".scn", StringComparison.OrdinalIgnoreCase) &&
                            !file.Path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var sourcePath = asset.SourcePath;
                        // Canonical res:// paths change ownership on a hot switch. An already
                        // dispatched worker cannot be cancelled by Paused and may still read
                        // the old skeleton/import while MountOverlay replaces its dependencies.
                        // Only immutable, session-private resource paths are safe to warm.
                        if (!IsImmutableResourcePath(sourcePath)) break;
                        if (Known.Add(sourcePath))
                        {
                            Pending.Enqueue(sourcePath);
                            added++;
                        }

                        break;
                    }
                }
            }

            if (added > 0)
            {
                if (!_gamePreloadSettled && !StartupGateWatch.IsRunning)
                {
                    StartupGateWatch.Start();
                }

                ModLog.Info(
                    $"[性能] 预加载排队 {added} 个皮肤场景（队列上限 {MaxQueuedPaths}）" +
                    (_gamePreloadSettled ? "。" : "，等游戏 Common 预加载完成后再派发。"));
                EnsureTimer();
            }
        }
        catch (Exception exception)
        {
            if (!_reportedFailure)
            {
                _reportedFailure = true;
                ModLog.Warn("[性能] 预加载排队失败（不影响换肤）：" + exception.GetBaseException().Message);
            }
        }
    }

    internal static bool IsImmutableResourcePath(string path) =>
        path.StartsWith("res://sts2_skin_runtime/", StringComparison.Ordinal) &&
        !path.Contains("..", StringComparison.Ordinal) && !path.Contains('\\');

    private static void EnsureTimer()
    {
        if (_timerRunning)
        {
            return;
        }

        try
        {
            if (Engine.GetMainLoop() is not SceneTree tree)
            {
                return;
            }

            // [延迟调度/Timer] 原触发时机：预加载队列非空时经 SceneTreeTimer(0.15s) 周期唤醒；
            // 最早可执行帧：入队后 0.15 秒到期帧；顺序依赖：需等游戏 Common 预加载结束且未 Paused 才派发单路后台加载；
            // 能否合并：_timerRunning 保证单定时器串行驱动；重试上限：最多 48 项（启动门禁 60 秒兜底，单项失败不重试）；
            // 取消条件：Pending 与 InFlight 均清空或 _cancelled。
            var enqueueFrame = DeferredDispatch.CurrentFrame();
            _timerRunning = true;
            var timer = tree.CreateTimer(TickSeconds);
            timer.Timeout += DeferredDispatch.WrapPollCallback(
                "SkinPreloader.Tick",
                "global",
                Tick,
                DeferredDispatchRisk.Normal,
                enqueueFrame);
        }
        catch (Exception exception)
        {
            _timerRunning = false;
            ModLog.Warn("[性能] 预加载定时器创建失败：" + exception.GetBaseException().Message);
        }
    }

    private static bool Tick()
    {
        _timerRunning = false;

        if (Paused)
        {
            // 重建外观期间不派发：主线程马上要同步读资源，此时后台再加载会和它互相等待。
            EnsureTimer();
            return false;
        }

        if (!_gamePreloadSettled)
        {
            // 启动阶段先让游戏自己的 IntroLogo/MainMenuEssentials/Common 预加载跑完，
            // 否则两边同时抢 WorkerThreadPool 的低优先级名额（8 线程 CPU 上只有 2 个）。
            if (StartupGateWatch.Elapsed.TotalSeconds >= StartupGateTimeoutSeconds)
            {
                MarkGamePreloadSettled($"等待超过 {StartupGateTimeoutSeconds:F0} 秒，兜底放行");
            }
            else
            {
                EnsureTimer();
                return false;
            }
        }

        var tickWatch = System.Diagnostics.Stopwatch.StartNew();
        var didWork = false;
        try
        {
            if (CompleteFinishedRequests())
            {
                didWork = true;
            }

            // 串行：上一个请求取回之前不发下一个。缓存命中的路径不占名额，可以在同一 tick 连续跳过。
            while (!_cancelled && CharacterPackPrefetch.CanSpeculateNow && InFlight.Count < MaxInFlight && Pending.Count > 0)
            {
                didWork = true;
                var path = Pending.Dequeue();
                if (InFlight.Contains(path))
                {
                    continue;
                }

                if (ResourceLoader.HasCached(path))
                {
                    _loaded++;
                    continue;
                }

                // useSubThreads: false —— 不把外部依赖再拆成子任务投进线程池（与游戏 AssetLoadingSession 一致）。
                RuntimeLoadSafety.RequireHeadroom(64L * 1024 * 1024);
                var error = ResourceLoader.LoadThreadedRequest(path, "", useSubThreads: false);
                if (error != Error.Ok)
                {
                    // 路径不存在/类型不支持：不重试，避免每 0.15 秒刷一次错误。
                    _failed++;
                    continue;
                }

                InFlight.Add(path);
            }
        }
        catch (AppearanceResourceUnavailableException exception)
        {
            didWork = true;
            Pending.Clear();
            Known.Clear();
            ModLog.Warn("[性能] 内存不足，已丢弃尚未派发的预加载：" + exception.Message);
        }
        catch (Exception exception)
        {
            didWork = true;
            if (!_reportedFailure)
            {
                _reportedFailure = true;
                ModLog.Warn("[性能] 预加载执行失败（不影响游戏）：" + exception.GetBaseException().Message);
            }
        }
        finally
        {
            _scheduledTotalMs += tickWatch.Elapsed.TotalMilliseconds;
            if (Pending.Count > 0 || InFlight.Count > 0)
            {
                EnsureTimer();
            }
            else
            {
                didWork = true;
                ModLog.Info(
                    $"[性能] 预加载完成：成功 {_loaded}，失败 {_failed}，" +
                    $"调度累计 {_scheduledTotalMs:F0} ms（主线程只付发起请求的开销）。");
                _loaded = 0;
                _failed = 0;
                _scheduledTotalMs = 0;
                Known.Clear();
            }
        }

        return didWork;
    }

    private static bool CompleteFinishedRequests()
    {
        var completed = false;
        foreach (var path in InFlight.ToArray())
        {
            var status = ResourceLoader.LoadThreadedGetStatus(path);
            switch (status)
            {
                case ResourceLoader.ThreadLoadStatus.Loaded:
                {
                    // 取回一次才算真正落进缓存，之后的同步加载就不会再读盘。
                    ResourceLoader.LoadThreadedGet(path);
                    InFlight.Remove(path);
                    _loaded++;
                    completed = true;
                    break;
                }
                case ResourceLoader.ThreadLoadStatus.Failed:
                case ResourceLoader.ThreadLoadStatus.InvalidResource:
                {
                    InFlight.Remove(path);
                    _failed++;
                    completed = true;
                    break;
                }
            }
        }

        return completed;
    }
}

/// <summary>
/// 游戏自己的 <c>Common</c> 预加载会话完成时放行 <see cref="SkinPreloader"/>。
/// <c>PreloadManager.LoadCommonAndMainMenuAssets</c> 内部不等待会话完成，对它做 Postfix 只能拿到"开始"，
/// 所以挂在每帧推进会话的 <c>AssetLoadingSession.Process</c> 上，看到 Common 完成即放行。
/// 放行后每次调用只做一次布尔判断就返回；任何异常都吞掉并直接放行，绝不把异常抛进游戏的加载循环。
/// </summary>
[HarmonyPatch(typeof(AssetLoadingSession), nameof(AssetLoadingSession.Process))]
internal static class GameCommonPreloadCompletePatch
{
    private static AccessTools.FieldRef<AssetLoadingSession, string>? _sessionName;
    private static bool _sessionNameResolved;

    private static void Postfix(AssetLoadingSession __instance)
    {
        if (SkinPreloader.GamePreloadSettled)
        {
            return;
        }

        try
        {
            if (!__instance.IsCompleted)
            {
                return;
            }

            if (!_sessionNameResolved)
            {
                _sessionNameResolved = true;
                try
                {
                    _sessionName = AccessTools.FieldRefAccess<AssetLoadingSession, string>("_name");
                }
                catch (Exception exception)
                {
                    ModLog.Warn("[性能] 读不到预加载会话名（将直接放行皮肤预加载）：" + exception.GetBaseException().Message);
                }
            }

            if (_sessionName == null)
            {
                SkinPreloader.MarkGamePreloadSettled("无法识别会话名，直接放行");
                return;
            }

            if (string.Equals(_sessionName(__instance), "Common", StringComparison.Ordinal))
            {
                SkinPreloader.MarkGamePreloadSettled("Common 完成");
            }
        }
        catch (Exception exception)
        {
            SkinPreloader.MarkGamePreloadSettled("识别出错，直接放行：" + exception.GetBaseException().Message);
        }
    }
}
