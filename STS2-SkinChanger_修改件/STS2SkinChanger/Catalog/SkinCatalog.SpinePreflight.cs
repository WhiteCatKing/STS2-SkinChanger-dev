using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace STS2SkinChanger.Catalog;

internal sealed partial class SkinCatalog
{
    private HashSet<(string Group, string Selection)>? _rejectedSpineSelections;

    internal string ResolveSpineSelection(string groupId, string selection) =>
        IsSpineSelectionRejected(groupId, selection) ? BaseOptionId : selection;

    internal bool IsSpineSelectionRejected(string groupId, string selection) =>
        _rejectedSpineSelections?.Contains(SpineSelectionKey(groupId, selection)) == true;

    internal bool RejectSpineSelection(string groupId, string selection) =>
        (_rejectedSpineSelections ??= []).Add(SpineSelectionKey(groupId, selection));

    private static (string, string) SpineSelectionKey(string groupId, string selection) =>
        (groupId.ToUpperInvariant(), ProviderSettingsVariantOptions.FamilyBaseId(selection).ToUpperInvariant());
}

internal static partial class SpineAtlasPagePolicy
{
    internal static bool IsAtlas(string path) =>
        path.EndsWith(".atlas", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".spatlas", StringComparison.OrdinalIgnoreCase);

    internal static IEnumerable<string> PagePaths(string atlasPath, byte[] bytes)
    {
        var text = Encoding.UTF8.GetString(bytes).Trim('\uFEFF', '\0', '\r', '\n', ' ', '\t');
        if (text.StartsWith('{'))
        {
            using var document = JsonDocument.Parse(text);
            if (!document.RootElement.TryGetProperty("atlas_data", out var data) || data.ValueKind != JsonValueKind.String)
                yield break;
            if (document.RootElement.TryGetProperty("source_path", out var source) &&
                source.ValueKind == JsonValueKind.String && source.GetString() is { Length: > 0 } sourcePath)
                atlasPath = sourcePath.Replace("res:///", "res://", StringComparison.Ordinal);
            text = data.GetString()!;
        }

        var lines = text.Split('\n');
        for (var i = 0; i + 1 < lines.Length; i++)
        {
            var page = lines[i].Trim();
            // A page begins a blank-line-separated block. Region names occur within that block.
            if (page.Length == 0 || (i > 0 && !string.IsNullOrWhiteSpace(lines[i - 1])))
                continue;
            var header = lines[i + 1].TrimStart();
            if (PageHeader().IsMatch(header))
                yield return ResolvePage(atlasPath, page);
        }
    }

    internal static IReadOnlyList<string> ValidateOverlay(IReadOnlyDictionary<string, byte[]> files)
    {
        var pages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (path, bytes) in files)
        {
            if (!path.StartsWith("res://sts2_skin_runtime/", StringComparison.OrdinalIgnoreCase) || !IsAtlas(path))
                continue;
            foreach (var page in PagePaths(path, bytes))
            {
                if (!pages.Add(page)) continue;
                if (!CanResolve(page, files, new HashSet<string>(StringComparer.OrdinalIgnoreCase)))
                    throw new MissingSpinePageException(page);
            }
        }
        return pages.ToArray();
    }

    private static bool CanResolve(string path, IReadOnlyDictionary<string, byte[]> files, HashSet<string> visited)
    {
        if (!visited.Add(path)) return false;
        foreach (var suffix in new[] { ".import", ".remap" })
        {
            if (!files.TryGetValue(path + suffix, out var bytes)) continue;
            var targets = RedirectTarget().Matches(Encoding.UTF8.GetString(bytes));
            // Godot follows the redirect first. Only the host's selected format is loaded later.
            return targets.Any(target =>
                CanResolve(target.Groups[1].Value, files, new HashSet<string>(visited, visited.Comparer)));
        }
        return files.ContainsKey(path);
    }

    private static string ResolvePage(string atlasPath, string page)
    {
        var path = page.StartsWith("res://", StringComparison.OrdinalIgnoreCase)
            ? page : atlasPath[..(atlasPath.LastIndexOf('/') + 1)] + page;
        var segments = new List<string>();
        if (!path.StartsWith("res://", StringComparison.OrdinalIgnoreCase)) return path;
        foreach (var segment in path[6..].Replace('\\', '/').Split('/'))
        {
            if (segment == ".." && segments.Count > 0) segments.RemoveAt(segments.Count - 1);
            else if (segment is not ("" or ".")) segments.Add(segment);
        }
        return "res://" + string.Join('/', segments);
    }

    [GeneratedRegex("^(size|format|filter|repeat|pma):", RegexOptions.IgnoreCase)]
    private static partial Regex PageHeader();

    [GeneratedRegex("""^\s*path(?:\.[a-z0-9_]+)?\s*=\s*"(res://[^"]+)" """, RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex RedirectTarget();
}

internal sealed class MissingSpinePageException(string pagePath, Exception? cause = null)
    : InvalidOperationException(pagePath, cause)
{
    internal string PagePath { get; } = pagePath;
}
