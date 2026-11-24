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
        Console.WriteLine("Relic bundle residency passed: shared preparation, binary UID isolation, lookup churn, selection, pressure and source change.");
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
