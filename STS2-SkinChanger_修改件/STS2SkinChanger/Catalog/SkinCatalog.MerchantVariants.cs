namespace STS2SkinChanger.Catalog;

internal sealed partial class SkinCatalog
{
    // A package remains selectable in every merchant family, but missing art is a native
    // fallback option, never a runtime provider that can replay the wrong room's callbacks.
    internal static void AddMerchantVariantFallbackOptions(IDictionary<string, SkinGroup> groups)
    {
        var ids = new[] { "merchant", "fake_merchant_monster" };
        var originals = ids.Where(groups.ContainsKey).SelectMany(id => groups[id].Options)
            .Where(o => !o.IsMerchantVariantFallback).DistinctBy(o => o.Id, StringComparer.OrdinalIgnoreCase).ToArray();
        if (originals.Length == 0) return;
        foreach (var id in ids)
        {
            if (!groups.TryGetValue(id, out var group))
                groups[id] = group = new SkinGroup(id, DisplayName(id));
            foreach (var option in originals)
            {
                if (group.Options.Any(o => o.Id.Equals(option.Id, StringComparison.OrdinalIgnoreCase))) continue;
                group.Options.Add(option with { Assets = new Dictionary<string, ResourceAsset>(), IsRuntimeProvider = false,
                    IsDirectCharacterRuntimeProvider = false, UsesPrivateMerchantRoomCallback = false,
                    IsMerchantVariantFallback = true });
            }
        }
    }
}
