using Godot;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

/// <summary>
/// Per-variant portraits (top-left run avatar, character-select icon, map marker, outline) for skins
/// that keep their looks in one pack and their variants in their own settings.
///
/// The Joan packs ship <c>portrait_palettes/&lt;appearance-1&gt;/s&lt;stage&gt;_{run,select,run_outline}_icon.png</c>
/// and their own code swaps those textures. That code only runs for the copy .NET actually loaded, so
/// the other re-export kept its default stage-1 portrait no matter which variant was selected. This
/// reads the same files from the selected pack and hands back the matching texture.
///
/// The files are raw PNGs inside the pack's PCK (no sibling <c>.import</c>), so they are decoded with
/// <see cref="Image.LoadPngFromBuffer"/> from the archive bytes.
///
/// Every lookup is memoised because the Godot getters that call it (<c>CharacterModel.IconTexture</c>,
/// <c>Icon</c>, <c>MapMarker</c>) run per frame; resolving the package root and the variant index on
/// every call was the source of the visible stutter when switching appearances.
/// </summary>
internal static class VariantPortraits
{
    private sealed record Entry(string Selection, Texture2D? Texture);

    private static readonly Dictionary<string, Entry> Cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The portrait the selected variant ships for this kind (<c>run</c>, <c>select</c>,
    /// <c>run_outline</c>) and damage stage, or null when the selection is not a settings variant or the
    /// package has no such file.
    /// </summary>
    internal static Texture2D? Resolve(string groupId, string kind, int stage)
    {
        try
        {
            var selection = SkinService.GetCharacterSelectionOptionId(groupId);
            if (selection == null)
            {
                return null;
            }

            var key = groupId + "|" + kind + "|" + stage;
            lock (Cache)
            {
                if (Cache.TryGetValue(key, out var hit))
                {
                    if (hit.Selection.Equals(selection, StringComparison.OrdinalIgnoreCase))
                    {
                        return GodotObject.IsInstanceValid(hit.Texture) ? hit.Texture : null;
                    }
                }
            }

            var texture = Load(groupId, selection, kind, stage);
            lock (Cache)
            {
                Cache[key] = new Entry(selection, texture);
            }

            return texture;
        }
        catch
        {
            return null;
        }
    }

    private static Texture2D? Load(string groupId, string selection, string kind, int stage)
    {
        if (ProviderSettingsVariantOptions.ContractOf(selection) is null)
        {
            return null;
        }

        var index = ProviderSettingsVariantOptions.VariantIndex(selection) ?? 0;

        // The pack's PNGs live inside its PCK and are not necessarily visible under the mounted res://
        // namespace, so read the bytes straight from the archive it shipped. Several folder layouts are
        // tried so a pack that follows the same idea (one portrait per appearance/stage) works without
        // new code; the first hit wins and a miss simply means "no per-variant portrait".
        foreach (var relative in PortraitCandidates(index, stage, kind))
        {
            var bytes = SkinService.Catalog?.ReadProviderFile(groupId, selection, relative);
            if (bytes == null || bytes.Length == 0)
            {
                continue;
            }

            var image = new Image();
            if (image.LoadPngFromBuffer(bytes) != Error.Ok)
            {
                continue;
            }

            return ImageTexture.CreateFromImage(image);
        }

        // The packs also ship one imported texture per stage (s2_select_icon.ctex) which is what their own
        // portrait code falls back to when no per-appearance PNG exists. Load it through Godot's resource
        // loader so every variant still gets the stage-correct portrait.
        var imported = $"{rootOf(groupId, selection)}/s{stage}_{kind}_icon.ctex";
        try
        {
            if (ResourceLoader.Exists(imported) &&
                ResourceLoader.Load<Texture2D>(imported) is { } stageTexture)
            {
                return stageTexture;
            }
        }
        catch
        {
            // Fall through: no stage texture for this pack.
        }

        return null;
    }

    private static string rootOf(string groupId, string selection) =>
        SkinService.Catalog?.GetAssetRootDirectory(groupId, selection) ?? string.Empty;

    /// <summary>
    /// Paths (relative to the package root) that may hold a per-variant portrait, in priority order.
    /// Joan packs use <c>portrait_palettes/&lt;appearance-1&gt;/s&lt;stage&gt;_&lt;kind&gt;_icon.png</c>;
    /// the alternatives are the obvious variants of that idea so similar packs need no code change.
    /// </summary>
    private static IEnumerable<string> PortraitCandidates(int index, int stage, string kind)
    {
        var file = $"s{stage}_{kind}_icon.png";
        // Index 0 is the appearance the pack itself calls the default; the packs ship no folder for it
        // and the canonical icon already is that art, so only the non-zero variants are looked up here.
        if (index > 0)
        {
            yield return $"portrait_palettes/{index}/{file}";
            yield return $"portraits/{index}/{file}";
            yield return $"variants/{index}/{file}";
            yield return $"appearance/{index}/{file}";
        }

        yield return $"portrait_palettes/{file}";
        yield return $"portraits/{file}";
    }

    /// <summary>
    /// Paints the selected variant's run portrait onto an already-built HUD node. The top bar keeps the
    /// Control instance it was given, so repainting it in place is the only way a variant change becomes
    /// visible without rebuilding the whole bar.
    /// </summary>
    internal static bool Paint(Node root, string groupId, int stage = 1)
    {
        var texture = Resolve(groupId, "run", stage);
        if (texture == null || root == null || !GodotObject.IsInstanceValid(root))
        {
            return false;
        }

        try
        {
            if (root is TextureRect self)
            {
                self.Texture = texture;
            }

            foreach (var node in Descendants(root))
            {
                if (node is TextureRect rect && GodotObject.IsInstanceValid(rect))
                {
                    rect.Texture = texture;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static IEnumerable<Node> Descendants(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            yield return child;
            foreach (var nested in Descendants(child))
            {
                yield return nested;
            }
        }
    }

    /// <summary>Drops every memoised portrait; called whenever the selection changes.</summary>
    internal static void ClearCache()
    {
        lock (Cache)
        {
            Cache.Clear();
        }
    }
}
