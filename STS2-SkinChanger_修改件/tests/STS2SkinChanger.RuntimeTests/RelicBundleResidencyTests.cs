using System.Buffers.Binary;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;
using STS2SkinChanger.Ui;

internal static class RelicBundleResidencyTests
{
    private const string Group = "defect";
    private const string Sprite = "res://images/atlases/relic_atlas.sprites/fixture.tres";
    private const string Atlas = "res://images/atlases/relic_atlas.png";
    private const string Big = "res://images/relics/fixture.png";
    private const string Payload = "res://.godot/exported/0/export-fixture.res";
    private static SkinChangerStorage _storage = null!;
    private static int _nativeLoads;
    private static int _mounts;
    private static AtlasTexture _baselineSprite = null!;
    private static Texture2D _baselineAtlas = null!;
    private static readonly HashSet<AtlasTexture> RestoredSlices = new(ReferenceEqualityComparer.Instance);

    internal static void Run()
    {
        var root = Directory.CreateTempSubdirectory("sc-relic-residency-");
        using var storage = new SkinChangerStorage(root.FullName, root.FullName, "test");
        _storage = storage;
        var guard = new Harmony("sc.tests.relic-residency");
        var logging = AuditLogSilencer.Install();
        var service = typeof(SkinService);
        var setCatalog = AccessTools.PropertySetter(service, nameof(SkinService.Catalog));
        var setConfig = AccessTools.PropertySetter(service, nameof(SkinService.Config));
        var previousCatalog = SkinService.Catalog;
        var previousConfig = SkinService.Config;
        var previousWriter = PckArchive.WriteOutput;
        var singles = (IDictionary)AccessTools.Field(service, "RuntimeResourceCache").GetValue(null)!;
        var bundles = (IDictionary)AccessTools.Field(service, "RelicBundles").GetValue(null)!;
        object? Invoke(string method, params object?[] args) => AccessTools.Method(service, method).Invoke(null, args);
        try
        {
            void Patch(MethodBase method, string name) =>
                guard.Patch(method, prefix: new HarmonyMethod(typeof(RelicBundleResidencyTests), name));
            Patch(AccessTools.PropertyGetter(typeof(SkinChangerPaths), nameof(SkinChangerPaths.Storage)), nameof(Storage));
            Patch(AccessTools.Method(typeof(SkinChangerPaths), "LoadResourcePack"), nameof(Mount));
            Patch(AccessTools.Method(service, "MountOverlay"), nameof(Skip));
            Patch(AccessTools.Method(typeof(RuntimeLoadSafety), "RequirePreparationHeadroom"), nameof(Skip));
            Patch(AccessTools.PropertyGetter(typeof(CharacterAppearanceRuntime), "IsCombatRuntimeScopeActive"), nameof(Skip));
            _baselineSprite = (AtlasTexture)Fake(typeof(AtlasTexture));
            _baselineAtlas = (Texture2D)Fake(typeof(CompressedTexture2D));
            Patch(AccessTools.Method(service, "GetBaselineRelicIcon"), nameof(BaselineSprite));
            Patch(AccessTools.PropertyGetter(typeof(AtlasTexture), nameof(AtlasTexture.Atlas)), nameof(BaselineAtlas));
            Patch(AccessTools.PropertySetter(typeof(AtlasTexture), nameof(AtlasTexture.Atlas)), nameof(SetAtlas));
            foreach (var property in new[] { nameof(AtlasTexture.Region), nameof(AtlasTexture.Margin), nameof(AtlasTexture.FilterClip) })
            {
                Patch(AccessTools.PropertyGetter(typeof(AtlasTexture), property), nameof(Skip));
                Patch(AccessTools.PropertySetter(typeof(AtlasTexture), property), nameof(Skip));
            }
            Patch(AccessTools.Method(typeof(Resource), nameof(Resource.EmitChanged)), nameof(Skip));
            Patch(AccessTools.Method(typeof(GodotObject), nameof(GodotObject.IsInstanceValid)), nameof(Valid));
            var load = typeof(ResourceLoader).GetMethods().Single(method =>
                method.Name == nameof(ResourceLoader.Load) && !method.IsGenericMethod);
            Patch(load, nameof(LoadResource));

            PckArchive.WriteOutput = null;
            var game = Path.Combine(root.FullName, "game.pck");
            var mod = Path.Combine(root.FullName, "provider.pck");
            PckArchive.Write(game, new Dictionary<string, byte[]> { ["res://fixture.bin"] = [1] });
            var binary = CardPoolOwnershipTests.MakeBinaryPortrait(Atlas, compress: false);
            PckArchive.Write(mod, new Dictionary<string, byte[]> {
                [Sprite + ".remap"] = Encoding.UTF8.GetBytes("[remap]\npath=\"" + Payload + "\"\n"),
                [Payload] = binary, [Atlas] = FakePng(), [Big] = FakePng()
            });

            using var archive = PckArchive.Open(mod);
            using var catalog = SkinCatalog.Build(game, []);
            var group = new SkinGroup(Group, Group);
            var assets = new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase);
            void Asset(string path, params string[] archivePaths)
            {
                var asset = new ResourceAsset(path);
                foreach (var file in archivePaths) asset.AddFile(archive, file);
                assets.Add(path, asset);
            }
            Asset(Sprite, Sprite + ".remap", Payload);
            Asset(Atlas, Atlas);
            Asset(Big, Big);
            group.Options.Add(new("fixture", "Fixture", assets));
            group.Options.Add(new("alternate", "Alternate", assets));
            ((List<SkinGroup>)catalog.Groups).Add(group);
            var config = new SkinConfig();
            config.Selections[Group] = "fixture";
            setCatalog.Invoke(null, [catalog]);
            setConfig.Invoke(null, [config]);
            var writes = 0;
            PckArchive.WriteOutput = (_, _, write) => { writes++; write(); };

            // The startup and demand paths both use this preparation. The first visible icon
            // must mount that exact file and reuse the full root set, including the big icon.
            var prepared = Invoke("PrepareSelectedRelicBundle", catalog, Group, "fixture")!;
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var roots = (string[])prepared.GetType().GetProperty("RelicRoots", flags)!.GetValue(prepared)!;
            Require(roots.ToHashSet().SetEquals([Sprite, Big]) && writes == 1 && _mounts == 0,
                "Startup preparation must include the same slices and large icons as the demand loader.");
            var diskPath = (string)prepared.GetType().GetProperty("OverlayPath", flags)!.GetValue(prepared)!;
            var writesAtPrewarm = writes;
            var first = SkinService.GetRelicIconOverride(Sprite);
            var big = SkinService.GetRelicBigIconOverride(Big);
            Require(first is AtlasTexture && big != null && writes == writesAtPrewarm && _mounts == 1 && _nativeLoads == 3,
                $"The first request must reuse the prebuilt PCK. Small={first?.GetType().Name}, big={big?.GetType().Name}, " +
                $"writes={writes}/{writesAtPrewarm}, mounts={_mounts}, loads={_nativeLoads}.");
            using (var generated = PckArchive.Open(diskPath))
            {
                var payload = generated.Paths.Single(path => path.EndsWith("export-fixture.res", StringComparison.Ordinal));
                var rewritten = generated.ReadFile(payload);
                var dependency = CardBinaryResource.Parse(rewritten)!.Dependencies(Sprite).Single();
                var dependencyBytes = Encoding.UTF8.GetBytes(dependency + "\0");
                var end = rewritten.AsSpan().IndexOf(dependencyBytes) + dependencyBytes.Length;
                Require(dependency.StartsWith("res://sts2_skin_runtime/", StringComparison.Ordinal) &&
                    BinaryPrimitives.ReadInt64LittleEndian(rewritten.AsSpan(end, sizeof(long))) == -1 &&
                    archive.ReadFile(Payload).SequenceEqual(binary),
                    "Only the generated binary must clear the UID and target the verified private texture.");
            }

            var incomplete = catalog.BuildIsolatedRelicResourceOverlay(Group, "fixture", roots, "missing-dependency");
            var incompleteFiles = incomplete.Files.ToDictionary(StringComparer.OrdinalIgnoreCase);
            incompleteFiles.Remove(incomplete.ResourcePaths[Atlas]);
            var incompletePayload = incompleteFiles.Single(pair => pair.Key.EndsWith("export-fixture.res", StringComparison.Ordinal));
            Require(SkinService.RewritePrivateRelicSliceUids(incomplete, incompleteFiles) == 0 &&
                ReferenceEquals(incompleteFiles[incompletePayload.Key], incompletePayload.Value),
                "An unresolved dependency must keep its original UID and bytes.");

            for (var index = 0; index < 325; index++)
                singles.Add("unrelated-" + index, Fake(typeof(CompressedTexture2D)));
            Invoke("TrimRuntimeResidency", new object?[] { null });
            Require(singles.Count == 256 && ReferenceEquals(first, SkinService.GetRelicIconOverride(Sprite)) &&
                _nativeLoads == 3 && _mounts == 1,
                "Trimming 325 independent resources must leave the entire relic bundle resident.");

            Invoke("RequestRuntimePressureCleanup");
            SkinService.TryRunDeferredRuntimePressureCleanup();
            Require(!bundles.Contains(Group) && !RestoredSlices.Contains((AtlasTexture)first!),
                "Memory pressure must release only the cache's ownership.");
            config.Selections[Group] = "alternate";
            Invoke("ClearRuntimeResourceCache", Group);
            Require(!bundles.Contains(Group) && RestoredSlices.Contains((AtlasTexture)first!) &&
                !ReferenceEquals(first, SkinService.GetRelicIconOverride(Sprite)),
                "A selection change must preserve legacy restoration even after a bundle has been evicted.");
            var oldGraph = SkinService.GetRelicIconOverride(Sprite);
            Invoke("RequestRuntimePressureCleanup");
            SkinService.TryRunDeferredRuntimePressureCleanup();
            Require(!bundles.Contains(Group) && !ReferenceEquals(oldGraph, SkinService.GetRelicIconOverride(Sprite)),
                "Deferred memory pressure must drop the decoded bundle and permit a later reload.");

            var freshMod = mod + ".next";
            File.Copy(mod, freshMod);
            File.SetLastWriteTimeUtc(freshMod, new DateTime(archive.Stamp.ModifiedUtcTicks, DateTimeKind.Utc).AddMinutes(2));
            File.Move(mod, mod + ".previous");
            File.Move(freshMod, mod);
            var checks = (CardSourceCheckCache)AccessTools.Field(service, "RelicSourceChecks").GetValue(null)!;
            checks.Clear(); // Force first-use stat without sleeping through the production TTL.
            var oldLoads = _nativeLoads;
            try
            {
                _ = SkinService.GetRelicIconOverride(Sprite);
                throw new Exception("Source change served the old bundle.");
            }
            catch (AppearanceResourceUnavailableException) { }
            Require(!bundles.Contains(Group) && _nativeLoads == oldLoads,
                "A replaced source must retire the bundle without decoding the stale catalog bytes.");

            VerifyAdditiveWorkshopHotRegistrationRelicCarryover(
                root,
                game,
                binary,
                service,
                setCatalog,
                setConfig,
                bundles,
                () => writes,
                Invoke);
        }
        finally
        {
            setCatalog.Invoke(null, [previousCatalog]);
            setConfig.Invoke(null, [previousConfig]);
            Invoke("ClearRelicBundleResidency");
            singles.Clear();
            ((IDictionary)AccessTools.Field(service, "PreparedRuntimeOverlays").GetValue(null)!).Clear();
            ((IDictionary)AccessTools.Field(service, "LastPreparedRuntimeOverlays").GetValue(null)!).Clear();
            PckArchive.WriteOutput = previousWriter;
            guard.UnpatchAll(guard.Id);
            logging.UnpatchAll(logging.Id);
            RestoredSlices.Clear();
            _nativeLoads = 0;
            _mounts = 0;
            storage.Dispose();
            root.Delete(recursive: true);
        }
        Console.WriteLine("Relic bundle residency passed: shared preparation, binary UID isolation, lookup churn, selection, pressure, source change, and additive Workshop carryover.");
    }

    private static void VerifyAdditiveWorkshopHotRegistrationRelicCarryover(
        DirectoryInfo root,
        string gamePath,
        byte[] binarySlice,
        Type service,
        MethodInfo setCatalog,
        MethodInfo setConfig,
        IDictionary bundles,
        Func<int> getWrites,
        Func<string, object?[], object?> invoke)
    {
        var sessionPreparedField = AccessTools.Field(service, "_sessionPreparedCount")
                                   ?? throw new InvalidOperationException("找不到 SkinService._sessionPreparedCount。");
        var workshopGamePackField = AccessTools.Field(service, "_workshopGamePack")!;
        var workshopDescriptorsField = AccessTools.Field(service, "_workshopDescriptors")!;
        var workshopStampsField = AccessTools.Field(service, "_workshopSourceStamps")!;
        var previousGamePack = workshopGamePackField.GetValue(null);
        var previousDescriptors = workshopDescriptorsField.GetValue(null);
        var previousStamps = workshopStampsField.GetValue(null);

        var relicPck = Path.Combine(root.FullName, "workshop_relic_old.pck");
        var unrelatedPck = Path.Combine(root.FullName, "workshop_unrelated_new.pck");
        var prevWriter = PckArchive.WriteOutput;
        PckArchive.WriteOutput = null;
        try
        {
            PckArchive.Write(relicPck, new Dictionary<string, byte[]>
            {
                [Sprite + ".remap"] = Encoding.UTF8.GetBytes("[remap]\npath=\"" + Payload + "\"\n"),
                [Payload] = binarySlice,
                [Atlas] = FakePng(),
                [Big] = FakePng()
            });
            PckArchive.Write(unrelatedPck, new Dictionary<string, byte[]>
            {
                ["res://images/unrelated/extra.png"] = FakePng()
            });
        }
        finally
        {
            PckArchive.WriteOutput = prevWriter;
        }

        var openedArchives = new List<PckArchive>();
        SkinCatalog BuildRelicFixtureCatalog(string pckPath, bool includeBigIcon, bool includeUnrelated)
        {
            var catalog = SkinCatalog.Build(gamePath, []);
            var archive = PckArchive.Open(pckPath);
            openedArchives.Add(archive);
            var group = new SkinGroup(Group, Group);
            var assets = new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase);
            void AddAsset(string path, params string[] archivePaths)
            {
                var asset = new ResourceAsset(path);
                foreach (var file in archivePaths) asset.AddFile(archive, file);
                assets.Add(path, asset);
            }
            AddAsset(Sprite, Sprite + ".remap", Payload);
            AddAsset(Atlas, Atlas);
            if (includeBigIcon) AddAsset(Big, Big);
            group.Options.Add(new("ws-relic", "Workshop Relic", assets));
            group.Options.Add(new("ws-other", "Other Relic", assets));
            ((List<SkinGroup>)catalog.Groups).Add(group);
            if (includeUnrelated)
            {
                var unrelatedGroup = new SkinGroup("silent", "Silent");
                unrelatedGroup.Options.Add(new("ws-unrelated", "Unrelated Skin", new Dictionary<string, ResourceAsset>()));
                ((List<SkinGroup>)catalog.Groups).Add(unrelatedGroup);
            }
            return catalog;
        }

        static Dictionary<string, (long Length, DateTime Modified)> BuildStamps(params string[] paths) =>
            paths.ToDictionary(
                path => path,
                path => { var info = new FileInfo(path); return (info.Length, info.LastWriteTimeUtc); },
                StringComparer.OrdinalIgnoreCase);

        try
        {
            invoke("ClearRelicBundleResidency", []);
            ((IDictionary)AccessTools.Field(service, "PreparedRuntimeOverlays").GetValue(null)!).Clear();
            ((IDictionary)AccessTools.Field(service, "LastPreparedRuntimeOverlays").GetValue(null)!).Clear();

            using var baselineCatalog = BuildRelicFixtureCatalog(relicPck, includeBigIcon: true, includeUnrelated: false);
            var config = new SkinConfig();
            config.Selections[Group] = "ws-relic";
            setCatalog.Invoke(null, [baselineCatalog]);
            setConfig.Invoke(null, [config]);
            var baseDescriptors = new[] { new SkinModDescriptor("ws-relic", "Workshop Relic", relicPck, false, root.FullName, false) };
            workshopDescriptorsField.SetValue(null, baseDescriptors);
            workshopStampsField.SetValue(null, BuildStamps(relicPck));

            // 1) Warm relic atlas on baseline catalog.
            var warmSmall = SkinService.GetRelicIconOverride(Sprite);
            var warmBig = SkinService.GetRelicBigIconOverride(Big);
            Require(warmSmall is AtlasTexture && warmBig != null && bundles.Contains(Group),
                "热身阶段应成功解码并驻留遗物图集。");

            var writesBeforeSwap = getWrites();
            var mountsBeforeSwap = _mounts;
            var loadsBeforeSwap = _nativeLoads;
            var quotaBeforeSwap = (int)sessionPreparedField.GetValue(null)!;

            // 2) Confirmed additive Workshop hot-registration of an unrelated new skin.
            using var additiveCatalog = BuildRelicFixtureCatalog(relicPck, includeBigIcon: true, includeUnrelated: true);
            var combinedDescriptors = baseDescriptors
                .Append(new SkinModDescriptor("ws-unrelated", "Unrelated Skin", unrelatedPck, false, root.FullName, false))
                .ToArray();
            var nextConfig = config.CloneForBundleTransaction();
            invoke("CommitAdditiveWorkshopCatalogSwapLocked", [
                baselineCatalog,
                additiveCatalog,
                nextConfig,
                combinedDescriptors,
                BuildStamps(relicPck, unrelatedPck)
            ]);

            var afterSmall = SkinService.GetRelicIconOverride(Sprite);
            var afterBig = SkinService.GetRelicBigIconOverride(Big);
            var afterPrepared = invoke("PrepareSelectedRelicBundle", [additiveCatalog, Group, "ws-relic"]);
            var quotaAfterSwap = (int)sessionPreparedField.GetValue(null)!;

            Require(ReferenceEquals(afterSmall, warmSmall) && ReferenceEquals(afterBig, warmBig) && afterPrepared != null,
                "追加无关工坊订阅后，原遗物图集必须直接复用原有已热驻留实例。");
            Require(getWrites() == writesBeforeSwap,
                $"追加无关工坊订阅不得重建遗物 PCK：before={writesBeforeSwap}, after={getWrites()}");
            Require(_mounts == mountsBeforeSwap,
                $"追加无关工坊订阅不得重新挂载遗物 PCK：before={mountsBeforeSwap}, after={_mounts}");
            Require(_nativeLoads == loadsBeforeSwap,
                $"追加无关工坊订阅不得重新解码遗物贴图：before={loadsBeforeSwap}, after={_nativeLoads}");
            Require(quotaAfterSwap == quotaBeforeSwap,
                $"追加无关工坊订阅不得重复消耗会话准备配额：before={quotaBeforeSwap}, after={quotaAfterSwap}");

            // 3) If residency was trimmed by pressure (PreparedRuntimeOverlays still warm), another additive
            //    swap still preserves the prepared PCK so PrepareSelectedRelicBundle does not rebuild or spend quota.
            invoke("ClearRelicBundleResidency", []);
            using var additiveCatalog2 = BuildRelicFixtureCatalog(relicPck, includeBigIcon: true, includeUnrelated: true);
            invoke("CommitAdditiveWorkshopCatalogSwapLocked", [
                additiveCatalog,
                additiveCatalog2,
                nextConfig.CloneForBundleTransaction(),
                combinedDescriptors,
                BuildStamps(relicPck, unrelatedPck)
            ]);
            var rewarmedPrepared = invoke("PrepareSelectedRelicBundle", [additiveCatalog2, Group, "ws-relic"]);
            Require(rewarmedPrepared != null &&
                    getWrites() == writesBeforeSwap &&
                    (int)sessionPreparedField.GetValue(null)! == quotaBeforeSwap,
                "驻留被压力清理后仅留准备项时，追加热登记仍须保留已验证的准备 PCK 且不消耗新配额。");

            // Re-warm bundle on additiveCatalog2 for invalidation checks below.
            var rewarmedSmall = SkinService.GetRelicIconOverride(Sprite);
            Require(rewarmedSmall != null && bundles.Contains(Group), "准备项复热应恢复驻留。");

            // 4) Negative case A: root resource set changed in new catalog -> MUST invalidate & rebuild.
            var writesBeforeRootChange = getWrites();
            var loadsBeforeRootChange = _nativeLoads;
            using var changedRootsCatalog = BuildRelicFixtureCatalog(relicPck, includeBigIcon: false, includeUnrelated: true);
            invoke("CommitAdditiveWorkshopCatalogSwapLocked", [
                additiveCatalog2,
                changedRootsCatalog,
                nextConfig.CloneForBundleTransaction(),
                combinedDescriptors,
                BuildStamps(relicPck, unrelatedPck)
            ]);
            Require(!bundles.Contains(Group),
                "新目录中遗物根资源集合变化时，必须淘汰旧驻留而不是继续复用。");
            var rebuiltAfterRootChange = SkinService.GetRelicIconOverride(Sprite);
            Require(rebuiltAfterRootChange != null &&
                    getWrites() == writesBeforeRootChange + 1 &&
                    _nativeLoads > loadsBeforeRootChange,
                "根资源集合变化后应按新目录重建并解码。");

            // 5) Negative case B: selection changed before swap -> MUST evict old residency.
            var changedSelectionConfig = nextConfig.CloneForBundleTransaction();
            changedSelectionConfig.Selections[Group] = "ws-other";
            using var selectionSwapCatalog = BuildRelicFixtureCatalog(relicPck, includeBigIcon: false, includeUnrelated: true);
            invoke("CommitAdditiveWorkshopCatalogSwapLocked", [
                changedRootsCatalog,
                selectionSwapCatalog,
                changedSelectionConfig,
                combinedDescriptors,
                BuildStamps(relicPck, unrelatedPck)
            ]);
            Require(!bundles.Contains(Group),
                "选择项发生变化时，旧选择的遗物驻留不得被转移。");

            // 6) Negative case C: source file modified on disk before swap -> MUST reject carryover.
            var warmForSourceCheck = SkinService.GetRelicIconOverride(Sprite);
            Require(warmForSourceCheck != null && bundles.Contains(Group), "来源变更前应处于驻留态。");
            File.SetLastWriteTimeUtc(relicPck, File.GetLastWriteTimeUtc(relicPck).AddMinutes(3));
            using var modifiedSourceCatalog = BuildRelicFixtureCatalog(relicPck, includeBigIcon: false, includeUnrelated: true);
            invoke("CommitAdditiveWorkshopCatalogSwapLocked", [
                selectionSwapCatalog,
                modifiedSourceCatalog,
                changedSelectionConfig.CloneForBundleTransaction(),
                combinedDescriptors,
                BuildStamps(relicPck, unrelatedPck)
            ]);
            Require(!bundles.Contains(Group),
                "旧遗物源包文件签名变化时，热登记转移必须拒绝复用并淘汰旧驻留。");

            // 7) Negative case D: file open throws IOException during carryover source verification ->
            //    degrade cleanly to cache miss without throwing or disposing the newly published Catalog.
            var ioConfig = nextConfig.CloneForBundleTransaction();
            ioConfig.Selections[Group] = "ws-relic";
            using var ioBaselineCatalog = BuildRelicFixtureCatalog(relicPck, includeBigIcon: true, includeUnrelated: false);
            setCatalog.Invoke(null, [ioBaselineCatalog]);
            setConfig.Invoke(null, [ioConfig]);
            workshopDescriptorsField.SetValue(null, baseDescriptors);
            workshopStampsField.SetValue(null, BuildStamps(relicPck));
            workshopGamePackField.SetValue(null, null);
            var warmBeforeIo = SkinService.GetRelicIconOverride(Sprite);
            Require(warmBeforeIo != null && bundles.Contains(Group), "I/O 异常对照前遗物应处于驻留态。");

            var lockedProbePath = Path.Combine(root.FullName, "locked_game_probe.pck");
            var preparedMap = (IDictionary)AccessTools.Field(service, "PreparedRuntimeOverlays").GetValue(null)!;
            var preparedKeysBeforeIo = preparedMap.Keys.Cast<string>().ToArray();
            Require(preparedKeysBeforeIo.Length > 0, "I/O 异常对照前应存在已准备的遗物覆盖项。");

            using var ioPublishedCatalog = BuildRelicFixtureCatalog(relicPck, includeBigIcon: true, includeUnrelated: true);
            using (var exclusiveLock = new FileStream(
                       lockedProbePath,
                       FileMode.Create,
                       System.IO.FileAccess.ReadWrite,
                       FileShare.None))
            {
                exclusiveLock.WriteByte(1);
                exclusiveLock.Flush();
                workshopGamePackField.SetValue(null, lockedProbePath);
                // Must NOT throw IOException even though CaptureRelicBundleSources cannot open lockedProbePath.
                invoke("CommitAdditiveWorkshopCatalogSwapLocked", [
                    ioBaselineCatalog,
                    ioPublishedCatalog,
                    ioConfig.CloneForBundleTransaction(),
                    combinedDescriptors,
                    BuildStamps(relicPck, unrelatedPck)
                ]);
            }

            workshopGamePackField.SetValue(null, null);
            Require(ReferenceEquals(SkinService.Catalog, ioPublishedCatalog),
                "验源打开文件抛 I/O 时，新目录必须保持已发布状态。");
            Require(!bundles.Contains(Group) && preparedKeysBeforeIo.All(k => !preparedMap.Contains(k)),
                "验源打开文件抛 I/O 时，受影响遗物项应降级为缓存未命中并从准备与驻留表中淘汰。");
            var reloadedAfterIo = SkinService.GetRelicIconOverride(Sprite);
            Require(reloadedAfterIo != null && bundles.Contains(Group),
                "新目录未被误释放，I/O 恢复后应能在已发布新目录上正常重建并加载遗物图集。");
        }
        finally
        {
            foreach (var opened in openedArchives)
            {
                try { opened.Dispose(); } catch { }
            }
            workshopGamePackField.SetValue(null, previousGamePack);
            workshopDescriptorsField.SetValue(null, previousDescriptors);
            workshopStampsField.SetValue(null, previousStamps);
        }
    }

    internal static void Audit(string game, string provider)
    {
        var logging = AuditLogSilencer.Install();
        try
        {
            var name = Path.GetFileNameWithoutExtension(provider);
            using var catalog = SkinCatalog.Build(game, [new(name, name, provider, false, Path.GetDirectoryName(provider))]);
            var option = catalog.Groups.Single(group => group.Id == Group).Options.Single(option => option.Id == name);
            var oldRoots = catalog.GetProviderRelicSpritePaths(Group, option);
            var roots = catalog.GetProviderRelicResourcePaths(Group, option);
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var overlay = catalog.BuildIsolatedRelicResourceOverlay(Group, name, roots, "relic-audit/1");
            var build = timer.Elapsed.TotalMilliseconds;
            var files = overlay.Files.ToDictionary(StringComparer.OrdinalIgnoreCase);
            timer.Restart();
            var cleaned = SkinService.RewritePrivateRelicSliceUids(overlay, files);
            Console.WriteLine($"真实遗物包：旧预建根={oldRoots.Count} 新预建与加载根={roots.Count} 私有文件={files.Count} " +
                $"安全改写={cleaned} 纹理估算={RuntimeLoadSafety.EstimateOverlayTextureBytes(files)}B " +
                $"离线组图={build:F1}ms 改写={timer.Elapsed.TotalMilliseconds:F1}ms");
            Require(cleaned > 0 && roots.Count > oldRoots.Count, "Real package did not exercise the diagnosed failure.");
        }
        finally { logging.UnpatchAll(logging.Id); }
    }

    private static byte[] FakePng()
    {
        byte[] bytes = new byte[24];
        bytes[0] = 0x89;
        "PNG"u8.CopyTo(bytes.AsSpan(1));
        "IHDR"u8.CopyTo(bytes.AsSpan(12));
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(16), 16);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(20), 16);
        return bytes;
    }

    private static Resource Fake(Type type)
    {
        var resource = (Resource)RuntimeHelpers.GetUninitializedObject(type);
        GC.SuppressFinalize(resource);
        return resource;
    }

    private static bool Storage(ref SkinChangerStorage __result) { __result = _storage; return false; }
    private static bool Mount(ref bool __result) { _mounts++; __result = true; return false; }
    private static bool Skip() => false;
    private static bool BaselineSprite(ref Texture2D __result) { __result = _baselineSprite; return false; }
    private static bool BaselineAtlas(ref Texture2D __result) { __result = _baselineAtlas; return false; }
    private static bool SetAtlas(AtlasTexture __instance, Texture2D __0)
    {
        if (ReferenceEquals(__0, _baselineAtlas)) RestoredSlices.Add(__instance);
        return false;
    }
    private static bool Valid(GodotObject? instance, ref bool __result) { __result = instance != null; return false; }
    private static bool LoadResource(string path, ref Resource __result)
    {
        _nativeLoads++;
        __result = Fake(path.Contains(".sprites/", StringComparison.Ordinal) ? typeof(AtlasTexture) : typeof(CompressedTexture2D));
        return false;
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}
