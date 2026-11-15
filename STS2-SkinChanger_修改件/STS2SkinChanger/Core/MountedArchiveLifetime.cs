namespace STS2SkinChanger.Core;

// Godot retains PCK directory entries after the file used to mount them has closed.
// An OS file lock is not a lifetime guarantee. Never delete/rewrite a reserved PCK
// name during this process; old native entries can still contain offsets into it.
internal sealed class MountedArchiveLifetime
{
    private readonly HashSet<string> _paths = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new();

    internal void Pin(string path)
    {
        lock (_sync) _paths.Add(Path.GetFullPath(path));
    }

    internal bool IsPinned(string path)
    {
        lock (_sync) return _paths.Contains(Path.GetFullPath(path));
    }
}
