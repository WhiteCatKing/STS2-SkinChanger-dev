using System.Collections;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class GlobalWriteFallbackTests
{
    private const string Group = "ironclad";
    private const string ProtectedCard = "res://images/packed/card_portraits/ironclad/fallback.png";
    private const string PrivateMember = "res://test/keeper.bin";
    private const string MerchantAtlas = "res://images/atlases/compressed_atlas.png.import";
    private const string MerchantOriginalAtlas = "res://images/atlases/ui_atlas.png.import";
    private static readonly HashSet<string> Groups = new(StringComparer.OrdinalIgnoreCase) { Group };
    private static readonly Dictionary<string, IReadOnlyList<string>> Packs = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<string> Warnings = [];
    private static readonly List<string> NativeMounts = [];
    private static SkinChangerStorage _storage = null!;
    private static SkinCatalog _catalog = null!;
    private static PckArchive _badArchive = null!;
    private static GlobalWriteSite _scenario;
    private static bool _sawRollback;

    internal static void Run()
    {
        var root = Directory.CreateTempSubdirectory("sc-blocked-mount-");
        using var storage = new SkinChangerStorage(root.FullName, root.FullName, "test");
        _storage = storage;
        var logging = AuditLogSilencer.Install();
        var harmony = new Harmony("sc.tests.global-write-fallback");
        var writer = PckArchive.WriteOutput;
        var savedCatalog = SkinService.Catalog;
        var savedConfig = SkinService.Config;
        var diagnostics = AppearanceDiagnostics.Enabled;
        var savedState = new[] { "SkippedRuntimeProviders", "MountedScopedRuntimeProviderPacks",
            "MountedLargeRuntimeProviderPacks", "MountedOverlayOwners", "MountedOverlayCache",
            "_cachedOverlayPlan", "_needsFullOverlayReassert", "_pendingRemountGroups", "_lastError" }
            .Select(CaptureState).ToArray();
        try
        {
            PckArchive.WriteOutput = null;
            AppearanceDiagnostics.Enabled = false;
            var game = Path.Combine(root.FullName, "game.pck");
            PckArchive.Write(game, new Dictionary<string, byte[]>
            {
                [ProtectedCard] = [1], [MerchantAtlas] = [2], [MerchantOriginalAtlas] = [3]
            });
            var bad = Path.Combine(root.FullName, "provider.pck");
            PckArchive.Write(bad, new Dictionary<string, byte[]>
            {
                [ProtectedCard] = [9], [PrivateMember] = [5], ["res://test/extra.bin"] = [6]
            });
            var unreadable = Path.Combine(root.FullName, "unprojectable.pck");
            File.WriteAllBytes(unreadable, [0, 0, 0, 0]);
            var deletion = MakePack(root.FullName, "provider-delete", ProtectedCard, 9);
            MarkEntryRemoved(deletion);
            var firstGood = MakePack(root.FullName, "good-sibling", "res://test/sibling.bin", 6);
            var separateGood = MakePack(root.FullName, "good-provider", "res://test/provider.bin", 7);
            using var catalog = SkinCatalog.Build(game, []);
            using var source = PckArchive.Open(bad);
            _catalog = catalog;
            _badArchive = source;
            Assign(nameof(SkinService.Catalog), catalog);
            var config = new SkinConfig();
            Assign(nameof(SkinService.Config), config);
            var group = new SkinGroup(Group, Group);
            group.Options.Add(new("provider", "Blocked provider", new Dictionary<string, ResourceAsset>(), IsRuntimeProvider: true));
            group.Options.Add(new("bad-visual", "Blocked visual layer", new Dictionary<string, ResourceAsset>()));
            ((List<SkinGroup>)catalog.Groups).Add(group);

            void Patch(Type type, string method, string prefix) => harmony.Patch(
                AccessTools.Method(type, method),
                prefix: new HarmonyMethod(typeof(GlobalWriteFallbackTests), prefix));
            harmony.Patch(AccessTools.PropertyGetter(typeof(SkinChangerPaths), nameof(SkinChangerPaths.Storage)),
                prefix: new HarmonyMethod(typeof(GlobalWriteFallbackTests), nameof(Storage)));
            foreach (var level in new[] { nameof(ModLog.Warn), nameof(ModLog.Error), nameof(ModLog.Info) })
                harmony.Patch(AccessTools.Method(typeof(ModLog), level),
                    prefix: new HarmonyMethod(typeof(GlobalWriteFallbackTests), nameof(Warn)) { priority = Priority.First });
            Patch(typeof(ProjectSettings), nameof(ProjectSettings.LoadResourcePack), nameof(Mount));
            Patch(typeof(SkinCatalog), "GetIndexedProviderResourcePackPaths", nameof(PackPaths));
            Patch(typeof(SkinCatalog), "ProviderUsesScopedMonsterRuntime", nameof(Scoped));
            Patch(typeof(SkinCatalog), "ProviderRequiresCoherentRuntimePackage", nameof(Coherent));
            Patch(typeof(SkinCatalog), nameof(SkinCatalog.BuildOverlay), nameof(BuildOverlay));
            Patch(typeof(FrameworkCompatibilityLayer), "SynchronizeSelections", nameof(Skip));
            Patch(typeof(ManagedSkinModLoader), "ConfigureRunEnvironmentProviders", nameof(Skip));
            Patch(typeof(ManagedSkinModLoader), "DeactivateProvidersExcept", nameof(Skip));
            Patch(typeof(ManagedSkinModLoader), "ActivateSelectedProviders", nameof(Skip));
            Patch(typeof(SkinService), "MountOverlayCore", nameof(OverlayCore));

            CheckMerchantAliases(catalog);

            _scenario = GlobalWriteSite.A08;
            Packs["provider"] = [firstGood, deletion];
            Packs["good"] = [separateGood];
            config.Selections[Group] = "provider";
            var saved = false;
            Invoke("MountStartupOverlay", Groups, (Action)(() => saved = true));
            Require(saved && ReferenceEquals(SkinService.Catalog, catalog) &&
                    config.GetSelection(Group) == "provider" &&
                    SkinService.GetDisplayedSelection(Group) == SkinCatalog.BaseOptionId &&
                    NativeMounts.SequenceEqual([separateGood], StringComparer.OrdinalIgnoreCase) &&
                    Warnings.Any(line => line.Contains("已拦截") && line.Contains("跳过提供者 provider")),
                $"A08 must keep startup alive and mount only the good provider. saved={saved}, " +
                $"configured={config.GetSelection(Group)}, effective={SkinService.GetDisplayedSelection(Group)}, " +
                $"mounts={string.Join('|', NativeMounts)}, warnings={string.Join('|', Warnings)}");
            ((IDictionary)AccessTools.Field(typeof(SkinService), "SkippedRuntimeProviders").GetValue(null)!).Clear();

            _scenario = GlobalWriteSite.A10;
            var projected = (string)Invoke("PrepareHighPriorityProviderPack", catalog, GlobalWriteSite.A10, bad)!;
            using (var clean = PckArchive.OpenForAudit(projected))
                Require(!clean.Contains(ProtectedCard) && clean.ReadFile(PrivateMember).SequenceEqual(new byte[] { 5 }),
                    "A10 must project only the protected overwrite out of an otherwise valid pack.");
            Require(GlobalResourceWrites.Snapshot()[^1] is
                    { Result: GlobalWriteResult.Stripped, SourcePack: var sourcePack, CanonicalCount: 1 } &&
                    sourcePack == bad, "The A10 ledger must identify the source pack and stripped path.");
            var mixedRemoval = Path.Combine(root.FullName, "mixed-removal.pck");
            const string privateRemoval = "res://test/remove.bin";
            using (var gameArchive = PckArchive.Open(game))
                PckArchive.WriteFromArchives(mixedRemoval,
                    new Dictionary<string, (PckArchive, string)> { [ProtectedCard] = (gameArchive, ProtectedCard) },
                    [MerchantAtlas, privateRemoval]);
            projected = (string)Invoke("PrepareHighPriorityProviderPack", catalog, GlobalWriteSite.A07, mixedRemoval)!;
            using (var clean = PckArchive.OpenForAudit(projected))
                Require(clean.RemovedPaths.SequenceEqual([privateRemoval]) &&
                        clean.ReadFile(ProtectedCard).SequenceEqual(new byte[] { 1 }),
                    "A07/A10 projection must retain exact originals and private removals and strip protected removals.");
            Packs["provider"] = [firstGood, unreadable];
            saved = false;
            Invoke("MountStartupOverlay", Groups, (Action)(() => saved = true));
            Require(saved && ReferenceEquals(SkinService.Catalog, catalog) &&
                    SkinService.GetDisplayedSelection(Group) == SkinCatalog.BaseOptionId &&
                    NativeMounts.Count == 1 &&
                    Warnings.Any(line => line.Contains("A10") && line.Contains("跳过提供者 provider")),
                "A10 must also allow startup to continue and leave an unsafe provider off the native pack stack.");
            ((IDictionary)AccessTools.Field(typeof(SkinService), "SkippedRuntimeProviders").GetValue(null)!).Clear();

            config.Selections[Group] = SkinCatalog.BaseOptionId;
            var configPath = storage.ConfigurationFile("skin_changer.json");
            config.Save(configPath);
            var savedBytes = File.ReadAllBytes(configPath);
            var mountedBefore = NativeMounts.Count;

            _scenario = GlobalWriteSite.A10;
            CheckSelectionRollback("provider", config, savedBytes, configPath, mountedBefore);
            Require(Warnings.Any(line => line.Contains("已拦截") && line.Contains("A10")),
                "A10 must record the real blocked promotion.");

            _scenario = GlobalWriteSite.A11;
            // Let A11 strip the conflicting entry, then corrupt its output to exercise real refusal.
            PckArchive.WriteOutput = (path, _, _) => File.WriteAllBytes(path, [0, 0, 0, 0]);
            try { CheckSelectionRollback("bad-visual", config, savedBytes, configPath, mountedBefore); }
            finally { PckArchive.WriteOutput = null; }
            Require(Warnings.Any(line => line.Contains("已拦截") && line.Contains("A11")) &&
                    Warnings.Any(line => line.Contains("本次切换已撤销")) &&
                    GlobalResourceWrites.Snapshot().Any(entry => entry is
                        { Site: GlobalWriteSite.A11, Result: GlobalWriteResult.Stripped, CanonicalCount: 1 } &&
                        entry.CanonicalPaths.Single() == ProtectedCard),
                "A11 must separately account for stripping and a later unreadable correction.");

            foreach (var site in new[] { GlobalWriteSite.A02, GlobalWriteSite.A06, GlobalWriteSite.A07 })
            {
                _scenario = site;
                var attempted = new List<string>();
                var selection = "fixture-" + site;
                var result = SkinService.WithSpinePageFallback(catalog, Group, selection, selected =>
                {
                    attempted.Add(selected);
                    if (selected != SkinCatalog.BaseOptionId)
                        SkinChangerPaths.LoadResourcePack(site, bad, replaceFiles: true, throwOnBlock: true);
                    return selected;
                });
                Require(attempted.SequenceEqual([selection, SkinCatalog.BaseOptionId]) &&
                        result == SkinCatalog.BaseOptionId && NativeMounts.Count == mountedBefore &&
                        Warnings.Any(line => line.Contains(site.ToString()) && line.Contains("回退原版")),
                    $"{site} must use the existing group-level original retry and never call the native loader for the rejected PCK.");
            }

            var nativeFailure = GlobalResourceWrites.MountPack(GlobalWriteSite.A10, separateGood, true,
                (_, _) => false, () => catalog.GlobalWriteProtection, Warnings.Add, "native-failure", throwOnBlock: true);
            Require(!nativeFailure && GlobalResourceWrites.Snapshot()[^1].Result == GlobalWriteResult.Failed,
                "A plain native false must stay on the old failure path.");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            logging.UnpatchAll(logging.Id);
            Assign(nameof(SkinService.Catalog), savedCatalog);
            Assign(nameof(SkinService.Config), savedConfig);
            AppearanceDiagnostics.Enabled = diagnostics;
            foreach (var restore in savedState) restore();
            PckArchive.WriteOutput = writer;
            Packs.Clear();
            NativeMounts.Clear();
            Warnings.Clear();
            storage.Dispose();
            root.Delete(recursive: true);
        }
        Console.WriteLine("Global write fallback passed: A08 startup, A10/A11 selection rollback, A02/A06/A07 original retry.");
    }

    private static void CheckMerchantAliases(SkinCatalog catalog)
    {
        const string alias = "res://sts2_skin_runtime/merchant/atlas.png.import";
        var aliases = new Dictionary<string, string> { [MerchantAtlas] = alias };
        var overlay = new RuntimeResourceOverlay(aliases, new Dictionary<string, byte[]>
        {
            [MerchantAtlas] = [9], [MerchantOriginalAtlas] = [3], [alias] = [9]
        }, aliases, new Dictionary<string, string>(), new HashSet<string> { MerchantAtlas, MerchantOriginalAtlas });
        var prepared = (RuntimeResourceOverlay)Invoke("StripRuntimeSharedWrites", catalog.GlobalWriteProtection,
            overlay, "merchant", "fixture")!;
        Require(!prepared.Files.ContainsKey(MerchantAtlas) &&
                prepared.Files[MerchantOriginalAtlas].SequenceEqual(new byte[] { 3 }) &&
                prepared.Files[alias].SequenceEqual(new byte[] { 9 }) &&
                prepared.ResourcePaths[MerchantAtlas] == alias &&
                !prepared.CanonicalDependencyPaths.Contains(MerchantAtlas),
            "A06 must preserve private merchant atlas data and allow identical game bytes.");
    }

    private static void CheckSelectionRollback(string selection, SkinConfig config, byte[] savedBytes,
        string configPath, int mountedBefore)
    {
        _sawRollback = false;
        var applied = SkinService.ApplySelection(Group, selection);
        var sameFile = File.ReadAllBytes(configPath).SequenceEqual(savedBytes);
        Require(!applied && _sawRollback &&
                config.GetSelection(Group) == SkinCatalog.BaseOptionId &&
                sameFile &&
                ReferenceEquals(SkinService.Catalog, _catalog) && NativeMounts.Count == mountedBefore &&
                SkinService.LastError?.Contains("已拦截") == true,
            $"{_scenario} must restore the successful selection and leave the saved file and engine stack unchanged. " +
            $"applied={applied}, rollback={_sawRollback}, selection={config.GetSelection(Group)}, " +
            $"sameFile={sameFile}, mounts={NativeMounts.Count}/{mountedBefore}, error={SkinService.LastError}, " +
            $"logs={string.Join('|', Warnings.TakeLast(9))}");
    }

    private static bool OverlayCore(bool skipBlockedProviders)
    {
        if (_scenario == GlobalWriteSite.A08)
        {
            Require(skipBlockedProviders, "Startup must opt into skipping blocked providers.");
            Invoke("EnsureScopedRuntimeProviderResourcesMounted", _catalog, new[] { "provider", "good" }, true);
            return false;
        }
        if (SkinService.Config.GetSelection(Group) == SkinCatalog.BaseOptionId)
        {
            _sawRollback = true;
            return false;
        }
        if (_scenario == GlobalWriteSite.A10)
        {
            Invoke("MountSelectedLargeRuntimeProviderPacks", _catalog, Groups, skipBlockedProviders, (Action)(() => { }));
            return false;
        }
        return _scenario == GlobalWriteSite.A11;
    }

    private static bool BuildOverlay(ref Dictionary<string, ResourceFile> __result)
    {
        if (_scenario != GlobalWriteSite.A11) return true;
        __result = new Dictionary<string, ResourceFile>
        {
            [ProtectedCard] = new(_badArchive, ProtectedCard),
            [PrivateMember] = new(_badArchive, PrivateMember)
        };
        return false;
    }

    private static bool Storage(ref SkinChangerStorage __result) { __result = _storage; return false; }
    private static bool Warn(string message) { Warnings.Add(message); return false; }
    private static bool Mount(string pack, ref bool __result) { NativeMounts.Add(pack); __result = true; return false; }
    private static bool Scoped(string optionId, ref bool __result)
    {
        __result = _scenario == GlobalWriteSite.A08 && Packs.ContainsKey(optionId);
        return false;
    }
    private static bool PackPaths(string optionId, ref IReadOnlyList<string> __result)
    {
        __result = Packs.GetValueOrDefault(optionId) ?? [];
        return false;
    }
    private static bool Coherent(ref bool __result) { __result = true; return false; }
    private static bool Skip() => false;

    private static string MakePack(string root, string name, string entry, byte marker)
    {
        var path = Path.Combine(root, name + ".pck");
        PckArchive.Write(path, new Dictionary<string, byte[]> { [entry] = [marker] });
        return path;
    }

    private static void MarkEntryRemoved(string path)
    {
        using var stream = File.Open(path, FileMode.Open, System.IO.FileAccess.ReadWrite);
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        stream.Position = 100; // v2 writer header
        var nameBytes = reader.ReadUInt32();
        stream.Seek(nameBytes + 32, SeekOrigin.Current);
        writer.Write(2U);
    }

    private static void Assign(string property, object? value) =>
        AccessTools.PropertySetter(typeof(SkinService), property).Invoke(null, [value]);

    private static object? Invoke(string method, params object?[] arguments)
    {
        try { return AccessTools.Method(typeof(SkinService), method).Invoke(null, arguments); }
        catch (TargetInvocationException error) when (error.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(error.InnerException).Throw();
            throw;
        }
    }

    private static Action CaptureState(string name)
    {
        var field = AccessTools.Field(typeof(SkinService), name);
        var original = field.GetValue(null);
        if (original is IDictionary dictionary)
        {
            var entries = new List<DictionaryEntry>();
            foreach (DictionaryEntry entry in dictionary) entries.Add(entry);
            dictionary.Clear();
            return () => { dictionary.Clear(); foreach (var entry in entries) dictionary.Add(entry.Key, entry.Value); };
        }
        if (original is HashSet<string> set)
        {
            var entries = set.ToArray();
            set.Clear();
            return () => { set.Clear(); set.UnionWith(entries); };
        }
        return () => field.SetValue(null, original);
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}
