namespace STS2SkinChanger.Core;

internal static class BoundedTraversal
{
    // Iterative DFS avoids stack overflow; the visited set rejects cycles and the pending
    // limit also bounds a broken/infinite child enumerator, before materializing its list.
    internal static IEnumerable<T> Walk<T>(T root, Func<T, IEnumerable<T>> children, int limit = 8192) where T : class
    {
        var pending = new Stack<T>();
        var visited = new HashSet<T>(ReferenceEqualityComparer.Instance);
        pending.Push(root);
        while (pending.TryPop(out var node))
        {
            if (!visited.Add(node)) continue;
            if (visited.Count > limit) throw new AppearanceResourceUnavailableException("节点扫描超出安全上限，已停止本次遍历。");
            yield return node;
            var next = children(node).Take(limit + 1).ToArray();
            if (pending.Count + next.Length > limit)
                throw new AppearanceResourceUnavailableException("待处理节点超出安全上限，已停止本次遍历。");
            for (var i = next.Length - 1; i >= 0; i--) pending.Push(next[i]);
        }
    }
}
