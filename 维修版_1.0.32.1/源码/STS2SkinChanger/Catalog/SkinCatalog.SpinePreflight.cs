using System.Text;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Catalog;

internal sealed partial class SkinCatalog
{
    private readonly Dictionary<string, string?> _spinePageChecks = new(StringComparer.OrdinalIgnoreCase);

    private static string SpineCheckKey(string groupId, string selection) =>
        groupId + "\n" + ProviderSettingsVariantOptions.FamilyBaseId(selection);

    internal bool IsSpineSelectionRejected(string groupId, string selection) =>
        _spinePageChecks.TryGetValue(SpineCheckKey(groupId, selection), out var page) && page != null;

    internal void RejectSpineSelection(string groupId, string selection, string page) =>
        _spinePageChecks[SpineCheckKey(groupId, selection)] = page;

    private static RuntimeResourceOverlay ValidateAliasedSpinePages(RuntimeResourceOverlay overlay)
    {
        // Canonical provider dependencies are covered by the metadata check. Only newly aliased
        // atlases need every relative page in this overlay; existing card atlas reuse stays intact.
        foreach (var file in overlay.Files.Where(pair =>
                     pair.Key.StartsWith("res://sts2_skin_runtime/", StringComparison.OrdinalIgnoreCase) &&
                     SpineAtlasPagePolicy.IsAtlas(pair.Key)))
        {
            var missing = SpineAtlasPagePolicy.FindMissingPage(
                file.Key, Encoding.UTF8.GetString(file.Value),
                page => SpineAtlasPagePolicy.CanResolve(page, overlay.Files));
            if (missing != null) throw new MissingSpinePageException(missing);
        }
        return overlay;
    }

    // Metadata-only check before any selected provider PCK/callback is activated. Texture bytes
    // stay lazy. The alias builder checks the concrete, rewritten closure again before mounting.
    internal string? FindMissingSelectedSpinePage(string groupId, string selection)
    {
        if (selection.Equals(BaseOptionId, StringComparison.OrdinalIgnoreCase)) return null;
        var key = SpineCheckKey(groupId, selection);
        if (_spinePageChecks.TryGetValue(key, out var cached)) return cached;
        var selected = Groups.FirstOrDefault(group => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?
            .Options.FirstOrDefault(option => option.Id.Equals(selection, StringComparison.OrdinalIgnoreCase));
        if (selected == null) return null;
        var indexes = GetSelectionProviderIndexes(selected);
        foreach (var index in indexes)
        {
            foreach (var atlas in index.Archive.Paths.Where(SpineAtlasPagePolicy.IsAtlas))
            {
                var missing = SpineAtlasPagePolicy.FindMissingPage(
                    atlas, Encoding.UTF8.GetString(index.Archive.ReadFile(atlas)),
                    page => CanResolvePage(page, index));
                if (missing == null) continue;
                _spinePageChecks[key] = missing;
                return missing;
            }
        }
        _spinePageChecks[key] = null;
        return null;

        bool CanResolvePage(string page, PckResourceIndex owner)
        {
            var asset = selected.Assets.GetValueOrDefault(page) ?? owner.TryBuildAsset(page) ??
                        indexes.Select(index => index.TryBuildAsset(page)).FirstOrDefault(asset => asset != null) ??
                        ResolveBaseline(page);
            if (asset == null) return false;
            var resource = CreateRuntimeResource(page, asset);
            if (resource.RemapFile == null) return resource.DirectFile != null;
            var targets = SpineAtlasPagePolicy.RedirectTargets(Encoding.UTF8.GetString(resource.RemapFile.Read()));
            return targets.Count > 0 && targets.All(target =>
                MatchImportedFile(target, resource.PayloadFiles) != null);
        }
    }
}
