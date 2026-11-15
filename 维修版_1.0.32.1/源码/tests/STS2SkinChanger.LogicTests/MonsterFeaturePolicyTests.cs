using STS2SkinChanger.Core.Features.Monster;

internal static class MonsterFeaturePolicyTests
{
    internal static void Run()
    {
        const string baseId = "__base__";
        string[] noneRegistered = [];
        Require(MonsterFeaturePolicy.ShouldBeActive(null, noneRegistered, baseId) == null,
            "目录未就绪时保留未知状态。");
        Require(MonsterFeaturePolicy.ShouldBeActive([], noneRegistered, baseId) == false,
            "没有任何分组时不需要怪物模块。");
        Require(MonsterFeaturePolicy.ShouldBeActive(
            [("cultists", new[] { baseId, "" }, true)], noneRegistered, baseId) == false,
            "怪物分组只有原版或空选项时不激活。");
        Require(MonsterFeaturePolicy.ShouldBeActive(
            [("cultists", new[] { baseId, "cute" }, true)], noneRegistered, baseId) == true,
            "选项带运行时怪物外观（DLL 型皮肤）就激活。");
        Require(MonsterFeaturePolicy.ShouldBeActive(
            [("cultists", new[] { baseId, "pck-skin" }, false)], ["cultists"], baseId) == true,
            "已登记的普通 PCK 怪物分组有皮肤也激活（两项运行时元数据均为空）。");
        Require(MonsterFeaturePolicy.ShouldBeActive(
            [("merchant", new[] { "Merchant2CuteII" }, true),
             ("fake_merchant_monster", new[] { "Merchant2CuteII" }, true)],
            ["merchant", "fake_merchant_monster"], baseId) == false,
            "真商人和假商人分组即使登记过也归商人模块。");
        Require(MonsterFeaturePolicy.ShouldBeActive(
            [("ironclad", new[] { "skin" }, false)], noneRegistered, baseId) == false,
            "角色分组不触发怪物模块。");
        Console.WriteLine("Monster feature policy tests passed.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
