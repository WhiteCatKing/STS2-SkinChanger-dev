namespace STS2SkinChanger.Core;

internal static class RuntimeDependencyPlan
{
    internal static string[] Order(IReadOnlyDictionary<string, string[]> dependencies)
    {
        var incoming = dependencies.Keys.ToDictionary(path => path, _ => 0, StringComparer.OrdinalIgnoreCase);
        var consumers = dependencies.Keys.ToDictionary(path => path, _ => new List<string>(), StringComparer.OrdinalIgnoreCase);
        foreach (var pair in dependencies)
            foreach (var path in pair.Value.Distinct(StringComparer.OrdinalIgnoreCase))
                if (incoming.ContainsKey(path) && !path.Equals(pair.Key, StringComparison.OrdinalIgnoreCase))
                { incoming[pair.Key]++; consumers[path].Add(pair.Key); }
        var ready = new Queue<string>(incoming.Where(pair => pair.Value == 0).Select(pair => pair.Key));
        var result = new List<string>();
        while (ready.TryDequeue(out var path))
        {
            result.Add(path);
            foreach (var consumer in consumers[path])
                if (--incoming[consumer] == 0) ready.Enqueue(consumer);
        }
        if (result.Count != dependencies.Count)
            throw new AppearanceResourceUnavailableException("资源依赖形成循环，已停止本次加载。");
        return result.ToArray();
    }
}
