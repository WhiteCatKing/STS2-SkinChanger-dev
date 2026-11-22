using System.Diagnostics;
using System.Globalization;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core.Features.Card;

internal static class CardPortraitPerformanceView
{
    private static bool _libraryOpen;
    private static bool _waitingForInitialGrid;
    private static bool _initialGridReported;
    private static int _visit;
    private static int _lastPage;
    private static long _nextPageSample;
    private static long _openingStarted;

    internal static void OnLibraryOpening()
    {
        SkinService.ResetCardPortraitMetricsWindow();
        _visit++;
        _openingStarted = Stopwatch.GetTimestamp();
        _libraryOpen = true;
        _waitingForInitialGrid = true;
        _initialGridReported = false;
        _lastPage = 0;
        _nextPageSample = 0;
    }

    internal static void ObserveInitialGrid(Task initialization)
    {
        if (!_libraryOpen || !_waitingForInitialGrid) return;
        _waitingForInitialGrid = false;
        _ = ReportAfterInitialGrid(initialization, _visit);
    }

    private static async Task ReportAfterInitialGrid(Task initialization, int visit)
    {
        try { await initialization; }
        catch
        {
            // The game already owns this task and its errors. Keep waiting for the next init.
            if (_libraryOpen && visit == _visit) _waitingForInitialGrid = true;
            return;
        }

        if (!_libraryOpen || visit != _visit) return;
        _initialGridReported = true;
        var elapsed = Stopwatch.GetElapsedTime(_openingStarted).TotalMilliseconds;
        WriteSummary("library_open", FormattableString.Invariant($"elapsed_ms={elapsed:F2}"));
    }

    internal static void OnLibraryClosed()
    {
        if (!_libraryOpen) return;
        _libraryOpen = false;
        WriteSummary("library_close", string.Empty);
    }

    internal static bool ShouldSamplePage()
    {
        if (!_libraryOpen || !_initialGridReported) return false;
        var now = System.Environment.TickCount64;
        if (now < _nextPageSample) return false;
        _nextPageSample = now + 100;
        return true;
    }

    internal static int ScrollPage(float viewportHeight, float scrollTop, float scrollPosition)
    {
        if (!float.IsFinite(viewportHeight) || viewportHeight <= 0 ||
            !float.IsFinite(scrollTop) || !float.IsFinite(scrollPosition))
        {
            return 0;
        }

        return (int)Math.Clamp(Math.Floor(((double)scrollTop - scrollPosition) / viewportHeight), 0, int.MaxValue);
    }

    internal static void OnPageObserved(int page)
    {
        if (page == _lastPage) return;
        _lastPage = page;
        WriteSummary("library_page", (page + 1L).ToString(CultureInfo.InvariantCulture));
    }

    internal static void OnSelectionChanged(string group, string selection)
    {
        if (!_libraryOpen) return;
        var visit = _visit;
        // Wait until the selection handler has refreshed the visible card nodes.
        UiInjectionGuard.Run("CardPortraitPerformance.Selection", () =>
            Callable.From(() =>
            {
                if (_libraryOpen && visit == _visit)
                    WriteSummary("card_selection", group + "/" + selection);
            }).CallDeferred());
    }

    private static void WriteSummary(string scene, string detail)
    {
        UiInjectionGuard.Run("CardPortraitPerformance.Report", () =>
        {
            var value = Performance.GetMonitor(Performance.Monitor.RenderTextureMemUsed);
            long? textureBytes = double.IsFinite(value) && value >= 0 && value < long.MaxValue
                ? (long)value
                : null;
            ModLog.Info(SkinService.TakeCardPortraitMetrics(scene, detail, textureBytes, GC.GetTotalMemory(false)));
        });
    }
}

[HarmonyPatch(typeof(NCardLibrary), nameof(NCardLibrary.OnSubmenuOpened))]
internal static class CardPortraitPerformanceOpenPatch
{
    private static void Prefix() =>
        UiInjectionGuard.Run("CardPortraitPerformance.Open", CardPortraitPerformanceView.OnLibraryOpening);
}

[HarmonyPatch(typeof(NCardLibrary), nameof(NCardLibrary.OnSubmenuClosed))]
internal static class CardPortraitPerformanceClosePatch
{
    private static void Postfix() => CardPortraitPerformanceView.OnLibraryClosed();
}

[HarmonyPatch(typeof(NCardGrid), "InitGrid", typeof(Task))]
internal static class CardPortraitPerformanceGridPatch
{
    private static void Postfix(NCardGrid __instance, Task __result) =>
        UiInjectionGuard.Run("CardPortraitPerformance.InitialGrid", () =>
        {
            if (__instance is NCardLibraryGrid)
                CardPortraitPerformanceView.ObserveInitialGrid(__result);
        });
}

[HarmonyPatch(typeof(NCardGrid), "UpdateScrollPosition")]
internal static class CardPortraitPerformanceScrollPatch
{
    private static readonly Func<NCardGrid, float>? ScrollTop =
        AccessTools.PropertyGetter(typeof(NCardGrid), "ScrollLimitTop")?.CreateDelegate<Func<NCardGrid, float>>();

    [HarmonyPrepare]
    private static bool Prepare() => ScrollTop != null && AccessTools.Field(typeof(NCardGrid), "_scrollContainer") != null;

    private static void Postfix(NCardGrid __instance, Control ____scrollContainer)
    {
        if (__instance is NCardLibraryGrid && CardPortraitPerformanceView.ShouldSamplePage())
            SamplePage(__instance, ____scrollContainer);
    }

    private static void SamplePage(NCardGrid grid, Control scrollContainer) =>
        UiInjectionGuard.Run("CardPortraitPerformance.Scroll", () =>
            CardPortraitPerformanceView.OnPageObserved(
                CardPortraitPerformanceView.ScrollPage(grid.Size.Y, ScrollTop!(grid), scrollContainer.Position.Y)));
}
