using STS2SkinChanger.Core;
using STS2SkinChanger.Core.Features.Background;

internal static class BackgroundFeaturePolicyTests
{
    internal static void Run()
    {
        const string baseId = "__base__";
        Require(BackgroundFeaturePolicy.ShouldBeActive(null, baseId) == null,
            "背景目录未就绪时保留未知状态。");
        Require(BackgroundFeaturePolicy.ShouldBeActive([], baseId) == false,
            "已就绪的空背景目录可以停用模块。");
        Require(BackgroundFeaturePolicy.ShouldBeActive(
            [baseId, "", " ", BackgroundSkinPolicy.FollowCategoryId, BackgroundSkinPolicy.ModDefaultId], baseId) == false,
            "原版、跟随分类和模组默认占位项不能单独触发模块。");
        Require(BackgroundFeaturePolicy.ShouldBeActive(
            ["__BASE__", "__FOLLOW__", "__MOD_DEFAULT__"], baseId) == false,
            "占位项按现有规则忽略大小写。");
        Require(BackgroundFeaturePolicy.ShouldBeActive(
            [baseId, BackgroundSkinPolicy.ProviderOptionId("game", "hive")], baseId) == true,
            "可切换的游戏战场也是有效背景选项。");
        Require(BackgroundFeaturePolicy.ShouldBeActive(
            [BackgroundSkinPolicy.ProviderOptionId("provider", "battlefield")], baseId) == true,
            "已发现的提供者背景选项应激活模块。");
        Console.WriteLine("Background feature policy tests passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
