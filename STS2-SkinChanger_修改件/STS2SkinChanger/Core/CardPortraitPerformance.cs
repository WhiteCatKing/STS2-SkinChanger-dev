using System.Diagnostics;
using System.Globalization;

namespace STS2SkinChanger.Core;

// Updates run under SkinService.Sync, alongside the portrait request/cache operations.
internal sealed class CardPortraitPerformance
{
    private const int MaxSessionIdentities = 16_384;
    private const int MaxWindowPaths = 4_096;
    private readonly HashSet<string> _loadedPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<ulong> _seenBackings = [];
    private readonly Dictionary<string, int> _windowAtlasLoads = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _windowLoadsByPath = new(StringComparer.OrdinalIgnoreCase);
    private long? _sampledTexturePeak;
    private long _sampledManagedPeak;
    private Counts _window;

    internal void Request() => _window.Requests++;
    internal void Hit() => _window.Hits++;
    internal void IsolatedLoad() => _window.IsolatedLoads++;
    internal void Fallback() => _window.Fallbacks++;

    internal void RecordNativeLoad(string path, long elapsed)
    {
        _window.NativeLoads++;
        RecordTiming(path, elapsed);
    }

    internal void RecordRasterDecode(string path, long elapsed)
    {
        _window.RasterDecodes++;
        RecordTiming(path, elapsed);
    }

    internal bool NeedsAtlasPath(ulong instanceId) =>
        _seenBackings.Count < MaxSessionIdentities && !_seenBackings.Contains(instanceId);

    internal void ObserveNativeTexture(string? pagePath, ulong instanceId, bool isAtlasPage)
    {
        if (_seenBackings.Contains(instanceId)) return;
        if (_seenBackings.Count >= MaxSessionIdentities)
        {
            _window.UntrackedBackings++;
            return;
        }
        _seenBackings.Add(instanceId);
        _window.NewNativeTextures++;
        if (!isAtlasPage) return;

        if (string.IsNullOrWhiteSpace(pagePath) ||
            (_windowAtlasLoads.Count >= MaxWindowPaths && !_windowAtlasLoads.ContainsKey(pagePath)))
        {
            _window.UntrackedBackings++;
            return;
        }

        _window.AtlasPageLoads++;
        _windowAtlasLoads[pagePath] = _windowAtlasLoads.GetValueOrDefault(pagePath) + 1;
    }

    internal string TakeSummary(string scene, string detail, long? textureBytes, long managedBytes)
    {
        if (textureBytes is { } current)
            _sampledTexturePeak = Math.Max(_sampledTexturePeak ?? 0, current);
        _sampledManagedPeak = Math.Max(_sampledManagedPeak, managedBytes);

        var repeatedPages = 0;
        var maxPageLoads = 0;
        string? busiestPage = null;
        foreach (var (pagePath, loads) in _windowAtlasLoads)
        {
            if (loads > 1) repeatedPages++;
            if (loads > maxPageLoads ||
                (loads == maxPageLoads && StringComparer.OrdinalIgnoreCase.Compare(pagePath, busiestPage) < 0))
            {
                maxPageLoads = loads;
                busiestPage = pagePath;
            }
        }

        var atlasPaths = _windowAtlasLoads.Count;
        var atlasSamples = string.Join('|', _windowAtlasLoads
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .Select(pair => FormattableString.Invariant($"{pair.Key}={pair.Value}")));
        var busiestLoad = _windowLoadsByPath.DefaultIfEmpty().MaxBy(pair => pair.Value);
        var window = _window;
        ResetWindow();
        return string.Create(CultureInfo.InvariantCulture,
            $"[卡图性能] scene={scene} detail={Quote(detail)} requests={window.Requests} hits={window.Hits} " +
            $"isolated={window.IsolatedLoads} fallback={window.Fallbacks} native_load={window.NativeLoads} " +
            $"raster_decode={window.RasterDecodes} native_texture_new={window.NewNativeTextures} " +
            $"atlas_new={window.AtlasPageLoads} atlas_repeat_pages={repeatedPages} atlas_max_loads={maxPageLoads} " +
            $"atlas_path={Quote(busiestPage ?? "-")} atlas_paths={atlasPaths} atlas_by_path={Quote(atlasSamples)} " +
            $"load_path_max_calls={busiestLoad.Value} load_path={Quote(busiestLoad.Key ?? "-")} " +
            $"load_paths_untracked={window.UntrackedLoadPaths} backing_untracked={window.UntrackedBackings} " +
            $"cold_n={window.Cold.Count} cold_avg_ms={window.Cold.AverageMs:F2} cold_max_ms={window.Cold.MaxMs:F2} " +
            $"hot_n={window.Hot.Count} hot_avg_ms={window.Hot.AverageMs:F2} hot_max_ms={window.Hot.MaxMs:F2} " +
            $"timing_untracked={window.UntrackedTimings} texture_bytes={Bytes(textureBytes)} " +
            $"texture_peak_bytes={Bytes(_sampledTexturePeak)} managed_bytes={managedBytes} managed_peak_bytes={_sampledManagedPeak}");
    }

    internal void ResetWindow()
    {
        _window = default;
        _windowAtlasLoads.Clear();
        _windowLoadsByPath.Clear();
    }

    private void RecordTiming(string path, long elapsed)
    {
        if (_windowLoadsByPath.TryGetValue(path, out var calls))
            _windowLoadsByPath[path] = calls + 1;
        else if (_windowLoadsByPath.Count < MaxWindowPaths)
            _windowLoadsByPath.Add(path, 1);
        else
            _window.UntrackedLoadPaths++;

        if (_loadedPaths.Contains(path))
        {
            _window.Hot.Add(elapsed);
        }
        else if (_loadedPaths.Count < MaxSessionIdentities)
        {
            _loadedPaths.Add(path);
            _window.Cold.Add(elapsed);
        }
        else
        {
            _window.UntrackedTimings++;
        }
    }

    private static string Quote(string value) =>
        string.Concat('"', value.Replace('"', '’').Replace('\n', ' ').Replace('\r', ' '), '"');

    private static string Bytes(long? bytes) => bytes?.ToString(CultureInfo.InvariantCulture) ?? "na";

    private struct Counts
    {
        internal int Requests;
        internal int Hits;
        internal int IsolatedLoads;
        internal int Fallbacks;
        internal int NativeLoads;
        internal int RasterDecodes;
        internal int NewNativeTextures;
        internal int AtlasPageLoads;
        internal int UntrackedBackings;
        internal int UntrackedTimings;
        internal int UntrackedLoadPaths;
        internal Timing Cold;
        internal Timing Hot;
    }

    private struct Timing
    {
        internal int Count;
        private long _sum;
        private long _max;

        private static readonly double TickToMilliseconds = 1000d / Stopwatch.Frequency;
        internal double AverageMs => Count == 0 ? 0 : _sum * TickToMilliseconds / Count;
        internal double MaxMs => _max * TickToMilliseconds;

        internal void Add(long elapsed)
        {
            Count++;
            _sum += elapsed;
            _max = Math.Max(_max, elapsed);
        }
    }
}
