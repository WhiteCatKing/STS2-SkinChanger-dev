using System.Text.Json;
using System.Text.RegularExpressions;

namespace STS2SkinChanger.Core;

internal static class SpineAtlasPagePolicy
{
    public static bool IsAtlas(string path) =>
        path.EndsWith(".atlas", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".spatlas", StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<string> PagePaths(string atlasPath, string text)
    {
        if (text.TrimStart().StartsWith('{'))
        {
            using var document = JsonDocument.Parse(text);
            if (!document.RootElement.TryGetProperty("atlas_data", out var data) ||
                data.ValueKind != JsonValueKind.String) return [];
            if (document.RootElement.TryGetProperty("source_path", out var source) &&
                source.ValueKind == JsonValueKind.String &&
                source.GetString() is { Length: > 0 } sourcePath)
                atlasPath = sourcePath;
            text = data.GetString()!;
        }

        var lines = text.Replace("\r", "").Split('\n');
        var pages = new List<string>();
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            // Atlas pages start a block and are followed by page properties. Region names may
            // also be unindented, but their properties (bounds/rotate/xy/size) are not page headers.
            if (line.Length == 0 || (i > 0 && lines[i - 1].Trim().Length > 0)) continue;
            if (i + 1 >= lines.Length) continue;
            var header = lines[i + 1].Trim();
            if (!new[] { "size:", "format:", "filter:", "repeat:", "pma:" }
                    .Any(key => header.StartsWith(key, StringComparison.OrdinalIgnoreCase))) continue;
            pages.Add(ResolvePage(atlasPath, line));
        }
        return pages.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public static string? FindMissingPage(
        string atlasPath, string text, Func<string, bool> canResolve) =>
        PagePaths(atlasPath, text).FirstOrDefault(page => !canResolve(page));

    public static bool CanResolve(string path, IReadOnlyDictionary<string, byte[]> files)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var resolved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return Resolve(path);

        bool Resolve(string current)
        {
            if (resolved.Contains(current)) return true;
            if (!visited.Add(current)) return false;
            // Godot follows an import/remap even when the raw source is present.
            foreach (var suffix in new[] { ".import", ".remap" })
            {
                if (!files.TryGetValue(current + suffix, out var redirect)) continue;
                var targets = RedirectTargets(System.Text.Encoding.UTF8.GetString(redirect));
                if (targets.Count == 0 || !targets.All(Resolve)) return false;
                resolved.Add(current);
                return true;
            }
            if (!files.ContainsKey(current)) return false;
            resolved.Add(current);
            return true;
        }
    }

    public static IReadOnlyList<string> RedirectTargets(string text) =>
        Regex.Matches(text, "(?m)^\\s*path(?:\\.[a-z0-9_]+)?\\s*=\\s*\"(res://[^\"]+)\"",
                RegexOptions.IgnoreCase)
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

    private static string ResolvePage(string atlasPath, string page)
    {
        var path = page.StartsWith("res://", StringComparison.OrdinalIgnoreCase)
            ? page : atlasPath[..(atlasPath.LastIndexOf('/') + 1)] + page;
        var segments = new List<string>();
        foreach (var segment in path[6..].Replace('\\', '/').Split('/'))
        {
            if (segment == ".." && segments.Count > 0) segments.RemoveAt(segments.Count - 1);
            else if (segment is not ("." or "")) segments.Add(segment);
        }
        return "res://" + string.Join('/', segments);
    }
}

internal sealed class MissingSpinePageException(string pagePath) : InvalidOperationException(pagePath)
{
    public string PagePath { get; } = pagePath;
}
