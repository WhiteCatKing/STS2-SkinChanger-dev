using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace STS2SkinChanger.Core;

internal enum DeferredDispatchRisk
{
    Normal = 0,
    HighRisk = 1,
    HighRiskEnterOnly = 2
}

internal readonly record struct DeferredDispatchRecord(
    long Sequence,
    DateTime TimestampUtc,
    string Phase,
    string TaskName,
    string Owner,
    ulong EnqueueFrame,
    ulong ExecuteFrame,
    int StreakCount,
    string Result,
    double ElapsedMs,
    DeferredDispatchRisk Risk);

/// <summary>
/// Step 1 of Mechanism Convergence #1: execution records, reentrancy observation, and crash breadcrumbs
/// for deferred / multi-frame / polling callbacks.
///
/// Design invariants for Step 1:
/// - Zero execution-timing changes: <see cref="CallDeferred"/> enqueues directly at the call site via
///   <c>Callable.From(...).CallDeferred()</c> (no unified Drain, no frame budget, no deduplication,
///   no retry cutoff). Critical ordering relative to third-party callbacks is preserved verbatim.
/// - Zero exception-behavior changes: callback exceptions propagate untouched; diagnostic failures
///   never mask or replace callback behavior.
/// - Thread-safe enqueue: safe to call from background threads (e.g. <see cref="VanillaIntegritySelfCheck"/>).
/// </summary>
internal static class DeferredDispatch
{
    internal const int DefaultRingBufferCapacity = 256;
    internal const int DefaultReentrancyWarnThreshold = 50;
    internal const int DefaultFlushEveryRecords = 32;
    internal const int DefaultMaxBreadcrumbFileBytes = 64 * 1024;
    internal const int DefaultKeepTailLinesOnTrim = 128;

    private const int MaxTrackedKeys = 512;
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly object Sync = new();

    [ThreadStatic]
    private static Stack<TaskKey>? _executingStack;

    private static RingSlot[] _ring = new RingSlot[DefaultRingBufferCapacity];
    private static int _ringHead;
    private static int _ringCount;
    private static long _sequence;
    private static long _lastFlushedSequence;
    private static int _unflushedNormalCount;

    private static readonly Dictionary<TaskKey, ReentrancyState> ReentrancyStates = new();
    private static readonly HashSet<TaskKey> WarnedReentrancyKeys = new();
    private static readonly Dictionary<TaskKey, long> PendingIdlePollCounts = new();
    private static readonly Dictionary<TaskKey, long> TotalIdlePollCounts = new();

    private static Action<Action> _enqueue = static action => Callable.From(action).CallDeferred();
    private static Func<ulong> _frameCounter = SafeGetEngineProcessFrame;
    private static Func<string?> _breadcrumbDirectoryProvider = static () => CrashDiagnostics.SessionDirectory;
    private static Action<string> _warnLogger = ModLog.Warn;

    private static int _ringCapacity = DefaultRingBufferCapacity;
    private static int _reentrancyWarnThreshold = DefaultReentrancyWarnThreshold;
    private static int _flushEveryRecords = DefaultFlushEveryRecords;
    private static int _maxBreadcrumbFileBytes = DefaultMaxBreadcrumbFileBytes;
    private static int _keepTailLinesOnTrim = DefaultKeepTailLinesOnTrim;

    /// <summary>
    /// Enqueues <paramref name="action"/> directly at the current call site via Godot's deferred queue
    /// (preserving exact FIFO order) while recording execution metadata, reentrancy streaks, and crash breadcrumbs.
    /// </summary>
    internal static void CallDeferred(
        string taskName,
        object? owner,
        Action action,
        DeferredDispatchRisk risk = DeferredDispatchRisk.Normal)
    {
        ArgumentNullException.ThrowIfNull(action);
        var wrapped = WrapCallback(taskName, owner, action, risk);
        _enqueue(wrapped);
    }

    /// <summary>
    /// Wraps a callback intended for non-<c>CallDeferred</c> scheduling hooks (e.g. <c>SceneTreeTimer.Timeout</c>)
    /// without altering when or how Godot invokes it.
    /// </summary>
    internal static Action WrapCallback(
        string taskName,
        object? owner,
        Action action,
        DeferredDispatchRisk risk = DeferredDispatchRisk.Normal,
        ulong? enqueueFrameOverride = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        var normalizedTask = NormalizeToken(taskName, "unnamed", 96);
        var normalizedOwner = FormatOwner(owner);
        var key = new TaskKey(normalizedTask, normalizedOwner);
        var enqueueFrame = enqueueFrameOverride ?? SafeReadFrame();
        var isSelfReentry = IsCurrentlyExecuting(key);

        return () => ExecuteTrackedCore(key, enqueueFrame, isSelfReentry, risk, action, null);
    }

    /// <summary>
    /// Wraps a periodic polling callback (e.g. <c>SceneTreeTimer.Timeout</c>). When
    /// <paramref name="pollAction"/> returns <c>false</c> (an empty/idle poll), only the idle poll
    /// counter is incremented and no per-tick breadcrumb record is appended or flushed.
    /// </summary>
    internal static Action WrapPollCallback(
        string taskName,
        object? owner,
        Func<bool> pollAction,
        DeferredDispatchRisk risk = DeferredDispatchRisk.Normal,
        ulong? enqueueFrameOverride = null)
    {
        ArgumentNullException.ThrowIfNull(pollAction);
        var normalizedTask = NormalizeToken(taskName, "unnamed", 96);
        var normalizedOwner = FormatOwner(owner);
        var key = new TaskKey(normalizedTask, normalizedOwner);
        var enqueueFrame = enqueueFrameOverride ?? SafeReadFrame();
        var isSelfReentry = IsCurrentlyExecuting(key);

        return () => ExecuteTrackedCore(key, enqueueFrame, isSelfReentry, risk, null, pollAction);
    }

    /// <summary>
    /// Executes <paramref name="action"/> synchronously on the current tick (e.g. inside an existing
    /// <c>ProcessFrame</c> or timer loop) while recording execution metadata and breadcrumbs.
    /// </summary>
    internal static void RunTracked(
        string taskName,
        object? owner,
        Action action,
        DeferredDispatchRisk risk = DeferredDispatchRisk.Normal,
        ulong? enqueueFrameOverride = null)
    {
        ArgumentNullException.ThrowIfNull(action);
        var normalizedTask = NormalizeToken(taskName, "unnamed", 96);
        var normalizedOwner = FormatOwner(owner);
        var key = new TaskKey(normalizedTask, normalizedOwner);
        var enqueueFrame = enqueueFrameOverride ?? SafeReadFrame();
        var isSelfReentry = IsCurrentlyExecuting(key);
        ExecuteTrackedCore(key, enqueueFrame, isSelfReentry, risk, action, null);
    }

    /// <summary>
    /// Executes a periodic polling step synchronously on the current tick. Empty polls (returning
    /// <c>false</c>) only increment the idle counter instead of writing per-tick breadcrumb rows.
    /// </summary>
    internal static bool RunTrackedPoll(
        string taskName,
        object? owner,
        Func<bool> pollAction,
        DeferredDispatchRisk risk = DeferredDispatchRisk.Normal,
        ulong? enqueueFrameOverride = null)
    {
        ArgumentNullException.ThrowIfNull(pollAction);
        var normalizedTask = NormalizeToken(taskName, "unnamed", 96);
        var normalizedOwner = FormatOwner(owner);
        var key = new TaskKey(normalizedTask, normalizedOwner);
        var enqueueFrame = enqueueFrameOverride ?? SafeReadFrame();
        var isSelfReentry = IsCurrentlyExecuting(key);
        return ExecuteTrackedCore(key, enqueueFrame, isSelfReentry, risk, null, pollAction);
    }

    internal static long GetIdlePollCount(string taskName, object? owner)
    {
        var key = new TaskKey(NormalizeToken(taskName, "unnamed", 96), FormatOwner(owner));
        lock (Sync)
        {
            return TotalIdlePollCounts.TryGetValue(key, out var count) ? count : 0;
        }
    }

    internal static ulong CurrentFrame() => SafeReadFrame();

    internal static DeferredDispatchRecord[] SnapshotRecords()
    {
        lock (Sync)
        {
            if (_ringCount == 0)
            {
                return [];
            }

            var result = new DeferredDispatchRecord[_ringCount];
            var start = (_ringHead - _ringCount + _ringCapacity) % _ringCapacity;
            for (var i = 0; i < _ringCount; i++)
            {
                result[i] = _ring[(start + i) % _ringCapacity].Record;
            }

            return result;
        }
    }

    /// <summary>
    /// Flushes any unflushed in-memory records to the current session's breadcrumb file.
    /// Safe to call on normal exit, forced shutdown, or fatal exception handling.
    /// </summary>
    internal static void FlushBreadcrumbs()
    {
        try
        {
            var directory = _breadcrumbDirectoryProvider();
            if (string.IsNullOrWhiteSpace(directory))
            {
                return;
            }

            lock (Sync)
            {
                FlushPendingLocked(directory);
            }
        }
        catch
        {
            // Breadcrumb flushing must never disrupt gameplay or shutdown.
        }
    }

    internal static string FormatBreadcrumbLine(in DeferredDispatchRecord record)
    {
        var riskTag = record.Risk == DeferredDispatchRisk.Normal ? "NORM" : "HIGH";
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{record.TimestampUtc:O} #{record.Sequence} [{record.Phase}][{riskTag}] task={record.TaskName} owner={record.Owner} enq={record.EnqueueFrame} run={record.ExecuteFrame} streak={record.StreakCount} ms={record.ElapsedMs:F2} res={record.Result}");
    }

    internal static void ConfigureForTests(
        Action<Action>? enqueue = null,
        Func<ulong>? frameCounter = null,
        Func<string?>? breadcrumbDirectoryProvider = null,
        Action<string>? warnLogger = null,
        int? ringCapacity = null,
        int? reentrancyWarnThreshold = null,
        int? flushEveryRecords = null,
        int? maxBreadcrumbFileBytes = null,
        int? keepTailLinesOnTrim = null)
    {
        lock (Sync)
        {
            _enqueue = enqueue ?? (static action => Callable.From(action).CallDeferred());
            _frameCounter = frameCounter ?? SafeGetEngineProcessFrame;
            _breadcrumbDirectoryProvider = breadcrumbDirectoryProvider ?? (static () => CrashDiagnostics.SessionDirectory);
            _warnLogger = warnLogger ?? ModLog.Warn;

            _ringCapacity = Math.Max(1, ringCapacity ?? DefaultRingBufferCapacity);
            _reentrancyWarnThreshold = Math.Max(1, reentrancyWarnThreshold ?? DefaultReentrancyWarnThreshold);
            _flushEveryRecords = Math.Max(1, flushEveryRecords ?? DefaultFlushEveryRecords);
            _maxBreadcrumbFileBytes = Math.Max(256, maxBreadcrumbFileBytes ?? DefaultMaxBreadcrumbFileBytes);
            _keepTailLinesOnTrim = Math.Max(1, keepTailLinesOnTrim ?? DefaultKeepTailLinesOnTrim);

            _ring = new RingSlot[_ringCapacity];
            _ringHead = 0;
            _ringCount = 0;
            _sequence = 0;
            _lastFlushedSequence = 0;
            _unflushedNormalCount = 0;
            ReentrancyStates.Clear();
            WarnedReentrancyKeys.Clear();
            PendingIdlePollCounts.Clear();
            TotalIdlePollCounts.Clear();
        }
    }

    internal static void ResetDefaultsForTests() => ConfigureForTests();

    private static bool ExecuteTrackedCore(
        TaskKey key,
        ulong enqueueFrame,
        bool isSelfReentry,
        DeferredDispatchRisk risk,
        Action? action,
        Func<bool>? pollAction)
    {
        var executeFrame = SafeReadFrame();
        var nowUtc = DateTime.UtcNow;
        var isPoll = pollAction != null;
        var flushEnter = risk is DeferredDispatchRisk.HighRisk or DeferredDispatchRisk.HighRiskEnterOnly;
        int streak;
        string? warningMessage = null;

        try
        {
            lock (Sync)
            {
                streak = AdvanceStreakLocked(key, executeFrame, nowUtc, isSelfReentry, isPoll);
                if (streak > _reentrancyWarnThreshold && WarnedReentrancyKeys.Add(key))
                {
                    warningMessage =
                        $"[延迟调度] 任务连续重入超过阈值（只观察、不截断）：task={key.TaskName}，owner={key.Owner}，次数={streak}，入队帧={enqueueFrame}，执行帧={executeFrame}";
                }

                if (flushEnter)
                {
                    var enterRecord = new DeferredDispatchRecord(
                        ++_sequence,
                        nowUtc,
                        "ENTER",
                        key.TaskName,
                        key.Owner,
                        enqueueFrame,
                        executeFrame,
                        streak,
                        "running",
                        0.0,
                        risk);
                    AppendRingLocked(enterRecord, persistToDisk: true);
                    var directory = _breadcrumbDirectoryProvider();
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        FlushPendingLocked(directory);
                    }
                }
            }

            if (warningMessage != null)
            {
                try
                {
                    _warnLogger(warningMessage);
                }
                catch
                {
                    // Warning log failure must never interfere with callback execution.
                }
            }
        }
        catch
        {
            streak = 1;
        }

        var stack = _executingStack ??= new Stack<TaskKey>();
        stack.Push(key);
        var startedAt = Stopwatch.GetTimestamp();
        Exception? failure = null;
        var didWork = true;
        try
        {
            if (pollAction != null)
            {
                didWork = pollAction();
            }
            else
            {
                action!();
            }

            return didWork;
        }
        catch (Exception exception)
        {
            failure = exception;
            throw;
        }
        finally
        {
            if (stack.Count > 0)
            {
                stack.Pop();
            }

            var elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
            try
            {
                lock (Sync)
                {
                    if (isPoll && failure == null && !didWork)
                    {
                        IncrementIdlePollCountLocked(key);
                    }
                    else
                    {
                        var phase = failure == null ? "DONE" : "FAIL";
                        long idleBeforeWork = 0;
                        if (isPoll && PendingIdlePollCounts.Remove(key, out var pendingIdle))
                        {
                            idleBeforeWork = pendingIdle;
                        }

                        var result = failure == null
                            ? (idleBeforeWork > 0
                                ? string.Create(CultureInfo.InvariantCulture, $"ok(idle={idleBeforeWork})")
                                : "ok")
                            : FormatFailureResult(failure);
                        var persistFinish = risk != DeferredDispatchRisk.HighRiskEnterOnly || failure != null;
                        var flushFinish = risk == DeferredDispatchRisk.HighRisk ||
                                          (risk == DeferredDispatchRisk.HighRiskEnterOnly && failure != null);
                        var finishRecord = new DeferredDispatchRecord(
                            ++_sequence,
                            DateTime.UtcNow,
                            phase,
                            key.TaskName,
                            key.Owner,
                            enqueueFrame,
                            executeFrame,
                            streak,
                            result,
                            elapsedMs,
                            risk);
                        AppendRingLocked(finishRecord, persistFinish);

                        if (flushFinish)
                        {
                            var directory = _breadcrumbDirectoryProvider();
                            if (!string.IsNullOrWhiteSpace(directory))
                            {
                                FlushPendingLocked(directory);
                            }
                        }
                        else if (persistFinish)
                        {
                            _unflushedNormalCount++;
                            if (_unflushedNormalCount >= _flushEveryRecords)
                            {
                                var directory = _breadcrumbDirectoryProvider();
                                if (!string.IsNullOrWhiteSpace(directory))
                                {
                                    FlushPendingLocked(directory);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Never mask or replace callback completion/exception.
            }
        }
    }

    private static void IncrementIdlePollCountLocked(TaskKey key)
    {
        if (!TotalIdlePollCounts.ContainsKey(key) && TotalIdlePollCounts.Count >= MaxTrackedKeys)
        {
            TotalIdlePollCounts.Clear();
            PendingIdlePollCounts.Clear();
        }

        TotalIdlePollCounts[key] = TotalIdlePollCounts.TryGetValue(key, out var total) ? total + 1 : 1;
        PendingIdlePollCounts[key] = PendingIdlePollCounts.TryGetValue(key, out var pending) ? pending + 1 : 1;
    }

    private static int AdvanceStreakLocked(
        TaskKey key,
        ulong executeFrame,
        DateTime nowUtc,
        bool isSelfReentry,
        bool isPoll)
    {
        if (ReentrancyStates.TryGetValue(key, out var existing))
        {
            var isContinuous = isPoll
                // Periodic / per-frame polling (e.g. FakeMerchantEventFrame awaiting Spine bounds
                // across ProcessFrame) is legitimate cross-frame waiting; only count recursive
                // self-reentry or repeated invocations within the same frame/idle as reentrancy.
                ? isSelfReentry ||
                  (executeFrame == existing.LastExecuteFrame &&
                   (executeFrame != 0 || (nowUtc - existing.LastExecuteUtc) <= TimeSpan.FromMilliseconds(250)))
                : isSelfReentry ||
                  (executeFrame >= existing.LastExecuteFrame && executeFrame - existing.LastExecuteFrame <= 2) ||
                  (executeFrame == 0 && existing.LastExecuteFrame == 0 && (nowUtc - existing.LastExecuteUtc) <= TimeSpan.FromMilliseconds(250));
            var nextStreak = isContinuous ? existing.Streak + 1 : 1;
            ReentrancyStates[key] = new ReentrancyState(executeFrame, nowUtc, nextStreak);
            return nextStreak;
        }

        if (ReentrancyStates.Count >= MaxTrackedKeys)
        {
            PruneStaleReentrancyStatesLocked(nowUtc);
        }

        ReentrancyStates[key] = new ReentrancyState(executeFrame, nowUtc, 1);
        return 1;
    }

    private static void PruneStaleReentrancyStatesLocked(DateTime nowUtc)
    {
        var cutoff = nowUtc - TimeSpan.FromMinutes(2);
        var toRemove = new List<TaskKey>();
        foreach (var pair in ReentrancyStates)
        {
            if (pair.Value.LastExecuteUtc < cutoff)
            {
                toRemove.Add(pair.Key);
            }
        }

        foreach (var key in toRemove)
        {
            ReentrancyStates.Remove(key);
        }

        if (ReentrancyStates.Count >= MaxTrackedKeys)
        {
            foreach (var key in ReentrancyStates
                         .OrderBy(pair => pair.Value.LastExecuteUtc)
                         .Take(ReentrancyStates.Count - (MaxTrackedKeys / 2))
                         .Select(pair => pair.Key)
                         .ToArray())
            {
                ReentrancyStates.Remove(key);
            }
        }
    }

    private static void AppendRingLocked(in DeferredDispatchRecord record, bool persistToDisk)
    {
        _ring[_ringHead] = new RingSlot(record, persistToDisk);
        _ringHead = (_ringHead + 1) % _ringCapacity;
        if (_ringCount < _ringCapacity)
        {
            _ringCount++;
        }
    }

    private static void FlushPendingLocked(string directory)
    {
        if (_ringCount == 0 || _sequence <= _lastFlushedSequence)
        {
            _unflushedNormalCount = 0;
            return;
        }

        var builder = new StringBuilder();
        var start = (_ringHead - _ringCount + _ringCapacity) % _ringCapacity;
        long newestSeen = _lastFlushedSequence;
        for (var i = 0; i < _ringCount; i++)
        {
            var slot = _ring[(start + i) % _ringCapacity];
            var record = slot.Record;
            if (record.Sequence <= _lastFlushedSequence)
            {
                continue;
            }

            if (record.Sequence > newestSeen)
            {
                newestSeen = record.Sequence;
            }

            if (!slot.PersistToDisk)
            {
                continue;
            }

            builder.Append(FormatBreadcrumbLine(record));
            builder.Append('\n');
        }

        if (builder.Length == 0)
        {
            _lastFlushedSequence = newestSeen;
            _unflushedNormalCount = 0;
            return;
        }

        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, CrashDiagnostics.BreadcrumbFileName);
        var bytes = Utf8NoBom.GetBytes(builder.ToString());
        using (var stream = new FileStream(path, FileMode.Append, System.IO.FileAccess.Write, FileShare.ReadWrite | FileShare.Delete))
        {
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();
        }

        _lastFlushedSequence = newestSeen;
        _unflushedNormalCount = 0;

        TrimBreadcrumbFileIfNeededLocked(path);
    }

    private static void TrimBreadcrumbFileIfNeededLocked(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists || info.Length <= _maxBreadcrumbFileBytes)
        {
            return;
        }

        var lines = File.ReadAllLines(path, Utf8NoBom);
        var keepCount = Math.Min(_keepTailLinesOnTrim, lines.Length);
        var trimmed = new StringBuilder();
        for (var i = lines.Length - keepCount; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            trimmed.Append(lines[i]);
            trimmed.Append('\n');
        }

        // If the kept tail still exceeds the byte budget (e.g. tiny budget in tests),
        // drop oldest lines until it fits cleanly inside _maxBreadcrumbFileBytes.
        var keptLines = trimmed.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var startIndex = 0;
        var encoded = Utf8NoBom.GetBytes(string.Join('\n', keptLines) + (keptLines.Length > 0 ? "\n" : string.Empty));
        while (encoded.Length > _maxBreadcrumbFileBytes && startIndex < keptLines.Length - 1)
        {
            startIndex++;
            encoded = Utf8NoBom.GetBytes(string.Join('\n', keptLines, startIndex, keptLines.Length - startIndex) + "\n");
        }

        var tempPath = path + ".trim.tmp";
        try
        {
            File.WriteAllBytes(tempPath, encoded);
            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }
        }
    }

    private static bool IsCurrentlyExecuting(TaskKey key)
    {
        var stack = _executingStack;
        if (stack == null || stack.Count == 0)
        {
            return false;
        }

        foreach (var active in stack)
        {
            if (active.Equals(key))
            {
                return true;
            }
        }

        return false;
    }

    private static ulong SafeReadFrame()
    {
        try
        {
            return _frameCounter();
        }
        catch
        {
            return 0;
        }
    }

    private static ulong SafeGetEngineProcessFrame()
    {
        try
        {
            return Engine.GetProcessFrames();
        }
        catch
        {
            return 0;
        }
    }

    private static string FormatOwner(object? owner)
    {
        if (owner == null)
        {
            return "global";
        }

        if (owner is string text)
        {
            return NormalizeToken(text, "global", 64);
        }

        if (owner is GodotObject godotObject)
        {
            var typeName = godotObject.GetType().Name;
            try
            {
                var instanceId = godotObject.GetInstanceId();
                if (instanceId != 0)
                {
                    return NormalizeToken($"{typeName}#{instanceId}", typeName, 64);
                }
            }
            catch
            {
                // Headless/disposed GodotObject falls back to managed identity hash.
            }

            return NormalizeToken($"{typeName}@{RuntimeHelpers.GetHashCode(godotObject)}", typeName, 64);
        }

        var name = owner.GetType().Name;
        return NormalizeToken($"{name}@{RuntimeHelpers.GetHashCode(owner)}", name, 64);
    }

    private static string FormatFailureResult(Exception exception)
    {
        var baseException = exception.GetBaseException();
        var raw = $"error:{baseException.GetType().Name}:{baseException.Message}";
        return NormalizeToken(raw, "error", 160);
    }

    private static string NormalizeToken(string? raw, string fallback, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return fallback;
        }

        var trimmed = raw.Trim();
        var sb = new StringBuilder(Math.Min(trimmed.Length, maxLength));
        foreach (var ch in trimmed)
        {
            if (sb.Length >= maxLength)
            {
                break;
            }

            if (ch is '\r' or '\n' or '\t')
            {
                sb.Append(' ');
            }
            else if (!char.IsControl(ch))
            {
                sb.Append(ch);
            }
        }

        var cleaned = sb.ToString().Trim();
        return cleaned.Length == 0 ? fallback : cleaned;
    }

    private readonly record struct RingSlot(DeferredDispatchRecord Record, bool PersistToDisk);

    private readonly record struct TaskKey(string TaskName, string Owner);

    private readonly record struct ReentrancyState(ulong LastExecuteFrame, DateTime LastExecuteUtc, int Streak);
}
