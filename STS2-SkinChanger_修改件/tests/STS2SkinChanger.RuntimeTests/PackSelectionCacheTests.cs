using System.Diagnostics;
using System.Text.Json;
using HarmonyLib;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class PackSelectionCacheTests
{
    private const long Threshold = 64L * 1024 * 1024;
    private static readonly StringComparer Id = StringComparer.OrdinalIgnoreCase;
    private static readonly System.Reflection.FieldInfo CosmeticIndexes =
        AccessTools.Field(typeof(SkinCatalog), "_cosmeticIndexes");

    internal static void Benchmark(string game, string config, string[] roots)
    {
        using var logging = new LogSilence();
        var mods = roots.SelectMany(root => Directory.EnumerateFiles(root, "*.pck", SearchOption.AllDirectories))
            .Select(Descriptor).Where(mod => mod != null).Cast<SkinModDescriptor>().ToArray();
        using var catalog = SkinCatalog.Build(game, mods);
        using var json = JsonDocument.Parse(File.ReadAllText(config).TrimStart('\uFEFF'));
        var selections = json.RootElement.GetProperty("Selections").EnumerateObject()
            .Where(item => item.Value.ValueKind == JsonValueKind.String)
            .ToDictionary(item => item.Name, item => item.Value.GetString()!, Id);
        string Selection(string group) => catalog.ResolveSpineSelection(group,
            selections.GetValueOrDefault(group, SkinCatalog.BaseOptionId));
        var affected = new HashSet<string>(["ironclad"], Id);
        var warm = Legacy(catalog, Selection, affected, out _);
        Console.WriteLine($"reference descriptors={mods.Length} groups={catalog.Groups.Count} " +
            $"options={catalog.Groups.Sum(group => group.Options.Count)} selected={warm.Packs.Length}");
        var times = new double[7];
        const int samples = 24;
        for (var i = 0; i < samples; i++)
        {
            Legacy(catalog, Selection, affected, out var sample);
            for (var j = 0; j < times.Length; j++) times[j] += sample[j];
        }
        string[] labels = ["全部", "选择/选项", "coherent", "companion", "包路径查询", "规范化/大小", "投影"];
        Console.WriteLine(string.Join(", ", labels.Select((label, index) => $"{label}={times[index] / samples:F3}ms")));

        var cache = new ProviderPackFileCache();
        Same(warm, catalog.SelectProviderPacks(Selection, affected, Threshold, cache));
        var warmStarted = Stopwatch.GetTimestamp();
        for (var i = 0; i < samples; i++)
            catalog.SelectProviderPacks(Selection, affected, Threshold, cache);
        var warmMs = Stopwatch.GetElapsedTime(warmStarted).TotalMilliseconds / samples;
        var coldStarted = Stopwatch.GetTimestamp();
        for (var i = 0; i < samples; i++)
        {
            cache.Clear(); // Same selections, fresh file state on every pass.
            catalog.SelectProviderPacks(Selection, affected, Threshold, cache);
        }
        var coldMs = Stopwatch.GetElapsedTime(coldStarted).TotalMilliseconds / samples;
        Console.WriteLine($"优化后：热缓存={warmMs:F3}ms, 重查全部来源={coldMs:F3}ms");
    }

    internal static void Run()
    {
        using var logging = new LogSilence();
        var temp = Directory.CreateTempSubdirectory("sc-pack-selection-");
        try
        {
            string Pack(string name, bool large = false)
            {
                var path = Path.Combine(temp.FullName, name + ".pck");
                PckArchive.Write(path, new Dictionary<string, byte[]> { [$"res://{name}/marker.bin"] = [1] });
                if (large)
                {
                    using var stream = File.OpenWrite(path);
                    stream.SetLength(Threshold);
                }
                return path;
            }
            var game = Pack("game");
            var shared = Pack("shared", true);
            var small = Pack("small");
            var coherent = Pack("coherent");
            var disposable = Pack("ordinary", true);
            var descriptors = new[] { shared, small, coherent, disposable }.Select(path =>
                new SkinModDescriptor(Path.GetFileNameWithoutExtension(path), path, path, false)).ToArray();
            using var catalog = SkinCatalog.Build(game, descriptors);
            AddFixtureGroups(catalog);
            var choices = new Dictionary<string, string>(Id)
            {
                ["necrobinder"] = "owner-shared", ["osty"] = "summon",
                ["regent"] = "tiny", ["defect"] = "script", ["ironclad"] = "ordinary"
            };
            string Select(string group) => choices.GetValueOrDefault(group, SkinCatalog.BaseOptionId);
            var affected = new HashSet<string>(["osty", "defect", "regent"], Id);
            long clock = 500;
            var probes = new List<string>();
            var cache = new ProviderPackFileCache(path =>
            {
                probes.Add(path);
                return ProviderPackFileCache.ReadStamp(path);
            }, () => clock);
            ProviderPackMountSelection Plan(SkinCatalog source, ProviderPackFileCache? observations = null) =>
                source.SelectProviderPacks(Select, affected, Threshold, observations);
            var first = Plan(catalog, cache);
            Same(Legacy(catalog, Select, affected, out _), first);
            Require(first.Selected.SequenceEqual(new[] { shared, coherent, disposable }, Id) &&
                    first.CompanionOnly.Count == 0 && first.Promoted.SetEquals([shared, coherent]),
                "Shared owner + companion, coherent below threshold and ordinary below threshold diverged.");
            Require(probes.Count == 4, "One filesystem observation per source, even across shared owners.");
            Same(Legacy(catalog, Select, affected, out _), Plan(catalog, cache));
            Require(probes.Count == 4, "Repeated selections within the window must reuse the probes.");
            affected.Clear();
            Require(Plan(catalog, cache).Promoted.Count == 0 && probes.Count == 4,
                "Affected groups apply on each call without discarding file metadata.");

            choices["necrobinder"] = SkinCatalog.BaseOptionId;
            var onlyCompanion = Plan(catalog, cache);
            Same(Legacy(catalog, Select, affected, out _), onlyCompanion);
            Require(onlyCompanion.CompanionOnly.SetEquals([shared]) && probes.Count == 8,
                "Selection change must recompute ownership and refresh sources immediately.");

            var previous = cache.Observe(disposable)!;
            static void ReplaceFile(string path, long bytes, DateTime writeTime)
            {
                var replacement = path + ".tmp";
                using (var stream = File.Create(replacement)) stream.SetLength(bytes);
                File.SetLastWriteTimeUtc(replacement, writeTime);
                // Move the old file away first. Windows refuses overwrite-rename onto an open
                // archive even though its existing handle permits a rename without replacement.
                File.Move(path, path + "." + Guid.NewGuid().ToString("N"));
                File.Move(replacement, path);
            }
            ReplaceFile(disposable, previous.Length, new DateTime(previous.ModifiedUtcTicks, DateTimeKind.Utc).AddMinutes(1));
            clock += 1999;
            Require(cache.Observe(disposable) == previous, "File status must be reused for at most two seconds.");
            clock++;
            var timestampOnly = cache.Observe(disposable)!;
            Require(timestampOnly.Length == previous.Length && timestampOnly.ModifiedUtcTicks != previous.ModifiedUtcTicks,
                "Timestamp-only replacement must refresh physical metadata after expiry.");

            // Prime the catalog-owned cache, then alter the on-disk size without changing selections.
            Require(Plan(catalog).Selected.Contains(disposable, Id), "Initial directory snapshot missed the large pack.");
            ReplaceFile(disposable, Threshold - 1, new DateTime(timestampOnly.ModifiedUtcTicks, DateTimeKind.Utc).AddMinutes(1));
            clock += 2000;
            var resized = Plan(catalog, cache);
            Same(Legacy(catalog, Select, affected, out _), resized);
            Require(!resized.Selected.Contains(disposable, Id), "A size change across the promotion boundary must affect the plan.");

            // Use a valid small PCK as the rescanned replacement.
            var replacementPck = Pack("replacement");
            File.Move(disposable, disposable + ".previous");
            File.Move(replacementPck, disposable);
            using var rescanned = SkinCatalog.Build(game, descriptors);
            AddFixtureGroups(rescanned);
            Require(!Plan(rescanned).Selected.Contains(disposable, Id),
                "A rebuilt catalog must take a fresh file observation for the same selection and path.");
            Same(Legacy(rescanned, Select, affected, out _), Plan(rescanned));
        }
        finally
        {
            temp.Delete(recursive: true);
        }
        Console.WriteLine("Provider pack selection passed: legacy equivalence, companions, thresholds, file identity, selection and rescan.");
    }

    private static void AddFixtureGroups(SkinCatalog catalog)
    {
        var groups = (List<SkinGroup>)catalog.Groups;
        groups.Clear();
        void Add(string group, string id, string provider)
        {
            var item = new SkinGroup(group, group);
            item.Options.Add(new(id, id, new Dictionary<string, ResourceAsset>(), IsRuntimeProvider: true, ProviderId: provider));
            groups.Add(item);
        }
        Add("necrobinder", "owner-shared", "shared");
        Add("osty", "summon", "shared");
        Add("regent", "tiny", "small");
        Add("defect", "script", "coherent");
        Add("ironclad", "ordinary", "ordinary");
        ((HashSet<string>)AccessTools.Field(typeof(SkinCatalog), "_managedGodotScriptProviders").GetValue(catalog)!).Add("coherent");
    }

    private static void Same((string[] Packs, HashSet<string> Companion, HashSet<string> Promotions) legacy,
        ProviderPackMountSelection actual)
    {
        Require(legacy.Packs.SequenceEqual(actual.Selected, Id) && legacy.Companion.SetEquals(actual.CompanionOnly) &&
                legacy.Promotions.SetEquals(actual.Promoted), "Selection differs from the original algorithm.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static SkinModDescriptor? Descriptor(string pck)
    {
        var manifest = Path.ChangeExtension(pck, ".json");
        if (!File.Exists(manifest)) return null;
        try
        {
            using var json = JsonDocument.Parse(File.ReadAllText(manifest).TrimStart('\uFEFF'));
            var root = json.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("id", out var id) ||
                id.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(id.GetString())) return null;
            return new SkinModDescriptor(id.GetString()!, id.GetString()!, pck,
                !root.TryGetProperty("affects_gameplay", out var gameplay) || gameplay.ValueKind != JsonValueKind.False,
                Path.GetDirectoryName(pck), File.Exists(Path.ChangeExtension(pck, ".dll")));
        }
        catch (JsonException) { return null; }
    }

    private static (string[] Packs, HashSet<string> Companion, HashSet<string> Promotions) Legacy(
        SkinCatalog catalog, Func<string, string> selection, IReadOnlySet<string> affected, out double[] elapsed)
    {
        var indexes = (IEnumerable<PckResourceIndex>)CosmeticIndexes.GetValue(catalog)!;
        var times = new double[7];
        var start = Stopwatch.GetTimestamp();
        var selected = catalog.Groups.Select(group =>
        {
            var id = selection(group.Id);
            return (group.Id, Option: group.Options.FirstOrDefault(option =>
                option.Id.Equals(id, StringComparison.OrdinalIgnoreCase) && option.IsRuntimeProvider));
        }).Where(item => item.Option != null).ToArray();
        // GetVisualSelection runs before walking the options even for groups with no options.
        // The same values are used here. A separate pass below matches the original snapshot.
        var snapshot = selected.ToDictionary(item => item.Id, item => item.Option!.Id, Id);
        foreach (var group in catalog.Groups.Where(group => !snapshot.ContainsKey(group.Id)))
            snapshot[group.Id] = selection(group.Id);
        times[1] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        var direct = new List<(string Group, string Path, bool Companion)>();
        foreach (var (group, option) in selected)
        {
            var provider = option!.EffectiveProviderId;
            var mark = Stopwatch.GetTimestamp();
            var coherent = catalog.ProviderRequiresCoherentRuntimePackage(provider);
            times[2] += Stopwatch.GetElapsedTime(mark).TotalMilliseconds;
            mark = Stopwatch.GetTimestamp();
            var companion = catalog.IsCompanionOnlyProviderSelection(provider, snapshot);
            times[3] += Stopwatch.GetElapsedTime(mark).TotalMilliseconds;
            mark = Stopwatch.GetTimestamp();
            // Preserve the original predicate, including resolving once per candidate index.
            var paths = indexes.Where(index => index.Mod.Id.Equals(
                    catalog.ResolveVisualProviderId(provider), StringComparison.OrdinalIgnoreCase))
                .Select(index => index.Archive.Path).Where(File.Exists).Distinct(Id).ToArray();
            times[4] += Stopwatch.GetElapsedTime(mark).TotalMilliseconds;
            mark = Stopwatch.GetTimestamp();
            direct.AddRange(paths.Select(Path.GetFullPath)
                .Where(path => coherent || new FileInfo(path).Length >= Threshold)
                .Select(path => (group, path, companion)));
            times[5] += Stopwatch.GetElapsedTime(mark).TotalMilliseconds;
        }
        var project = Stopwatch.GetTimestamp();
        var packs = direct.Select(pack => pack.Path).Distinct(Id).ToArray();
        var companionOnly = direct.GroupBy(pack => pack.Path, Id).Where(group => group.All(pack => pack.Companion))
            .Select(group => group.Key).ToHashSet(Id);
        var promotions = direct.Where(pack => affected.Contains(pack.Group)).Select(pack => pack.Path).ToHashSet(Id);
        times[6] = Stopwatch.GetElapsedTime(project).TotalMilliseconds;
        times[0] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        elapsed = times;
        return (packs, companionOnly, promotions);
    }

    private sealed class LogSilence : IDisposable
    {
        private readonly HarmonyLib.Harmony _harmony = AuditLogSilencer.Install();
        public void Dispose() => _harmony.UnpatchAll(_harmony.Id);
    }
}
