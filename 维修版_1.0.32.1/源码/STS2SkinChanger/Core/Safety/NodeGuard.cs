using Godot;

namespace STS2SkinChanger.Core.Safety;

/// <summary>
/// "空串行"守卫（需求 2）：null、已释放、正在释放的 Godot 对象，以及空路径，都在调用链入口挡掉，
/// 不让它们沿着重建 / 回放链传下去，最后在原生层炸掉。
/// </summary>
internal static class NodeGuard
{
    /// <summary>对象非 null、未被释放、（若是节点）不在释放队列里。</summary>
    public static bool IsAlive([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] GodotObject? value) =>
        value != null &&
        GodotObject.IsInstanceValid(value) &&
        !(value is Node node && node.IsQueuedForDeletion());

    /// <summary>节点活着且在场景树里。</summary>
    public static bool IsAliveInTree([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] Node? node) =>
        IsAlive(node) && node.IsInsideTree();

    /// <summary>路径非空且不是纯空白。</summary>
    public static bool HasPath([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string? path) =>
        !string.IsNullOrWhiteSpace(path);

    /// <summary>资源路径存在才加载；不存在返回 null，绝不抛（报告"经验教训 6"）。</summary>
    public static T? LoadIfExists<T>(string? path) where T : Resource
    {
        if (!HasPath(path))
        {
            return null;
        }

        try
        {
            return ResourceLoader.Exists(path) ? ResourceLoader.Load<T>(path) : null;
        }
        catch
        {
            return null;
        }
    }
}
