using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Catalog;

// Describes the bytes actually written at each private path. A later incremental mount can
// refer to an earlier page, but only metadata for files that were really mounted is admitted.
internal sealed record CardPrivateFile(CardFileStamp? Stamp, string[] References, string ImportHash, bool IsKnown = true)
{
    internal static CardPrivateFile Inspect(ResourceFile source, string alias, byte[] bytes)
    {
        CardPrivateFile Unknown() => new(source.SourceStamp, [], "", IsKnown: false);
        if (source.SourceStamp is not { } stamp) return Unknown();
        var ext = Path.GetExtension(alias).ToLowerInvariant();
        var data = bytes.AsSpan();
        if (ext == ".ctex" && data.StartsWith("GST2"u8) ||
            ext == ".png" && data.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) ||
            (ext is ".jpg" or ".jpeg") && data.StartsWith(new byte[] { 255, 216, 255 }) ||
            ext == ".webp" && data.StartsWith("RIFF"u8) && data.Length >= 12 && data[8..12].SequenceEqual("WEBP"u8))
            return new(stamp, [], "");
        if (ext is ".remap" or ".import")
        {
            var targets = new List<string>();
            foreach (var line in Encoding.UTF8.GetString(bytes).Split('\n'))
            {
                var key = line.Split('=', 2);
                if (key.Length != 2 || !(key[0].Trim() == "path" ||
                    key[0].Trim().StartsWith("path.", StringComparison.Ordinal))) continue;
                targets.Add(key[1].Trim().Trim('"'));
            }
            return targets.Count == 0 ? Unknown() :
                new(stamp, targets.ToArray(), Convert.ToHexString(SHA256.HashData(bytes)));
        }
        if (ext is not (".res" or ".tres")) return Unknown();
        if (CardBinaryResource.Parse(bytes) is { } binary)
            return binary.RootType is "AtlasTexture" or "ImageTexture" or "CompressedTexture2D" or "PortableCompressedTexture2D"
                ? new(stamp, binary.Dependencies(alias).ToArray(), "") : Unknown();

        var text = Encoding.UTF8.GetString(bytes);
        if (!text.StartsWith("[gd_resource type=\"AtlasTexture\"", StringComparison.Ordinal)) return Unknown();
        var references = new List<string>();
        foreach (var line in text.Split('\n').Select(value => value.Trim()))
        {
            if (!line.StartsWith("[ext_resource", StringComparison.Ordinal))
            {
                // Only the exported AtlasTexture form declares external paths in ext_resource.
                if (line.Contains("://", StringComparison.Ordinal) || line.Contains("Resource(", StringComparison.Ordinal)
                    && !line.Contains("ExtResource(", StringComparison.Ordinal)) return Unknown();
                continue;
            }
            var path = Regex.Match(line, @"\bpath=""([^""]+)""");
            if (!path.Success || line.Contains("uid://", StringComparison.Ordinal)) return Unknown();
            references.Add(path.Groups[1].Value);
        }
        return new(stamp, references.ToArray(), "");
    }
}

internal sealed record CardPrivateIdentity(CardFileStamp Stamp, string Key);

internal sealed class CardPrivateGraph
{
    private readonly Dictionary<string, CardPrivateFile> _files = new(StringComparer.Ordinal);
    private readonly Dictionary<string, CardPrivateIdentity> _resolved = new(StringComparer.Ordinal);

    private readonly HashSet<CardFileStamp> _sources = [];
    internal bool HasFiles => _files.Count != 0;
    internal IReadOnlyCollection<CardFileStamp> Sources => _sources;

    internal void Add(string path, CardPrivateFile file)
    {
        _files.Add(path, file);
        if (file.Stamp != null) _sources.Add(file.Stamp);
        _resolved.Clear();
    }

    internal CardPrivateIdentity? Resolve(string root, string token)
    {
        var prefix = $"res://sts2_skin_runtime/{token}/";
        if (!root.StartsWith(prefix, StringComparison.Ordinal)) return null;
        if (_resolved.TryGetValue(root, out var cached)) return cached;
        var visited = new SortedSet<string>(StringComparer.Ordinal);
        CardFileStamp? stamp = null;
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        bool Visit(string path)
        {
            if (!path.StartsWith(prefix, StringComparison.Ordinal) ||
                path[prefix.Length..].Split('/').Any(part => part is "." or ".." or "")) return false;
            if (!_files.TryGetValue(path, out var file) && !_files.ContainsKey(path + ".import") &&
                !_files.ContainsKey(path + ".remap")) return false;
            if (visiting.Count >= 64 || visited.Count >= 256 || visiting.Contains(path)) return false;
            if (!visited.Add(path)) return true;
            visiting.Add(path);
            if (file != null)
            {
                if (!file.IsKnown || file.Stamp == null || (stamp != null && stamp != file.Stamp)) return false;
                stamp = file.Stamp;
                if (!file.References.All(Visit)) return false;
            }
            var complete = (!_files.ContainsKey(path + ".import") || Visit(path + ".import")) &&
                (!_files.ContainsKey(path + ".remap") || Visit(path + ".remap"));
            visiting.Remove(path);
            return complete;
        }

        if (!Visit(root) || stamp == null) return null;
        var graphKey = string.Join("\n", visited.Select(path => path + "\n" + _files.GetValueOrDefault(path)?.ImportHash));
        return _resolved[root] = new(stamp, $"{stamp.Key}\nnative\n{root}\n{graphKey}");
    }
}
