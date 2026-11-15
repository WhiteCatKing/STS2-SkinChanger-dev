using STS2SkinChanger.Core.Features.Event;

internal static class EventFeaturePolicyTests
{
    internal static void Run()
    {
        const string baseId = "__base__";
        Require(EventFeaturePolicy.ShouldBeActive(null, baseId) == null,
            "目录未就绪时保留未知状态。");
        Require(EventFeaturePolicy.ShouldBeActive([], baseId) == false,
            "没装事件皮肤时不需要事件模块。");
        Require(EventFeaturePolicy.ShouldBeActive(
            [("event:abyssal_baths", new[] { baseId, "" })], baseId) == false,
            "只有原版或空选项时不激活。");
        Require(EventFeaturePolicy.ShouldBeActive(
            [("event:abyssal_baths", new[] { baseId, "nsfw-text-pack" })], baseId) == true,
            "任何事件分组有皮肤（含只改文字的包）就激活。");
        Require(EventFeaturePolicy.ShouldBeActive(
            [("fake_merchant_monster", new[] { "Merchant2CuteII" })], baseId) == true,
            "假商人事件的皮肤挂在 fake_merchant_monster 上，也要激活。");
        Require(EventFeaturePolicy.ShouldBeActive(
            [("ironclad", new[] { "character-skin" }), ("merchant", new[] { "pck" })], baseId) == false,
            "角色和真商人分组不触发事件模块。");
        Require(EventFeaturePolicy.ShouldBeActive(
            [("Event:abyssal_baths", new[] { "custom" })], baseId) == false,
            "事件分组前缀沿用 EventSkinPolicy 的大小写规则（小写 event:）。");
        Console.WriteLine("Event feature policy tests passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
