namespace STS2SkinChanger.Core;

internal sealed class CacheWriteNoticeState
{
    private readonly object _sync = new();
    private readonly HashSet<CacheWriteRejectionReason> _shown = [];
    private CacheWriteRejectionReason? _pending;
    private object? _run;
    internal void Report(CacheWriteRejectionReason reason) { lock (_sync) _pending = reason; }
    internal void SetRun(object? run)
    {
        lock (_sync)
        {
            if (ReferenceEquals(run, _run)) return;
            _run = run;
            _shown.Clear();
        }
    }
    internal bool TryTake(out CacheWriteRejectionReason reason, out bool first)
    {
        lock (_sync)
        {
            reason = _pending.GetValueOrDefault();
            first = false;
            if (_pending == null) return false;
            _pending = null;
            first = _shown.Add(reason);
            return true;
        }
    }
}
