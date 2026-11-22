using System.Diagnostics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using STS2SkinChanger.Core;
using STS2SkinChanger.Core.Features.Card;

internal static class CardPortraitPerformanceTests
{
    internal static void Run()
    {
        CheckWindowAndSessionCounters();
        CheckResetBeforeLibraryOpening();
        CheckScrollPage();
        CheckRuntimeTargets();
        Console.WriteLine("Card portrait performance passed: bounded load observations, log deltas, page boundaries and real Harmony targets.");
    }

    private static void CheckWindowAndSessionCounters()
    {
        const string root = "res://alias/strike.tres";
        const string pageA = "res://alias/atlas_0.png";
        const string pageB = "res://other/atlas_0.png";
        var time = Stopwatch.Frequency / 1000;
        var metrics = new CardPortraitPerformance();

        metrics.Request();
        metrics.Hit();
        metrics.Request();
        metrics.IsolatedLoad();
        metrics.Request();
        metrics.Fallback();

        metrics.RecordNativeLoad(root, 3 * time);
        metrics.RecordNativeLoad(root.ToUpperInvariant(), 7 * time);
        metrics.RecordRasterDecode("raw-raster", 5 * time);
        metrics.ObserveNativeTexture(pageA, 10, isAtlasPage: true);
        Require(!metrics.NeedsAtlasPath(10), "Reused page objects must not fetch their native path again.");
        metrics.ObserveNativeTexture(pageA, 10, isAtlasPage: true);
        metrics.ObserveNativeTexture(pageA, 11, isAtlasPage: true);
        metrics.ObserveNativeTexture(pageB, 12, isAtlasPage: true);
        metrics.ObserveNativeTexture(null, 13, isAtlasPage: true);
        metrics.ObserveNativeTexture(null, 14, isAtlasPage: false);
        var first = metrics.TakeSummary("library_open", "one\ntwo", 200, 120);
        Contains(first, "scene=library_open detail=\"one two\"", "requests=3 hits=1 isolated=1 fallback=1",
            "native_load=2 raster_decode=1 native_texture_new=5",
            "atlas_new=3 atlas_repeat_pages=1 atlas_max_loads=2 atlas_path=\"" + pageA + "\"",
            "atlas_paths=2 atlas_by_path=" + '"' + pageA + "=2|" + pageB + "=1" + '"',
            "load_path_max_calls=2 load_path=" + '"' + root + '"',
            "backing_untracked=1", "cold_n=2 cold_avg_ms=4.00 cold_max_ms=5.00",
            "hot_n=1 hot_avg_ms=7.00 hot_max_ms=7.00", "texture_bytes=200 texture_peak_bytes=200",
            "managed_bytes=120 managed_peak_bytes=120");

        // Readings are per checkpoint, but cache identity and sample peaks span the session.
        metrics.RecordNativeLoad(root, 2 * time);
        metrics.ObserveNativeTexture(pageA, 11, isAtlasPage: true);
        metrics.ObserveNativeTexture(pageA, 15, isAtlasPage: true);
        var second = metrics.TakeSummary("library_page", "2", 150, 180);
        Contains(second, "requests=0 hits=0 isolated=0 fallback=0",
            "native_load=1 raster_decode=0 native_texture_new=1",
            "atlas_new=1 atlas_repeat_pages=0 atlas_max_loads=1",
            "cold_n=0 cold_avg_ms=0.00 cold_max_ms=0.00",
            "hot_n=1 hot_avg_ms=2.00", "texture_bytes=150 texture_peak_bytes=200",
            "managed_bytes=180 managed_peak_bytes=180");
        Contains(metrics.TakeSummary("library_close", "", null, 40),
            "texture_bytes=na texture_peak_bytes=200", "managed_bytes=40 managed_peak_bytes=180");

        var bounded = new CardPortraitPerformance();
        for (ulong id = 1; id <= 16_385; id++)
        {
            bounded.ObserveNativeTexture(pageA, id, isAtlasPage: true);
            bounded.RecordNativeLoad("root/" + id, time);
        }
        Contains(bounded.TakeSummary("library_page", "", null, 0),
            "native_load=16385", "native_texture_new=16384", "backing_untracked=1", "timing_untracked=1");
    }

    private static void CheckResetBeforeLibraryOpening()
    {
        const string root = "res://alias/strike.tres";
        const string page = "res://alias/atlas_0.png";
        var time = Stopwatch.Frequency / 1000;
        var metrics = new CardPortraitPerformance();
        metrics.TakeSummary("library_close", "", 200, 120);
        metrics.Request();
        metrics.IsolatedLoad();
        metrics.RecordNativeLoad(root, time);
        metrics.ObserveNativeTexture(page, 10, isAtlasPage: true);

        metrics.ResetWindow();

        metrics.Request();
        metrics.RecordNativeLoad(root, 2 * time);
        metrics.ObserveNativeTexture(page, 10, isAtlasPage: true);
        Contains(metrics.TakeSummary("library_open", "", 100, 60),
            "requests=1 hits=0 isolated=0", "native_load=1 raster_decode=0 native_texture_new=0",
            "atlas_new=0", "atlas_paths=0", "load_path_max_calls=1",
            "cold_n=0 cold_avg_ms=0.00 cold_max_ms=0.00", "hot_n=1 hot_avg_ms=2.00",
            "texture_peak_bytes=200", "managed_peak_bytes=120");
    }

    private static void CheckScrollPage()
    {
        Require(CardPortraitPerformanceView.ScrollPage(900, 10, 15) == 0 &&
                CardPortraitPerformanceView.ScrollPage(900, 10, -880) == 0 &&
                CardPortraitPerformanceView.ScrollPage(900, 10, -890) == 1 &&
                CardPortraitPerformanceView.ScrollPage(900, 10, -1800) == 2 &&
                CardPortraitPerformanceView.ScrollPage(0, 0, -900) == 0,
            "A page must be one viewport of scroll distance from the native top position.");
    }

    private static void CheckRuntimeTargets()
    {
        var harmony = new Harmony("sc.test.card-portrait-performance");
        try
        {
            harmony.CreateClassProcessor(typeof(CardPortraitPerformanceOpenPatch)).Patch();
            harmony.CreateClassProcessor(typeof(CardPortraitPerformanceClosePatch)).Patch();
            harmony.CreateClassProcessor(typeof(CardPortraitPerformanceGridPatch)).Patch();
            harmony.CreateClassProcessor(typeof(CardPortraitPerformanceScrollPatch)).Patch();
            foreach (var (type, method, args) in new[]
                     {
                         (typeof(NCardLibrary), nameof(NCardLibrary.OnSubmenuOpened), Type.EmptyTypes),
                         (typeof(NCardLibrary), nameof(NCardLibrary.OnSubmenuClosed), Type.EmptyTypes),
                         (typeof(NCardGrid), "InitGrid", new[] { typeof(Task) }),
                         (typeof(NCardGrid), "UpdateScrollPosition", new[] { typeof(double) })
                     })
            {
                Require(Harmony.GetPatchInfo(AccessTools.Method(type, method, args))?.Owners.Contains(harmony.Id) == true,
                    $"Missing portrait performance hook on {type.Name}.{method}.");
            }
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
        }
    }

    private static void Contains(string summary, params string[] expected)
    {
        foreach (var value in expected)
            Require(summary.Contains(value, StringComparison.Ordinal), $"Missing summary field: {value}\n{summary}");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
