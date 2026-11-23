using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Core;

internal enum VanillaIntegritySeverity
{
    Suspect,
    Confirmed
}

internal sealed record VanillaIntegrityEntry(
    string Path,
    ulong Size,
    string? RedirectTarget = null,
    byte[]? Md5 = null,
    Func<byte[]?>? ReadBytes = null);

internal sealed record VanillaIntegrityModPack(
    string ModId,
    string DisplayName,
    bool IsManagedBySkinChanger,
    IReadOnlyDictionary<string, VanillaIntegrityEntry> Entries);

internal sealed record VanillaIntegrityFinding(
    VanillaIntegritySeverity Severity,
    string Path,
    string ModName,
    string ModId);

internal sealed record VanillaIntegrityReport(
    int CheckedCount,
    IReadOnlyList<VanillaIntegrityFinding> Findings,
    long ElapsedMs,
    long MainThreadMs = 0,
    int MainThreadFrames = 0)
{
    internal int SuspectCount => Findings.Count(item => item.Severity == VanillaIntegritySeverity.Suspect);

    internal int ConfirmedCount => Findings.Count(item => item.Severity == VanillaIntegritySeverity.Confirmed);
}

internal sealed record VanillaIntegrityRawMod(
    string ModId,
    string DisplayName,
    string ModPath,
    bool IsLoaded,
    bool HasPck,
    bool IsManagedBySkinChanger);

internal sealed record VanillaIntegrityModSource(
    string ModId,
    string DisplayName,
    string PckPath,
    bool IsManagedBySkinChanger);

internal sealed record VanillaIntegrityBudget(
    long MaxConfirmFileBytes = VanillaIntegritySelfCheck.DefaultMaxConfirmFileBytes,
    long MaxTotalConfirmBytes = VanillaIntegritySelfCheck.DefaultMaxTotalConfirmBytes,
    long PerFrameByteBudget = VanillaIntegritySelfCheck.DefaultPerFrameByteBudget,
    int ChunkBytes = VanillaIntegritySelfCheck.DefaultChunkBytes,
    double PerFrameTimeBudgetMs = VanillaIntegritySelfCheck.DefaultPerFrameTimeBudgetMs);

internal interface IVanillaIntegrityVfsHandle : IDisposable
{
    long Length { get; }

    byte[]? ReadChunk(int maxBytes);
}

internal interface IVanillaIntegrityVfsReader
{
    IVanillaIntegrityVfsHandle? OpenRead(string resourcePath, long offset);
}

internal static partial class VanillaIntegritySelfCheck
{
    private const string LogPrefix = "[完整性自检]";
    private const string LegacyAncientCardTemplateRoot =
        "res://images/atlases/compressed.sprites/card_template/";
    private const string AncientCardTemplateRoot =
        "res://images/atlases/compressed_atlas.sprites/";
    private const string AncientCompressedUiRoot =
        "res://images/compressed_ui/";

    internal const long DefaultMaxConfirmFileBytes = 32L * 1024 * 1024;
    internal const long DefaultMaxTotalConfirmBytes = 64L * 1024 * 1024;
    internal const long DefaultPerFrameByteBudget = 2L * 1024 * 1024;
    internal const int DefaultChunkBytes = 256 * 1024;
    internal const double DefaultPerFrameTimeBudgetMs = 2.5;

    internal static readonly VanillaIntegrityBudget DefaultBudget = new();

    private static readonly string[] AncientTemplateCandidatePaths =
    [
        LegacyAncientCardTemplateRoot + "ancient_card_border.tres",
        LegacyAncientCardTemplateRoot + "ancient_card_text_bg_attack.tres",
        LegacyAncientCardTemplateRoot + "ancient_card_text_bg_skill.tres",
        LegacyAncientCardTemplateRoot + "ancient_card_text_bg_power.tres",
        AncientCardTemplateRoot + "ancient_card_border.tres",
        AncientCardTemplateRoot + "ancient_text_bg_attack.tres",
        AncientCardTemplateRoot + "ancient_text_bg_skill.tres",
        AncientCardTemplateRoot + "ancient_text_bg_power.tres",
        AncientCompressedUiRoot + "ancient_card_border.png",
        AncientCompressedUiRoot + "ancient_text_bg_attack.png",
        AncientCompressedUiRoot + "ancient_text_bg_skill.png",
        AncientCompressedUiRoot + "ancient_text_bg_power.png"
    ];

    private static readonly HashSet<string> AncientTemplateSet =
        new(AncientTemplateCandidatePaths, StringComparer.OrdinalIgnoreCase);

    private static readonly FieldInfo? ManagedModInstancesField =
        AccessTools.Field(typeof(ManagedSkinModLoader), "ManagedModInstances");

    private static readonly FieldInfo? SkinServiceSyncField =
        AccessTools.Field(typeof(SkinService), "Sync");

    private static readonly FieldInfo? SkinServiceMountedOverlayOwnersField =
        AccessTools.Field(typeof(SkinService), "MountedOverlayOwners");

    private static readonly FieldInfo? SkinServiceRuntimeCanonicalDependencyPathsField =
        AccessTools.Field(typeof(SkinService), "RuntimeCanonicalDependencyPaths");

    private static int _scheduled;

    internal static bool AreSelfOverlayHooksBound() =>
        SkinServiceSyncField != null &&
        SkinServiceMountedOverlayOwnersField != null &&
        SkinServiceRuntimeCanonicalDependencyPathsField != null;

    internal static void ScheduleOnce()
    {
        if (Interlocked.CompareExchange(ref _scheduled, 1, 0) != 0)
        {
            return;
        }

        try
        {
            // [延迟调度] 原触发时机：主菜单首次 _Ready 触发 ScheduleOnce()；最早可执行帧：本帧末 idle；
            // 顺序依赖：需在主线程抓取 ModManager.Mods 与内存覆盖快照；能否合并：一次性（CAS 防重）；
            // 重试上限：1 次；取消条件：已执行或异常跳过。
            DeferredDispatch.CallDeferred("VanillaIntegritySelfCheck.Begin", "global", BeginOnMainThread);
        }
        catch (Exception exception)
        {
            ModLog.Warn($"{LogPrefix} 调度自检失败（已跳过）：{exception.GetBaseException().Message}");
        }
    }

    internal static bool IsSampledRootPath(string path)
    {
        var normalized = NormalizeResourcePath(path);
        return SkinCatalog.IsSharedGameAtlasPath(normalized) ||
               SkinCatalog.IsCanonicalCharacterSelectPortrait(normalized) ||
               IsAncientTemplatePath(normalized);
    }

    internal static bool IsAncientTemplatePath(string path)
    {
        var stripped = StripRedirectSuffix(NormalizeResourcePath(path));
        return AncientTemplateSet.Contains(stripped);
    }

    internal static IReadOnlyList<string> SelectSampledPaths(
        IReadOnlyDictionary<string, VanillaIntegrityEntry> gameEntries)
    {
        var normalizedMap = BuildNormalizedEntryMap(gameEntries);
        var selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<(string Path, int Depth)>();

        foreach (var path in normalizedMap.Keys)
        {
            if (IsSampledRootPath(path) && selected.Add(path))
            {
                queue.Enqueue((path, 0));
            }
        }

        // Expand up to 2 hops: e.g. ancient_card_border.tres -> compressed_atlas.png(.import) -> .godot/imported/*.ctex.
        while (queue.Count > 0)
        {
            var (currentPath, depth) = queue.Dequeue();
            if (depth >= 2 || !normalizedMap.TryGetValue(currentPath, out var entry))
            {
                continue;
            }

            foreach (var dependency in ExtractDependencyPaths(entry))
            {
                foreach (var candidate in ExpandCandidateLookupPaths(dependency))
                {
                    if (normalizedMap.ContainsKey(candidate) && selected.Add(candidate))
                    {
                        queue.Enqueue((candidate, depth + 1));
                    }
                }
            }
        }

        return selected.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    internal static IReadOnlyList<VanillaIntegrityModSource> ResolveNativeModSources(
        IReadOnlyList<VanillaIntegrityRawMod> rawMods,
        Func<string, bool>? fileExists = null)
    {
        var exists = fileExists ?? File.Exists;
        var sources = new List<VanillaIntegrityModSource>();

        foreach (var raw in rawMods)
        {
            // Match the game's native ModManager.TryLoadMod rules strictly:
            // loaded state + manifest.has_pck == true + exact <id>.pck (no legacy fallback).
            if (!raw.IsLoaded || !raw.HasPck || raw.IsManagedBySkinChanger)
            {
                continue;
            }

            var modId = raw.ModId;
            if (!IsSafeModFileStem(modId) || Entry.IsSelfModId(modId) || string.IsNullOrWhiteSpace(raw.ModPath))
            {
                continue;
            }

            var pckPath = Path.Combine(raw.ModPath, modId + ".pck");
            if (!exists(pckPath))
            {
                continue;
            }

            var displayName = !string.IsNullOrWhiteSpace(raw.DisplayName) ? raw.DisplayName : modId;
            sources.Add(new VanillaIntegrityModSource(modId, displayName, pckPath, IsManagedBySkinChanger: false));
        }

        return sources;
    }

    internal static VanillaIntegrityReport Evaluate(
        IReadOnlyDictionary<string, VanillaIntegrityEntry> gameEntries,
        IReadOnlyList<VanillaIntegrityModPack> modPacks,
        Func<string, byte[]?>? readLiveBytes = null,
        long elapsedMs = 0,
        IReadOnlySet<string>? selfOverlayPaths = null,
        long mainThreadMs = 0,
        int mainThreadFrames = 0)
    {
        var prepared = PrepareInMemoryScan(gameEntries, modPacks);
        var normalizedSelfOverlays = NormalizePathSet(selfOverlayPaths);
        var reader = readLiveBytes == null ? null : new DelegateVfsReader(readLiveBytes);
        using var session = new ConfirmSession(
            prepared,
            reader,
            DefaultBudget,
            normalizedSelfOverlays,
            selfOverlayIncomplete: false);

        while (!session.StepFrame(isOwnedBySelfOverlay: null, elapsedMsProvider: () => 0.0))
        {
        }

        return new VanillaIntegrityReport(
            prepared.CheckedCount,
            session.Findings,
            Math.Max(0, elapsedMs),
            Math.Max(0, mainThreadMs),
            Math.Max(0, mainThreadFrames));
    }

    internal static VanillaIntegrityReport ScanArchives(
        string gamePckPath,
        IReadOnlyList<VanillaIntegrityModSource> modSources,
        Func<string, byte[]?>? readLiveBytes = null,
        IReadOnlySet<string>? selfOverlayPaths = null,
        VanillaIntegrityBudget? budget = null)
    {
        var reader = readLiveBytes == null ? null : new DelegateVfsReader(readLiveBytes);
        return ScanArchives(gamePckPath, modSources, reader, selfOverlayPaths, budget);
    }

    internal static VanillaIntegrityReport ScanArchives(
        string gamePckPath,
        IReadOnlyList<VanillaIntegrityModSource> modSources,
        IVanillaIntegrityVfsReader? vfsReader,
        IReadOnlySet<string>? selfOverlayPaths = null,
        VanillaIntegrityBudget? budget = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var prepared = PrepareScanFromDisk(gamePckPath, modSources);
        var normalizedSelfOverlays = NormalizePathSet(selfOverlayPaths);
        using var session = new ConfirmSession(
            prepared,
            vfsReader,
            budget ?? DefaultBudget,
            normalizedSelfOverlays,
            selfOverlayIncomplete: false);

        long confirmTicks = 0;
        while (true)
        {
            var frameWatch = Stopwatch.StartNew();
            var done = session.StepFrame(
                isOwnedBySelfOverlay: null,
                elapsedMsProvider: () => frameWatch.Elapsed.TotalMilliseconds);
            frameWatch.Stop();
            confirmTicks += frameWatch.ElapsedTicks;
            if (done)
            {
                break;
            }
        }

        stopwatch.Stop();
        var mainThreadMs = TicksToRoundedMs(confirmTicks);
        return new VanillaIntegrityReport(
            prepared.CheckedCount,
            session.Findings,
            stopwatch.ElapsedMilliseconds,
            mainThreadMs,
            session.FramesUsed);
    }

    internal static IReadOnlyList<string> FormatLogLines(VanillaIntegrityReport report)
    {
        var lines = new List<string>(report.Findings.Count + 1);
        foreach (var finding in report.Findings)
        {
            var tag = finding.Severity == VanillaIntegritySeverity.Confirmed ? "确认" : "嫌疑";
            lines.Add($"{LogPrefix} {tag} {finding.Path} ← {finding.ModName}({finding.ModId})");
        }

        lines.Add(
            $"{LogPrefix} 检查 {report.CheckedCount} 项，嫌疑 {report.SuspectCount}，确认 {report.ConfirmedCount}，耗时 {report.ElapsedMs} ms，主线程 {report.MainThreadMs} ms（{report.MainThreadFrames} 帧）");
        return lines;
    }

    internal static ConfirmSession CreateConfirmSession(
        IReadOnlyDictionary<string, VanillaIntegrityEntry> gameEntries,
        IReadOnlyList<VanillaIntegrityModPack> modPacks,
        IVanillaIntegrityVfsReader? vfsReader,
        VanillaIntegrityBudget? budget = null,
        IReadOnlySet<string>? selfOverlayPaths = null,
        bool selfOverlayIncomplete = false)
    {
        var prepared = PrepareInMemoryScan(gameEntries, modPacks);
        return new ConfirmSession(
            prepared,
            vfsReader,
            budget ?? DefaultBudget,
            NormalizePathSet(selfOverlayPaths),
            selfOverlayIncomplete);
    }

    private static void BeginOnMainThread()
    {
        try
        {
            var totalStopwatch = Stopwatch.StartNew();
            var frameStopwatch = Stopwatch.StartNew();

            // Pure in-memory snapshot only: zero File.Exists / Directory enumeration / PCK I/O on main thread.
            var executablePath = OS.GetExecutablePath();
            var commandLineArgs = GameCommandLine.Arguments().ToArray();
            var rawMods = SnapshotRawModsInMemory();
            var (selfOverlayPaths, selfOverlayComplete) = SnapshotSelfOverlayPathsInMemory();

            frameStopwatch.Stop();
            var snapshotTicks = frameStopwatch.ElapsedTicks;
            const int snapshotFrames = 1;

            _ = Task.Run(() =>
            {
                try
                {
                    var gamePckPath = GamePackLocator.Resolve(
                        executablePath,
                        commandLineArgs,
                        message => ModLog.Warn($"{LogPrefix} 定位游戏主资源包：{message}"));
                    var modSources = ResolveNativeModSources(rawMods);
                    var prepared = PrepareScanFromDisk(gamePckPath, modSources);

                    var runtimeState = new RuntimeConfirmRunner(
                        prepared,
                        totalStopwatch,
                        snapshotTicks,
                        snapshotFrames,
                        selfOverlayPaths,
                        selfOverlayComplete);
                    // [延迟调度] 原触发时机：后台 Task.Run 完成磁盘 PCK 预扫后从工作线程切回主线程；最早可执行帧：切回后下一 idle 帧；
                    // 顺序依赖：线程安全入队，必须在主线程访问 Godot VFS；能否合并：单次交接不可合并；
                    // 重试上限：由下游 ConfirmSession 按候选条数有界分帧收敛；取消条件：全部条目核验完毕或异常释放。
                    DeferredDispatch.CallDeferred("VanillaIntegritySelfCheck.Step", runtimeState, runtimeState.StepOnMainThread);
                }
                catch (Exception exception)
                {
                    ModLog.Warn($"{LogPrefix} 后台扫描失败（已跳过）：{exception.GetBaseException().Message}");
                }
            });
        }
        catch (Exception exception)
        {
            ModLog.Warn($"{LogPrefix} 启动自检失败（已跳过）：{exception.GetBaseException().Message}");
        }
    }

    private static VanillaIntegrityRawMod[] SnapshotRawModsInMemory()
    {
        var managedInstances = ManagedModInstancesField?.GetValue(null) as HashSet<Mod>;
        var providerRoots = ManagedSkinModLoader.ProviderRoots;
        var partTakeoverRoots = ManagedSkinModLoader.PartTakeoverRoots;
        var preloadedSet = new HashSet<Mod>(ManagedSkinModLoader.SkinProvidersBeforeSelf, ReferenceEqualityComparer.Instance);

        var snapshots = new List<VanillaIntegrityRawMod>();
        foreach (var mod in ModManager.Mods)
        {
            var modId = mod.manifest?.id ?? string.Empty;
            var displayName = !string.IsNullOrWhiteSpace(mod.manifest?.name)
                ? mod.manifest!.name!
                : modId;
            var isLoaded = mod.state == ModLoadState.Loaded;
            var hasPck = mod.manifest?.hasPck == true;
            var isManaged = isLoaded && hasPck && !string.IsNullOrWhiteSpace(modId) && !Entry.IsSelfModId(modId)
                ? IsModIsolatedBySkinChanger(
                    mod,
                    managedInstances,
                    providerRoots,
                    partTakeoverRoots,
                    preloadedSet)
                : false;

            snapshots.Add(new VanillaIntegrityRawMod(
                modId,
                displayName,
                mod.path ?? string.Empty,
                isLoaded,
                hasPck,
                isManaged));
        }

        return snapshots.ToArray();
    }

    private static (HashSet<string> Paths, bool Complete) SnapshotSelfOverlayPathsInMemory()
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!AreSelfOverlayHooksBound())
        {
            return (paths, Complete: false);
        }

        var sync = SkinServiceSyncField!.GetValue(null);
        var lockTaken = false;
        try
        {
            if (sync != null)
            {
                Monitor.TryEnter(sync, millisecondsTimeout: 2, ref lockTaken);
                if (!lockTaken)
                {
                    return (paths, Complete: false);
                }
            }

            if (SkinServiceMountedOverlayOwnersField!.GetValue(null) is IDictionary mountedOwners)
            {
                foreach (var key in mountedOwners.Keys)
                {
                    if (key is string resourcePath && !string.IsNullOrWhiteSpace(resourcePath))
                    {
                        paths.Add(NormalizeResourcePath(resourcePath));
                    }
                }
            }
            else
            {
                return (paths, Complete: false);
            }

            if (SkinServiceRuntimeCanonicalDependencyPathsField!.GetValue(null) is IDictionary runtimeDeps)
            {
                foreach (var value in runtimeDeps.Values)
                {
                    if (value is IEnumerable<string> groupPaths)
                    {
                        foreach (var resourcePath in groupPaths)
                        {
                            if (!string.IsNullOrWhiteSpace(resourcePath))
                            {
                                paths.Add(NormalizeResourcePath(resourcePath));
                            }
                        }
                    }
                }
            }
            else
            {
                return (paths, Complete: false);
            }

            return (paths, Complete: true);
        }
        catch
        {
            return (paths, Complete: false);
        }
        finally
        {
            if (lockTaken && sync != null)
            {
                Monitor.Exit(sync);
            }
        }
    }

    private static (List<string> MatchedPaths, bool Complete) MatchSelfOverlayPathsInMemory(
        IReadOnlyList<PreparedSuspectGroup> suspectGroups)
    {
        var matched = new List<string>();
        if (!AreSelfOverlayHooksBound())
        {
            return (matched, Complete: false);
        }

        var sync = SkinServiceSyncField!.GetValue(null);
        var lockTaken = false;
        try
        {
            if (sync != null)
            {
                Monitor.TryEnter(sync, millisecondsTimeout: 2, ref lockTaken);
                if (!lockTaken)
                {
                    return (matched, Complete: false);
                }
            }

            if (SkinServiceMountedOverlayOwnersField!.GetValue(null) is not IDictionary mountedOwners ||
                SkinServiceRuntimeCanonicalDependencyPathsField!.GetValue(null) is not IDictionary runtimeDeps)
            {
                return (matched, Complete: false);
            }

            foreach (var group in suspectGroups)
            {
                if (mountedOwners.Contains(group.Path))
                {
                    matched.Add(group.Path);
                    continue;
                }

                foreach (var value in runtimeDeps.Values)
                {
                    if (value is ISet<string> groupPaths && groupPaths.Contains(group.Path))
                    {
                        matched.Add(group.Path);
                        break;
                    }
                }
            }

            return (matched, Complete: true);
        }
        catch
        {
            return (matched, Complete: false);
        }
        finally
        {
            if (lockTaken && sync != null)
            {
                Monitor.Exit(sync);
            }
        }
    }

    internal static bool IsModIsolatedBySkinChanger(
        Mod mod,
        HashSet<Mod>? managedInstances,
        IReadOnlyCollection<string> providerRoots,
        IReadOnlyCollection<string> partTakeoverRoots,
        HashSet<Mod> preloadedBeforeSelf)
    {
        try
        {
            if (SkinWorkshopService.IsHotRegisteredResourceMod(mod))
            {
                return true;
            }
        }
        catch
        {
            // Ignore workshop service state errors during early/offline checks.
        }

        if (managedInstances != null)
        {
            return managedInstances.Contains(mod);
        }

        if (preloadedBeforeSelf.Contains(mod))
        {
            return false;
        }

        var normalizedRoot = NormalizeFileSystemPath(mod.path);
        if (partTakeoverRoots.Contains(normalizedRoot))
        {
            return false;
        }

        return providerRoots.Contains(normalizedRoot) || ManagedSkinModLoader.IsManagedProviderForDisplay(mod);
    }

    private static PreparedDiskScan PrepareInMemoryScan(
        IReadOnlyDictionary<string, VanillaIntegrityEntry> gameEntries,
        IReadOnlyList<VanillaIntegrityModPack> modPacks)
    {
        var normalizedGame = BuildNormalizedEntryMap(gameEntries);
        var sampledPaths = SelectSampledPaths(normalizedGame);
        var normalizedPacks = modPacks
            .Where(pack => !pack.IsManagedBySkinChanger)
            .Select(pack => (Pack: pack, Entries: BuildNormalizedEntryMap(pack.Entries)))
            .ToArray();

        var groups = new List<PreparedSuspectGroup>();
        foreach (var path in sampledPaths)
        {
            var gameEntry = normalizedGame[path];
            var gameRedirect = ResolveRedirectSignature(gameEntry);
            var cheapCandidates = new List<(VanillaIntegrityModPack Pack, VanillaIntegrityEntry Entry)>();

            foreach (var (pack, entries) in normalizedPacks)
            {
                if (!entries.TryGetValue(path, out var modEntry))
                {
                    continue;
                }

                if (IsCheaplyIdentical(gameEntry, gameRedirect, modEntry))
                {
                    continue;
                }

                cheapCandidates.Add((pack, modEntry));
            }

            if (cheapCandidates.Count == 0)
            {
                continue;
            }

            var gameHash = ComputeEntrySha256(gameEntry);
            var candidates = new List<PreparedCandidate>(cheapCandidates.Count);
            foreach (var (pack, modEntry) in cheapCandidates)
            {
                var modHash = ComputeEntrySha256(modEntry);
                if (gameHash != null && modHash != null && gameHash.SequenceEqual(modHash))
                {
                    continue;
                }

                candidates.Add(new PreparedCandidate(pack.DisplayName, pack.ModId, modHash));
            }

            if (candidates.Count > 0)
            {
                groups.Add(new PreparedSuspectGroup(path, gameHash, candidates));
            }
        }

        return new PreparedDiskScan(sampledPaths.Count, groups);
    }

    private static PreparedDiskScan PrepareScanFromDisk(
        string gamePckPath,
        IReadOnlyList<VanillaIntegrityModSource> modSources)
    {
        using var gameArchive = PckArchive.Open(gamePckPath);
        var gameEntries = BuildGameSampledArchiveEntries(gameArchive);
        var sampledPaths = SelectSampledPaths(gameEntries);

        var activeSources = modSources
            .Where(source => !source.IsManagedBySkinChanger && !string.IsNullOrWhiteSpace(source.PckPath))
            .ToArray();

        var suspectGroups = new Dictionary<string, List<PreparedCandidate>>(StringComparer.OrdinalIgnoreCase);
        var vanillaHashes = new Dictionary<string, byte[]?>(StringComparer.OrdinalIgnoreCase);

        foreach (var source in activeSources)
        {
            if (!File.Exists(source.PckPath))
            {
                continue;
            }

            try
            {
                using var modArchive = PckArchive.Open(source.PckPath);
                foreach (var path in sampledPaths)
                {
                    if (!modArchive.Contains(path))
                    {
                        continue;
                    }

                    var gameEntry = gameEntries[path];
                    var gameRedirect = ResolveRedirectSignature(gameEntry);
                    var modEntry = BuildArchiveEntry(modArchive, path, readRedirectText: IsTextRedirectFile(path));
                    if (IsCheaplyIdentical(gameEntry, gameRedirect, modEntry))
                    {
                        continue;
                    }

                    if (!vanillaHashes.TryGetValue(path, out var gameHash))
                    {
                        gameHash = ComputeEntrySha256(gameEntry);
                        vanillaHashes[path] = gameHash;
                    }

                    var modHash = ComputeEntrySha256(modEntry);
                    if (gameHash != null && modHash != null && gameHash.SequenceEqual(modHash))
                    {
                        continue;
                    }

                    if (!suspectGroups.TryGetValue(path, out var list))
                    {
                        list = [];
                        suspectGroups[path] = list;
                    }

                    list.Add(new PreparedCandidate(source.DisplayName, source.ModId, modHash));
                }
            }
            catch (Exception exception)
            {
                ModLog.Warn($"{LogPrefix} 读取 {source.DisplayName}({source.ModId}) PCK 失败，已跳过：{exception.GetBaseException().Message}");
            }
        }

        var orderedGroups = sampledPaths
            .Where(suspectGroups.ContainsKey)
            .Select(path => new PreparedSuspectGroup(path, vanillaHashes.GetValueOrDefault(path), suspectGroups[path]))
            .ToArray();
        return new PreparedDiskScan(sampledPaths.Count, orderedGroups);
    }

    private static Dictionary<string, VanillaIntegrityEntry> BuildGameSampledArchiveEntries(PckArchive gameArchive)
    {
        var entries = new Dictionary<string, VanillaIntegrityEntry>(StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<(string Path, int Depth)>();

        foreach (var rawPath in gameArchive.Paths)
        {
            var normalized = NormalizeResourcePath(rawPath);
            if (!IsSampledRootPath(normalized) || entries.ContainsKey(normalized))
            {
                continue;
            }

            var entry = BuildArchiveEntry(gameArchive, normalized, readRedirectText: IsTextRedirectFile(normalized));
            entries[normalized] = entry;
            queue.Enqueue((normalized, 0));
        }

        while (queue.Count > 0)
        {
            var (currentPath, depth) = queue.Dequeue();
            if (depth >= 2 || !entries.TryGetValue(currentPath, out var entry))
            {
                continue;
            }

            foreach (var dependency in ExtractDependencyPaths(entry))
            {
                foreach (var candidate in ExpandCandidateLookupPaths(dependency))
                {
                    if (entries.ContainsKey(candidate) || !gameArchive.Contains(candidate))
                    {
                        continue;
                    }

                    var depEntry = BuildArchiveEntry(gameArchive, candidate, readRedirectText: IsTextRedirectFile(candidate));
                    entries[candidate] = depEntry;
                    queue.Enqueue((candidate, depth + 1));
                }
            }
        }

        return entries;
    }

    private static VanillaIntegrityEntry BuildArchiveEntry(
        PckArchive archive,
        string normalizedPath,
        bool readRedirectText)
    {
        string? redirect = null;
        if (readRedirectText)
        {
            try
            {
                var text = Encoding.UTF8.GetString(archive.ReadFile(normalizedPath));
                redirect = FormatRedirectSignature(ExtractPathsFromText(text));
            }
            catch
            {
                redirect = null;
            }
        }

        return new VanillaIntegrityEntry(
            normalizedPath,
            archive.GetFileSize(normalizedPath),
            redirect,
            archive.GetFileMd5(normalizedPath),
            () => archive.ReadFile(normalizedPath));
    }

    private static Dictionary<string, VanillaIntegrityEntry> BuildNormalizedEntryMap(
        IReadOnlyDictionary<string, VanillaIntegrityEntry> source)
    {
        var map = new Dictionary<string, VanillaIntegrityEntry>(source.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var pair in source)
        {
            var normalized = NormalizeResourcePath(string.IsNullOrWhiteSpace(pair.Value.Path) ? pair.Key : pair.Value.Path);
            map[normalized] = pair.Value with { Path = normalized };
        }

        return map;
    }

    private static HashSet<string> NormalizePathSet(IReadOnlySet<string>? paths)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (paths == null)
        {
            return result;
        }

        foreach (var path in paths)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                result.Add(NormalizeResourcePath(path));
            }
        }

        return result;
    }

    private static bool IsCheaplyIdentical(
        VanillaIntegrityEntry gameEntry,
        string? gameRedirect,
        VanillaIntegrityEntry modEntry)
    {
        if (modEntry.Size != gameEntry.Size)
        {
            return false;
        }

        var modRedirect = ResolveRedirectSignature(modEntry);
        if (!string.Equals(gameRedirect, modRedirect, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (HasUsableMd5(gameEntry.Md5) && HasUsableMd5(modEntry.Md5))
        {
            return gameEntry.Md5!.SequenceEqual(modEntry.Md5!);
        }

        // Without usable MD5 on both sides, we cannot rule out a same-size modification cheaply.
        return false;
    }

    private static bool HasUsableMd5(byte[]? md5)
    {
        if (md5 == null || md5.Length != 16)
        {
            return false;
        }

        for (var i = 0; i < md5.Length; i++)
        {
            if (md5[i] != 0)
            {
                return true;
            }
        }

        return false;
    }

    private static string? ResolveRedirectSignature(VanillaIntegrityEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.RedirectTarget))
        {
            var targets = ExtractPathsFromText(entry.RedirectTarget);
            return targets.Count > 0
                ? FormatRedirectSignature(targets)
                : NormalizeResourcePath(entry.RedirectTarget);
        }

        if (IsTextRedirectFile(entry.Path) && entry.ReadBytes != null)
        {
            try
            {
                var bytes = entry.ReadBytes();
                if (bytes is { Length: > 0 })
                {
                    return FormatRedirectSignature(ExtractPathsFromText(Encoding.UTF8.GetString(bytes)));
                }
            }
            catch
            {
                // Ignore read failure during cheap redirect extraction.
            }
        }

        return null;
    }

    private static IReadOnlyList<string> ExtractDependencyPaths(VanillaIntegrityEntry entry)
    {
        var results = new List<string>();
        if (!string.IsNullOrWhiteSpace(entry.RedirectTarget))
        {
            var parsed = ExtractPathsFromText(entry.RedirectTarget);
            if (parsed.Count > 0)
            {
                results.AddRange(parsed);
            }
            else if (entry.RedirectTarget.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(NormalizeResourcePath(entry.RedirectTarget));
            }
        }
        else if (IsTextRedirectFile(entry.Path) && entry.ReadBytes != null)
        {
            try
            {
                var bytes = entry.ReadBytes();
                if (bytes is { Length: > 0 })
                {
                    results.AddRange(ExtractPathsFromText(Encoding.UTF8.GetString(bytes)));
                }
            }
            catch
            {
                // Ignore unreadable text dependencies.
            }
        }

        return results;
    }

    private static IEnumerable<string> ExpandCandidateLookupPaths(string dependency)
    {
        var normalized = NormalizeResourcePath(dependency);
        yield return normalized;
        if (!normalized.EndsWith(".import", StringComparison.OrdinalIgnoreCase) &&
            !normalized.EndsWith(".remap", StringComparison.OrdinalIgnoreCase))
        {
            yield return normalized + ".import";
            yield return normalized + ".remap";
        }
    }

    private static IReadOnlyList<string> ExtractPathsFromText(string text)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match match in ResPathRegex().Matches(text))
        {
            var value = NormalizeResourcePath(match.Value);
            set.Add(value);
        }

        return set.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string? FormatRedirectSignature(IReadOnlyList<string> paths) =>
        paths.Count == 0 ? null : string.Join("|", paths);

    private static byte[]? ComputeEntrySha256(VanillaIntegrityEntry entry)
    {
        if (entry.ReadBytes == null)
        {
            return null;
        }

        try
        {
            return ComputeSha256(entry.ReadBytes());
        }
        catch
        {
            return null;
        }
    }

    private static byte[]? ComputeSha256(byte[]? bytes) =>
        bytes == null ? null : SHA256.HashData(bytes);

    private static bool IsTextRedirectFile(string path) =>
        path.EndsWith(".import", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase);

    private static string StripRedirectSuffix(string path)
    {
        if (path.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
        {
            return path[..^7];
        }

        if (path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase))
        {
            return path[..^6];
        }

        return path;
    }

    private static bool IsSafeModFileStem(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value is not ("." or "..") &&
        value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
        value.IndexOfAny(['/', '\\', ':']) < 0;

    internal static string NormalizeResourcePath(string path)
    {
        var normalized = path.Trim().Replace('\\', '/').TrimStart('/');
        if (normalized.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[6..];
        }

        while (normalized.Contains("//", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("//", "/", StringComparison.Ordinal);
        }

        return "res://" + normalized.Trim('/');
    }

    private static string NormalizeFileSystemPath(string path)
    {
        try
        {
            return Path.GetFullPath(path).TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return path.Trim();
        }
    }

    private static long TicksToRoundedMs(long stopwatchTicks)
    {
        if (stopwatchTicks <= 0)
        {
            return 0;
        }

        return Math.Max(0, (long)Math.Round(Stopwatch.GetElapsedTime(0, stopwatchTicks).TotalMilliseconds));
    }

    [GeneratedRegex("res://[^\\s\"'\\]\\),;]+", RegexOptions.IgnoreCase)]
    private static partial Regex ResPathRegex();

    internal sealed class ConfirmSession : IDisposable
    {
        private readonly PreparedDiskScan _prepared;
        private readonly IVanillaIntegrityVfsReader? _reader;
        private readonly VanillaIntegrityBudget _budget;
        private readonly HashSet<string> _selfOverlayPaths;
        private readonly List<VanillaIntegrityFinding> _findings = [];
        private bool _selfOverlayIncomplete;

        private int _groupIndex;
        private long _currentOffset;
        private long _currentExpectedLength;
        private IncrementalHash? _currentHash;
        private long _totalBytesReserved;
        private bool _disposed;

        internal ConfirmSession(
            PreparedDiskScan prepared,
            IVanillaIntegrityVfsReader? reader,
            VanillaIntegrityBudget budget,
            HashSet<string> selfOverlayPaths,
            bool selfOverlayIncomplete)
        {
            _prepared = prepared;
            _reader = reader;
            _budget = new VanillaIntegrityBudget(
                Math.Max(1, budget.MaxConfirmFileBytes),
                Math.Max(1, budget.MaxTotalConfirmBytes),
                Math.Max(1, budget.PerFrameByteBudget),
                Math.Max(1, budget.ChunkBytes),
                Math.Max(0.1, budget.PerFrameTimeBudgetMs));
            _selfOverlayPaths = selfOverlayPaths;
            _selfOverlayIncomplete = selfOverlayIncomplete;
        }

        internal int FramesUsed { get; private set; }

        internal long TotalBytesRead { get; private set; }

        internal IReadOnlyList<VanillaIntegrityFinding> Findings => _findings;

        internal void MergeSelfOverlayPaths(IEnumerable<string> paths, bool snapshotComplete)
        {
            if (!snapshotComplete)
            {
                _selfOverlayIncomplete = true;
            }

            foreach (var path in paths)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    _selfOverlayPaths.Add(NormalizeResourcePath(path));
                }
            }
        }

        internal bool StepFrame(
            Func<string, bool>? isOwnedBySelfOverlay = null,
            Func<double>? elapsedMsProvider = null)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            FramesUsed++;
            if (_groupIndex >= _prepared.SuspectGroups.Count)
            {
                return true;
            }

            var frameWatch = elapsedMsProvider == null ? Stopwatch.StartNew() : null;
            double GetElapsedMs() => elapsedMsProvider != null
                ? elapsedMsProvider()
                : frameWatch!.Elapsed.TotalMilliseconds;

            long frameBytesRead = 0;
            var didWorkThisFrame = false;

            while (_groupIndex < _prepared.SuspectGroups.Count)
            {
                if (didWorkThisFrame &&
                    (frameBytesRead >= _budget.PerFrameByteBudget ||
                     GetElapsedMs() >= _budget.PerFrameTimeBudgetMs))
                {
                    return false;
                }

                var group = _prepared.SuspectGroups[_groupIndex];
                if (_reader == null || IsAmbiguousOrSelfOwned(group.Path, isOwnedBySelfOverlay))
                {
                    ResetCurrentGroupState();
                    AppendSuspectOnly(group);
                    _groupIndex++;
                    didWorkThisFrame = true;
                    continue;
                }

                IVanillaIntegrityVfsHandle? handle;
                try
                {
                    handle = _reader.OpenRead(group.Path, _currentOffset);
                }
                catch
                {
                    handle = null;
                }

                if (handle == null)
                {
                    ResetCurrentGroupState();
                    AppendSuspectOnly(group);
                    _groupIndex++;
                    didWorkThisFrame = true;
                    continue;
                }

                using (handle)
                {
                    var length = handle.Length;
                    if (_currentHash == null)
                    {
                        if (length <= 0 ||
                            length > _budget.MaxConfirmFileBytes ||
                            _totalBytesReserved + length > _budget.MaxTotalConfirmBytes)
                        {
                            AppendSuspectOnly(group);
                            _groupIndex++;
                            didWorkThisFrame = true;
                            continue;
                        }

                        _totalBytesReserved += length;
                        _currentExpectedLength = length;
                        _currentOffset = 0;
                        _currentHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
                    }
                    else if (length != _currentExpectedLength)
                    {
                        // VFS entry changed between frames; conservatively keep Suspect.
                        ResetCurrentGroupState();
                        AppendSuspectOnly(group);
                        _groupIndex++;
                        didWorkThisFrame = true;
                        continue;
                    }

                    var groupFinished = false;
                    while (_currentOffset < _currentExpectedLength)
                    {
                        var remainingInFile = _currentExpectedLength - _currentOffset;
                        var remainingInFrame = _budget.PerFrameByteBudget - frameBytesRead;
                        if (remainingInFrame <= 0)
                        {
                            break;
                        }

                        var toRead = (int)Math.Min(_budget.ChunkBytes, Math.Min(remainingInFile, remainingInFrame));
                        byte[]? chunk;
                        try
                        {
                            chunk = handle.ReadChunk(toRead);
                        }
                        catch
                        {
                            chunk = null;
                        }

                        didWorkThisFrame = true;
                        if (chunk == null || chunk.Length == 0)
                        {
                            ResetCurrentGroupState();
                            AppendSuspectOnly(group);
                            _groupIndex++;
                            groupFinished = true;
                            break;
                        }

                        _currentHash.AppendData(chunk);
                        _currentOffset += chunk.Length;
                        frameBytesRead += chunk.Length;
                        TotalBytesRead += chunk.Length;

                        if (_currentOffset >= _currentExpectedLength)
                        {
                            var liveHash = _currentHash.GetHashAndReset();
                            ResetCurrentGroupState();
                            if (IsAmbiguousOrSelfOwned(group.Path, isOwnedBySelfOverlay))
                            {
                                AppendSuspectOnly(group);
                            }
                            else
                            {
                                AppendResolvedGroup(group, liveHash);
                            }

                            _groupIndex++;
                            groupFinished = true;
                            break;
                        }

                        if (frameBytesRead >= _budget.PerFrameByteBudget ||
                            GetElapsedMs() >= _budget.PerFrameTimeBudgetMs)
                        {
                            break;
                        }
                    }

                    if (!groupFinished)
                    {
                        return false;
                    }
                }
            }

            return _groupIndex >= _prepared.SuspectGroups.Count;
        }

        private bool IsAmbiguousOrSelfOwned(string path, Func<string, bool>? dynamicCheck)
        {
            if (_selfOverlayIncomplete || _selfOverlayPaths.Contains(path))
            {
                return true;
            }

            if (dynamicCheck != null)
            {
                try
                {
                    if (dynamicCheck(path))
                    {
                        _selfOverlayPaths.Add(path);
                        return true;
                    }
                }
                catch
                {
                    _selfOverlayIncomplete = true;
                    return true;
                }
            }

            return false;
        }

        private void AppendSuspectOnly(PreparedSuspectGroup group)
        {
            foreach (var candidate in group.Candidates)
            {
                _findings.Add(new VanillaIntegrityFinding(
                    VanillaIntegritySeverity.Suspect,
                    group.Path,
                    candidate.ModName,
                    candidate.ModId));
            }
        }

        private void AppendResolvedGroup(PreparedSuspectGroup group, byte[] liveHash)
        {
            var liveMatchesVanilla = group.VanillaSha256 != null && liveHash.SequenceEqual(group.VanillaSha256);
            var confirmedIndex = -1;
            if (!liveMatchesVanilla)
            {
                // Later-loaded native packs win in Godot's LoadResourcePack; pick the last matching active pack.
                for (var i = group.Candidates.Count - 1; i >= 0; i--)
                {
                    var candidateHash = group.Candidates[i].Sha256;
                    if (candidateHash != null && liveHash.SequenceEqual(candidateHash))
                    {
                        confirmedIndex = i;
                        break;
                    }
                }
            }

            for (var i = 0; i < group.Candidates.Count; i++)
            {
                var candidate = group.Candidates[i];
                var severity = i == confirmedIndex
                    ? VanillaIntegritySeverity.Confirmed
                    : VanillaIntegritySeverity.Suspect;
                _findings.Add(new VanillaIntegrityFinding(severity, group.Path, candidate.ModName, candidate.ModId));
            }
        }

        private void ResetCurrentGroupState()
        {
            _currentHash?.Dispose();
            _currentHash = null;
            _currentOffset = 0;
            _currentExpectedLength = 0;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            ResetCurrentGroupState();
        }
    }

    private sealed class RuntimeConfirmRunner
    {
        private readonly PreparedDiskScan _prepared;
        private readonly Stopwatch _totalStopwatch;
        private readonly ConfirmSession _session;
        private long _mainThreadTicks;
        private int _mainThreadFrames;

        internal RuntimeConfirmRunner(
            PreparedDiskScan prepared,
            Stopwatch totalStopwatch,
            long initialMainThreadTicks,
            int initialMainThreadFrames,
            HashSet<string> initialSelfOverlayPaths,
            bool initialSelfOverlayComplete)
        {
            _prepared = prepared;
            _totalStopwatch = totalStopwatch;
            _mainThreadTicks = initialMainThreadTicks;
            _mainThreadFrames = initialMainThreadFrames;
            _session = new ConfirmSession(
                prepared,
                GodotVfsReader.Instance,
                DefaultBudget,
                initialSelfOverlayPaths,
                selfOverlayIncomplete: !initialSelfOverlayComplete);
        }

        internal void StepOnMainThread()
        {
            var frameWatch = Stopwatch.StartNew();
            try
            {
                if (_prepared.SuspectGroups.Count > 0)
                {
                    var (matchedSelfOverlays, complete) = MatchSelfOverlayPathsInMemory(_prepared.SuspectGroups);
                    _session.MergeSelfOverlayPaths(matchedSelfOverlays, complete);
                }

                var completed = _session.StepFrame(
                    isOwnedBySelfOverlay: null,
                    elapsedMsProvider: () => frameWatch.Elapsed.TotalMilliseconds);

                frameWatch.Stop();
                _mainThreadTicks += frameWatch.ElapsedTicks;
                _mainThreadFrames++;

                if (!completed)
                {
                    // [延迟调度] 原触发时机：单帧 2.5 ms / 2 MiB 预算用尽且仍有剩余待检项；最早可执行帧：下一帧；
                    // 顺序依赖：主线程单链串行推进同一 ConfirmSession；能否合并：单链串行无需合并；
                    // 重试上限：按待检条目总数有界收敛；取消条件：completed == true 或异常释放 _session。
                    DeferredDispatch.CallDeferred("VanillaIntegritySelfCheck.Step", this, StepOnMainThread);
                    return;
                }

                _totalStopwatch.Stop();
                var report = new VanillaIntegrityReport(
                    _prepared.CheckedCount,
                    _session.Findings,
                    _totalStopwatch.ElapsedMilliseconds,
                    TicksToRoundedMs(_mainThreadTicks),
                    _mainThreadFrames);
                _session.Dispose();

                var lines = FormatLogLines(report);
                for (var i = 0; i < lines.Count; i++)
                {
                    var isSummary = i == lines.Count - 1;
                    if (!isSummary || report.Findings.Count > 0)
                    {
                        ModLog.Warn(lines[i]);
                    }
                    else
                    {
                        ModLog.Info(lines[i]);
                    }
                }
            }
            catch (Exception exception)
            {
                _session.Dispose();
                ModLog.Warn($"{LogPrefix} 输出自检结果失败（已跳过）：{exception.GetBaseException().Message}");
            }
        }
    }

    private sealed class DelegateVfsReader : IVanillaIntegrityVfsReader
    {
        private readonly Func<string, byte[]?> _readLiveBytes;

        internal DelegateVfsReader(Func<string, byte[]?> readLiveBytes)
        {
            _readLiveBytes = readLiveBytes;
        }

        public IVanillaIntegrityVfsHandle? OpenRead(string resourcePath, long offset)
        {
            var bytes = _readLiveBytes(resourcePath);
            if (bytes == null || offset < 0 || offset > bytes.LongLength)
            {
                return null;
            }

            return new ByteArrayVfsHandle(bytes, offset);
        }
    }

    private sealed class ByteArrayVfsHandle : IVanillaIntegrityVfsHandle
    {
        private readonly byte[] _bytes;
        private long _offset;

        internal ByteArrayVfsHandle(byte[] bytes, long offset)
        {
            _bytes = bytes;
            _offset = offset;
        }

        public long Length => _bytes.LongLength;

        public byte[]? ReadChunk(int maxBytes)
        {
            if (maxBytes <= 0 || _offset >= _bytes.LongLength)
            {
                return [];
            }

            var count = (int)Math.Min(maxBytes, _bytes.LongLength - _offset);
            var buffer = new byte[count];
            Buffer.BlockCopy(_bytes, (int)_offset, buffer, 0, count);
            _offset += count;
            return buffer;
        }

        public void Dispose()
        {
        }
    }

    private sealed class GodotVfsReader : IVanillaIntegrityVfsReader
    {
        internal static readonly GodotVfsReader Instance = new();

        public IVanillaIntegrityVfsHandle? OpenRead(string resourcePath, long offset)
        {
            if (offset < 0 || !Godot.FileAccess.FileExists(resourcePath))
            {
                return null;
            }

            var file = Godot.FileAccess.Open(resourcePath, Godot.FileAccess.ModeFlags.Read);
            if (file == null)
            {
                return null;
            }

            var length = unchecked((long)file.GetLength());
            if (offset > length)
            {
                file.Dispose();
                return null;
            }

            if (offset > 0)
            {
                file.Seek(unchecked((ulong)offset));
            }

            return new GodotVfsHandle(file, length);
        }
    }

    private sealed class GodotVfsHandle : IVanillaIntegrityVfsHandle
    {
        private readonly Godot.FileAccess _file;

        internal GodotVfsHandle(Godot.FileAccess file, long length)
        {
            _file = file;
            Length = length;
        }

        public long Length { get; }

        public byte[]? ReadChunk(int maxBytes) =>
            maxBytes <= 0 ? [] : _file.GetBuffer(maxBytes);

        public void Dispose() => _file.Dispose();
    }

    internal sealed record PreparedCandidate(string ModName, string ModId, byte[]? Sha256);

    internal sealed record PreparedSuspectGroup(
        string Path,
        byte[]? VanillaSha256,
        IReadOnlyList<PreparedCandidate> Candidates);

    internal sealed record PreparedDiskScan(
        int CheckedCount,
        IReadOnlyList<PreparedSuspectGroup> SuspectGroups);
}

[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
internal static class VanillaIntegrityMainMenuReadyPatch
{
    private static void Postfix()
    {
        try
        {
            VanillaIntegritySelfCheck.ScheduleOnce();
        }
        catch (Exception exception)
        {
            ModLog.Warn("[完整性自检] 主菜单入口调度失败（已跳过）：" + exception.GetBaseException().Message);
        }
    }
}
