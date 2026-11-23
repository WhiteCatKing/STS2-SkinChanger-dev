using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Core;

// Called under SkinService.Sync. A cached observation expires by elapsed time, even on a hit.
internal sealed class CardSourceCheckCache(Func<CardFileStamp, bool> probe, Func<long> clock)
{
    private const long MaxAgeMs = 2000;
    // Enough for the private and fallback caches even when every card is a separate loose file.
    private readonly BoundedLruCache<CardFileStamp, (long CheckedAt, bool Current)> _checks = new(2048);

    internal bool IsCurrent(CardFileStamp stamp)
    {
        var now = clock();
        if (_checks.TryGetValue(stamp, out var cached) && now - cached.CheckedAt < MaxAgeMs)
            return cached.Current;

        var current = probe(stamp);
        _checks.Set(stamp, (now, current), out _);
        return current;
    }

    // Recheck each source once when next used, without statting all past selections at once.
    internal void Clear() => _checks.Clear();
}
