using STS2SkinChanger.Core;

namespace STS2SkinChanger.Catalog;

internal sealed partial class SkinCatalog
{
    private void AddVakuuVanillaArtOption()
    {
        // Only offer the built-in variant if this game version really ships both images.
        if (ResolveBaseline(VakuuVanillaArt.NormalPath) == null || ResolveBaseline(VakuuVanillaArt.EasterEggPath) == null) return;
        var group = _groups.FirstOrDefault(candidate => candidate.Id == "vakuu");
        if (group == null) { group = new SkinGroup("vakuu", "Vakuu"); _groups.Add(group); }
        if (group.Options.All(option => option.Id != VakuuVanillaArt.EasterEggId))
            group.Options.Insert(0, new SkinOption(VakuuVanillaArt.EasterEggId, VakuuVanillaArt.EasterEggId,
                new Dictionary<string, ResourceAsset>()));
    }
}
