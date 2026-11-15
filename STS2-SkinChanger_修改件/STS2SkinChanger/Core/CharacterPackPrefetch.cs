namespace STS2SkinChanger.Core;

// OS file-cache warming only. No Godot ResourceLoader, script registration or pack mounting
// on this worker; cancelled/stale work cannot mutate the active character's dependencies.
internal static class CharacterPackPrefetch
{
    private static readonly object Sync = new();
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly CancellationTokenSource Shutdown = new();
    private static readonly Dictionary<string, (long Size, DateTime Stamp)> Warm = new(StringComparer.OrdinalIgnoreCase);
    private static int _pending;
    private static string? _lastFailure;
    internal static string? TakeFailure() => Interlocked.Exchange(ref _lastFailure, null);

    internal static void Cancel() => Shutdown.Cancel();
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
        catch { /* optional cache hint */ }
    }

    internal static void Schedule(IEnumerable<string> candidates)
    {
        if (Shutdown.IsCancellationRequested || Interlocked.CompareExchange(ref _pending, 1, 0) != 0) return;
        var paths = candidates.Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToArray();
        _ = Task.Run(() => Run(paths));
    }

    private static async Task Run(string[] paths)
    {
        try
        {
            await Gate.WaitAsync(Shutdown.Token).ConfigureAwait(false);
            try
            {
                var buffer = new byte[256 * 1024];
                long remaining = 256L * 1024 * 1024;
                foreach (var path in paths)
                {
                    Shutdown.Token.ThrowIfCancellationRequested();
                    if (IsWarm(path)) continue;
                    var length = new FileInfo(path).Length;
                    if (length <= 0 || length > 128L * 1024 * 1024 || length > remaining) continue;
                    RuntimeLoadSafety.RequireHeadroom(0);
                    using var deadline = CancellationTokenSource.CreateLinkedTokenSource(Shutdown.Token);
                    deadline.CancelAfter(TimeSpan.FromSeconds(8));
                    await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read,
                        FileShare.Read | FileShare.Delete, buffer.Length, FileOptions.Asynchronous | FileOptions.SequentialScan);
                    // Bounded by the initial length: growing files cannot extend the work forever.
                    long readTotal = 0;
                    while (readTotal < length)
                    {
                        var count = await stream.ReadAsync(buffer.AsMemory(0, (int)Math.Min(buffer.Length, length - readTotal)), deadline.Token).ConfigureAwait(false);
                        if (count == 0) throw new EndOfStreamException(path);
                        readTotal += count;
                    }
                    remaining -= length;
                    MarkWarm(path);
                }
            }
            finally { Gate.Release(); }
        }
        catch (OperationCanceledException) { /* timeout/exit is a normal speculative cancellation */ }
        catch (Exception error)
        {
            // Report on the main thread at the next scheduling opportunity.
            Interlocked.Exchange(ref _lastFailure, error.GetBaseException().Message);
        }
        finally { Interlocked.Exchange(ref _pending, 0); }
    }
}
