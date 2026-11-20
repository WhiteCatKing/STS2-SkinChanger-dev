using Godot;
using STS2SkinChanger.Ui;
using System.Runtime.CompilerServices;

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
        bindings.Add(new(canonical, alias, resource, previous));
        TakeOverRuntimeCanonicalPath(resource, canonical, previous);
    }

    private static void TakeOverRuntimeCanonicalPath(Resource resource, string canonical, Resource? previous)
    {
        // Godot take_over_path clears the displaced Resource's ResourcePath even while
        // a live scene still owns it. Keep that resource on its immutable private path
        // BEFORE the next graph claims the canonical name. Joan's native palette and
        // stage handlers identify their live atlas/skeleton through this exact path.
        if (previous != null && !ReferenceEquals(previous, resource) && RuntimeSourceAliases.TryGetValue(previous, out var displaced) &&
            !displaced.Path.Equals(canonical, StringComparison.OrdinalIgnoreCase))
            RestoreRuntimeResourceAlias(previous, displaced.Path, canonical);
        resource.TakeOverPath(canonical);
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
        resource.TakeOverPath(target);
        if (resource is PackedScene) NonCombatSceneIdentity.Register(target, canonical);
    }

    private static Resource? TryGetBoundRuntimeDependency(string canonical, string alias)
    {
        if (!ResourceLoader.HasCached(canonical)) return null;
        var cached = ResourceLoader.Load<Resource>(canonical, null, ResourceLoader.CacheMode.Reuse);
        return cached != null && RuntimeSourceAliases.TryGetValue(cached, out var source) && source.Path == alias ? cached : null;
    }
}
