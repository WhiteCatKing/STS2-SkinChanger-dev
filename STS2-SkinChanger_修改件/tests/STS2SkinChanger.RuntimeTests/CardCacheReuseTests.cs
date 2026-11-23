using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class CardCacheReuseTests
{
    private const string Prefix = "res://sts2_skin_runtime/test-switch/";
    private const string Card = "res://same-card.tres";
    private static readonly ConditionalWeakTable<AtlasTexture, Texture2D> Pages = new();
    private static readonly ConditionalWeakTable<Texture2D, string> PageNames = new();
    private static WeakReference<Texture2D>? _privatePage;
    private static ResourceFile? _raster;
    private static int _diskReads;

    internal static void Run()
    {
        CheckSourceCheckWindow();
        CardPortraitSessionTests.Run(privateReuse: true);
    }
    internal static Texture2D? Backing(AtlasTexture atlas) => Pages.TryGetValue(atlas, out var page) ? page : null;
    internal static Texture2D? LoadAtlas(string path, ResourceLoader.CacheMode mode)
    {
        if (path != Prefix + "same-card.tres" && path != Prefix + "sibling.tres") return null;
        Check(mode == ResourceLoader.CacheMode.Ignore, "Private external page must use Godot's dependency cache.");
        var atlas = (AtlasTexture)CardPortraitSessionTests.FakeTexture(typeof(AtlasTexture));
        if (_privatePage == null || !_privatePage.TryGetTarget(out var page))
        {
            page = CardPortraitSessionTests.FakeTexture(typeof(CompressedTexture2D));
            PageNames.Add(page, Prefix + "page.png");
            _privatePage = new(page);
        }
        Pages.Add(atlas, page);
        return atlas;
    }

    internal static void Check(string directory, Harmony harmony, Type stateType, IDictionary states,
        Type requestType, MethodInfo getter)
    {
        void Patch(MethodBase method, string prefix) => harmony.Patch(method,
            prefix: new HarmonyMethod(typeof(CardCacheReuseTests), prefix));
        Patch(AccessTools.Method(typeof(SkinCatalog), "FindCardImageFile"), nameof(FindRaster));
        Patch(AccessTools.Method(typeof(CardFileStamp), nameof(CardFileStamp.IsCurrent)), nameof(CountDiskRead));
        Patch(AccessTools.PropertyGetter(typeof(Resource), nameof(Resource.ResourcePath)), nameof(PagePath));
        harmony.Patch(AccessTools.Method(typeof(SkinService), "LoadRasterImageFromBuffer"),
            transpiler: new HarmonyMethod(typeof(CardCacheReuseTests), nameof(OfflineImageCalls)));
        Patch(AccessTools.Method(typeof(ImageTexture), nameof(ImageTexture.CreateFromImage)), nameof(CreateTexture));
        var file = Path.Combine(directory, "a.pck");
        var raw = Path.Combine(directory, "b.json");
        File.WriteAllBytes(file, [1]);
        File.WriteAllBytes(raw, [2]);
        static CardFileStamp Stamp(string path)
        {
            using var stream = File.OpenRead(path);
            return CardFileStamp.From(stream);
        }
        var stamp = Stamp(file);
        _raster = new ResourceFile("res://private/same-card.png", [137,80,78,71,13,10,26,10])
            { InlineStamp = Stamp(raw) };
        var baseKey = SkinCatalog.BaseOptionId + "\nbase";
        var previousBase = states[baseKey];
        var aKey = "test\npack-a\nprovider";
        var clear = AccessTools.Method(typeof(SkinService), "ClearCardPortraitCache");
        var root = Prefix + "same-card.tres";
        var page = Prefix + "page.png";
        var payload = Prefix + "page.ctex";
        object State(string path)
        {
            var state = Activator.CreateInstance(stateType, nonPublic: true)!;
            ((Dictionary<string,string>)AccessTools.Property(stateType, "ResourcePaths").GetValue(state)!)[Card] = path;
            return state;
        }
        var selected = State(root);
        AccessTools.Property(stateType, "AliasToken").SetValue(selected, "test-switch");
        var graph = (CardPrivateGraph)AccessTools.Property(stateType, "PrivateGraph").GetValue(selected)!;
        graph.Add(root, new(stamp, [page], ""));
        graph.Add(page + ".import", new(stamp, [payload], "params-a"));
        graph.Add(payload, new(stamp, [], ""));
        var key = graph.Resolve(root, "test-switch")!;
        Check(key.Key.Contains(page) && key.Key.Contains("params-a") && key.Key.Contains(stamp.Key),
            "Private page, physical identity and import parameters must participate in the key.");
        graph.Add(Prefix + "foreign", new(Stamp(raw), [page], ""));
        graph.Add(Prefix + "open", new(stamp, ["res://canonical/page.png"], ""));
        Check(graph.Resolve(Prefix + "foreign", "test-switch") == null &&
            graph.Resolve(Prefix + "open", "test-switch") == null, "Mixed or open chains cannot share.");
        Texture2D? Request(string selection, bool selectionChanged = true)
        {
            if (selectionChanged) clear.Invoke(null, ["test"]);
            bool provider = selection != SkinCatalog.BaseOptionId;
            var request = Activator.CreateInstance(requestType, "test", selection, Card,
                $"test\n{selection}\n{Card}", provider, false)!;
            return (Texture2D?)getter.Invoke(null, [request]);
        }
        try
        {
            states[aKey] = selected;
            states[baseKey] = State("res://fixture-default.tres");
            SkinService.BeginCardLibraryVisit();
            var a = Request("pack-a");
            var b = Request("pack-b");
            var vanilla = Request(SkinCatalog.BaseOptionId);
            Check(a != null && b != null && vanilla != null && !ReferenceEquals(a, b) && !ReferenceEquals(a, vanilla) && !ReferenceEquals(b, vanilla),
                "Same card must resolve to three separate textures.");
            var cold = SkinService.TakeCardPortraitMetrics("fixture-cold", "", null, 0);
            Check(cold.Contains("atlas_new=1 ") && cold.Contains("raster_decode=1 ") && cold.Contains("reuse_bytes=16384 "),
                "Unexpected cold counters: " + cold);
            for (var round = 0; round < 3; round++)
            {
                Check(ReferenceEquals(a, Request("pack-a")) && ReferenceEquals(b, Request("pack-b")) &&
                    ReferenceEquals(vanilla, Request(SkinCatalog.BaseOptionId)), "Repeated cycle reloaded or crossed providers.");
            }
            var warm = SkinService.TakeCardPortraitMetrics("fixture-warm", "", null, 0);
            Check(warm.Contains("atlas_new=0 ") && warm.Contains("raster_decode=0 ") && warm.Contains("reuse_bytes=16384 "),
                "Unexpected repeated counters: " + warm);
            Console.WriteLine(cold);
            Console.WriteLine(warm);
            SkinService.BeginCardLibraryVisit();
            _diskReads = 0;
            for (var redraw = 0; redraw < 24; redraw++)
                Check(ReferenceEquals(a, Request("pack-a", selectionChanged: false)), "Fast hit lost its page.");
            Check(_diskReads == 1, "24 consecutive portrait reads must share one file check.");
            Check(ReferenceEquals(a, Request("pack-a")) && _diskReads == 2,
                "A selection change must bypass the previous file check once.");
            var paths = (Dictionary<string,string>)AccessTools.Property(stateType, "ResourcePaths").GetValue(selected)!;
            paths[Card] = Prefix + "sibling.tres";
            graph.Add(paths[Card], new(stamp, [page], ""));
            var sibling = Request("pack-a") as AtlasTexture;
            Check(sibling != null && !ReferenceEquals(a, sibling) &&
                ReferenceEquals(Backing((AtlasTexture)a!), Backing(sibling)),
                "Two private slices must retain their own roots and share a single page.");
            var siblingWindow = SkinService.TakeCardPortraitMetrics("fixture-sibling", "", null, 0);
            Check(siblingWindow.Contains("atlas_new=0 ") && siblingWindow.Contains("reuse_bytes=16384 "),
                "A second slice charged the same backing twice.");
            CompareWorkingSet();
            paths[Card] = Prefix + "foreign";
            var mixed = Request("pack-a");
            Check(mixed != null && !ReferenceEquals(mixed, Request("pack-a")),
                "Changing selection must release reuse ownership of an unproven or mixed-source chain.");
            paths[Card] = root;
            Check(ReferenceEquals(a, Request("pack-a")), "Original page should still be resident.");
            File.SetLastWriteTimeUtc(file, new DateTime(stamp.ModifiedUtcTicks, DateTimeKind.Utc).AddMinutes(1));
            SkinService.BeginCardLibraryVisit();
            Check(Request("pack-a", selectionChanged: false) == null && !stamp.IsCurrent(),
                "Opening the library must catch the changed stamp before a cached page is returned.");
            File.AppendAllText(raw, "new");
            Check(Request("pack-b") == null, "Size change served the old PNG.");
            Check(SkinService.TakeCardPortraitMetrics("invalidated", "", null, 0).Contains("reuse_bytes=0 "),
                "Obsolete residency was not removed.");
        }
        finally
        {
            _raster = null;
            _privatePage = null;
            states.Remove(aKey);
            if (previousBase == null) states.Remove(baseKey);
            else states[baseKey] = previousBase;
        }
    }

    // Compare the old shared budget and the new provider partition using the same LRU,
    // pixel sizes and request order. No real GPU allocation or decode timing is inferred.
    private static void CompareWorkingSet()
    {
        foreach (var partitioned in new[] { false, true })
        {
            var oldCache = new CardPortraitSessionCache();
            var providers = partitioned
                ? (CardPortraitSessionCache)AccessTools.Field(typeof(SkinService), "PrivateCardPortraits").GetValue(null)!
                : oldCache;
            var initialBytes = providers.EstimatedBytes;
            var performance = new CardPortraitPerformance();
            void Browse()
            {
                for (var i = 0; i < 49; i++)
                {
                    var key = "pressure-a/" + i;
                    providers.GetOrLoad(key, false, () =>
                    {
                        var atlas = (AtlasTexture)CardPortraitSessionTests.FakeTexture(typeof(AtlasTexture));
                        var page = CardPortraitSessionTests.FakeTexture(typeof(CompressedTexture2D), 1109, 832);
                        PageNames.Add(page, Prefix + key);
                        Pages.Add(atlas, page);
                        performance.RecordNativeLoad(key, 0);
                        return atlas;
                    }, performance);
                }
                for (var i = 0; i < 79; i++)
                {
                    var key = "pressure-b/" + i;
                    providers.GetOrLoad(key, false, () =>
                    {
                        performance.RecordRasterDecode(key, 0);
                        return CardPortraitSessionTests.FakeTexture(typeof(ImageTexture), 800, 608);
                    });
                }
                oldCache.GetOrLoad("default", false, () => CardPortraitSessionTests.FakeTexture(typeof(CompressedTexture2D), 1024));
            }
            Browse();
            performance.ResetWindow();
            Browse();
            var summary = performance.TakeSummary(partitioned ? "budget-new" : "budget-old", "simulated", null, 0);
            var bytes = providers.EstimatedBytes - initialBytes;
            Console.WriteLine($"{summary} provider_or_shared_bytes={bytes}");
            Check(summary.Contains(partitioned ? "atlas_new=0 " : "atlas_new=49 ") &&
                summary.Contains(partitioned ? "raster_decode=0 " : "raster_decode=79 "),
                "Same working set must demonstrate the difference in repeated loads.");
            if (partitioned)
            {
                providers.Invalidate("pressure-a/");
                providers.Invalidate("pressure-b/");
            }
            oldCache.Clear();
        }
    }

    private static void CheckSourceCheckWindow()
    {
        var now = 1000L;
        var reads = 0;
        var current = true;
        var stamp = new CardFileStamp("fixture.pck", 1, 0);
        var checks = new CardSourceCheckCache(_ => { reads++; return current; }, () => now);

        Check(checks.IsCurrent(stamp) && reads == 1, "First observation must query the source.");
        current = false;
        now += 1999;
        Check(checks.IsCurrent(stamp) && reads == 1, "A hit must retain its original check deadline.");
        now++;
        Check(!checks.IsCurrent(stamp) && !checks.IsCurrent(stamp) && reads == 2,
            "Expiry must discover and memoize a changed source.");
        current = true;
        checks.Clear();
        Check(checks.IsCurrent(stamp) && reads == 3, "Clearing must force a fresh check.");
    }

    private static void CountDiskRead() => _diskReads++;

    private static bool FindRaster(string selectionId, ref ResourceFile? __result)
    {
        if (selectionId != "pack-b") return true;
        __result = _raster;
        return false;
    }
    private static bool PagePath(Resource __instance, ref string __result)
    {
        __result = __instance is Texture2D texture && PageNames.TryGetValue(texture, out var path) ? path : "";
        return false;
    }
    private static bool CreateTexture(ref ImageTexture __result)
    {
        __result = (ImageTexture)CardPortraitSessionTests.FakeTexture(typeof(ImageTexture));
        return false;
    }
    private static IEnumerable<CodeInstruction> OfflineImageCalls(IEnumerable<CodeInstruction> body)
    {
        foreach (var op in body)
        {
            var replacement = op.operand switch
            {
                ConstructorInfo ctor when ctor.DeclaringType == typeof(Image) => nameof(BlankImage),
                MethodInfo m when m.Name == nameof(Image.LoadPngFromBuffer) => nameof(DecodePng),
                MethodInfo m when m.Name == nameof(Image.IsEmpty) => nameof(Nonempty),
                _ => null
            };
            if (replacement != null)
            {
                op.opcode = OpCodes.Call;
                op.operand = AccessTools.Method(typeof(CardCacheReuseTests), replacement);
            }
            yield return op;
        }
    }
    private static Image BlankImage()
    {
        var image = (Image)RuntimeHelpers.GetUninitializedObject(typeof(Image));
        GC.SuppressFinalize(image);
        return image;
    }
    private static Error DecodePng(Image image, byte[] bytes) => Error.Ok;
    private static bool Nonempty(Image image) => false;
    private static void Check(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
}
