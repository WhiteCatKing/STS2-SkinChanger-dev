using STS2SkinChanger.Core.Features.Merchant;

internal static class MerchantFeaturePolicyTests
{
    internal static void Run()
    {
        const string baseId = "__base__";
        Require(MerchantFeaturePolicy.ShouldBeActive(null, baseId) == null,
            "目录未就绪时保留未知状态。");
        Require(MerchantFeaturePolicy.ShouldBeActive([], baseId) == false,
            "空目录不需要商人模块。");
        Require(MerchantFeaturePolicy.ShouldBeActive(
            [("merchant", new[] { baseId, "", " " }), ("fake_merchant_monster", new[] { baseId })], baseId) == false,
            "只有原版或空选项时不激活。");
        Require(MerchantFeaturePolicy.ShouldBeActive(
            [("merchant", new[] { baseId, "pck-only" })], baseId) == true,
            "商人非原版资源选项应激活，不能仅检查 DLL 提供者。");
        Require(MerchantFeaturePolicy.ShouldBeActive(
            [("fake_merchant_monster", new[] { "style-part" })], baseId) == true,
            "假商人部件选项也应激活。");
        Require(MerchantFeaturePolicy.ShouldBeActive(
            [("ironclad", new[] { "character-skin" }), ("merchant", new[] { baseId })], baseId) == false,
            "角色的商店外观不应触发商人 NPC 模块。");
        Require(MerchantFeaturePolicy.ShouldBeActive(
            [("MERCHANT", new[] { "__BASE__", "custom" })], baseId) == true,
            "分组判定应沿用大小写不敏感规则。");
        Require(MerchantFeaturePolicy.ShouldBeActive(
            [("merchant", new[] { "__BASE__" })], baseId) == false,
            "原版 ID 的大小写差异不能变成新选项。");
        Console.WriteLine("Merchant feature policy tests passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
