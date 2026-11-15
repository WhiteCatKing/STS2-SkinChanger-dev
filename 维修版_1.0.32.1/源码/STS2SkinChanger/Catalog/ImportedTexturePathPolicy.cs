namespace STS2SkinChanger.Catalog;

internal static class ImportedTexturePathPolicy
{
    // Godot's hash identifies the source path, not its compression format or pixel contents.
    public static string? SourceKey(string path)
    {
        const string prefix = "res://.godot/imported/";
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;
        var name = path[prefix.Length..];
        var suffix = new[] { ".s3tc.ctex", ".bptc.ctex" }
            .FirstOrDefault(value => name.EndsWith(value, StringComparison.OrdinalIgnoreCase));
        if (suffix == null) return null;
        var stem = name[..^suffix.Length];
        var separator = stem.LastIndexOf('-');
        return separator > 0 && stem.Length - separator - 1 == 32 &&
               stem[(separator + 1)..].All(Uri.IsHexDigit)
            ? stem : null;
    }
}
