using System.Text;
using System.Text.Json;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

internal static class SpineResourcePolicyTests
{
    internal static void Run()
    {
        const string stem = "res://.godot/imported/portrait.png-1b865d1e0a90d3a7fe38379cdb48c8d3";
        Require(ImportedTexturePathPolicy.SourceKey(stem + ".s3tc.ctex") ==
                ImportedTexturePathPolicy.SourceKey(stem + ".bptc.ctex"),
            "同名同 hash 的 s3tc/bptc 应归到同一源图。");
        Require(ImportedTexturePathPolicy.SourceKey(stem + ".s3tc.ctex") !=
                ImportedTexturePathPolicy.SourceKey(stem.Replace("1b865", "2b865") + ".bptc.ctex"),
            "同名不同 hash 不能合并。");
        Require(ImportedTexturePathPolicy.SourceKey(stem + ".spskel") == null &&
                ImportedTexturePathPolicy.SourceKey("res://portrait.png-x.s3tc.ctex") == null,
            "不能用纹理格式推断其他资源或没有源 hash 的路径。");

        const string atlasPath = "res://sts2_skin_runtime/test/animations/hero/hero.atlas";
        const string atlas = "\r\nfirst.png\r\nsize: 10,10\r\nfilter: Linear,Linear\r\n" +
                             "region\r\n  rotate: false\r\n  xy: 0,0\r\n  size: 10,10\r\n\r\n" +
                             "../second.ctex\r\n\tfilter: Linear,Linear\r\nregion2\r\n\tbounds: 0,0,5,5\r\n";
        var pages = SpineAtlasPagePolicy.PagePaths(atlasPath, atlas);
        Require(pages.SequenceEqual(new[] {
                "res://sts2_skin_runtime/test/animations/hero/first.png",
                "res://sts2_skin_runtime/test/animations/second.ctex" }),
            "多页、缩进、CRLF 和相对目录应解析准确，区域不能当作贴图页。");
        var json = JsonSerializer.Serialize(new { source_path = atlasPath, atlas_data = atlas });
        Require(SpineAtlasPagePolicy.PagePaths("res://_payload/hero.spatlas", json).SequenceEqual(pages),
            "导入 sp atlas 要按 JSON 的 source_path 定位页，不能按 payload 目录定位。");

        var files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
        {
            [pages[0] + ".import"] = Encoding.UTF8.GetBytes(
                "[remap]\npath.s3tc=\"res://payload/first.s3tc.ctex\"\npath.bptc=\"res://payload/first.s3tc.ctex\"\n"),
            ["res://payload/first.s3tc.ctex"] = [1],
            [pages[1]] = [2]
        };
        Require(SpineAtlasPagePolicy.FindMissingPage(atlasPath, atlas,
                page => SpineAtlasPagePolicy.CanResolve(page, files)) == null,
            "完整别名页及可解析重定向不能被拦截，包括多个格式指向同一载荷。");
        files.Remove(pages[1]);
        Require(SpineAtlasPagePolicy.FindMissingPage(atlasPath, atlas,
                page => SpineAtlasPagePolicy.CanResolve(page, files)) == pages[1],
            "只缺第二页也必须在原生加载前发现。");
        files[pages[1]] = [2];
        files.Remove("res://payload/first.s3tc.ctex");
        Require(!SpineAtlasPagePolicy.CanResolve(pages[0], files),
            "只有 .import 没有实际载荷仍是缺页。");
        files[pages[0]] = [3];
        Require(!SpineAtlasPagePolicy.CanResolve(pages[0], files),
            "原图不能掩盖 Godot 优先采用的坏重定向。");
        Require(SpineAtlasPagePolicy.FindMissingPage("res://plain.atlas", "", _ => false) == null &&
                !SpineAtlasPagePolicy.IsAtlas("res://scene.tscn"),
            "没有图集页时不能扩大拦截范围。");
        Console.WriteLine("Spine resource policy tests passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
