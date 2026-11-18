namespace STS2SkinChanger.Catalog;

internal static class ImportedTexturePathPolicy
{
    internal static string? SourceKey(string path)
    {
        const string prefix = "res://.godot/imported/";
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !path.EndsWith(".ctex", StringComparison.OrdinalIgnoreCase))
            return null;
        // The hash identifies the source path across compression formats, not the image bytes.
        var separator = path.LastIndexOf('-');
        return separator >= prefix.Length && path.Length > separator + 33 &&
               path[separator + 33] == '.' && path.AsSpan(separator + 1, 32).IndexOfAnyExcept("0123456789abcdefABCDEF") < 0
            ? path[..(separator + 33)] : null;
    }

    internal static Dictionary<string, string> UnambiguousSources(Dictionary<string, string> importedToSource) =>
        importedToSource.Select(pair => (Key: SourceKey(pair.Key), Source: pair.Value))
            .Where(pair => pair.Key != null)
            .GroupBy(pair => pair.Key!, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Select(pair => pair.Source).Distinct(StringComparer.OrdinalIgnoreCase).Count() == 1)
            .ToDictionary(group => group.Key, group => group.First().Source, StringComparer.OrdinalIgnoreCase);
}
