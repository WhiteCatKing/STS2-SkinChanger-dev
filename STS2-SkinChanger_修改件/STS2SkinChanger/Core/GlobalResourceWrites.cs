using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Core;

// IDs match the individual call sites in docs/agents/global-write-policy.md.
internal enum GlobalWriteSite
{
    A01 = 1, A02, A03, A04, A05, A06, A07, A08, A09, A10,
    A11, A12, A13, A14, A15, A16, A17, A18, A19, A20,
    A21, A22, A23, A24, A25, A26, A27, A28, A29, A30,
    A31, A32
}

internal enum GlobalWriteKind { PackReplace, PackFill, TakeOver, Preload, Replace, ReplaceDeep, ResourcePath }
internal enum GlobalWriteResult { Succeeded, Failed, Restored, OriginalRestored, Blocked, Stripped }

internal sealed class GlobalWriteBlockedException(GlobalWriteSite site, string path, ForbiddenGlobalWrite rule, string? reason = null)
    : InvalidOperationException(reason == null
        ? $"[全局写入] 已拦截 {rule} {path} ← {site}"
        : $"[全局写入] 已拦截无法安全投影的包 {path} ← {site}：{reason}")
{
    internal GlobalWriteSite Site { get; } = site;
    internal string ResourcePath { get; } = path;
    internal ForbiddenGlobalWrite Rule { get; } = rule;
    internal string? Reason { get; } = reason;
}

internal sealed record GlobalWriteEntry(
    GlobalWriteSite Site, string Caller, GlobalWriteKind Kind, GlobalWriteResult Result,
    IReadOnlyList<string> CanonicalPaths, int CanonicalCount, IReadOnlyList<string> OriginalPaths,
    int OriginalCount, int Violations, bool Incomplete, string? SourcePack = null);

internal sealed record GlobalPackProjection(IReadOnlyDictionary<string, ForbiddenGlobalWrite> Stripped, int OriginalCount);

internal static class GlobalResourceWrites
{
    private const int LedgerLimit = 128;
    private const int PackCacheLimit = 32;
    private const int PathSampleLimit = 256;
    private const int DependencyLimit = 512;
    private static readonly object Sync = new();
    private static readonly Queue<GlobalWriteEntry> Ledger = new();
    private static readonly Dictionary<PackKey, Observation> PackObservations = new();
    private static readonly ConditionalWeakTable<Resource, OriginalResource> OriginalResources = new();
    private static readonly Func<string, bool, bool> NativeMount = static (path, replace) =>
        ProjectSettings.LoadResourcePack(path, replace);
    private static GlobalWriteProtection? _cachedProtection;
    private static long _total;
    private static long _failed;
    private static long _restored;
    private static long _originalRestored;
    private static long _blocked;
    private static long _stripped;
    private static long _violations;
    private static long _incomplete;
    private static int _summarized;

    internal static bool MountPack(GlobalWriteSite site, string path, bool replaceFiles,
        [CallerMemberName] string caller = "", bool throwOnBlock = false) =>
        MountPack(site, path, replaceFiles, NativeMount, CurrentProtection, ModLog.Warn, caller, throwOnBlock);

    internal static bool MountPack(GlobalWriteSite site, string path, bool replaceFiles,
        Func<string, bool, bool> mount, Func<GlobalWriteProtection?> protection,
        Action<string> warn, string caller, bool throwOnBlock = false) =>
        Write(site, caller, replaceFiles ? GlobalWriteKind.PackReplace : GlobalWriteKind.PackFill,
            () => TryObservePack(path, replaceFiles, protection), () => mount(path, replaceFiles),
            static mounted => mounted, warn, restoring: site == GlobalWriteSite.A12, throwOnBlock: throwOnBlock,
            projectedPack: throwOnBlock && IsProjectedSite(site) ? path : null);

    internal static void CheckMount(GlobalWriteSite site, string path, bool replaceFiles,
        [CallerMemberName] string caller = "")
    {
        var observation = TryObservePack(path, replaceFiles, CurrentProtection);
        var projectedPack = IsProjectedSite(site) ? path : null;
        if (!observation.Blocked && (projectedPack == null || observation.ReadError == null)) return;
        // Preflight records only actual rejections. Accepted packs get their entry at mount time.
        Write(site, caller, replaceFiles ? GlobalWriteKind.PackReplace : GlobalWriteKind.PackFill,
            () => observation, static () => false, static mounted => mounted, ModLog.Warn, throwOnBlock: true,
            projectedPack: projectedPack);
    }

    private static bool IsProjectedSite(GlobalWriteSite site) =>
        site is GlobalWriteSite.A07 or GlobalWriteSite.A10 or GlobalWriteSite.A11;

    internal static GlobalPackProjection InspectHighPriorityPack(PckArchive archive, GlobalWriteProtection protection)
    {
        var stripped = new Dictionary<string, ForbiddenGlobalWrite>(StringComparer.OrdinalIgnoreCase);
        var originals = 0;
        foreach (var path in archive.Paths)
        {
            var rule = protection.Decide(path, fromGame: false);
            if (rule == ForbiddenGlobalWrite.None) continue;
            if (protection.MatchesGameEntry(archive, path, path)) originals++;
            else stripped.Add(path, rule);
        }
        foreach (var path in archive.RemovedPaths)
        {
            var rule = protection.Decide(path, fromGame: false);
            if (rule != ForbiddenGlobalWrite.None) stripped[path] = rule;
        }
        return new GlobalPackProjection(stripped, originals);
    }

    internal static void RecordStripped(GlobalWriteSite site, string sourcePack, IReadOnlyCollection<string> paths,
        [CallerMemberName] string caller = "")
    {
        if (paths.Count == 0) return;
        lock (Sync)
        {
            _stripped += paths.Count;
            if (Ledger.Count == LedgerLimit) Ledger.Dequeue();
            Ledger.Enqueue(new GlobalWriteEntry(site, caller, GlobalWriteKind.PackReplace, GlobalWriteResult.Stripped,
                paths.ToArray(), paths.Count, [], 0, 0, false, sourcePack));
        }
        ModLog.Info($"[全局写入] {site}/{caller} 已剔除 {paths.Count} 条：{sourcePack}");
    }

    internal static bool TakeOver(GlobalWriteSite site, Resource resource, string path,
        bool restoring = false, Action? beforeWrite = null,
        [CallerMemberName] string caller = "") =>
        WritePath(site, caller, GlobalWriteKind.TakeOver, path, () =>
        {
            beforeWrite?.Invoke();
            resource.TakeOverPath(path);
        }, restoring, resource);

    internal static bool SetPreloadAsset(GlobalWriteSite site, AssetCache cache, string path, Resource? asset,
        bool restoring = false, [CallerMemberName] string caller = "") =>
        WritePath(site, caller, GlobalWriteKind.Preload, path, () => cache.SetAsset(path, asset!), restoring, asset);

    // Call only after authenticating the live bytes for the original resource at canonicalPath.
    internal static void RememberVerifiedOriginal(Resource resource, string canonicalPath) =>
        OriginalResources.AddOrUpdate(resource, new OriginalResource(canonicalPath));

    internal static void VerifyOriginalResource(Resource resource, string canonicalPath, string loadedPath,
        byte[] gameBytes)
    {
        try
        {
            OriginalResources.Remove(resource);
            if (CurrentProtection()?.Decide(canonicalPath, fromGame: false) is { } rule &&
                rule != ForbiddenGlobalWrite.None &&
                Godot.FileAccess.GetFileAsBytes(loadedPath).AsSpan().SequenceEqual(gameBytes))
                RememberVerifiedOriginal(resource, canonicalPath);
        }
        catch (Exception) { /* Missing proof cannot grant a protected cache write. */ }
    }

    internal static void SetResourcePath(GlobalWriteSite site, Resource resource, string path,
        [CallerMemberName] string caller = "") =>
        WritePath(site, caller, GlobalWriteKind.ResourcePath, path, () => resource.ResourcePath = path);

    internal static T LoadReplacing<T>(GlobalWriteSite site, string path, string? typeHint,
        ResourceLoader.CacheMode mode, bool restoring = false, [CallerMemberName] string caller = "")
        where T : class =>
        Write(site, caller,
            mode == ResourceLoader.CacheMode.ReplaceDeep ? GlobalWriteKind.ReplaceDeep : GlobalWriteKind.Replace,
            () => mode == ResourceLoader.CacheMode.ReplaceDeep
                ? ObserveDependencies(path, CurrentProtection())
                : Observe([path], CurrentProtection(), GlobalWriteKind.Replace),
            () =>
            {
                var resource = ResourceLoader.Load<T>(path, typeHint, mode);
                // Replace can refresh an existing object in place. A prior byte verification
                // cannot attest the new contents loaded into that same object.
                if (resource is Resource replaced) OriginalResources.Remove(replaced);
                return resource;
            }, static resource => resource != null, ModLog.Warn,
            restoring);

    internal static GlobalWriteEntry[] Snapshot()
    {
        lock (Sync) return Ledger.ToArray();
    }

    internal static void ReportStartupOnce()
    {
        if (Interlocked.Exchange(ref _summarized, 1) != 0) return;
        try
        {
            string summary;
            lock (Sync)
                summary = $"[全局写入] 启动共 {_total} 次，失败 {_failed}，原版恢复 {_originalRestored}，" +
                          $"其他恢复 {_restored}，已拦截 {_blocked}，已剔除 {_stripped} 条，" +
                          $"违规路径 {_violations}，审计不完整 {_incomplete}，台账保留 {Ledger.Count} 条";
            ModLog.Info(summary);
        }
        catch (Exception) { /* Diagnostics cannot interrupt the main menu. */ }
    }

    private static GlobalWriteProtection? CurrentProtection()
    {
        try { return SkinService.Catalog?.GlobalWriteProtection; }
        catch (Exception) { return null; } // Path shapes alone cannot identify game-owned resources.
    }

    private static bool WritePath(GlobalWriteSite site, string caller, GlobalWriteKind kind, string path,
        Action write, bool restoring = false, Resource? resource = null) =>
        Write(site, caller, kind, () => Observe([path], CurrentProtection(), kind, isOriginal: (_, canonical) =>
            resource is not null && OriginalResources.TryGetValue(resource, out var origin) &&
            origin.Path.Equals(canonical, StringComparison.OrdinalIgnoreCase)), () =>
        {
            write();
            return true;
        }, static success => success, ModLog.Warn, restoring);

    private static T Write<T>(
        GlobalWriteSite site, string caller, GlobalWriteKind kind, Func<Observation> inspect,
        Func<T> write, Func<T, bool> succeeded, Action<string> warn, bool restoring = false, bool throwOnBlock = false,
        string? projectedPack = null)
    {
        var observation = Observation.Unknown;
        try
        {
            observation = inspect();
            foreach (var (path, rule) in observation.ForbiddenPaths)
            {
                try { warn($"[全局写入] {(observation.Blocked ? "已拦截" : "违规")} {rule} {path} ← {site}/{caller}"); }
                catch (Exception) { /* Keep the engine call independent of the logger. */ }
            }
            if (observation.ForbiddenCount > observation.ForbiddenPaths.Length)
                warn($"[全局写入] {site}/{caller} 另有 {observation.ForbiddenCount - observation.ForbiddenPaths.Length} 条违规路径未逐条输出");
        }
        catch (Exception) { /* The engine still makes the same attempt if the audit cannot read a path. */ }

        var result = GlobalWriteResult.Failed;
        try
        {
            if (projectedPack != null && observation.ReadError is { } reason)
            {
                result = GlobalWriteResult.Blocked;
                var failure = new GlobalWriteBlockedException(site, projectedPack, ForbiddenGlobalWrite.None, reason);
                try { warn(failure.Message); } catch (Exception) { }
                throw failure;
            }
            if (observation.Blocked)
            {
                result = GlobalWriteResult.Blocked;
                if (throwOnBlock && observation.FirstBlock is { } block)
                    throw new GlobalWriteBlockedException(site, block.Path, block.Rule);
                return default!;
            }
            var value = write();
            if (succeeded(value))
                result = observation.OriginalCount != 0 ? GlobalWriteResult.OriginalRestored :
                    restoring ? GlobalWriteResult.Restored : GlobalWriteResult.Succeeded;
            return value;
        }
        finally
        {
            try
            {
                lock (Sync)
                {
                    _total++;
                    if (result == GlobalWriteResult.Failed) _failed++;
                    if (result == GlobalWriteResult.Restored) _restored++;
                    if (result == GlobalWriteResult.OriginalRestored) _originalRestored++;
                    if (result == GlobalWriteResult.Blocked) _blocked++;
                    _violations += observation.ForbiddenCount;
                    if (observation.Incomplete) _incomplete++;
                    if (Ledger.Count == LedgerLimit) Ledger.Dequeue();
                    Ledger.Enqueue(new GlobalWriteEntry(site, caller, kind, result, observation.Paths,
                        observation.Count, observation.OriginalPaths, observation.OriginalCount,
                        observation.ForbiddenCount, observation.Incomplete));
                }
            }
            catch (Exception) { /* Preserve the exact result or exception from the engine call. */ }
        }
    }

    private static Observation TryObservePack(string path, bool replaceFiles, Func<GlobalWriteProtection?> protection)
    {
        try { return ObservePack(path, replaceFiles, protection()); }
        catch (Exception exception) { return Observation.Unknown with { ReadError = exception.Message }; }
    }

    private static Observation ObservePack(string path, bool replaceFiles, GlobalWriteProtection? protection)
    {
        var file = new FileInfo(path);
        var key = new PackKey(file.FullName, file.Length, file.LastWriteTimeUtc.Ticks, replaceFiles);
        lock (Sync)
        {
            if (!ReferenceEquals(protection, _cachedProtection))
            {
                PackObservations.Clear();
                _cachedProtection = protection;
            }
            if (PackObservations.TryGetValue(key, out var cached)) return cached;
        }

        using var archive = PckArchive.OpenForAudit(path);
        var removals = archive.RemovedPaths.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var observation = Observe(archive.Paths.Concat(removals).Distinct(StringComparer.OrdinalIgnoreCase), protection,
            replaceFiles ? GlobalWriteKind.PackReplace : GlobalWriteKind.PackFill,
            isOriginal: (source, canonical) => protection?.MatchesGameEntry(archive, source, canonical) == true,
            removals: removals);

        lock (Sync)
        {
            if (ReferenceEquals(protection, _cachedProtection) && !PackObservations.ContainsKey(key))
            {
                if (PackObservations.Count == PackCacheLimit)
                    PackObservations.Remove(PackObservations.Keys.First());
                PackObservations.Add(key, observation);
            }
        }
        return observation;
    }

    private static Observation ObserveDependencies(string root, GlobalWriteProtection? protection)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { root };
        var queue = new Queue<string>();
        queue.Enqueue(root);
        var incomplete = false;
        while (queue.TryDequeue(out var path))
        {
            string[] dependencies;
            try { dependencies = ResourceLoader.GetDependencies(path); }
            catch (Exception)
            {
                incomplete = true;
                continue;
            }
            foreach (var dependency in dependencies)
            {
                var parts = dependency.Split("::", StringSplitOptions.None);
                var next = parts[^1];
                if (parts.Length == 3 && ResourceUid.TextToId(parts[0]) is var id && ResourceUid.HasId(id))
                    next = ResourceUid.GetIdPath(id);

                if (!next.StartsWith("res://", StringComparison.OrdinalIgnoreCase) || seen.Contains(next)) continue;
                if (seen.Count == DependencyLimit)
                {
                    return Observe(seen, protection, GlobalWriteKind.ReplaceDeep, incomplete: true);
                }
                seen.Add(next);
                queue.Enqueue(next);
            }
        }
        return Observe(seen, protection, GlobalWriteKind.ReplaceDeep, incomplete);
    }

    private static Observation Observe(IEnumerable<string> paths, GlobalWriteProtection? protection,
        GlobalWriteKind kind, bool incomplete = false, Func<string, string, bool>? isOriginal = null,
        IReadOnlySet<string>? removals = null)
    {
        var canonical = new List<string>();
        var originals = new List<string>();
        var forbidden = new List<(string Path, ForbiddenGlobalWrite Rule)>();
        var count = 0;
        var originalCount = 0;
        var forbiddenCount = 0;
        (string Path, ForbiddenGlobalWrite Rule)? firstBlock = null;
        foreach (var rawPath in paths)
        {
            if (string.IsNullOrEmpty(rawPath)) continue;
            var path = rawPath.Replace('\\', '/');
            if (path.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
                path = "res://" + path[6..].TrimStart('/');
            else if (!path.Contains("://", StringComparison.Ordinal))
                path = "res://" + path.TrimStart('/');
            var rule = Decide(path, fromGame: false, protection);
            if (rule == ForbiddenGlobalWrite.None && protection?.IsCanonical(path) != true) continue;

            count++;
            if (canonical.Count < PathSampleLimit) canonical.Add(path);
            // ReplaceDeep only reloads the VFS content already visible to Godot (D5).
            if (rule == ForbiddenGlobalWrite.None || kind == GlobalWriteKind.ReplaceDeep) continue;

            var removal = removals?.Contains(rawPath) == true;
            if (!removal && kind == GlobalWriteKind.PackFill && protection?.HasExactGameEntry(path) == true) continue;
            var fromGame = false;
            if (!removal && isOriginal != null)
            {
                try { fromGame = isOriginal(rawPath, path); }
                catch (Exception) { incomplete = true; }
            }
            if (fromGame)
            {
                originalCount++;
                if (originals.Count < PathSampleLimit) originals.Add(path);
                rule = Decide(path, fromGame: true, protection);
            }
            if (rule == ForbiddenGlobalWrite.None) continue;

            // Godot applies explicit PCK deletions separately from ordinary low-priority fills.
            if (removal || kind is GlobalWriteKind.PackReplace or GlobalWriteKind.TakeOver or GlobalWriteKind.Preload)
                firstBlock ??= (path, rule);
            forbiddenCount++;
            if (forbidden.Count < PathSampleLimit) forbidden.Add((path, rule));
        }
        return new Observation(canonical.ToArray(), count, originals.ToArray(), originalCount,
            forbidden.ToArray(), forbiddenCount, firstBlock,
            incomplete || protection == null || forbiddenCount > PathSampleLimit);
    }

    private static ForbiddenGlobalWrite Decide(string path, bool fromGame, GlobalWriteProtection? protection) =>
        protection?.Decide(path, fromGame) ?? ForbiddenGlobalWrite.None;

    private sealed record OriginalResource(string Path);

    private readonly record struct PackKey(string FullPath, long Length, long LastWriteTicks, bool ReplaceFiles);

    private sealed record Observation(string[] Paths, int Count, string[] OriginalPaths, int OriginalCount,
        (string Path, ForbiddenGlobalWrite Rule)[] ForbiddenPaths, int ForbiddenCount,
        (string Path, ForbiddenGlobalWrite Rule)? FirstBlock, bool Incomplete, string? ReadError = null)
    {
        internal bool Blocked => FirstBlock.HasValue;
        internal static readonly Observation Unknown = new([], 0, [], 0, [], 0, null, true);
    }
}

[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
internal static class GlobalWriteSummaryPatch
{
    private static void Postfix() => GlobalResourceWrites.ReportStartupOnce();
}
