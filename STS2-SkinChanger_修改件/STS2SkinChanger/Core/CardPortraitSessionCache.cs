using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;

namespace STS2SkinChanger.Core;

// Called under SkinService's lock on the game thread. Owns only the textures we load or create.
internal sealed class CardPortraitSessionCache
{
    private const int MaxPaths = 1024;
    private readonly Dictionary<string, Entry?> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<ulong> _backingTextures = [];
    private long _estimatedBytes;

    internal bool TryGet(string path, bool wrapAtlas, out Texture2D? texture)
    {
        texture = null;
        if (!_entries.TryGetValue(path, out var entry)) return false;
        if (entry != null && GodotObject.IsInstanceValid(entry.Source))
            texture = entry.Resolve(wrapAtlas);
        return true;
    }

    internal Texture2D? GetOrLoad(string path, bool wrapAtlas, Func<Texture2D?> load)
    {
        if (TryGet(path, wrapAtlas, out var cached)) return cached;
        if (_entries.Count >= MaxPaths || _estimatedBytes >= RuntimeLoadSafety.ResidentCacheBudget)
            return null;

        RuntimeLoadSafety.RequireHeadroom(0);
        // Keep failed paths for this session as well, including refusals after decoding.
        // A second request key for the same alias must not try allocating the texture again.
        _entries.Add(path, null);
        var source = load();
        if (source == null) return null;
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
            // Ignore can share an external atlas. Charge it once and use the full atlas size,
            // since a small portrait region can keep a much larger texture alive.
            var bytes = _backingTextures.Contains(textureId) ? 0L :
                checked((long)backing.GetWidth() * backing.GetHeight() * 8);
            if (bytes > RuntimeLoadSafety.ResidentCacheBudget - _estimatedBytes)
                return null;

            var entry = new Entry(source);
            _entries[path] = entry;
            _backingTextures.Add(textureId);
            _estimatedBytes += bytes;
            return entry.Resolve(wrapAtlas);
        }
        finally
        {
            if (_entries[path] == null) DisposeTexture(source);
        }
    }

    internal void Clear()
    {
        foreach (var entry in _entries.Values)
        {
            if (entry == null) continue;
            DisposeTexture(entry.View);
            DisposeTexture(entry.Source);
        }
        _entries.Clear();
        _backingTextures.Clear();
        _estimatedBytes = 0;
    }

    private static void DisposeTexture(Texture2D? texture)
    {
        try { texture?.Dispose(); }
        catch (Exception exception)
        {
            ModLog.Warn("释放卡图会话缓存失败：" + exception.GetBaseException().Message);
        }
    }

    private sealed class Entry(Texture2D source)
    {
        internal Texture2D Source { get; } = source;
        internal AtlasTexture? View { get; private set; }

        internal Texture2D? Resolve(bool wrapAtlas)
        {
            // ImageTexture is already a full canvas. Wrapping it changes the size metadata
            // used by NCard. Imported plain textures need a persistent atlas view.
            if (!wrapAtlas || Source is AtlasTexture or ImageTexture) return Source;
            if (View != null) return GodotObject.IsInstanceValid(View) ? View : null;
            return View = new AtlasTexture
            {
                Atlas = Source,
                Region = new Rect2(0, 0, Source.GetWidth(), Source.GetHeight())
            };
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
