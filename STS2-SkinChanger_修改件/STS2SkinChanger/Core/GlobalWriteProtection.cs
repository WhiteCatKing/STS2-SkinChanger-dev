using System.Collections.Concurrent;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Core;

[Flags]
internal enum ForbiddenGlobalWrite { None = 0, F1 = 1, F2 = 2, F3 = 4 }

internal enum GlobalWriteScope { Publication, SharedPayloadFilter, SelectPortraitFilter, SafePackFilter }

internal sealed class GlobalWriteProtection
{
    private const string CardRoot = "res://images/packed/card_portraits/";
    private const string AtlasRoot = "res://images/atlases/";
    private readonly HashSet<string> _canonical = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ForbiddenGlobalWrite> _protected = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ForbiddenGlobalWrite> _legacyImports = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, byte[]> _gameHashes = new(StringComparer.OrdinalIgnoreCase);
    private readonly PckArchive _game;

    internal GlobalWriteProtection(PckArchive game, Func<string, IEnumerable<string>> dependencies)
    {
        _game = game;
        var redirectDependencies = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
        var pending = new Queue<(string Path, ForbiddenGlobalWrite Rule)>();
        foreach (var path in game.Paths)
        {
            _canonical.Add(path);
            var logical = WithoutRedirect(path);
            _canonical.Add(logical);
            var rule = DirectRule(logical);
            if (logical != path)
            {
                var legacy = SkinCatalog.IsCanonicalCharacterSelectPortrait(path)
                    ? ForbiddenGlobalWrite.F3 : ForbiddenGlobalWrite.None;
                if (path.StartsWith(AtlasRoot, StringComparison.OrdinalIgnoreCase)) legacy |= ForbiddenGlobalWrite.F1;
                if (path.StartsWith(CardRoot, StringComparison.OrdinalIgnoreCase)) legacy |= ForbiddenGlobalWrite.F2;
                if (legacy != ForbiddenGlobalWrite.None)
                {
                    var targets = dependencies(path);
                    redirectDependencies[path] = targets;
                    foreach (var target in targets.Where(IsImportedDependency))
                        _legacyImports[target] = _legacyImports.GetValueOrDefault(target) | legacy;
                }
            }
            if (rule == ForbiddenGlobalWrite.None && IsTopLevelAtlasPage(logical))
                rule = ForbiddenGlobalWrite.F1;
            if (rule == ForbiddenGlobalWrite.None && logical != path &&
                path.StartsWith(AtlasRoot, StringComparison.OrdinalIgnoreCase))
            {
                // Match the base-game atlas imports that SkinCatalog already protects.
                foreach (var dependency in redirectDependencies[path])
                {
                    if (dependency.StartsWith("res://.godot/", StringComparison.OrdinalIgnoreCase))
                        pending.Enqueue((dependency, ForbiddenGlobalWrite.F1));
                }
            }
            else if (rule != ForbiddenGlobalWrite.None)
            {
                pending.Enqueue((path, rule));
            }
        }

        while (pending.TryDequeue(out var entry))
        {
            var path = entry.Path;
            var previous = _protected.GetValueOrDefault(path);
            var added = entry.Rule & ~previous;
            if (added == ForbiddenGlobalWrite.None) continue;

            _protected[path] = previous | added;
            if (game.Contains(path))
            {
                foreach (var dependency in redirectDependencies.GetValueOrDefault(path) ?? dependencies(path))
                    pending.Enqueue((dependency, added));
            }
            var logical = WithoutRedirect(path);
            if (logical != path)
            {
                pending.Enqueue((logical, added));
                continue;
            }
            if (game.Contains(path + ".import")) pending.Enqueue((path + ".import", added));
            if (game.Contains(path + ".remap")) pending.Enqueue((path + ".remap", added));
        }
    }

    internal bool IsCanonical(string path) => _canonical.Contains(path);
    internal bool HasExactGameEntry(string path) => _game.Contains(path);

    internal bool MatchesGameEntry(string path, ReadOnlySpan<byte> bytes) =>
        _game.Contains(path) &&
        _game.GetFileSize(path) == (ulong)bytes.Length &&
        _gameHashes.GetOrAdd(path, _game.HashFileSha256).AsSpan()
            .SequenceEqual(System.Security.Cryptography.SHA256.HashData(bytes));

    internal bool MatchesGameEntry(PckArchive candidate, string candidatePath, string canonicalPath)
    {
        if (!_game.Contains(canonicalPath) || !candidate.Contains(candidatePath) ||
            _game.GetFileSize(canonicalPath) != candidate.GetFileSize(candidatePath))
            return false;

        // Do not use the PCK directory's supplied MD5 as proof of the published contents.
        var baseline = _gameHashes.GetOrAdd(canonicalPath, _game.HashFileSha256);
        return baseline.AsSpan().SequenceEqual(candidate.HashFileSha256(candidatePath));
    }

    internal ForbiddenGlobalWrite Decide(string path, bool fromGame, GlobalWriteScope scope = GlobalWriteScope.Publication)
    {
        if (fromGame) return ForbiddenGlobalWrite.None;
        if (scope == GlobalWriteScope.Publication)
            return _protected.TryGetValue(path, out var flags) && _game.Contains(path) ? flags : ForbiddenGlobalWrite.None;
        if (!_game.Contains(path)) return ForbiddenGlobalWrite.None;

        // These scopes keep the pre-existing projection boundaries. The broader publish
        // check still runs against the resulting pack when it reaches the actual mount.
        var legacy = _legacyImports.GetValueOrDefault(path);
        if (SkinCatalog.IsSharedGameAtlasPath(path)) legacy |= ForbiddenGlobalWrite.F1;
        if (SkinCatalog.IsCanonicalCharacterSelectPortrait(path)) legacy |= ForbiddenGlobalWrite.F3;
        return scope switch
        {
            GlobalWriteScope.SharedPayloadFilter => legacy & (ForbiddenGlobalWrite.F1 | ForbiddenGlobalWrite.F2),
            GlobalWriteScope.SelectPortraitFilter => legacy & ForbiddenGlobalWrite.F3,
            _ => legacy
        };
    }

    private static bool IsImportedDependency(string path) =>
        path.StartsWith("res://.godot/", StringComparison.OrdinalIgnoreCase);

    private static ForbiddenGlobalWrite DirectRule(string path)
    {
        var logical = WithoutRedirect(path);
        if (logical.StartsWith(CardRoot, StringComparison.OrdinalIgnoreCase)) return ForbiddenGlobalWrite.F2;
        if (SkinCatalog.IsCanonicalCharacterSelectPortrait(logical)) return ForbiddenGlobalWrite.F3;
        return logical.StartsWith(AtlasRoot, StringComparison.OrdinalIgnoreCase) &&
               SkinCatalog.IsSharedGameAtlasPath(logical) ? ForbiddenGlobalWrite.F1 : ForbiddenGlobalWrite.None;
    }

    private static string WithoutRedirect(string path) =>
        path.EndsWith(".import", StringComparison.OrdinalIgnoreCase) ? path[..^7] :
        path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase) ? path[..^6] : path;

    private static bool IsTopLevelAtlasPage(string path) =>
        path.StartsWith(AtlasRoot, StringComparison.OrdinalIgnoreCase) &&
        path.IndexOf('/', AtlasRoot.Length) < 0 &&
        (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
         path.EndsWith(".tpsheet", StringComparison.OrdinalIgnoreCase));
}
