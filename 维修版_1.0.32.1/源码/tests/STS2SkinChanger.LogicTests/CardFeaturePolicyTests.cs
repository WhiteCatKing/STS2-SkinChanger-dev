using STS2SkinChanger.Core.Features.Card;

internal static class CardFeaturePolicyTests
{
    internal static void Run()
    {
        const string baseId = "__base__";
        Require(CardFeaturePolicy.ShouldBeActive(null, baseId) == null,
            "卡图目录未就绪时保留未知状态。");
        Require(CardFeaturePolicy.ShouldBeActive([], baseId) == false,
            "空卡图目录可以停用模块。");
        Require(CardFeaturePolicy.ShouldBeActive(
            [(baseId, true), ("__BASE__", true), ("", true), (" ", true)], baseId) == false,
            "原版和空 ID 即使有资源也不能独立触发模块。");
        Require(CardFeaturePolicy.ShouldBeActive(
            [("character-without-card-art", false), ("empty-card-option", false)], baseId) == false,
            "只带角色外观或没有卡图内容的选项不需要卡图模块。");
        Require(CardFeaturePolicy.ShouldBeActive(
            [(baseId, false), ("card-pack", true)], baseId) == true,
            "装有有效卡图选项时即可启用，无需当前选择该皮肤。");
        Require(CardFeaturePolicy.ShouldBeActive(
            [("unrelated", false), ("single-card-or-character-art", true)], baseId) == true,
            "单卡或角色包贡献的卡图与卡图整包使用同一启用规则。");
        Console.WriteLine("Card feature policy tests passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
