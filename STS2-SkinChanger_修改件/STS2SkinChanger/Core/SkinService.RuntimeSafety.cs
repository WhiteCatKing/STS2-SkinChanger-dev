using Godot;

namespace STS2SkinChanger.Core;

internal static partial class SkinService
{
    private static readonly ResourceFailureCircuit RuntimeFailures = new();
    private static int _sessionPreparedCount;
    private static long _sessionPreparedBytes;

    internal static void PrefetchCharacterChoices(string currentGroup)
    {
        if (CharacterPackPrefetch.TakeFailure() is { } failure) ModLog.Info("[预读] 已停止本次预读：" + failure);
        if (!SkinPreloader.GamePreloadSettled || Catalog == null) return;
        try
        {
            var groups = new[] { currentGroup, "ironclad", "silent", "regent", "necrobinder", "defect", "watcher" };
            var choices = groups.Distinct(StringComparer.OrdinalIgnoreCase)
                .SelectMany(group => GetSelectionResourcePackPaths(group, Config.GetSelection(group))).ToList();
            var family = Catalog.Groups.FirstOrDefault(group => group.Id == currentGroup);
            if (family != null)
                foreach (var option in family.Options.Where(option => option.IsRuntimeProvider).Take(12))
                    choices.AddRange(GetSelectionResourcePackPaths(currentGroup, option.Id));
            CharacterPackPrefetch.Schedule(choices.Distinct(StringComparer.OrdinalIgnoreCase).Take(4).ToArray());
        }
        catch (Exception error) { ModLog.Info("[预读] 跳过不可用的选角资源：" + error.GetBaseException().Message); }
    }

    // Evict only our strong references. Never Free/Dispose live Resources, and never delete
    // mounted PCKs: scenes and Spine can still own them after this cache releases its reference.
    private static void TrimRuntimeResidency(string? keepKey = null)
    {
        var bytes = RuntimeResourceBundles.Values.Sum(bundle => bundle.EstimatedBytes);
        foreach (var pair in RuntimeResourceBundles.OrderBy(pair => pair.Value.LastUse).ToArray())
        {
            if (RuntimeResourceBundles.Count <= RuntimeLoadSafety.BundleLimit &&
                bytes <= RuntimeLoadSafety.ResidentCacheBudget) break;
            if (pair.Key == keepKey) continue;
            RuntimeResourceBundles.Remove(pair.Key);
            bytes -= pair.Value.EstimatedBytes;
            var released = pair.Value.Resources.Values.ToHashSet(ReferenceEqualityComparer.Instance);
            foreach (var key in RuntimeResourceCache.Where(entry => released.Contains(entry.Value)).Select(entry => entry.Key).ToArray())
                RuntimeResourceCache.Remove(key);
        }
        // Standalone icon/scene lookups also retain references outside bundles.
        while (RuntimeResourceCache.Count > 256) RuntimeResourceCache.Remove(RuntimeResourceCache.Keys.First());
    }

    private static Resource LoadCheckedRuntimeResource(string path, bool sharedDependencies, long estimatedBytes)
    {
        // A root can decode its whole graph before returning. Recheck the graph estimate at
        // every root, rather than assuming a small allocation just because the root is a .tres.
        RuntimeLoadSafety.RequireHeadroom(Math.Max(32L * 1024 * 1024, estimatedBytes));
        var leaf = new[] { ".ctex", ".png", ".jpg", ".jpeg", ".webp", ".skel", ".spskel" }
            .Any(extension => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
        var resource = ResourceLoader.Load<Resource>(path, null,
            sharedDependencies || leaf ? ResourceLoader.CacheMode.Reuse : ResourceLoader.CacheMode.IgnoreDeep);
        if (resource == null || !GodotObject.IsInstanceValid(resource) ||
            resource is PackedScene scene && !scene.CanInstantiate() ||
            resource is Texture2D texture && (texture.GetWidth() <= 0 || texture.GetHeight() <= 0))
            throw new AppearanceResourceUnavailableException("资源无效，停止建模而不向 Spine 传递半成品：" + path);
        return resource;
    }
}
