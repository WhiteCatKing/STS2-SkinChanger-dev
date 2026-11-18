using STS2SkinChanger.Catalog;
using STS2SkinChanger.Pck;

// #5：带遗物图的先古包（AncientWaifus）原本因为「遗物图只认角色外观分组」而永远不被接管。
// 夹具照游戏真实结构：relic_atlas.sprites/<id>.tres 是 AtlasTexture 文本、引用图集页；
// 先古包只替换这几张切片。断言遗物图归属到提供它的先古分组（orobas）。
internal static class RelicOwnershipTests
{
    private const string AncientBg = "res://animations/backgrounds/orobas/orobas.png";
    private const string RelicAtlas = "res://images/atlases/relic_atlas.png";
    private const string RelicSprite = "res://images/atlases/relic_atlas.sprites/biiig_hug.tres";

    private const string BaselineSprite =
        "[gd_resource type=\"AtlasTexture\" load_steps=2 format=3]\n\n" +
        "[ext_resource type=\"Texture2D\" path=\"res://images/atlases/relic_atlas.png\" id=\"1\"]\n\n" +
        "[resource]\natlas = ExtResource(\"1\")\nregion = Rect2(980, 456, 79, 79)\nmargin = Rect2(2, 4, 6, 6)\n";

    internal static void Run()
    {
        var root = Directory.CreateTempSubdirectory("sc-relic-owner-");
        try
        {
            var game = Path.Combine(root.FullName, "game.pck");
            PckArchive.Write(game, new Dictionary<string, byte[]>
            {
                [AncientBg] = [1, 1],
                [RelicAtlas] = [1, 2],
                [RelicSprite] = System.Text.Encoding.UTF8.GetBytes(BaselineSprite)
            });

            var pack = Path.Combine(root.FullName, "ancient.pck");
            PckArchive.Write(pack, new Dictionary<string, byte[]>
            {
                [AncientBg] = [9, 9],
                // 只替换切片：AtlasTexture 文本指向同一张图集页
                [RelicSprite] = System.Text.Encoding.UTF8.GetBytes(BaselineSprite.Replace("980, 456", "12, 34"))
            });

            var catalog = SkinCatalog.Build(game, new[]
            {
                new SkinModDescriptor("AncientPack", "Ancient Pack", pack, false, root.FullName)
            });

            var owner = catalog.FindRelicIconOwnerGroup(RelicSprite);
            Require(owner == "orobas",
                $"先古包里的遗物图必须归属到提供它的先古分组（orobas），实际是 {owner ?? "null"}。");

            var group = catalog.Groups.Single(candidate => candidate.Id == "orobas");
            var selected = group.Options.Single(option => option.Id == "AncientPack");
            var relicPaths = catalog.GetProviderRelicSpritePaths(selected);
            Require(relicPaths.Contains(RelicSprite, StringComparer.OrdinalIgnoreCase),
                "选中该先古皮肤时必须带出它自己的遗物图，遗物才不会显示原版。");

            Console.WriteLine("Relic ownership policy passed.");
        }
        finally
        {
            try { root.Delete(recursive: true); } catch { }
        }
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}
