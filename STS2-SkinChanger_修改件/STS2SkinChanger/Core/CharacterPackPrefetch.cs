using System.Diagnostics;

namespace STS2SkinChanger.Core;

// Interruptible file-I/O queue. Workers never touch Godot objects, mounts or decoders.
internal static class CharacterPackPrefetch
{
    private sealed class Work(string path, long sequence)
    {
        internal readonly string Path = path;
        internal readonly long Sequence = sequence;
        internal readonly TaskCompletionSource<bool> Done = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal long Offset, Length = -1;
        internal int Demand;
    }
    private static readonly object Sync = new();
    private static readonly CancellationTokenSource Shutdown = new();
    private static readonly Dictionary<string, Work> Pending = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, (long Size, DateTime Stamp)> Warm = new(StringComparer.OrdinalIgnoreCase);
    private static bool _running;
    private static long _sequence, _lastAction;
    private static int _foreground;
    private static string? _lastFailure;
    internal static string? TakeFailure() => Interlocked.Exchange(ref _lastFailure, null);
    internal static void NoteAction() => Interlocked.Exchange(ref _lastAction, Stopwatch.GetTimestamp());
    internal static bool CanSpeculate(long now, long lastAction, int foreground) => foreground == 0 &&
        (lastAction == 0 || Stopwatch.GetElapsedTime(lastAction, now).TotalMilliseconds >= 350);
    internal static bool CanSpeculateNow => CanSpeculate(Stopwatch.GetTimestamp(),
        Interlocked.Read(ref _lastAction), Volatile.Read(ref _foreground));
    internal static IDisposable HoldForeground()
    {
        Interlocked.Increment(ref _foreground);
        return new ForegroundLease();
    }
    private sealed class ForegroundLease : IDisposable
    {
        private int _disposed;
        public void Dispose() { if (Interlocked.Exchange(ref _disposed, 1) == 0) Interlocked.Decrement(ref _foreground); }
    }
    internal static void Cancel()
    {
        Shutdown.Cancel();
        lock (Sync) { foreach (var work in Pending.Values) work.Done.TrySetCanceled(); Pending.Clear(); }
    }
    internal static bool IsWarm(string path)
    {
        try
        {
            var file = new FileInfo(path);
            lock (Sync) return Warm.TryGetValue(path, out var state) && state == (file.Length, file.LastWriteTimeUtc);
        }
        catch { return false; }
    }
    internal static void MarkWarm(string path)
    {
        try
        {
            var file = new FileInfo(path);
            lock (Sync)
            {
                if (Warm.Count >= 64) Warm.Remove(Warm.Keys.First());
                Warm[path] = (file.Length, file.LastWriteTimeUtc);
            }
        }
        catch { }
    }
    internal static void Schedule(IEnumerable<string> candidates)
    {
        foreach (var path in candidates.Distinct(StringComparer.OrdinalIgnoreCase).Take(64)) _ = Enqueue(path, false);
    }
    private static Work? Enqueue(string path, bool demand)
    {
        if (Shutdown.IsCancellationRequested || IsWarm(path)) return null;
        lock (Sync)
        {
            if (Shutdown.IsCancellationRequested) return null;
            if (!Pending.TryGetValue(path, out var work))
            {
                if (Pending.Count >= 64)
                {
                    var victim = Pending.Values.Where(item => item.Demand == 0).OrderByDescending(item => item.Sequence).FirstOrDefault();
                    if (!demand || victim == null) return null;
                    Pending.Remove(victim.Path); victim.Done.TrySetCanceled();
                }
                Pending[path] = work = new(path, ++_sequence);
                _ = work.Done.Task.ContinueWith(task => { _ = task.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            }
            if (demand) work.Demand++;
            if (!_running) { _running = true; _ = Task.Run(Run); }
            return work;
        }
    }
    internal static async Task WarmAsync(IEnumerable<string> paths, CancellationToken token = default,
        Action<double>? progress = null, bool demand = true)
    {
        var work = paths.Distinct(StringComparer.OrdinalIgnoreCase).Take(64)
            .Select(path => Enqueue(path, demand)).Where(item => item != null).Cast<Work>().ToArray();
        try
        {
            var completed = 0;
            await Task.WhenAll(work.Select(async item =>
            {
                await item.Done.Task.WaitAsync(token).ConfigureAwait(false);
                progress?.Invoke((double)Interlocked.Increment(ref completed) / Math.Max(1, work.Length));
            })).ConfigureAwait(false);
            progress?.Invoke(1);
        }
        finally
        {
            if (demand) lock (Sync) foreach (var item in work) item.Demand = Math.Max(0, item.Demand - 1);
        }
    }
    private static async Task Run()
    {
        var buffer = new byte[256 * 1024];
        while (!Shutdown.IsCancellationRequested)
        {
            Work? work;
            lock (Sync)
            {
                if (Pending.Count == 0) { _running = false; return; }
                work = Pending.Values.OrderByDescending(item => item.Demand > 0).ThenBy(item => item.Sequence).First();
                if (_foreground != 0 || work.Demand == 0 && !CanSpeculate(Stopwatch.GetTimestamp(), Interlocked.Read(ref _lastAction), _foreground)) work = null;
            }
            if (work == null) { await Task.Delay(25).ConfigureAwait(false); continue; }
            try
            {
                RuntimeLoadSafety.RequireHeadroom(0);
                if (work.Length < 0) work.Length = new FileInfo(work.Path).Length;
                var limit = work.Demand > 0 ? 512L * 1024 * 1024 : 128L * 1024 * 1024;
                if (work.Length > limit) { Complete(work, false); continue; }
                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(Shutdown.Token);
                deadline.CancelAfter(TimeSpan.FromSeconds(8));
                await using (var stream = new FileStream(work.Path, FileMode.Open, FileAccess.Read,
                    FileShare.Read | FileShare.Delete, buffer.Length, FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    stream.Position = work.Offset;
                    var read = await stream.ReadAsync(buffer.AsMemory(0, (int)Math.Min(buffer.Length, work.Length - work.Offset)), deadline.Token).ConfigureAwait(false);
                    if (read == 0 && work.Offset < work.Length) throw new EndOfStreamException(work.Path);
                    work.Offset += read;
                }
                if (work.Offset >= work.Length) Complete(work, true);
                if (!SkinPreloader.GamePreloadSettled && work.Demand == 0) await Task.Delay(10).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                lock (Sync)
                    if (Pending.TryGetValue(work.Path, out var current) && ReferenceEquals(current, work))
                        Pending.Remove(work.Path);
                if (error is OperationCanceledException && Shutdown.IsCancellationRequested) work.Done.TrySetCanceled();
                else { work.Done.TrySetException(error); Interlocked.Exchange(ref _lastFailure, error.GetBaseException().Message); }
            }
        }
        lock (Sync) _running = false;
    }
    private static void Complete(Work work, bool warmed)
    {
        lock (Sync)
        {
            if (Pending.TryGetValue(work.Path, out var current) && ReferenceEquals(current, work))
            {
                Pending.Remove(work.Path);
                if (warmed) MarkWarm(work.Path);
                work.Done.TrySetResult(true);
            }
        }
    }
}
