using STS2SkinChanger.Core;

namespace STS2SkinChanger.Catalog;

internal sealed partial class SkinCatalog
{
    private readonly ProviderPackFileCache _providerPackFiles = new();

    // Called under SkinService.Sync. A catalog rebuilt by a directory scan starts with no file observations.
    internal ProviderPackMountSelection SelectProviderPacks(Func<string, string> getSelection,
        IReadOnlySet<string> affectedGroups, long promotionThreshold, ProviderPackFileCache? fileCache = null)
    {
        fileCache ??= _providerPackFiles;
        var snapshot = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var selected = new List<(string GroupId, string ProviderId)>();
        foreach (var group in Groups)
        {
            var selection = getSelection(group.Id);
            var option = group.Options.FirstOrDefault(option =>
                option.Id.Equals(selection, StringComparison.OrdinalIgnoreCase) && option.IsRuntimeProvider);
            snapshot.Add(group.Id, option?.Id ?? selection);
            if (option != null) selected.Add((group.Id, option.EffectiveProviderId));
        }
        fileCache.UseSelections(snapshot);

        var providers = new Dictionary<string, (string[] Paths, bool CompanionOnly)>(StringComparer.OrdinalIgnoreCase);
        var direct = new List<SelectedProviderPack>();
        foreach (var (group, provider) in selected)
        {
            if (!providers.TryGetValue(provider, out var facts))
            {
                var coherent = ProviderRequiresCoherentRuntimePackage(provider);
                var companionOnly = IsCompanionOnlyProviderSelection(provider, snapshot);
                var paths = GetIndexedProviderResourcePackPaths(provider)
                    .Select(path => (Path: path, Stamp: fileCache.Observe(path)))
                    .Where(file => file.Stamp != null)
                    .DistinctBy(file => file.Path, StringComparer.OrdinalIgnoreCase)
                    .Where(file => coherent || file.Stamp!.Length >= promotionThreshold)
                    .Select(file => file.Stamp!.Path)
                    .ToArray();
                facts = (paths, companionOnly);
                providers.Add(provider, facts);
            }
            foreach (var path in facts.Paths)
                direct.Add(new(group, path, facts.CompanionOnly, provider));
        }

        var packs = direct.ToArray();
        return new(packs, packs.Select(pack => pack.Path).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            ProviderPackMountSelection.CompanionOnlyPaths(packs),
            packs.Where(pack => affectedGroups.Contains(pack.GroupId)).Select(pack => pack.Path)
                .ToHashSet(StringComparer.OrdinalIgnoreCase));
    }
}

internal sealed record SelectedProviderPack(string GroupId, string Path, bool CompanionOnly, string ProviderId);

internal sealed record ProviderPackMountSelection(SelectedProviderPack[] Direct, string[] Selected,
    HashSet<string> CompanionOnly, HashSet<string> Promoted)
{
    // A physical pack shared between companions and a normal group must remain promoted.
    internal static HashSet<string> CompanionOnlyPaths(IEnumerable<SelectedProviderPack> packs) => packs
        .GroupBy(pack => pack.Path, StringComparer.OrdinalIgnoreCase)
        .Where(group => group.All(pack => pack.CompanionOnly))
        .Select(group => group.Key)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);
}

internal sealed record ProviderPackFileStamp(string Path, long Length, long ModifiedUtcTicks);

internal sealed class ProviderPackFileCache
{
    private const long MaxAgeMs = 2000;
    private readonly Func<string, ProviderPackFileStamp?> _probe;
    private readonly Func<long> _clock;
    private readonly BoundedLruCache<string, (long CheckedAt, ProviderPackFileStamp? Stamp)> _observations =
        new(512, StringComparer.Ordinal);
    private IReadOnlyDictionary<string, string>? _selections;

    internal ProviderPackFileCache(Func<string, ProviderPackFileStamp?>? probe = null, Func<long>? clock = null)
    {
        _probe = probe ?? ReadStamp;
        _clock = clock ?? (() => Environment.TickCount64);
    }

    internal void UseSelections(IReadOnlyDictionary<string, string> selections)
    {
        if (_selections?.Count == selections.Count && selections.All(pair =>
                _selections.TryGetValue(pair.Key, out var choice) &&
                choice.Equals(pair.Value, StringComparison.OrdinalIgnoreCase))) return;
        Clear();
        _selections = selections;
    }

    internal ProviderPackFileStamp? Observe(string path)
    {
        var now = _clock();
        if (_observations.TryGetValue(path, out var observed) && now - observed.CheckedAt < MaxAgeMs)
            return observed.Stamp;

        var stamp = _probe(path);
        _observations.Set(path, (now, stamp), out _);
        return stamp;
    }

    internal void Clear() => _observations.Clear();

    internal static ProviderPackFileStamp? ReadStamp(string path)
    {
        // FileInfo caches the same stat for Exists, Length and LastWriteTimeUtc.
        var file = new FileInfo(path);
        return file.Exists ? new(file.FullName, file.Length, file.LastWriteTimeUtc.Ticks) : null;
    }
}
