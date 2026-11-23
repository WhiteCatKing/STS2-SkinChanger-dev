namespace STS2SkinChanger.Core;

// Owns cache references only. Live scene owners are independent of the selection or bundle key.
internal sealed class RuntimeBundleResidency<T>(long budget, int limit) where T : class
{
    private readonly Dictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<RetiredBundle> _retired = [];
    private long _access;
    private string? _preferredFamily;

    internal void Prefer(string family) => _preferredFamily = family;

    internal long Bytes
    {
        get
        {
            _retired.RemoveAll(bundle => !HasLiveHolder(bundle.Holders));
            return _entries.Values.Sum(entry => entry.Bytes) + _retired.Sum(bundle => bundle.Bytes);
        }
    }
    internal int Count => _entries.Count;
    internal string LastEviction { get; private set; } = "-";

    internal T? Find(string key)
    {
        if (!_entries.TryGetValue(key, out var entry)) return null;
        entry.LastUse = ++_access;
        return entry.Value;
    }

    internal bool Store(string key, string family, T value, long bytes, Func<long, bool> hasHeadroom)
    {
        LastEviction = "-";
        if (_entries.ContainsKey(key)) return false;
        if (bytes > budget)
        {
            LastEviction = "单包超额";
            return false;
        }

        if (!hasHeadroom(0))
        {
            TrimForPressure();
            return false;
        }

        while (Count >= limit || bytes > budget - Bytes)
        {
            var preferred = _preferredFamily ?? family;
            var allowPreferredEviction = family.Equals(preferred, StringComparison.OrdinalIgnoreCase);
            var victim = _entries
                .Where(pair => !pair.Value.IsHeld &&
                    (allowPreferredEviction || !pair.Value.Family.Equals(preferred, StringComparison.OrdinalIgnoreCase)))
                .OrderBy(pair => pair.Value.Family.Equals(preferred, StringComparison.OrdinalIgnoreCase))
                .ThenBy(pair => pair.Value.LastUse)
                .FirstOrDefault();
            if (victim.Key == null)
            {
                LastEviction = allowPreferredEviction ? "持有项占满预算" : "优先保留本机外观";
                return false;
            }
            LastEviction = Count >= limit ? "包数量上限" : "纹理预算";
            _entries.Remove(victim.Key);
        }

        _entries.Add(key, new(value, family, bytes, ++_access));
        return true;
    }

    internal Lease? Hold(string key)
    {
        if (!_entries.TryGetValue(key, out var entry)) return null;
        _ = HasLiveHolder(entry.Holders);
        var lease = new Lease(entry.Value);
        entry.Holders.Add(new(lease));
        return lease;
    }

    // A changed source is immediately unavailable to new callers. Existing holders and native
    // scenes are allowed to finish with the old graph; we never dispose their resources here.
    internal void Forget(string key)
    {
        if (_entries.Remove(key, out var entry) && entry.IsHeld)
            _retired.Add(new(entry.Bytes, entry.Holders));
    }

    internal void TrimForPressure()
    {
        foreach (var pair in _entries.Where(pair => !pair.Value.IsHeld).ToArray())
            _entries.Remove(pair.Key);
        LastEviction = "内存压力";
    }

    internal void Clear()
    {
        foreach (var key in _entries.Keys.ToArray()) Forget(key);
        _preferredFamily = null;
        _ = Bytes;
    }

    private static bool HasLiveHolder(List<WeakReference<Lease>> holders)
    {
        holders.RemoveAll(holder => !holder.TryGetTarget(out var lease) || !lease.Active);
        return holders.Count != 0;
    }

    internal sealed class Lease(T value) : IDisposable
    {
        private T? _value = value;
        internal bool Active => _value != null;
        public void Dispose() => _value = null;
    }

    private sealed class Entry(T value, string family, long bytes, long lastUse)
    {
        internal T Value { get; } = value;
        internal string Family { get; } = family;
        internal long Bytes { get; } = bytes;
        internal long LastUse { get; set; } = lastUse;
        internal List<WeakReference<Lease>> Holders { get; } = [];
        internal bool IsHeld => HasLiveHolder(Holders);
    }

    private sealed record RetiredBundle(long Bytes, List<WeakReference<Lease>> Holders);
}
