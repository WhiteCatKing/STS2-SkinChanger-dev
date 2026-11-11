using Godot;
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
/// </summary>
internal static class SkinPreloader
{
    private const int MaxQueuedPaths = 48;
    private const double TickSeconds = 0.15;
    private const int RequestsPerTick = 2;

    private static readonly Queue<string> Pending = new();
    private static readonly HashSet<string> Known = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> InFlight = new(StringComparer.OrdinalIgnoreCase);
    private static bool _timerRunning;
    private static bool _reportedFailure;
    private static int _loaded;
    private static int _failed;
    private static double _scheduledTotalMs;

    /// <summary>
    /// 为这些分组当前选中的皮肤排队预加载。重复调用是安全的：已在队列/已加载的路径会被跳过。
    /// </summary>
    internal static void Schedule(
        SkinCatalog catalog,
        IReadOnlyDictionary<string, string> selections,
        IEnumerable<string> groupIds)
    {
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
                ModLog.Info($"[性能] 预加载排队 {added} 个皮肤场景（队列上限 {MaxQueuedPaths}）。");
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

            _timerRunning = true;
            var timer = tree.CreateTimer(TickSeconds);
            timer.Timeout += Tick;
        }
        catch (Exception exception)
        {
            _timerRunning = false;
            ModLog.Warn("[性能] 预加载定时器创建失败：" + exception.GetBaseException().Message);
        }
    }

    private static void Tick()
    {
        _timerRunning = false;
        var tickWatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            CompleteFinishedRequests();

            for (var i = 0; i < RequestsPerTick && Pending.Count > 0; i++)
            {
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

                var error = ResourceLoader.LoadThreadedRequest(path, "", useSubThreads: true);
                if (error != Error.Ok)
                {
                    // 路径不存在/类型不支持：不重试，避免每 0.15 秒刷一次错误。
                    _failed++;
                    continue;
                }

                InFlight.Add(path);
            }
        }
        catch (Exception exception)
        {
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
                ModLog.Info(
                    $"[性能] 预加载完成：成功 {_loaded}，失败 {_failed}，" +
                    $"调度累计 {_scheduledTotalMs:F0} ms（主线程只付发起请求的开销）。");
                _loaded = 0;
                _failed = 0;
                _scheduledTotalMs = 0;
                Known.Clear();
            }
        }
    }

    private static void CompleteFinishedRequests()
    {
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
                    break;
                }
                case ResourceLoader.ThreadLoadStatus.Failed:
                case ResourceLoader.ThreadLoadStatus.InvalidResource:
                {
                    InFlight.Remove(path);
                    _failed++;
                    break;
                }
            }
        }
    }
}
