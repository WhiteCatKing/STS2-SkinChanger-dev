using Godot;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Pck;
using STS2SkinChanger.Ui;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace STS2SkinChanger.Core;

internal static partial class SkinService
{
    private static readonly ResourceFailureCircuit RuntimeFailures = new();
    private static int _sessionPreparedCount;
    private static long _sessionPreparedBytes;
    private sealed record SourceAlias(string Path);
    private static readonly ConditionalWeakTable<Resource, SourceAlias> RuntimeSourceAliases = new();
    private static long _retainedRuntimePathGeneration;
    private static bool _runtimePressureCleanupPending;

    private static void RequestRuntimePressureCleanup() =>
        Volatile.Write(ref _runtimePressureCleanupPending, true);

    internal static void TryRunDeferredRuntimePressureCleanup()
    {
        if (!Volatile.Read(ref _runtimePressureCleanupPending)) return;
        lock (Sync)
        {
            if (!_runtimePressureCleanupPending ||
                CharacterAppearanceRuntime.IsCombatRuntimeScopeActive) return;

            _runtimePressureCleanupPending = false;
            // Release optional cache ownership only. Active scenes retain their own resources.
            RuntimeResourceBundles.Clear();
            StageRuntimeBundles.TrimForPressure();
            ClearRelicBundleResidency();
            RuntimeResourceCache.Clear();
            GC.Collect(2, GCCollectionMode.Optimized, blocking: false, compacting: false);
        }
    }

    internal static void PrefetchCharacterChoices(string currentGroup)
    {
        if (CharacterPackPrefetch.TakeFailure() is { } failure) ModLog.Info("[预读] 已停止本次预读：" + failure);
        if (!SkinPreloader.GamePreloadSettled || Catalog == null) return;
        try
        {
            var groups = new[] { currentGroup, "ironclad", "silent", "regent", "necrobinder", "defect", "watcher" };
            var choices = groups.Distinct(StringComparer.OrdinalIgnoreCase)
                .SelectMany(group => GetSelectionResourcePackPaths(group, Config.GetSelection(group))).ToList();
            var family = Catalog.Groups.FirstOrDefault(group => group.Id == currentGroup);
            if (family != null)
                foreach (var option in family.Options.Where(option => option.IsRuntimeProvider).Take(48))
                    choices.AddRange(GetSelectionResourcePackPaths(currentGroup, option.Id));
            CharacterPackPrefetch.Schedule(choices.Distinct(StringComparer.OrdinalIgnoreCase).Take(64).ToArray());
        }
        catch (Exception error) { ModLog.Info("[预读] 跳过不可用的选角资源：" + error.GetBaseException().Message); }
    }

    // Evict only our strong references. Never Free/Dispose live Resources, and never delete
    // mounted PCKs: scenes and Spine can still own them after this cache releases its reference.
    private static void TrimRuntimeResidency(string? keepKey = null)
    {
        var bytes = RuntimeResourceBundles.Values.Sum(bundle => bundle.EstimatedBytes);
        foreach (var pair in RuntimeResourceBundles.OrderBy(pair => pair.Value.LastUse).ToArray())
        {
            if (RuntimeResourceBundles.Count <= RuntimeLoadSafety.BundleLimit &&
                bytes <= RuntimeLoadSafety.ResidentCacheBudget) break;
            if (pair.Key == keepKey) continue;
            RuntimeResourceBundles.Remove(pair.Key);
            bytes -= pair.Value.EstimatedBytes;
            var released = pair.Value.Resources.Values.ToHashSet(ReferenceEqualityComparer.Instance);
            foreach (var key in RuntimeResourceCache.Where(entry => released.Contains(entry.Value)).Select(entry => entry.Key).ToArray())
                RuntimeResourceCache.Remove(key);
        }
        // Standalone icon/scene lookups also retain references outside bundles.
        while (RuntimeResourceCache.Count > 256) RuntimeResourceCache.Remove(RuntimeResourceCache.Keys.First());
    }

    // Every graph that passes the existing single-graph limit fits on its own. Keep a separate
    // bounded pool for these complete graphs, away from the standalone resource lookup cap.
    private const long RelicBundleBudget = RuntimeLoadSafety.MaximumSingleLoadBytes;
    private static readonly Dictionary<string, RelicBundle> RelicBundles = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConditionalWeakTable<AtlasTexture, RelicSliceOrigin> DisplayedRelicSlices = new();
    private static readonly HashSet<string> LoadingRelicBundles = new(StringComparer.OrdinalIgnoreCase);
    private static readonly CardSourceCheckCache RelicSourceChecks =
        new(static stamp => stamp.IsCurrent(), static () => System.Environment.TickCount64);

    private sealed record RelicBundle(
        string Selection, PreparedRuntimeOverlay Prepared,
        Dictionary<string, Resource> Resources)
    {
        internal long LastUse { get; set; } = Stopwatch.GetTimestamp();
    }

    private sealed record RelicSliceOrigin(string GroupId, string ResourcePath);

    private static void ClearRelicBundleResidency()
    {
        RelicBundles.Clear(); // The native view still owns any resource already assigned to it.
        RelicSourceChecks.Clear();
    }

    private static void StoreRelicBundle(string groupId, RelicBundle bundle)
    {
        RelicBundles[groupId] = bundle;
        var bytes = RelicBundles.Values.Sum(entry => entry.Prepared.EstimatedBytes);
        foreach (var entry in RelicBundles.OrderBy(entry => entry.Value.LastUse).ToArray())
        {
            if (RelicBundles.Count <= RuntimeLoadSafety.BundleLimit && bytes <= RelicBundleBudget) break;
            if (entry.Key.Equals(groupId, StringComparison.OrdinalIgnoreCase)) continue;
            RelicBundles.Remove(entry.Key);
            bytes -= entry.Value.Prepared.EstimatedBytes;
        }
    }

    private static CardFileStamp[] CaptureRelicBundleSources(SkinCatalog catalog, SkinOption option)
    {
        var sources = new Dictionary<string, CardFileStamp>(StringComparer.OrdinalIgnoreCase);
        foreach (var asset in option.Assets.Values)
            foreach (var file in asset.Files)
                if (file.SourceStamp is { } stamp) sources.TryAdd(stamp.PhysicalPath, stamp);

        // Relics can be in an auxiliary pack whose files are absent from the character's asset
        // list. Prefer the directory's registered stamps, then observe unregistered session packs.
        if (_workshopGamePack is { } game && File.Exists(game))
        {
            using var stream = File.OpenRead(game);
            var stamp = CardFileStamp.From(stream);
            sources.TryAdd(stamp.PhysicalPath, stamp);
        }
        var providers = option.IsComposition ? option.CompositionSourceProviderIds : [option.EffectiveProviderId];
        foreach (var provider in providers)
        {
            var paths = catalog.GetProviderResourcePackPaths(provider)
                .Concat(_workshopDescriptors.Where(mod => mod.Id.Equals(provider, StringComparison.OrdinalIgnoreCase))
                    .Select(mod => mod.PckPath).OfType<string>());
            foreach (var path in paths)
            {
                var full = System.IO.Path.GetFullPath(path);
                if (sources.ContainsKey(full)) continue;
                if (_workshopSourceStamps.TryGetValue(path, out var registered))
                    sources.Add(full, new(full, registered.Length, registered.Modified.Ticks));
                else if (File.Exists(full))
                {
                    using var stream = File.OpenRead(full);
                    sources.Add(full, CardFileStamp.From(stream));
                }
            }
        }
        return sources.Values.OrderBy(stamp => stamp.PhysicalPath, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void RequireRelicBundleSources(PreparedRuntimeOverlay prepared, bool fresh = false)
    {
        if (prepared.RelicSources.All(stamp => fresh ? stamp.IsCurrent() : RelicSourceChecks.IsCurrent(stamp))) return;
        if (PreparedRuntimeOverlays.TryGetValue(prepared.Key, out var current) && ReferenceEquals(current, prepared))
            PreparedRuntimeOverlays.Remove(prepared.Key);
        foreach (var entry in LastPreparedRuntimeOverlays.Where(entry => ReferenceEquals(entry.Value, prepared)).ToArray())
            LastPreparedRuntimeOverlays.Remove(entry.Key);
        throw new AppearanceResourceUnavailableException("遗物皮肤资源包已变更，等待重新扫描。");
    }

    /// <summary>
    /// Transfers already-validated relic prepared overlays and resident bundles across a confirmed
    /// additive Workshop hot-registration swap so unchanged relic atlases do not rebuild PCKs,
    /// re-decode textures, or consume extra session preparation quota. Any entry whose selection,
    /// source file stamps, or root resource set no longer matches is dropped so it reloads cleanly.
    /// </summary>
    private static void CarryOverValidatedRelicOverlaysOnAdditiveCatalogSwap(
        SkinCatalog previousCatalog,
        SkinCatalog nextCatalog)
    {
        if (ReferenceEquals(previousCatalog, nextCatalog))
        {
            return;
        }

        var reboundByPrevious = new Dictionary<PreparedRuntimeOverlay, PreparedRuntimeOverlay>(
            ReferenceEqualityComparer.Instance);
        var failedPrevious = new HashSet<PreparedRuntimeOverlay>(ReferenceEqualityComparer.Instance);

        bool TryRebindOverlay(PreparedRuntimeOverlay previous, out PreparedRuntimeOverlay rebound)
        {
            if (reboundByPrevious.TryGetValue(previous, out rebound!))
            {
                return true;
            }

            if (failedPrevious.Contains(previous) ||
                !previous.IsRelicCatalog(previousCatalog) ||
                string.IsNullOrWhiteSpace(previous.RelicGroupId) ||
                string.IsNullOrWhiteSpace(previous.RelicOptionId) ||
                previous.RelicRoots.Length == 0)
            {
                failedPrevious.Add(previous);
                rebound = null!;
                return false;
            }

            try
            {
                if (previous.OverlayPath != null && !File.Exists(previous.OverlayPath))
                {
                    failedPrevious.Add(previous);
                    rebound = null!;
                    return false;
                }

                var nextOption = nextCatalog.Groups
                    .FirstOrDefault(group => group.Id.Equals(previous.RelicGroupId, StringComparison.OrdinalIgnoreCase))?
                    .Options.FirstOrDefault(option => option.Id.Equals(previous.RelicOptionId, StringComparison.OrdinalIgnoreCase));
                if (nextOption == null)
                {
                    failedPrevious.Add(previous);
                    rebound = null!;
                    return false;
                }

                var nextRoots = nextCatalog.GetProviderRelicResourcePaths(previous.RelicGroupId, nextOption)
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                if (nextRoots.Length != previous.RelicRoots.Length ||
                    !nextRoots.SequenceEqual(previous.RelicRoots, StringComparer.OrdinalIgnoreCase))
                {
                    failedPrevious.Add(previous);
                    rebound = null!;
                    return false;
                }

                var keySelection = ProviderSettingsVariantOptions.ContractOf(nextOption.Id) != null
                    ? ProviderSettingsVariantOptions.FamilyBaseId(nextOption.Id)
                    : nextOption.Id;
                var expectedKey = RuntimeOverlayKey(
                    previous.RelicGroupId,
                    keySelection,
                    nextRoots,
                    includeProviderDependencies: false,
                    reuseMountedPrivateDependencies: false,
                    isolateRelicCanonicalPaths: true);
                if (!expectedKey.Equals(previous.Key, StringComparison.OrdinalIgnoreCase))
                {
                    failedPrevious.Add(previous);
                    rebound = null!;
                    return false;
                }

                var nextSources = CaptureRelicBundleSources(nextCatalog, nextOption);
                if (!RelicSourceStampsMatch(previous.RelicSources, nextSources) ||
                    !nextSources.All(stamp => stamp.IsCurrent()))
                {
                    failedPrevious.Add(previous);
                    rebound = null!;
                    return false;
                }

                rebound = previous with
                {
                    RelicCatalog = new WeakReference<SkinCatalog>(nextCatalog),
                    RelicSources = nextSources
                };
                reboundByPrevious[previous] = rebound;
                return true;
            }
            catch (Exception)
            {
                // Treat any per-entry verification or file I/O failure as a cache miss so the
                // published catalog swap succeeds and this relic bundle reloads cleanly on demand.
                failedPrevious.Add(previous);
                rebound = null!;
                return false;
            }
        }

        foreach (var (key, prepared) in PreparedRuntimeOverlays.ToArray())
        {
            if (!prepared.IsRelicCatalog(previousCatalog))
            {
                continue;
            }

            if (TryRebindOverlay(prepared, out var rebound))
            {
                PreparedRuntimeOverlays[key] = rebound;
            }
            else
            {
                PreparedRuntimeOverlays.Remove(key);
            }
        }

        foreach (var (groupId, prepared) in LastPreparedRuntimeOverlays.ToArray())
        {
            if (!prepared.IsRelicCatalog(previousCatalog))
            {
                continue;
            }

            if (TryRebindOverlay(prepared, out var rebound))
            {
                LastPreparedRuntimeOverlays[groupId] = rebound;
            }
            else
            {
                LastPreparedRuntimeOverlays.Remove(groupId);
            }
        }

        foreach (var (groupId, bundle) in RelicBundles.ToArray())
        {
            if (!bundle.Prepared.IsRelicCatalog(previousCatalog))
            {
                continue;
            }

            var currentSelection = Config.GetSelection(groupId);
            var selectionMatchesPrepared =
                string.Equals(bundle.Prepared.RelicOptionId, bundle.Selection, StringComparison.OrdinalIgnoreCase) ||
                (ProviderSettingsVariantOptions.ContractOf(bundle.Selection) != null &&
                 string.Equals(
                     ProviderSettingsVariantOptions.FamilyBaseId(bundle.Selection),
                     ProviderSettingsVariantOptions.FamilyBaseId(bundle.Prepared.RelicOptionId ?? string.Empty),
                     StringComparison.OrdinalIgnoreCase));
            if (currentSelection.Equals(bundle.Selection, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(bundle.Prepared.RelicGroupId, groupId, StringComparison.OrdinalIgnoreCase) &&
                selectionMatchesPrepared &&
                TryRebindOverlay(bundle.Prepared, out var reboundPrepared))
            {
                var reboundBundle = bundle with { Prepared = reboundPrepared };
                reboundBundle.LastUse = bundle.LastUse;
                RelicBundles[groupId] = reboundBundle;
            }
            else
            {
                RelicBundles.Remove(groupId);
            }
        }
    }

    private static bool RelicSourceStampsMatch(CardFileStamp[] expected, CardFileStamp[] actual)
    {
        if (expected.Length != actual.Length)
        {
            return false;
        }

        for (var i = 0; i < expected.Length; i++)
        {
            if (!expected[i].PhysicalPath.Equals(actual[i].PhysicalPath, StringComparison.OrdinalIgnoreCase) ||
                expected[i].Length != actual[i].Length ||
                expected[i].ModifiedUtcTicks != actual[i].ModifiedUtcTicks)
            {
                return false;
            }
        }

        return true;
    }

    private static readonly Regex RelicRemapTarget = new(@"(?m)^[ \t]*path(?:\.[a-zA-Z0-9_]+)?[ \t]*=[ \t]*""(?<path>res://[^""]+)""",
        RegexOptions.CultureInvariant);

    // Only rewrite the binary copies emitted into our private relic PCK. Every fallback path
    // must already resolve inside the same overlay. Unrecognized encodings retain their bytes.
    internal static int RewritePrivateRelicSliceUids(RuntimeResourceOverlay overlay, Dictionary<string, byte[]> files)
    {
        var rewritten = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (source, alias) in overlay.ResourcePaths)
        {
            if (!SkinCatalog.IsRelicAtlasSpritePath(source)) continue;
            var payload = alias;
            if (files.TryGetValue(alias + ".remap", out var redirect))
            {
                var target = RelicRemapTarget.Match(Encoding.UTF8.GetString(redirect));
                if (!target.Success) continue;
                payload = target.Groups["path"].Value;
            }
            if (!payload.StartsWith("res://sts2_skin_runtime/", StringComparison.Ordinal) ||
                rewritten.Contains(payload) || !files.TryGetValue(payload, out var bytes)) continue;
            try
            {
                var binary = CardBinaryResource.Parse(bytes);
                if (binary?.RootType != "AtlasTexture") continue;
                var dependencies = binary.Dependencies(source).ToArray();
                if (dependencies.Length == 0 || !dependencies.All(IsPrivateDependency)) continue;
                files[payload] = binary.Rewrite(source, overlay.SourceAliases, overlay.PayloadAliases);
                rewritten.Add(payload);
            }
            catch (Exception error) when (error is InvalidDataException or EndOfStreamException or OverflowException
                or NotSupportedException)
            {
                // The existing loader still has its original UID/text path fallback.
            }
        }
        return rewritten.Count;

        bool IsPrivateDependency(string path)
        {
            var extension = System.IO.Path.GetExtension(path).ToLowerInvariant();
            if (extension is not (".png" or ".ctex" or ".webp" or ".jpg" or ".jpeg")) return false;
            var local = overlay.SourceAliases.GetValueOrDefault(path) ?? overlay.PayloadAliases.GetValueOrDefault(path);
            if (local == null || !local.StartsWith("res://sts2_skin_runtime/", StringComparison.Ordinal))
                return false;
            var redirect = files.GetValueOrDefault(local + ".import") ?? files.GetValueOrDefault(local + ".remap");
            if (redirect == null) return files.ContainsKey(local);
            var targets = RelicRemapTarget.Matches(Encoding.UTF8.GetString(redirect));
            return targets.Count > 0 && targets.All(target =>
            {
                var targetPath = target.Groups["path"].Value;
                return targetPath.StartsWith("res://sts2_skin_runtime/", StringComparison.Ordinal) &&
                    files.ContainsKey(targetPath);
            });
        }
    }

    private static Resource LoadCheckedRuntimeResource(string path, bool sharedDependencies, long estimatedBytes)
    {
        // A root can decode its whole graph before returning. Recheck the graph estimate at
        // every root, rather than assuming a small allocation just because the root is a .tres.
        if (!sharedDependencies || !ResourceLoader.HasCached(path))
            RuntimeLoadSafety.RequireHeadroom(Math.Max(32L * 1024 * 1024, estimatedBytes));
        var leaf = new[] { ".ctex", ".png", ".jpg", ".jpeg", ".webp", ".skel", ".spskel" }
            .Any(extension => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
        var resource = ResourceLoader.Load<Resource>(path, null,
            sharedDependencies || leaf ? ResourceLoader.CacheMode.Reuse : ResourceLoader.CacheMode.IgnoreDeep);
        if (resource == null || !GodotObject.IsInstanceValid(resource) ||
            resource is PackedScene scene && !scene.CanInstantiate() ||
            resource is Texture2D texture && (texture.GetWidth() <= 0 || texture.GetHeight() <= 0))
            throw new AppearanceResourceUnavailableException("资源无效，停止建模而不向 Spine 传递半成品：" + path);
        return resource;
    }

    private static void BindRuntimeDependency(string canonical, string alias, Resource resource,
        List<CanonicalRuntimeResourceOwnershipEntry> bindings)
    {
        // Select portraits and shared game atlas textures are served privately, never globally.
        if (STS2SkinChanger.Catalog.SkinCatalog.IsCanonicalCharacterSelectPortrait(canonical) ||
            STS2SkinChanger.Catalog.SkinCatalog.IsSharedGameAtlasPath(canonical)) return;

        PrivateMerchantResourceIdentity.Register(resource, canonical);
        var previous = ResourceLoader.HasCached(canonical)
            ? ResourceLoader.Load<Resource>(canonical, null, ResourceLoader.CacheMode.Reuse) : null;
        RuntimeSourceAliases.Remove(resource);
        RuntimeSourceAliases.Add(resource, new(alias));
        if (ReferenceEquals(previous, resource)) return;
        TakeOverRuntimeCanonicalPath(resource, canonical, previous,
            () => bindings.Add(new(canonical, alias, resource, previous)));
    }

    private static void TakeOverRuntimeCanonicalPath(Resource resource, string canonical, Resource? previous,
        Action? registerBinding = null)
    {
        // Godot take_over_path clears the displaced Resource's ResourcePath even while
        // a live scene still owns it. Keep that resource on its immutable private path
        // BEFORE the next graph claims the canonical name. Joan's native palette and
        // stage handlers identify their live atlas/skeleton through this exact path.
        GlobalResourceWrites.TakeOver(GlobalWriteSite.A16, resource, canonical, beforeWrite: () =>
        {
            registerBinding?.Invoke();
            if (previous != null && !ReferenceEquals(previous, resource) &&
                RuntimeSourceAliases.TryGetValue(previous, out var displaced) &&
                !displaced.Path.Equals(canonical, StringComparison.OrdinalIgnoreCase))
                RestoreRuntimeResourceAlias(previous, displaced.Path, canonical);
        });
    }

    private static void RestoreRuntimeResourceAlias(Resource resource, string alias, string canonical)
    {
        // A text scene may already have decoded another copy under this immutable alias.
        // Returning an older resource there must not erase THAT live copy's path either.
        var target = alias;
        if (ResourceLoader.HasCached(alias) && !ReferenceEquals(
                ResourceLoader.Load<Resource>(alias, null, ResourceLoader.CacheMode.Reuse), resource))
            target = "res://sts2_skin_runtime/retained/" + Interlocked.Increment(ref _retainedRuntimePathGeneration) + "/" +
                (alias.StartsWith("res://", StringComparison.Ordinal) ? alias[6..] : alias);
        GlobalResourceWrites.TakeOver(GlobalWriteSite.A17, resource, target, restoring: true);
        if (resource is PackedScene) NonCombatSceneIdentity.Register(target, canonical);
    }

    private static Resource? TryGetBoundRuntimeDependency(string canonical, string alias)
    {
        if (!ResourceLoader.HasCached(canonical)) return null;
        var cached = ResourceLoader.Load<Resource>(canonical, null, ResourceLoader.CacheMode.Reuse);
        return cached != null && RuntimeSourceAliases.TryGetValue(cached, out var source) && source.Path == alias ? cached : null;
    }
}
