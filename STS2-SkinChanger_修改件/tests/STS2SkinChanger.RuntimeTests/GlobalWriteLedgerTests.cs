using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class GlobalWriteLedgerTests
{
    internal static void Run()
    {
        var log = AuditLogSilencer.Install();
        var previousWriter = PckArchive.WriteOutput;
        using var temporary = new TempPackDirectory();
        try
        {
            PckArchive.WriteOutput = null;
            CheckForwardingAndRules(temporary.Path);
        }
        finally
        {
            PckArchive.WriteOutput = previousWriter;
            log.UnpatchAll(log.Id);
        }
        Console.WriteLine("Global write ledger passed: original byte proof, protected writes, forwarding and bounded history.");
    }

    private static void CheckForwardingAndRules(string root)
    {
        const string atlas = "res://images/atlases/card_atlas_0.png";
        const string atlasRemap = atlas + ".import";
        const string atlasPayload = "res://.godot/imported/card_atlas_0.png-ledger.ctex";
        const string card = "res://images/packed/card_portraits/ironclad/strike.png";
        const string cardPayload = "res://.godot/imported/strike.png-ledger.ctex";
        const string portrait = "res://images/packed/character_select/char_select_ironclad.tres";
        const string portraitTexture = "res://images/portraits/ironclad.png";
        const string portraitPayload = "res://.godot/imported/ironclad.png-ledger.ctex";
        const string allowedScene = "res://scenes/creature_visuals/ironclad.tscn";
        const string alias = "res://sts2_skin_runtime/test/strike.png";
        static byte[] Reference(string path) => Encoding.UTF8.GetBytes($"path=\"{path}\"\n");

        var game = Path.Combine(root, "game.pck");
        PckArchive.Write(game, new Dictionary<string, byte[]>
        {
            [atlas] = [1], [atlasRemap] = Reference(atlasPayload), [atlasPayload] = [1],
            [card] = [2], [card + ".import"] = Reference(cardPayload), [cardPayload] = [2],
            [portrait] = Reference(portraitTexture), [portraitTexture + ".import"] = Reference(portraitPayload),
            [portraitPayload] = [3], [allowedScene] = [4]
        });
        using var catalog = SkinCatalog.Build(game, []);
        var protection = catalog.GlobalWriteProtection;
        ForbiddenGlobalWrite Check(string path) => protection.Decide(path, fromGame: false);
        Require(Check(atlas) == ForbiddenGlobalWrite.F1 &&
                Check(atlasRemap) == ForbiddenGlobalWrite.F1 &&
                Check(atlasPayload) == ForbiddenGlobalWrite.F1, "F1 must cover roots, redirects and payloads.");
        Require(Check(card) == ForbiddenGlobalWrite.F2 && Check(cardPayload) == ForbiddenGlobalWrite.F2,
            "F2 must cover roots and imported payloads.");
        Require(Check(portrait) == ForbiddenGlobalWrite.F3 &&
                Check(portraitTexture + ".import") == ForbiddenGlobalWrite.F3 &&
                Check(portraitPayload) == ForbiddenGlobalWrite.F3, "F3 must follow the portrait dependency graph.");
        Require(Check(alias) == ForbiddenGlobalWrite.None && !protection.IsCanonical(alias),
            "Private aliases must not be logged as vanilla writes.");
        Require(Check("res://images/packed/card_portraits/watcher_beautified/strike.png") == ForbiddenGlobalWrite.None &&
                Check("res://images/packed/card_portraits/status/beta/wither.png") == ForbiddenGlobalWrite.None &&
                Check("res://images/packed/character_select/char_select_watcher.tres") == ForbiddenGlobalWrite.None,
            "A stock-looking but unindexed path must not take a private mod resource out of circulation.");
        Require(protection.Decide(atlasPayload, fromGame: true) == ForbiddenGlobalWrite.None &&
                protection.Decide(card, fromGame: false, GlobalWriteScope.SafePackFilter) == ForbiddenGlobalWrite.None &&
                protection.Decide(portraitPayload, fromGame: false, GlobalWriteScope.SafePackFilter) == ForbiddenGlobalWrite.None,
            "Authentic game content is allowed, and D2 must keep the old, narrower filters.");

        var pack = Path.Combine(root, "mixed.pck");
        PckArchive.Write(pack, new Dictionary<string, byte[]>
        {
            [atlasPayload] = [9], [cardPayload] = [9], [portraitPayload] = [9], [allowedScene] = [9], [alias] = [9]
        });
        var warnings = new List<string>();
        var calls = 0;
        bool NativeFalse(string path, bool replace)
        {
            calls++;
            Require(warnings.Count == 0, "A low-priority fill must ignore protected entries already in the game PCK.");
            Require(path == pack && !replace, "The engine must receive the original arguments.");
            return false;
        }
        Require(!GlobalResourceWrites.MountPack(GlobalWriteSite.A15, pack, false,
            NativeFalse, () => protection, warnings.Add, nameof(CheckForwardingAndRules)), "Native false must pass through.");
        var failed = GlobalResourceWrites.Snapshot()[^1];
        Require(calls == 1 && failed is { Site: GlobalWriteSite.A15, Kind: GlobalWriteKind.PackFill,
                    Result: GlobalWriteResult.Failed, CanonicalCount: 4, Violations: 0, Incomplete: false },
            "A low-priority native failure must stay a failure, without a policy violation.");
        Require(failed.CanonicalPaths.ToHashSet(StringComparer.OrdinalIgnoreCase)
            .SetEquals([atlasPayload, cardPayload, portraitPayload, allowedScene]), "Recorded paths must be canonical.");

        Require(!GlobalResourceWrites.MountPack(GlobalWriteSite.A11, pack, true,
                (_, _) => { calls++; return true; }, () => protection, warnings.Add, nameof(CheckForwardingAndRules)) &&
                calls == 1 && warnings.Count == 3 && warnings.All(w => w.Contains("已拦截") && w.Contains("A11/")) &&
                GlobalResourceWrites.Snapshot()[^1] is { Result: GlobalWriteResult.Blocked, Violations: 3, OriginalCount: 0 },
            "Non-original high-priority writes must skip the native call and return false.");

        var originalPack = Path.Combine(root, "original.pck");
        var restored = new Dictionary<string, byte[]> { [atlasPayload] = [1], [cardPayload] = [2], [portraitPayload] = [3] };
        PckArchive.Write(originalPack, restored);
        Require(GlobalResourceWrites.MountPack(GlobalWriteSite.A11, originalPack, true,
                (path, replace) => { calls++; return path == originalPack && replace; }, () => protection, warnings.Add,
                nameof(CheckForwardingAndRules)) && calls == 2 && warnings.Count == 3 &&
                GlobalResourceWrites.Snapshot()[^1] is { Result: GlobalWriteResult.OriginalRestored, OriginalCount: 3, Violations: 0 } original &&
                original.OriginalPaths.ToHashSet().SetEquals(restored.Keys),
            "The same protected paths with exact game bytes must be allowed and recorded as original restoration.");

        CheckCacheWrites(catalog, card, portrait);

        var absentProtected = Path.Combine(root, "new-card.pck");
        PckArchive.Write(absentProtected, new Dictionary<string, byte[]> { [card] = [9] });
        var originalException = new IOException("native failure");
        try
        {
            GlobalResourceWrites.MountPack(GlobalWriteSite.A15, absentProtected, false, (_, _) => throw originalException,
                () => protection, _ => throw new InvalidOperationException("logger failure"), nameof(CheckForwardingAndRules));
            throw new Exception("The original exception was swallowed.");
        }
        catch (IOException exception) when (ReferenceEquals(exception, originalException)) { }
        Require(GlobalResourceWrites.Snapshot()[^1] is { Result: GlobalWriteResult.Failed, Violations: 0 },
            "A native exception must propagate unchanged without recording a protected fill violation.");

        var removalPack = Path.Combine(root, "removal.pck");
        PckArchive.Write(removalPack, new Dictionary<string, byte[]> { [card] = [] });
        using (var stream = File.Open(removalPack, FileMode.Open, System.IO.FileAccess.ReadWrite))
        using (var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            // The local writer emits a 100-byte Godot v2 header, followed by one directory entry.
            stream.Position = 100;
            var nameBytes = reader.ReadUInt32();
            stream.Seek(nameBytes + 32, SeekOrigin.Current);
            writer.Write(2U);
        }
        using (var ordinary = PckArchive.Open(removalPack))
            Require(!ordinary.Contains(card), "The ordinary PCK reader must keep removal entries out of its file view.");
        foreach (var replace in new[] { true, false })
        {
            Require(!GlobalResourceWrites.MountPack(GlobalWriteSite.A15, removalPack, replace,
                    (_, _) => throw new Exception("Protected removal reached the native loader."),
                    () => protection, _ => { }, nameof(CheckForwardingAndRules)) &&
                    GlobalResourceWrites.Snapshot()[^1] is { Violations: 1, Result: GlobalWriteResult.Blocked },
                "Explicit removal is not a low-priority fill and must be blocked.");
        }

        var absent = Path.Combine(root, "unreadable.pck");
        var nativeCalls = 0;
        for (var i = 0; i < 130; i++)
            Require(GlobalResourceWrites.MountPack(GlobalWriteSite.A01, absent, true,
                (path, replace) => { nativeCalls++; return path == absent && replace; },
                () => protection, _ => { }, nameof(CheckForwardingAndRules)), "Index failure must not block a successful native call.");
        var ledger = GlobalResourceWrites.Snapshot();
        Require(nativeCalls == 130 && ledger.Length == 128 &&
                ledger.All(entry => entry is { Site: GlobalWriteSite.A01, Result: GlobalWriteResult.Succeeded, Incomplete: true }),
            "The ledger must remain bounded and mark failed observations.");
    }

    private static void CheckCacheWrites(SkinCatalog catalog, string protectedPath, string otherProtectedPath)
    {
        var harmony = new Harmony("sc.tests.global-cache-writes");
        var previousCatalog = SkinService.Catalog;
        var assignCatalog = AccessTools.PropertySetter(typeof(SkinService), nameof(SkinService.Catalog));
        _takeOvers = _preloadWrites = 0;
        try
        {
            assignCatalog.Invoke(null, [catalog]);
            harmony.Patch(AccessTools.Method(typeof(Resource), nameof(Resource.TakeOverPath)),
                prefix: new HarmonyMethod(typeof(GlobalWriteLedgerTests), nameof(TakeOverStub)));
            harmony.Patch(AccessTools.Method(typeof(AssetCache), nameof(AssetCache.SetAsset)),
                prefix: new HarmonyMethod(typeof(GlobalWriteLedgerTests), nameof(PreloadStub)));
            var asset = (Resource)RuntimeHelpers.GetUninitializedObject(typeof(Resource));
            GC.SuppressFinalize(asset);
            var cache = (AssetCache)RuntimeHelpers.GetUninitializedObject(typeof(AssetCache));
            var preparation = 0;
            Require(!GlobalResourceWrites.TakeOver(GlobalWriteSite.A16, asset, protectedPath,
                        beforeWrite: () => preparation++) &&
                    !GlobalResourceWrites.SetPreloadAsset(GlobalWriteSite.A25, cache, protectedPath, asset, restoring: true) &&
                    _takeOvers == 0 && _preloadWrites == 0 && preparation == 0,
                "Unknown content must leave both protected cache entries and pre-write side effects alone.");

            GlobalResourceWrites.RememberVerifiedOriginal(asset, protectedPath);
            Require(!GlobalResourceWrites.TakeOver(GlobalWriteSite.A16, asset, otherProtectedPath),
                "Proof for one path must not grant writes to any other protected path.");
            Require(GlobalResourceWrites.TakeOver(GlobalWriteSite.A16, asset, protectedPath) &&
                    GlobalResourceWrites.SetPreloadAsset(GlobalWriteSite.A20, cache, protectedPath, asset) &&
                    _takeOvers == 1 && _preloadWrites == 1 &&
                    GlobalResourceWrites.Snapshot().TakeLast(2).All(entry =>
                        entry is { Result: GlobalWriteResult.OriginalRestored, Violations: 0, OriginalCount: 1 }),
                "Verified original resources must reach both native caches and be recorded as original restoration.");
        }
        finally
        {
            assignCatalog.Invoke(null, [previousCatalog]);
            harmony.UnpatchAll(harmony.Id);
        }
    }

    private static int _takeOvers;
    private static int _preloadWrites;
    private static bool TakeOverStub() { _takeOvers++; return false; }
    private static bool PreloadStub() { _preloadWrites++; return false; }

    // The native mount is substituted in headless tests. This still indexes the exact PCK
    // produced by the real fixture and exercises the production audit and ledger.
    internal static void AssertSafePack(SkinCatalog catalog, GlobalWriteSite site, string path, string fixture)
    {
        var policyStart = Stopwatch.GetTimestamp();
        var protection = catalog.GlobalWriteProtection;
        var policy = Stopwatch.GetElapsedTime(policyStart).TotalMilliseconds;
        static bool Mount(string _, bool __) => true;
        void Observe() => GlobalResourceWrites.MountPack(site, path, true,
            Mount, () => protection, _ => { }, fixture);

        var start = Stopwatch.GetTimestamp();
        Observe();
        var first = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        start = Stopwatch.GetTimestamp();
        for (var i = 0; i < 20; i++) Observe();
        var repeat = Stopwatch.GetElapsedTime(start).TotalMilliseconds / 20;
        var entry = GlobalResourceWrites.Snapshot()[^1];
        Require(entry.Site == site && Enum.IsDefined(entry.Site) &&
                entry.Caller == fixture &&
                entry.Result is GlobalWriteResult.Succeeded or GlobalWriteResult.OriginalRestored && !entry.Incomplete &&
                entry.Violations == 0 && entry.CanonicalPaths.All(p =>
                    protection.Decide(p, fromGame: entry.OriginalPaths.Contains(p, StringComparer.OrdinalIgnoreCase)) == ForbiddenGlobalWrite.None),
            fixture + " published protected paths: " + string.Join(", ",
                entry.CanonicalPaths.Where(p => protection.Decide(p, fromGame: false) != ForbiddenGlobalWrite.None)));
        Console.WriteLine(FormattableString.Invariant(
            $"Global write fixture {fixture} ({site}): policy={policy:F3} ms, first={first:F3} ms, cached={repeat:F3} ms, canonical={entry.CanonicalCount}, originals={entry.OriginalCount}, blocked=0"));
    }

    internal static void AssertSafeGeneratedPack(
        SkinCatalog catalog, GlobalWriteSite site, IReadOnlyDictionary<string, byte[]> files, string fixture)
    {
        using var temporary = new TempPackDirectory();
        var writer = PckArchive.WriteOutput;
        try
        {
            PckArchive.WriteOutput = null;
            var path = Path.Combine(temporary.Path, "overlay.pck");
            PckArchive.Write(path, files);
            AssertSafePack(catalog, site, path, fixture);
        }
        finally
        {
            PckArchive.WriteOutput = writer;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class TempPackDirectory : IDisposable
    {
        private readonly DirectoryInfo _directory = Directory.CreateTempSubdirectory("sc-global-ledger-");
        internal string Path => _directory.FullName;
        public void Dispose()
        {
            try { _directory.Delete(recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
