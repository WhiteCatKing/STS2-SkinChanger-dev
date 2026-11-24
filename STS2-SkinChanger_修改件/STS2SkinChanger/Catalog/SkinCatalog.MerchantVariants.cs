namespace STS2SkinChanger.Catalog;

internal sealed partial class SkinCatalog
{
    internal static bool IsMerchantAppearanceGroup(string id) =>
        id is "merchant" or "fake_merchant_monster" or "blitz_merchant";

    // A package remains selectable in every merchant family, but missing art is a native
    // fallback option, never a runtime provider that can replay the wrong room's callbacks.
    internal static void AddMerchantVariantFallbackOptions(IDictionary<string, SkinGroup> groups)
    {
        var ids = new[] { "merchant", "fake_merchant_monster" };
        var originals = ids.Where(groups.ContainsKey).SelectMany(id => groups[id].Options)
            .Where(o => !o.IsMerchantVariantFallback).DistinctBy(o => o.Id, StringComparer.OrdinalIgnoreCase).ToArray();
        if (originals.Length == 0) return;
        if (Core.MerchantVariantScenes.HasBlitz)
        {
            const string blitz = "blitz_merchant";
            // 部件接管选项覆盖的是各商人房间共用的原版资源（例如商店的手）：复制进闪电商人
            // 分组只会多出一个等价下拉项，还会让只做部件接管的风格包同时出现在两个分组里、
            // 被当成完整的商人皮肤。它留在 merchant 分组就够，闪电商人这里只列真正的整包外观。
            var blitzOptions = originals.Where(option => !option.IsBaselinePartOverride).ToArray();
            if (blitzOptions.Length > 0)
            {
                var group = new SkinGroup(blitz, Core.ModLocalization.BlitzMerchantTitle);
                foreach (var option in blitzOptions)
                    group.Options.Add(option);
                groups[blitz] = group;
            }
        }

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
