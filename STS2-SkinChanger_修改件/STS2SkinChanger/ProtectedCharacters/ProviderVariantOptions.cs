// 受保护兼容模块：十字军/贞德及同包变体。修改前阅读本目录 AGENTS.md 和 README.md。
using System.Globalization;

namespace STS2SkinChanger.Catalog;

/// <summary>
/// General compatibility for skin packs that ship several interchangeable character scenes in
/// the same PCK (for example CrusaderSkin's Default/Aggressor/Banneret/Templar/Infernal).
///
/// The provider remains untouched. Each sibling CombatVisual scene becomes a first-class option
/// in the existing character dropdown; the option carries its own scene asset, so preview, hot
/// switching and the normal application pipeline all select it directly.
///
/// A provider that instead resolves sub-variants through a getter is handled separately: Skin
/// Changer keeps CharacterModel.get_VisualsPath / get_MerchantAnimPath / get_RestSiteAnimPath
/// alive while the provider is selected, and only overrides them when a synthetic option is
/// explicitly chosen. To add support for another pack, no name list is needed: put the variant
/// scenes next to the base scene in the PCK with the pattern
/// <c>scenes/creature_visuals/&lt;group&gt;_&lt;variant&gt;.tscn</c>, plus optional
/// <c>scenes/merchant/characters/&lt;group&gt;_&lt;variant&gt;_merchant.tscn</c> and
/// <c>scenes/rest_site/characters/&lt;group&gt;_&lt;variant&gt;_rest_site.tscn</c>.
/// </summary>
internal static class ProviderVariantOptions
{
    internal static void Expand(IDictionary<string, SkinGroup> groups)
    {
        foreach (var group in groups.Values)
        {
            var options = group.Options.ToArray();
            foreach (var option in options)
            {
                if (option.Id.Contains("::variant:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var canonical = FindScene(option, group.Id);
                if (canonical == null) continue;
                var variants = FindVariants(option, group.Id).ToArray();
                if (variants.Length == 0) continue;

                var at = group.Options.IndexOf(option);
                group.Options.RemoveAt(at);
                group.Options.Insert(at++, option with
                {
                    Name = option.Name + "_默认"
                });
                foreach (var (suffix, sourcePath) in variants)
                {
                    var variant = new ResourceAsset(sourcePath);
                    variant.AddFile(canonical.Value.Value.Files[0].Archive, sourcePath);
                    var assets = new Dictionary<string, ResourceAsset>(option.Assets, StringComparer.Ordinal)
                    {
                        [canonical.Value.Key] = variant
                    };
                    group.Options.Insert(at++, option with
                    {
                        Id = option.Id + "::variant:" + suffix,
                        Name = option.Name + "_" + Label(suffix),
                        ProviderId = option.EffectiveProviderId,
                        Assets = assets
                    });
                }
            }
        }
    }

    private static KeyValuePair<string, ResourceAsset>? FindScene(
        SkinOption option, string stem)
    {
        foreach (var pair in option.Assets)
        {
            var path = pair.Value.SourcePath;
            if (!path.StartsWith("res://scenes/creature_visuals/", StringComparison.OrdinalIgnoreCase)) continue;
            if (!path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase)) continue;
            if (Path.GetFileNameWithoutExtension(path).Equals(stem, StringComparison.OrdinalIgnoreCase))
            {
                return pair;
            }
        }
        return null;
    }

    private static IEnumerable<(string Suffix, string Path)> FindVariants(SkinOption option, string groupId)
    {
        var prefix = "res://scenes/creature_visuals/" + groupId + "_";
        foreach (var pair in option.Assets)
        {
            var path = pair.Value.SourcePath;
            if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
            if (!path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase)) continue;
            var suffix = Path.GetFileNameWithoutExtension(path)[(groupId.Length + 1)..];
            if (suffix.Length == 0 || suffix.Contains('/')) continue;
            yield return (suffix, path);
        }
    }

    private static string Label(string suffix) =>
        suffix.ToLowerInvariant() switch
        {
            "aggressor" => "侵略者",
            "banneret" => "方旗骑士",
            "templar" => "圣殿骑士",
            "infernal" => "渊狱",
            "default" => "默认皮肤",
            _ => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(suffix.Replace('_', ' '))
        };
}
