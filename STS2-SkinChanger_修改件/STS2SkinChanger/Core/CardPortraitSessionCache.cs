using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace STS2SkinChanger.Core;

// Called under SkinService's lock on the game thread. Owns only the textures we load or create.
internal sealed class CardPortraitSessionCache
{
    private const int MaxPaths = 1024;
    private readonly Dictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly LinkedList<string> _lruOrder = new();
    private readonly HashSet<string> _failedPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<ulong, int> _backingTextureRefCounts = [];
    private long _estimatedBytes;

    internal long EstimatedBytes => _estimatedBytes;
    internal int Count => _entries.Count;

    internal bool TryGet(string path, bool wrapAtlas, out Texture2D? texture)
    {
        texture = null;
        if (_failedPaths.Contains(path)) return true;
        if (!_entries.TryGetValue(path, out var entry)) return false;
        if (GodotObject.IsInstanceValid(entry.Source))
        {
            Touch(entry);
            texture = entry.Resolve(wrapAtlas);
            return true;
        }

        RemoveEntry(path, entry);
        return false;
    }

    internal Texture2D? GetOrLoad(
        string path, bool wrapAtlas, Func<Texture2D?> load, CardPortraitPerformance? performance = null)
    {
        if (TryGet(path, wrapAtlas, out var cached)) return cached;

        RuntimeLoadSafety.RequireHeadroom(0);
        EnsureReleasePump();
        var source = load();
        if (source == null)
        {
            RememberFailedPath(path);
            return null;
        }

        var stored = false;
        try
        {
            var backing = source;
            for (var depth = 0; backing is AtlasTexture atlas && atlas.Atlas is { } inner; depth++)
            {
                if (depth >= 8)
                    throw new AppearanceResourceUnavailableException("卡图图集依赖层数过多。");
                backing = inner;
            }
            var textureId = backing.GetInstanceId();
            if (performance != null)
            {
                var isAtlasPage = !ReferenceEquals(source, backing);
                string? pagePath = null;
                if (isAtlasPage && performance.NeedsAtlasPath(textureId))
                {
                    try { pagePath = backing.ResourcePath; }
                    catch (Exception) { /* The measurement must not reject an otherwise usable texture. */ }
                }
                performance.ObserveNativeTexture(pagePath, textureId, isAtlasPage);
            }
            // Ignore can share an external atlas. Charge it once and use the full atlas size,
            // since a small portrait region can keep a much larger texture alive.
            var backingBytes = checked((long)backing.GetWidth() * backing.GetHeight() * 8);
            var alreadyTracked = _backingTextureRefCounts.ContainsKey(textureId);
            var incrementalBytes = alreadyTracked ? 0L : backingBytes;

            // A single card texture that meets or exceeds the entire resident budget cannot fit
            // even in an empty cache: reject, dispose on the game thread, and do not decode again.
            if (incrementalBytes >= RuntimeLoadSafety.ResidentCacheBudget)
            {
                RememberFailedPath(path);
                return null;
            }

            // Evict least-recently-used entries when browsing many card tabs (e.g. Card Library)
            // so the resident estimate stays within budget without permanently breaking later tabs.
            // Evicted entries drop cache ownership without force-disposing active native textures.
            while (_lruOrder.First is { } oldestNode &&
                   (_entries.Count >= MaxPaths ||
                    incrementalBytes > RuntimeLoadSafety.ResidentCacheBudget - _estimatedBytes))
            {
                var oldestPath = oldestNode.Value;
                if (_entries.TryGetValue(oldestPath, out var oldestEntry))
                {
                    RemoveEntry(oldestPath, oldestEntry);
                    // Recompute in case evicting the last sibling released the same backing texture.
                    alreadyTracked = _backingTextureRefCounts.ContainsKey(textureId);
                    incrementalBytes = alreadyTracked ? 0L : backingBytes;
                }
                else
                {
                    _lruOrder.RemoveFirst();
                }
            }

            if (incrementalBytes > RuntimeLoadSafety.ResidentCacheBudget - _estimatedBytes)
            {
                RememberFailedPath(path);
                return null;
            }

            var lruNode = _lruOrder.AddLast(path);
            var entry = new Entry(source, textureId, backingBytes, lruNode);
            _entries[path] = entry;
            TrackForGameThreadRelease(source);
            if (_backingTextureRefCounts.TryGetValue(textureId, out var count))
            {
                _backingTextureRefCounts[textureId] = count + 1;
            }
            else
            {
                _backingTextureRefCounts[textureId] = 1;
                _estimatedBytes += backingBytes;
            }
            stored = true;
            return entry.Resolve(wrapAtlas);
        }
        finally
        {
            if (!stored) DisposeTexture(source);
        }
    }

    internal void Clear()
    {
        // Nodes and provider dictionaries can hold the exact same managed wrappers across runs.
        // Drop our references even when the game has already freed the old card nodes.
        _entries.Clear();
        _lruOrder.Clear();
        _failedPaths.Clear();
        _backingTextureRefCounts.Clear();
        _estimatedBytes = 0;
        DrainRetiredTextures();
    }

    private void Touch(Entry entry)
    {
        if (entry.LruNode.List != _lruOrder || ReferenceEquals(entry.LruNode, _lruOrder.Last))
            return;
        _lruOrder.Remove(entry.LruNode);
        _lruOrder.AddLast(entry.LruNode);
    }

    private void RemoveEntry(string path, Entry entry)
    {
        _entries.Remove(path);
        if (entry.LruNode.List == _lruOrder)
            _lruOrder.Remove(entry.LruNode);
        if (_backingTextureRefCounts.TryGetValue(entry.BackingTextureId, out var count))
        {
            if (count <= 1)
            {
                _backingTextureRefCounts.Remove(entry.BackingTextureId);
                _estimatedBytes = Math.Max(0L, _estimatedBytes - entry.BackingBytes);
            }
            else
            {
                _backingTextureRefCounts[entry.BackingTextureId] = count - 1;
            }
        }
    }

    private void RememberFailedPath(string path)
    {
        if (_failedPaths.Count < MaxPaths)
            _failedPaths.Add(path);
    }

    private static readonly ConditionalWeakTable<Texture2D, TextureRetirement> TrackedTextures = new();
    private static readonly ConcurrentQueue<Texture2D> RetiredTextures = new();
    private static SceneTree? _releaseTree;

    private static void EnsureReleasePump()
    {
        if (_releaseTree != null) return;
        if (Engine.GetMainLoop() is not SceneTree tree)
            throw new AppearanceResourceUnavailableException("卡图释放队列需要在游戏场景树中运行。");

        tree.ProcessFrame += DrainRetiredTextures;
        _releaseTree = tree;
    }

    private static void TrackForGameThreadRelease(Texture2D texture)
    {
        // A dependent value may reference its weak key. The CLR only finalizes this value after
        // the wrapper is unreachable by the cache, the engine and other mods. Our finalizer
        // transports the wrapper without touching Godot; only the game thread calls Dispose.
        TrackedTextures.GetValue(texture, static value => new TextureRetirement(value));
        GC.SuppressFinalize(texture);
    }

    private static void DrainRetiredTextures()
    {
        // Bound the release work for any one frame after a large gallery is collected.
        for (var released = 0; released < 32 && RetiredTextures.TryDequeue(out var texture); released++)
            DisposeTexture(texture);
    }

    private sealed class TextureRetirement(Texture2D texture)
    {
        ~TextureRetirement() => RetiredTextures.Enqueue(texture);
    }

    private static void DisposeTexture(Texture2D? texture)
    {
        try { texture?.Dispose(); }
        catch (Exception exception)
        {
            ModLog.Warn("释放卡图会话缓存失败：" + exception.GetBaseException().Message);
        }
    }

    private sealed class Entry(Texture2D source, ulong backingTextureId, long backingBytes, LinkedListNode<string> lruNode)
    {
        internal Texture2D Source { get; } = source;
        internal ulong BackingTextureId { get; } = backingTextureId;
        internal long BackingBytes { get; } = backingBytes;
        internal LinkedListNode<string> LruNode { get; } = lruNode;
        internal AtlasTexture? View { get; private set; }

        internal Texture2D? Resolve(bool wrapAtlas)
        {
            // ImageTexture is already a full canvas. Wrapping it changes the size metadata
            // used by NCard. Imported plain textures need a persistent atlas view.
            if (!wrapAtlas || Source is AtlasTexture or ImageTexture) return Source;
            if (View != null) return GodotObject.IsInstanceValid(View) ? View : null;
            var view = new AtlasTexture
            {
                Atlas = Source,
                Region = new Rect2(0, 0, Source.GetWidth(), Source.GetHeight())
            };
            TrackForGameThreadRelease(view);
            return View = view;
        }
    }
}

[HarmonyPatch(typeof(RunManager), nameof(RunManager.CleanUp))]
internal static class CardPortraitSessionCleanupPatch
{
    [HarmonyPrefix]
    private static void Prefix(RunManager __instance, out bool __state) => __state = __instance.IsInProgress;

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(bool __state)
    {
        if (__state) SkinService.ResetCardPortraitSession();
    }
}
