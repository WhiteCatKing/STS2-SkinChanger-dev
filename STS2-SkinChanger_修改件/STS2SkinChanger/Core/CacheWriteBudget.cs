namespace STS2SkinChanger.Core;

internal enum CacheWriteRejectionReason { Budget, DiskSpace, ProtectedArchive }

internal sealed class CacheWriteRejectedException(string message,
    CacheWriteRejectionReason reason = CacheWriteRejectionReason.Budget) : IOException(message)
{
    internal CacheWriteRejectionReason Reason { get; } = reason;
    internal string? DisplayMessage { get; set; }
    public override string Message => DisplayMessage ?? base.Message;
}

// One instance survives catalog reloads. Reserved PCK names may still be read by Godot.
internal sealed class CacheWriteBudget
{
    internal const long DefaultLimit = 16L * 1024 * 1024 * 1024;
    internal const long DiskReserve = 128L * 1024 * 1024;
    private readonly object _sync = new();
    private readonly string[] _roots;
    private readonly long _limit;
    private readonly string? _currentSession;
    private readonly Func<string, long> _availableBytes;
    private readonly HashSet<string> _protected = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, long>? _files;

    internal CacheWriteBudget(IEnumerable<string> roots, long limit = DefaultLimit, string? currentSession = null,
        Func<string, long>? availableBytes = null)
    {
        _roots = roots.Select(Path.GetFullPath).ToArray();
        _limit = limit;
        _currentSession = currentSession;
        _availableBytes = availableBytes ?? (path => new DriveInfo(Path.GetPathRoot(path)!).AvailableFreeSpace);
    }

    internal void Protect(string path)
    {
        lock (_sync) _protected.Add(Path.GetFullPath(path));
    }

    internal void Write(string path, long bytes, Action write)
    {
        path = Path.GetFullPath(path);
        if (bytes < 0) throw new ArgumentOutOfRangeException(nameof(bytes));
        lock (_sync)
        {
            var root = _roots.FirstOrDefault(value => path.StartsWith(
                Path.TrimEndingDirectorySeparator(value) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
            if (root == null)
            {
                RequireDiskSpace(path, bytes);
                write();
                return;
            }
            if (!SkinChangerStorage.SafePath(path, root)) throw new IOException("资源包缓存路径含链接：" + path);
            if (_protected.Contains(path))
                throw new CacheWriteRejectedException("已保留的资源包不能覆盖写入：" + path, CacheWriteRejectionReason.ProtectedArchive);
            _files ??= Inventory();
            if (Required(path, bytes) > _limit)
            {
                _files = Inventory();
                foreach (var old in _files.Keys.OrderBy(File.GetLastAccessTimeUtc).ToArray())
                {
                    if (Required(path, bytes) <= _limit) break;
                    if (!old.EndsWith(".pck", StringComparison.OrdinalIgnoreCase) ||
                        old.Equals(path, StringComparison.OrdinalIgnoreCase) || _protected.Contains(old) ||
                        HasOtherSessionLease(old)) continue;
                    try { File.Delete(old); _files.Remove(old); }
                    catch (IOException) { }
                    catch (UnauthorizedAccessException) { }
                }
            }
            if (Required(path, bytes) > _limit)
                throw new CacheWriteRejectedException("皮肤缓存达到总量预算，保留当前外观，请重启游戏后再切换。");
            RequireDiskSpace(path, bytes);
            // Protect before returning to the caller that mounts this file.
            _protected.Add(path);
            try
            {
                write();
                _files[path] = new FileInfo(path).Length;
            }
            catch
            {
                _protected.Remove(path);
                try { File.Delete(path); } catch (IOException) { } catch (UnauthorizedAccessException) { }
                _files = null;
                throw;
            }
        }
    }

    private long Required(string path, long bytes) =>
        checked(_files!.Values.Sum() - _files.GetValueOrDefault(path) + bytes);

    private void RequireDiskSpace(string path, long bytes)
    {
        var required = checked(bytes + DiskReserve);
        var available = _availableBytes(path);
        if (available < required)
            throw new CacheWriteRejectedException($"缓存盘剩余 {available / 1024 / 1024} MiB，资源包及预留空间需要 {required / 1024 / 1024} MiB，已停止本次写入。", CacheWriteRejectionReason.DiskSpace);
    }

    private bool HasOtherSessionLease(string file)
    {
        for (var directory = Path.GetDirectoryName(file); directory != null &&
             !_roots.Contains(directory, StringComparer.OrdinalIgnoreCase); directory = Path.GetDirectoryName(directory))
        {
            var lease = Path.Combine(directory, ".active");
            if (directory.Equals(_currentSession, StringComparison.OrdinalIgnoreCase) || !File.Exists(lease)) continue;
            try { using var probe = new FileStream(lease, FileMode.Open, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException) { return true; }
            catch (UnauthorizedAccessException) { return true; }
        }
        return false;
    }

    private Dictionary<string, long> Inventory()
    {
        var files = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        foreach (var root in _roots)
        {
            if (!Directory.Exists(root)) continue;
            var pending = new Stack<string>();
            pending.Push(root);
            while (pending.TryPop(out var directory))
            {
                foreach (var child in Directory.EnumerateDirectories(directory))
                    if (SkinChangerStorage.SafePath(child, root)) pending.Push(child);
                foreach (var file in Directory.EnumerateFiles(directory))
                    if (SkinChangerStorage.SafePath(file, root)) files[file] = new FileInfo(file).Length;
            }
        }
        return files;
    }
}
